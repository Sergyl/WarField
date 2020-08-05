using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Profiling;

public class PlayerCode : NetworkBehaviour
{
	#region GAMEOBJECTS
	public GameObject bulletPrefab;
	public GameObject UI;
    public GameObject Win;
    public GameObject Lose;
	public GameObject StreamingList;
	public GameObject StreamingListAndroid;
	public GameObject ChatUI;
	public GameObject Crosshair;
	public GameObject ClickShoot;
	public Text UIWinner;
	#endregion

	#region ANDROID
	[SerializeField] private JoystickFunction JoystickMovement;
	public GameObject JoystickAndroid;
	public GameObject JoystickCamera;
	public GameObject ButtonForInitAndroid;
	public GameObject ButtonForExitAndroid;
	public GameObject ButtonForDevices;
	public GameObject ButtonForChat;
	private bool isAndroid = false;
	#endregion


	#region CHAT
	public InputField uMessageInput;
	public Button uSend;
	public Text Name;
	#endregion

	#region VARIABLES
	public Transform bulletSpawn;
    public RectTransform HealthFromHead;
    public RectTransform HealthForScreen;
    private CharacterController controller;
	private Vector3 moveDirection = Vector3.zero;
	private string nameWinner = "";
	private int[] lifeOfEnemy;
	private int[] scoreOfEnemy;
	private int cont = 0;
	private bool CursorLockedVar;
	private bool finish = false;
	private bool lose = false;
	private readonly float sensibility = 1.0f;
	private readonly float speed = 10.0f;
	private readonly float gravity = 150.0f;
	private float horizontalRotation;
	private float verticalRotation;
	#endregion

	#region VARIABLES PARA PRUEBAS
	//private Sampler sampler;
	//private readonly int numberSamples = 1000;
	//private int numberSamplesWritten = 0;
	//private string getDate;
	#endregion

	//------------------------------------------------ VARIABLES JUGADOR AUTOMATICO -----------------------------------------------//
	#region AUTOMATICO
	private Transform enemy;
	private readonly Transform player;
	private RaycastHit theHit;
	private bool onRange = false;
	private bool isShooting = false;
	private readonly float MoveSpeed = 10.0f;
	private readonly float MaxDist = 100.0f;
	private readonly float MinDist = 10.0f;
	#endregion
	//-----------------------------------------------------------------------------------------------------------------------------//


	//------------------------------------------------ VARIABLES JUGADOR TORPEDO -----------------------------------------------//
	private float timeShoot = 0.0f;
	private float timeSeconds = 0.0f;
	private int contadorSegundos = 0;
	//-----------------------------------------------------------------------------------------------------------------------------//

	[SyncVar(hook = "OnChangeName")]
	public string pName = "Player";

	[SyncVar(hook = "OnChangeColor")]
	public Color playerColor = Color.white;

	[SyncVar]
	private bool isSending = false;

	[SyncVar]
	private bool isBusy = false;

	[SyncVar]
	private string date;

	[SyncVar]
	private bool win = false;


	private void Start()
	{
		if (!isLocalPlayer)
			return;

		// ESTE CODIGO SIRVE PARA RECOGER MUESTRAS DE LA API DE NETWORKING. APARECE COMENTADO PORQUE
		// NO ES NECESARIO COGER MUESTRAS YA.

		//	if (isServer)
		//	getDate = System.DateTime.Now.ToString("HH-mm-ss").ToString();

		//Se guarda en la variable controller el componente Character Controller que se encuentra adherido a este GameObject.
		controller = GetComponent<CharacterController>();

		// Bloquea el cursor al centro de la pantalla.
		Cursor.lockState = CursorLockMode.Locked;

		//Esconde el cursor durante el tiempo de juego.
		Cursor.visible = (false);
		CursorLockedVar = true;

		//Visualiza la interfaz de usuario correspondiente al jugador.
		UI.SetActive(true);

		// Si la plataforma en la que se ejecuta el juego es Android, aparecerán los joystick y los botones adicionales. En caso contrario, no serán visibles.
		if (Application.platform == RuntimePlatform.Android)
		{
			JoystickAndroid.SetActive(true);
			ClickShoot.SetActive(true);
			JoystickCamera.SetActive(true);
			ButtonForInitAndroid.SetActive(true);
			ButtonForExitAndroid.SetActive(true);
			ButtonForDevices.SetActive(true);
			isAndroid = true;
		}
		else
		{
			isAndroid = false;
		}

		//------------------------------------------------ START DEL JUGADOR AUTOMATICO -----------------------------------------------//
		if (pName == "IA")
			InvokeRepeating("Shoot", 0.1f, 0.4f);
		//-----------------------------------------------------------------------------------------------------------------------------//
	}

	void OnDestroy()
	{
		if (!isLocalPlayer)
			return;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = (true);
		CursorLockedVar = false;
	}

	public override void OnStartServer()
	{
		SetPlayerName();
		SetPlayerColor();
	}

	public override void OnStartClient()
	{
		SetPlayerName();
		SetPlayerColor();
	}

	void Update()
	{
		if (!isLocalPlayer)
			return;

		if ((lose || win) && Crosshair.activeSelf)
			Crosshair.SetActive(false);


		// ESTE CODIGO SIRVE PARA RECOGER MUESTRAS DE LA API DE NETWORKING. APARECE COMENTADO PORQUE
		// NO ES NECESARIO COGER MUESTRAS YA.

		/*
		if(isServer && !isAndroid)
		{
			sampler = Sampler.Get("NetworkIdentity.UNetStaticUpdate()");
			float sampleValue = (sampler.GetRecorder().elapsedNanoseconds / 1000000.0f);

			if (numberSamplesWritten <= numberSamples)
			{
				StreamWriter sw = new StreamWriter(Application.dataPath + "/LogUNET-"+getDate+".txt", true);
				sw.Write(System.Math.Round(sampleValue, 3));
				sw.WriteLine();
				sw.Close();
				numberSamplesWritten++;
			}
		}
		*/

		//Función para comprobar si el juego tiene la variable Finish a true o a false
		CheckFinish(finish);

		//Mediante esta función, si el jugador detecta que ha perdido la partida aparecerá su cartel de derrota, y lo mismo con la victoria.
		PutLoseOrWinScreen();

		//Mediante este if permitirá que aparezca la ventana de daño cuando algún enemigo o jugador causa daño.
		if (HealthForScreen.sizeDelta != HealthFromHead.sizeDelta)
			UI.GetComponent<Image>().enabled = true;
		else
			UI.GetComponent<Image>().enabled = false;

		//Mediante esta línea, la barra de control de salud que aparece en la parte superior de la pantalla del jugador, será la misma que la que aparece en la parte superior del avatar.
		HealthForScreen.sizeDelta = HealthFromHead.sizeDelta;


		/* Dependiendo del nombre que hayamos introducido en el Lobby al jugador, este se comportará de una manera u otra. Este tipo de jugadores aparece
		 *  en la memoria, pero son los siguientes:
		 *  
		 *  -- Jugador automatico: Un jugador IA
		 *  -- Jugador torpedo: Se encargará de sobrecargar la red con acciones innecesarias
		 *  -- Jugador normal: Aquel jugador que es controlado sin problema
		 */


		//------------------------------------------------ UPDATE DEL JUGADOR AUTOMATICO -----------------------------------------------//
		if (pName == "IA")
		{
			//Mediante estas dos líneas el jugador IA no podrá volar.
			moveDirection.y = moveDirection.y - (gravity * Time.deltaTime);
			controller.Move(moveDirection * Time.deltaTime);

			// Este try/catch realiza la misma función que la que realizan los enemigos. Se encarga de dotar de cierta inteligencia artifical al jugador automática.
			// Buscará a los enemigos que se encuentren más cercanos, y aquellos a los que esté lo suficientemente cerca podrá empezar a dispararles.
			try
			{
				// Mira al enemigo que ha detectado (las funciones FindNearestTransformEnemy y GetClosestEnemy son las mismas que las que se utilizan en los scripts de los enemigos)
				enemy = GetClosestEnemy(FindNearestTransformEnemy());
				transform.LookAt(enemy.Find("Target"));

				// Si el avatar se encuentra a mayor distancia que la Min y menor distancia que la máxima...
				if ((Vector3.Distance(transform.position, enemy.Find("Target").position) >= MinDist + 2.0f) && (Vector3.Distance(transform.position, enemy.Find("Target").position) <= MaxDist))
				{
					if (theHit.collider.tag == "Enemy")
						transform.position += transform.forward * MoveSpeed * Time.deltaTime;
				}

				// Si el avatar se encuentra a demasiado cerca del enemigo, se alejará de este (recordemos que los enemigos le quitan salud al jugador si estos entran en contacto directo)
				if (Vector3.Distance(transform.position, enemy.Find("Target").position) <= (MinDist * 2))
				{
					transform.position += -transform.forward * (MoveSpeed * 2) * Time.deltaTime;
				}

				// Si el avatar tiene en su punto de mira a un enemigo, empezará a dispararle.
				if (Physics.Raycast(bulletSpawn.position, bulletSpawn.TransformDirection(Vector3.forward), out theHit, MaxDist))
				{
					if (theHit.collider.tag == "Enemy")
					{
						if (theHit.distance <= MaxDist)
							onRange = true;
						else
							onRange = false;

						if (onRange && theHit.collider.tag == "Enemy")
							isShooting = true;
						else
							isShooting = false;
					}
				}
			}
			catch (System.NullReferenceException)
			{
				return;
			}
		}
		//-----------------------------------------------------------------------------------------------------------------------------//



		//------------------------------------------------ UPDATE DEL JUGADOR TORPEDO -----------------------------------------------//
		if (pName == "Petardo")
		{
			timeShoot += Time.fixedDeltaTime;
			timeSeconds += Time.fixedDeltaTime;

			//Code for gravity
			moveDirection.y = moveDirection.y - (gravity * Time.deltaTime);
			controller.Move(moveDirection * Time.deltaTime);

			//Tiempo en el que el jugador petardo repite su acción molesta (es tontería poner menos de 0.015, ya que el Update se ejecuta como mínimo a esa cadencia)
			if (!(timeShoot < 0))
			{
				cont++;
				Debug.Log("Número de balas: " + cont);
				if (GameObject.FindWithTag("Castillo") != null)
				{
					transform.LookAt(GameObject.FindGameObjectWithTag("Castillo").transform);
					onRange = true;
					isShooting = true;
					ShootPetardo();
				}
				timeShoot = 0.0f;
			}

			if (!(timeSeconds <= 1))
			{
				contadorSegundos++;
				Debug.Log("Contador de segundos: " + contadorSegundos);
				timeSeconds = 0;
				cont = 0;
			}
		}
		//-----------------------------------------------------------------------------------------------------------------------------//



		if(pName != "IA" && pName != "Petardo")
		{
			//------------------------------------------------ UPDATE DEL JUGADOR NORMAL -----------------------------------------------//

			// SI ESTAMOS EN LA VERSIÓN DE PC
			if (!isAndroid)
			{

				//Si el chat no está abierto, podré moverme. Evitará la incomodidad de escribir y mover tu avatar al mismo tiempo.
				if (!ChatUI.GetComponent<Canvas>().enabled && controller.isGrounded)
				{
					// Si el jugador NO HA PERDIDO LA PARTIDA, podrá moverse.
					if (!lose)
					{
						//Mediante este código se podrá mover el avatar con las teclas (W A S D) y las flechas.
						//Mediante Input.GetAxis se recoge si el jugador ha presionado dichas teclas.
						moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
						// Con la información obtenida, en estas tres líneas se aplica ese movimiento al avatar mediante la función Move del componente Controller. 
						moveDirection = transform.TransformDirection(moveDirection);
						moveDirection = moveDirection * (speed / 2);
						controller.Move(moveDirection * Time.deltaTime);

						//Si se detecta que el jugador ha presionado el click izquierdo de su ratón, ejecutará la función de disparo.
						if (Input.GetKeyDown(KeyCode.Mouse0))
						{
							CmdFire();
						}
					}
				}

				//Codigo para rotar la cámara cuando el jugador gira su ratón. Muy similar al de mover el avatar.
				horizontalRotation += sensibility * Input.GetAxis("Mouse X");
				verticalRotation -= sensibility * Input.GetAxis("Mouse Y");
				controller.transform.eulerAngles = new Vector3(verticalRotation, horizontalRotation, 0.0f);

				//Codigo para evitar que el jugar suba demasiado en el eje Y ( es decir, que no vuele )
				moveDirection.y = moveDirection.y - (gravity * Time.deltaTime);
				controller.Move(moveDirection * Time.deltaTime);


				//Si el jugador presione la tecla F1, creará una sala de streaming. Para ello utilizará la función InitStreaming que se encuentra dentro del script CallApp.
				if (Input.GetKeyDown(KeyCode.F1))
				{
					GetComponent<CallApp>().InitStreaming();
				}

				//Codigo para abrir la lista de streaming abiertos. Cuando la lista se encuentra abierta el cursor que ha sido escondido en la función Start reaparecerá.
				// Si el jugador presiona la tecla Ctrl izquierda de su teclado y el cursor está bloqueado....
				if (Input.GetKeyDown(KeyCode.LeftControl) && CursorLockedVar)
				{
					//Función para mostrar el cursor y permitir interactuar con él
					VisibleCursor();

					// Si la lista de streaming NO estaba mostrada....
					if (!StreamingList.activeSelf)
					{
						// Se muestra la la lista y se muestran los jugadores que tienen su variable isSending activada...
						StreamingList.SetActive(true);
						StreamingList.GetComponent<StreamingList>().OnActivateSearch();
					}
					// Si la lista de streaming YA estaba abierta...
					else
					{
						// Se oculta la lista.
						StreamingList.SetActive(false);
					}
				} 
				// Si el jugador presiona la tecla Ctrl izquierda de su teclado y está desbloqueado....
				// ESTE ELSE SE PONE PARA EVITAR EL BUG DE QUE EN OCASIONES EL CURSOR SE QUEDE ACTIVA AUN SIN TENER LA LISTA DE STREAMING ABIERTA, LO QUE IMPIDE JUGAR BIEN.
				else if (Input.GetKeyDown(KeyCode.LeftControl) && !CursorLockedVar)
				{
					// Se oculta  el cursor y se cierra la lista.
					HideCursor();
					StreamingList.SetActive(false);
				}

				//Codigo para habilitar el chat.
				// Si isBusy es false y isSending también, no hay necesitadad de mostrar el chat, en caso contrario lo mostrará.
				// Mediante este if se corrige el problema de que los mensajes antiguos desaparezcan del chat cuando el jugador lo oculta.
				if (!isBusy && !isSending)
				{
					ChatUI.GetComponent<Canvas>().enabled = false;
					ChatUI.SetActive(false);
				}
				else
				{
					ChatUI.SetActive(true);
				}

				// Si el jugador presiona la tecla Ctrl derecha, el cursor está bloqueado, y el jugador está emitiendo o está en estado ocupado...
				if (Input.GetKeyDown(KeyCode.RightControl) && CursorLockedVar && (isBusy || isSending))
				{
					//Función para mostrar el cursor y permitir interactuar con él
					VisibleCursor();

					// Si el chat NO está mostrado en pantalla....
					if (!ChatUI.GetComponent<Canvas>().enabled)
					{
						// Muestra el chat en pantalla y lo hace interactuable.
						ChatUI.GetComponent<Canvas>().enabled = true;
						uMessageInput.interactable = true;
						uSend.Select();
						uMessageInput.Select();
					}
					else
					// Se esconde la ventana del chat, puesto que ya estaba abierta antes de pulsar la tecla.
					{
						ChatUI.GetComponent<Canvas>().enabled = false;
						uMessageInput.interactable = false;
					}
				}
				// ESTE ELSE SE PONE PARA EVITAR EL BUG DE QUE EN OCASIONES EL CURSOR SE QUEDE ACTIVA AUN SIN EL CHAT ABIERTO, LO QUE IMPIDE JUGAR BIEN.
				else if (Input.GetKeyDown(KeyCode.RightControl) && !CursorLockedVar && (isBusy || isSending))
				{
					HideCursor();
					ChatUI.GetComponent<Canvas>().enabled = false;
					uMessageInput.interactable = false;
				}

				//Code for visible Cursor and Escape from scene
				if (Input.GetKeyDown(KeyCode.Escape) && CursorLockedVar && !StreamingList.activeSelf)
				{
					VisibleCursor();
				}
				else if (Input.GetKeyDown(KeyCode.Escape) && !CursorLockedVar && !StreamingList.activeSelf)
				{
					HideCursor();
				}

				//Si se pulsa F9, encenderá el micrófono y la cámara para transmitir cuando se encuentre en una sala de streaming.
				if (Input.GetKeyDown(KeyCode.F9))
				{
					GetComponent<CallApp>().TurnOnOrOffDevices();
				}

				//Permite abandonar una sala de streaming
				if (Input.GetKeyDown(KeyCode.F10))
				{
					GetComponent<CallApp>().LeftConnection();
				}
			}


			// SI ESTAMOS EN LA VERSIÓN DE ANDROID!!!
			if (isAndroid)
			{
				//Permita habiliar o deshabilitar el chat en función del estado en que se esté.
				// Modo ocupado y acitvo : se muestra chat.
				// Modo deocupado: no se muestra chat, puesto que no se necesita.
				if (!isBusy && !isSending)
				{
					ChatUI.GetComponent<Canvas>().enabled = false;
					ChatUI.SetActive(false);
					ButtonForChat.SetActive(false);
				}
				else
				{
					ButtonForChat.SetActive(true);
					ChatUI.SetActive(true);
				}

				// Codigo para evitar que el jugadodr pueda volar.
				moveDirection.y = moveDirection.y - (gravity * Time.deltaTime);
				controller.Move(moveDirection * Time.deltaTime);

				// Si el jugador NO HA PERDIDO LA PARTIDA, podrá moverse.
				if (!lose)
				{
					//Se recoge la información del movimiento de los joystick mediante la función JoystickMovement que proporciona UJoystick.
					float v = JoystickMovement.Vertical / 4;
					float h = JoystickMovement.Horizontal / 4;


					// Realiza el movimiento del avatar de manera similar que con la versión de PC
					moveDirection = new Vector3(h, 0.0f, v);
					moveDirection = transform.TransformDirection(moveDirection);
					moveDirection = moveDirection * (speed / 2);
					controller.Move(moveDirection * Time.deltaTime);
				}

				// Genera un cuadrado invisible en la parte izquierda superior de la pantalla del jugador.
				Rect bottomRight = new Rect(Screen.width / 2, Screen.height / 2, Screen.width / 2, Screen.height / 2);

				//Permite abrir la lista de streaming si el jugador presiona 2 veces la pantalla. (doble-tap)
				var fingerCount = 0;
				foreach (Touch touch in Input.touches)
				{
					if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
					{
						fingerCount++;
					}
				}
				if (fingerCount == 2)
				{
					var touchPos = Input.GetTouch(0).position;
					if (bottomRight.Contains(touchPos) && !StreamingListAndroid.activeSelf)
					{
						StreamingListAndroid.SetActive(true);
						StreamingListAndroid.GetComponent<StreamingList>().OnActivateSearch();

					}
					else if (bottomRight.Contains(touchPos) && StreamingListAndroid.activeSelf)
					{
						StreamingListAndroid.SetActive(false);
					}
				}

		/*		if (Input.touchCount == 2)
				{
					var touchPos = Input.GetTouch(0).position;
					if (bottomRight.Contains(touchPos) && !StreamingListAndroid.activeSelf)
					{
						StreamingListAndroid.SetActive(true);
						StreamingListAndroid.GetComponent<StreamingList>().OnActivateSearch();

					}
					else if (bottomRight.Contains(touchPos) && StreamingListAndroid.activeSelf)
					{
						StreamingListAndroid.SetActive(false);
					}
				}*/
			}
		}
	}
	//-----------------------------------------------------------------------------------------------------------------------------//


	//------------------------------------------------ FUNCIONES DEL JUGADOR AUTOMATICO -----------------------------------------------//

	// Función de disparo.
	void Shoot()
	{
		if (onRange && !finish && isShooting)
		{
			CmdFire();
		}
	}

	// Función de disparo del jugador malicioso.
	void ShootPetardo()
	{
		if (onRange && isShooting)
		{
			CmdFire();
		}
	}

	// Esta función devuelve un vector del tipo Transform, el cual almacena las posiciones de todos los enemigos del mapa.
	public Transform[] FindNearestTransformEnemy()
	{
		GameObject[] enemies;
		Transform[] enemiesLocation;

		enemies = GameObject.FindGameObjectsWithTag("Enemy");
		enemiesLocation = new Transform[enemies.Length];

		for (int i = 0; i < enemies.Length; i++)
		{
			enemiesLocation[i] = enemies[i].transform;
		}

		return enemiesLocation;
	}

	// Esta función se encarga de recorger el vector de tipo Transform obtenido de la función FindNearestTransform(), y calcular cual es el enemigo
	// que se encuentra más cerca
	Transform GetClosestEnemy(Transform[] enemies)
	{
		Transform tMin = null;
		float minDist = Mathf.Infinity;
		Vector3 currentPos = transform.position;
		foreach (Transform t in enemies)
		{
			float dist = Vector3.Distance(t.position, currentPos);
			if (dist < minDist)
			{
				tMin = t;
				minDist = dist;
			}
		}
		return tMin;
	}


	//-----------------------------------------------------------------------------------------------------------------------------//

	// Función que lee la variable Finish, si el juego ha terminado, el GameObject GameManager establecerá el valor de la variable Finish true.
	public void CheckFinish(bool finish)
	{
		// Si la variable Finish es true... la partida ha terminado, por lo tanto....
		if (finish)
		{
			// Si el jugador no ha perdido todas sus vidas...
			if (GetComponent<Health>().GetLive() != 0)
			{
				// Comprueba si es el jugador con más puntos de entre los jugadores que no han perdido todas sus vidas... Si es el jugador con más puntos
				if (MaxScoreOfLifePlayer() == GetComponent<Score>().GetScore())
					// Gana la partida, y notifica su nombre mediante un Command
					CmdIsWin();
				else
					// Pierde la partida,  y pone la variable Lose a true para mostrar en pantalla al jugador que ha perdido
					lose = true;
			} else
			// Si ha perdido todas las vidas no es necesaria ninguna comprobación, ya que ha perdido la partida sí o sí.
			{
				lose = true;
			}

			// Si el jugador ha perdido la partida y no conoce al jugador ganador...
			if(lose && nameWinner == "")
			{
				// Busca al jugador con la variable Win a true, coge su nombre y lo muestra.
				foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
				{
					if (player.GetComponent<PlayerCode>().GetWin())
						nameWinner = player.GetComponent<PlayerCode>().GetName();
				}

				if (nameWinner != "")
					UIWinner.text = ("Ganador: " + nameWinner);
				else
					UIWinner.text = ("No hay ganador");

			}
		}

		// Si la variable Finish es false... la partida NO ha terminado, por lo tanto....
		if (!finish)
		{	
			// Comprobará que el jugador no ha perdido todas sus vidas. En caso de que las haya perdido, entonces habrá perdido también la partida, independientemente
			// de como vayan el resto de jugadores.
			if (GetComponent<Health>().GetLive() == 0)
				lose = true;
		}
	}

	// Función para extraer la puntuación máxima de entre los jugadores que todavían no han perdido todas su vidas.
    public int MaxScoreOfLifePlayer()
    {
        scoreOfEnemy = new int[GameObject.FindGameObjectsWithTag("Player").Length];
        int i = 0;

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
			if (player.GetComponent<Health>().GetLive() != 0)
			{
				scoreOfEnemy[i] = player.GetComponent<Score>().GetScore();
				i++;
			}
        }

        return Mathf.Max(scoreOfEnemy);
    }

	// Mediante esa función, se mostrará el mensaje correspondiente al resultado de la partida.
	public void PutLoseOrWinScreen()
	{
		// Si el jugador ha ganado, WIN será true, por lo tanto...
		if (win)
			// Se muestra el mensaje de victoria
			Win.SetActive(true);
		// Si el jugador NO ha ganado, WIN será false , por lo tanto...
		else
			// NO se muestra el mensaje de victoria
			Win.SetActive(false);

		// Si el jugador ha perdido, LOSE será true, por lo tanto...
		if (lose)
			// Se muestra el mensaje de derrota.
			Lose.SetActive(true);
		// Si el jugador ha perdido, LOSE será true, por lo tanto...
		else
			// NO se muestra el mensaje de derrota.
			Lose.SetActive(false);

		// Permitirá que los enemigos no ataquen a los jugadores ya derrotados.
		if (lose)
			transform.Find("TargetTargetBody").tag = "Untagged";
	}

	// Función para encender el chat desde el móvil.
	public void OnPressChat()
	{
		if (!ChatUI.GetComponent<Canvas>().enabled)
		{
			ChatUI.GetComponent<Canvas>().enabled = true;
			uMessageInput.interactable = true;
			uSend.Select();
			uMessageInput.Select();
		}
		else
		{
			ChatUI.GetComponent<Canvas>().enabled = false;
			uMessageInput.interactable = false;
		}
	}

	//Función para guardar el nombre puesto en el Lobby dentro de la variable pName
	public void OnChangeName(string newName)
	{
		pName = newName;
		SetPlayerName();
	}

	// Poner el nombre de la variable pName en lo alto del avatar del jugador.
	public void SetPlayerName()
	{
		Name.text = (pName.ToString());
	}

	//Función para guardar el color escogido en el Lobby dentro de la variable playerColor
	public void OnChangeColor(Color newColor)
	{
		playerColor = newColor;
		SetPlayerColor();
	}

	// Dibuja en el avatar el color seleccionado por el jugador en el Lobby.
	public void SetPlayerColor()
	{
		foreach (Renderer r in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			r.material.color = playerColor;
		}
	}

	// Función para visibilizar el cursor del ratón.
	public void VisibleCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		CursorLockedVar = false;
	}

	// Función para esconder el cursor del ratón.
	public void HideCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		CursorLockedVar = true;
	}

	// Función para disparar el arma desde el móvil.
	public void OnClickShoot()
	{
		if (!lose)
			CmdFire();
	}

	/*
	 * Diversas funciones Command para modificar variables y que estas sean sincronizadas al resto de jugadores.
	 */

	[Command]
	public void CmdIsWin()
	{
		win = true;
	}

	[Command]
	public void CmdIsSending()
	{
		isSending = true;
	}

	[Command]
	public void CmdNotSending()
	{
		isSending = false;
	}

	[Command]
	public void CmdIsBusy()
	{
		isBusy = true;
	}

	[Command]
	public void CmdNotBusy()
	{
		isBusy = false;
	}

	//Función de disparo, es la función básica. 
	[Command]
    public void CmdFire()
    {
		// Crea la bala mediante el GameObject correspondiente al de la bala
		var bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

		// Agrega una velocidad cinética fija a la bala antes de que esta sea creada
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 150;

		// Nombra a la bala con el nombre del jugador, de esta manera se sabrá a quien poner los puntos
		bullet.name = pName;

		// Crea la bala en todos los jugadores.
		NetworkServer.Spawn(bullet);

		// Destroye la bala después de 2 segundos de su creación, de esta manera las balas no se almacenarán infinitamente en la escena.
		Destroy(bullet, 2.0f);
	}

	public void SetWin(bool win)
	{
		this.win = win;
	}

	public void SetLose(bool lose)
	{
		this.lose = lose;
	}

	public void SetFinish(bool finish)
	{
		this.finish = finish;
	}

	public void SetDate(string date)
	{
		this.date = date;
	}

	public bool GetWin()
	{
		return win;
	}

	public bool GetFinish()
	{
		return finish;
	}

	public string GetName()
	{
		return pName;
	}

	public bool IsSending()
	{
		return isSending;
	}

	public bool IsBusy()
	{
		return isBusy;
	}

	public string GetDate()
	{
		return date;
	}
}