using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Byn.Net;
using System.Collections.Generic;
using System;
using Unity.Collections;
using System.Text;

public class CallApp : NetworkBehaviour
{
	#region CONECTION
	private string uSignalingUrl = "ws://localhost:12776/callapp";
	//private string uSignalingUrl = "ws://192.168.0.15:12776/callapp";

	// Es el servidor de señalización que se utiliza. En este caso es el público que ofrece el asset, aunque se puede usar el código en función de que valor  se le ponga a la variable uSignalingUrl
	//private string uSignalingUrl = "wss://because-why-not.com:12777/callapp";

	//Servidores STUN de Google públicos.
	private string uIceServer = "stun:stun.l.google.com:19302";
	private string uIceServer2 = "stun2.l.google.com:19302";
	private string address;
	private bool mIsServer = false;
	private bool isTransmitter = false;
	private bool IsRecording = false;
	private bool isSpeaking = false;
	private bool isConnectedAsClient = false;
	#endregion

	#region CHAT
	public InputField uMessageInput;
	public Button uSend;
	public MessageList uOutput;
	#endregion

	#region CAMARA
	private WebCamTexture webCam;
	public GameObject CamaraWeb;
	private Texture2D currentTexture;
	private Texture2D getTexture;
	private float timer = 0.0f;
	private float timerForCam = 0.0f;
	private float timerToggleTexture = 0.0f;
	private const int MAX_WIDTH = 100;
	private const int MAX_HEIGHT = 100;
    #endregion

    #region MICRO
    private const int FREQUENCY_RATE = 16000;
    private AudioClip microphone;
	private AudioClip speakers;
	private int lastPos, pos = 0;
	private List<float> read = new List<float>();
	#endregion

	#region SERVER
	private IBasicNetwork mNetwork = null;
	private List<ConnectionId> mConnections = new List<ConnectionId>();
	#endregion

	private void Start()
	{
		if (!isLocalPlayer)
			return;

		// Crea una instancia del tipo WebRtcNetworkFactory lista para ser usada en caso de que el jugador cree o se una a partida. Esta clase la incluye el asset WebRTC Network.
		WebRtcNetworkFactory factory = WebRtcNetworkFactory.Instance;
		if (factory != null)
		{
			Debug.Log("WebRTCNetwork creada");
		}
		else
		{
			Debug.Log("WebRTCNetwork no se ha creado correctamente");
		}
	}

	private void Reset()
	{
		Debug.Log("Reiniciado");

		mIsServer = false;
		mConnections = new List<ConnectionId>();
		GetComponent<PlayerCode>().CmdNotBusy();
		GetComponent<PlayerCode>().CmdNotSending();
		Cleanup();
	}

	private void Cleanup()
	{
		mNetwork.Dispose();
		mNetwork = null;
	}

	private void OnDestroy()
	{
		if (mNetwork != null)
		{
			Cleanup();
		}
	}

	// Función para enviar datos de tipo texto. Usado para enviar los mensajes del chat.
	private void SendString(string msg)
	{
		if (mNetwork == null || mConnections.Count == 0)
		{
			Append("No hay nadie conectado a tu sala.");
		}
		else
		{
			// Se coge el mensaje que se haya puesto en el chat, y se guarda en la variable msg, la cual se convertirá a bytes mediante Encoding.UTF8
			byte[] msgData = Encoding.UTF8.GetBytes(msg);

			// Recorrerá un vector interno en donde se almacenan las conexiones que hay dentro de una sala de streaming. Mediante mConnections se identifica a quien enviar los mensajes.
			foreach (ConnectionId id in mConnections)
			{
				//Mediante la función SendData se envia los bytes del msg a la id especificada por el canal UNREALIABLE que proporciona el asset.
				mNetwork.SendData(id, msgData, 0, msgData.Length, false);
			}
		}
	}

	// Función para enviar datos de tipo imagen. Usado el streaming de la cámara.
	private void SendVideoInBytes()
	{
        if (mNetwork != null && GetComponent<PlayerCode>().IsBusy())
        {
			// Se extrae los pixeles de la cámara web y se pintan en una textura.
			currentTexture.SetPixels(webCam.GetPixels());
			// Se aplica a la textura los pixeles para que la pinte.
			currentTexture.Apply();
			// Convierte la textura de la imagen de la cámara a tipo JPG, especificando la calidad con la que se convertirá.
			//Por defecto se codifica a una calidad de 75, pondremos 65 para reducir la carga de esta función. No reperctue
			//demasiado en la calidad este valor.
            byte[] pngBytes = currentTexture.EncodeToJPG(65);

			// Recorrerá un vector interno en donde se almacenan las conexiones que hay dentro de una sala de streaming. Mediante mConnections se identifica a quien enviar los mensajes.
			foreach (ConnectionId id in mConnections)
            {
				//Mediante la función SendData se envia los bytes de la imagen a la id especificada por el canal UNREALIABLE que proporciona el asset.
				mNetwork.SendData(id, pngBytes, 0, pngBytes.Length, false);
            }
        }
	}

	// Función para enviar datos de tipo audio. Usado el streaming del audio
	private void SendAudioBytes()
	{
		// Si el jugador está ocupado (es decir, esta comunicandose con otro)....
        if (mNetwork != null && GetComponent<PlayerCode>().IsBusy())
        {
			// Se recoge mediante este if las muestras obtenidas a través del micrófono...
			Debug.Log(pos);
			if ((pos = Microphone.GetPosition(null)) > 30)
            {
                if (lastPos > pos)
                    lastPos = 0;

                if ((pos - lastPos) > 0)
                {
                    float[] sample = new float[((pos - lastPos) * microphone.channels)];
                    microphone.GetData(sample, lastPos);
					// Las muestras obtenidas en float se pasan a bytes para que estos puedan ser enviados a través de la red....
                    byte[] bytesFromAudio = ToByteArray(sample);
					
                    lastPos = pos;

					// Recorrerá un vector interno en donde se almacenan las conexiones que hay dentro de una sala de streaming. Mediante mConnections se identifica a quien enviar los mensajes.
					foreach (ConnectionId id in mConnections)
                    {
						//Mediante la función SendData se envia los bytes del msg a la id especificada por el canal REALIABLE que proporciona el asset.
						mNetwork.SendData(id, bytesFromAudio, 0, bytesFromAudio.Length, true);
                    }
                }
            }
        }
	}

	private void FixedUpdate()
	{
		//Sin este return no se oye el audio
		if (!isLocalPlayer)
			return;

		HandleNetwork();
		timerForCam += Time.deltaTime;

		if (isTransmitter)
		{
			//Este timer servirá para limitar el número de frames que manda la cámara a la red de manera que el rendimiento en el consumo
			//de la generación de la imagen y el envío sea aceptable.
			timer += Time.deltaTime;

			//Se ha establecido un valor de 0.1, este valor no ha sido calculado, se ha llegado a él a través de pruebas de 
			// calidad/rendimiento. Este valor es adecuado pues no sobrecarga en exceso a Unity y la imagen se ve lo suficientemente
			//fluida
			if (IsRecording && timer > 0.1)
			{
				SendVideoInBytes();
				timer = 0;
			}

			if (isSpeaking)
				SendAudioBytes();
		}

		if (!GetComponent<PlayerCode>().IsBusy() && timerForCam > 0.5)
		{
			TurnDownTextures();
			TurnDownCam();
			DesactivateMicrophone();
			timerForCam = 0;
		}

		if(GetComponent<PlayerCode>().IsBusy())
		{
			if (timerToggleTexture > 0)
				timerToggleTexture -= Time.deltaTime;

			if (timerToggleTexture <= 0)
				TurnDownTextures();
		}
	}

	// Esta función realiza la función de leer que mensajes les llega desde la red...
	private void HandleNetwork()
	{
		if (mNetwork != null)
		{
			mNetwork.Update();

			NetworkEvent evt;

			while (mNetwork != null && mNetwork.Dequeue(out evt))
			{
				switch (evt.Type)
				{
					// El servidor de señalización informa de que se hace creado la sala de setreaming....
					case NetEventType.ServerInitialized:
						{
							mIsServer = true;
							address = evt.Info;
							Debug.Log("Server started. Address: " + address);
							Debug.Log("ConnectionId: " + evt.ConnectionId);

							// Pone a true la variable isSending
							GetComponent<PlayerCode>().CmdIsSending();
						}
						break;
					// El servidor de señalización informa de que ha habido un problema al crear la sala....
					case NetEventType.ServerInitFailed:
						{
							mIsServer = false;
							Reset();
						}
						break;
					// El servidor de señalización informa de que la sala se ha cerrado....
					case NetEventType.ServerClosed:
						{
							mIsServer = false;
							// Pone a false la variable isSending
							GetComponent<PlayerCode>().CmdNotSending();
							isTransmitter = false;
						}
						break;
					// El servidor de señalización informa de que se ha añadido alguien a la sala...
					case NetEventType.NewConnection:
						{
							mConnections.Add(evt.ConnectionId);
							// Pone a true la variable isBusy
							GetComponent<PlayerCode>().CmdIsBusy();

							if (!mIsServer)
							{
								Append(DateTime.Now.ToString()+ ": Te has unido a una sala.");
								isConnectedAsClient = true;
							}
							else
							{
								Append(DateTime.Now.ToString()+ ": Un jugador se ha unido a tu sala.");
							}
						}
						break;
					// El servidor de señalización informa de que no se ha podido el jugador a una sala de streaming...
					case NetEventType.ConnectionFailed:
						{
							Reset();
							DesactivateCamera();
						}
						break;
					// El servidor de señalización informa de que se ha abandonado la sala...
					case NetEventType.Disconnected:
						{
							mConnections.Remove(evt.ConnectionId);
							if (!mIsServer)
							{
								Reset();
								isConnectedAsClient = false;
							}
							else
							{
								Append(DateTime.Now.ToString() + ": Se han desconectado de tu sala.");
							}

							// Pone a false la variable isBusy
							GetComponent<PlayerCode>().CmdNotBusy();
							isTransmitter = false;
						}
						break;
					// Canal RELIABLE del asset, aquí se llamará a la función de recepción de audio...
					case NetEventType.ReliableMessageReceived:
						{
							HandleIncommingStreamingAudio(ref evt);
						}
						break;
					// Canal UNRELIABLE del asset, aquí se llamará a la función de recepción de vídeo y mensajes...
					case NetEventType.UnreliableMessageReceived:
						{
							HandleIncommingStreamingVideoAndText(ref evt);
						}
						break;
				}
			}

			//finish this update by flushing the messages out if the network wasn't destroyed during update
			if (mNetwork != null)
				mNetwork.Flush();
		}
	}

	// Función de recepción de audio...
	private void HandleIncommingStreamingAudio(ref NetworkEvent evt)
	{
		MessageDataBuffer buffer = (MessageDataBuffer)evt.MessageData;
		// Se extrae el mensaje del campo de datos que llega desde la red en la variable data....
		byte[] data = evt.GetDataAsByteArray();
		// Los datos se pasan a float para que puedan ser reproducidos por el componente Audio Source de Unity...
        float[] audioData = ToFloatArray(data);

		// Se almacena un número de muestras de audio...
		for (int i = 0; i < audioData.Length; i++)
		{
			read.Add(audioData[i]);
		}

		// Cuando el número de muestras almacedanas es igual/mayor a la frecuencia, se reproducen...
		if (data != null && read.Count >= FREQUENCY_RATE )
		{
			AudioSource audioSource = GetComponent<AudioSource>();
			speakers.SetData(read.ToArray(), 0);
			audioSource.clip = speakers;
			audioSource.Play();
			read.Clear();
		}

		//return the buffer so the network can reuse it
		buffer.Dispose();
	}

	private void HandleIncommingStreamingVideoAndText(ref NetworkEvent evt)
	{
		MessageDataBuffer buffer = (MessageDataBuffer)evt.MessageData;
		// Se extrae el mensaje del campo de datos que llega desde la red en la variable data....
		byte[] data = evt.GetDataAsByteArray();

		// Si el mensaje que llega es mayor a 250, son datos de vídeo...
		if (data != null && data.Length > 250)
		{
			timerToggleTexture = 3.0f;
			getTexture.LoadImage(data);
			getTexture.Apply();
			CamaraWeb.GetComponent<Image>().enabled = true;
			CamaraWeb.GetComponent<Image>().material.mainTexture = getTexture;
		}
		// Si el mensaje que llega es menor a 250, son datos de texto...
		else if (data != null && data.Length <= 250 )
		{
			// Se extrae el mensaje de los bytes...
			string msg = Encoding.UTF8.GetString(buffer.Buffer, 0, buffer.ContentLength);

			// Se pinta el mensaje en el chat...
			if(mIsServer)
			{
				SendString(msg);
				Append(msg);
			}
			else
			{
				Append(msg);
			}
		}

		//return the buffer so the network can reuse it
		buffer.Dispose();
	}

	// Crea una sala de streaming...
	public void InitStreaming()
	{
		// Si no hay servidor creado....
		if (!mIsServer)
		{
			// Se crea una instancia del tipo WebRtcNetworkFactory, la cual tiene las direcciones especificadas al principio del script...
			mNetwork = WebRtcNetworkFactory.Instance.CreateDefault(uSignalingUrl, new IceServer[] { new IceServer(uIceServer), new IceServer(uIceServer2) });
			// Crea una sala cuyo nombre será el del jugador...
			mNetwork.StartServer(GetComponent<PlayerCode>().GetName().ToString());
			// Inicializa la textura y el componente de audio para recibir los datos...
			CreateImageAndVoice();
		}
	}

	// Función para entrar a una sala....
	public void OnClickJoinStreaming(Text dir)
	{
		// Si no se encuentra añadidado ya a otra sala...
		if (!isConnectedAsClient)
		{
			// Se crea una instancia del tipo WebRtcNetworkFactory, la cual tiene las direcciones especificadas al principio del script...
			mNetwork = WebRtcNetworkFactory.Instance.CreateDefault(uSignalingUrl, new IceServer[] { new IceServer(uIceServer), new IceServer(uIceServer2) });
			// Se conecta a la sala cuyo nombre se haya extraido del botón presionado en la lista de streaming abiertos...
			mNetwork.Connect(dir.text.ToString());
			// Si la cámara o el micrófono están activos...
			if (IsRecording || isSpeaking)
				// Está transimiendo...
				isTransmitter = true;

			// Inicializa la textura y el componente de audio para recibir los datos...
			CreateImageAndVoice();
			Debug.Log("Entrando al streaming...");
		} else
		{
			Debug.Log("Ya eres miembro de una sala de streaming, no puedes abrir más conexiones");
		}
	}

	// Función para abandonar una sala de streaming...
	public void LeftConnection()
	{
		if (mNetwork != null)
		{
			// Si no es el servidor... abandonado la sala...
			if (!mIsServer)
			{
				Cleanup();
				// Limpia su vector de conexiones...
				mConnections.Clear();
				// Se pone con la variable isBusy a false...
				GetComponent<PlayerCode>().CmdNotBusy();
				// Ni transmite ni está dentro de una sala...
				isTransmitter = false;
				isConnectedAsClient = false;
			}
			// Si es el servidor... abandonado la sala...
			else
			{
				// Se ejecuta la función de cierre de sala...
				Reset();
			}
		}
	}

	// Función para iniciar/apagar el envío de los datos multimedia...
	public void TurnOnOrOffDevices()
	{
		//Control de la camara
		if (!IsRecording)
			ActiveCamera();
		else
			TurnDownCam();

		//Control del microfono
		if (!isSpeaking)
			ActiveMicrophone();
		else
			DesactivateMicrophone();

		//Control variable isTrasmitter
		if (IsRecording || isSpeaking)
			isTransmitter = true;

		if (!IsRecording && !isSpeaking)
			isTransmitter = false;
	}

	// Elimina el cuadrado de la interfaz de usuario donde se muestra la cámara...
	public void TurnDownTextures()
	{
		if (CamaraWeb.GetComponent<Image>().enabled)
			CamaraWeb.GetComponent<Image>().enabled = false;

	}

	// Pausa la cámara...
	public void TurnDownCam()
	{
		if (webCam != null)
		{
			if (webCam.isPlaying)
				webCam.Stop();
			IsRecording = false;
		}
	}

	// Inicia el micrófono...
    private void ActiveMicrophone()
    {
        string nameDevice = null;
        int minFreq = 0;
        int maxFreq = 0;

        foreach (string s in Microphone.devices)
        {
            Microphone.GetDeviceCaps(nameDevice, out minFreq, out maxFreq);
            Debug.Log("Device Name: " + s + " [" + minFreq.ToString() + "-" + maxFreq.ToString() + "]");
            nameDevice = s;
        }

        if (Microphone.devices.Length != 0)
        {
            microphone = Microphone.Start(nameDevice, true, 10, FREQUENCY_RATE);
            isSpeaking = true;
        }
        else
        {
            Debug.Log("Microfono no encotrado");
        }
    }

	// Apaga el micrófono....
	private void DesactivateMicrophone()
	{
		string nameDevice = null;

		foreach (string s in Microphone.devices)
		{
			nameDevice = s;
		}

		if (Microphone.devices.Length != 0)
		{
			Microphone.End(nameDevice);
			isSpeaking = false;
		}
	}

	// Crea el objeto AudioClipToReturn en donde se pegará los datos de audio que recibe de la red para posteriormente reproducirlos...
	private static AudioClip AudioClipCreateEmpty(string ClipName, int Length)
	{
		AudioClip AudioClipToReturn = AudioClip.Create(ClipName, Length, 1, FREQUENCY_RATE, false);
		return AudioClipToReturn;
	}

	// Inicia la transmisión de la cámara....
	private void ActiveCamera()
	{
		string frontCamName = null;

		WebCamDevice[] devices = WebCamTexture.devices;
		for (int i = 0; i < devices.Length; i++)
		{
			if (devices[i].isFrontFacing)
				frontCamName = devices[i].name;
		}

		if (devices.Length != 0)
		{
			webCam = new WebCamTexture(frontCamName)
			{
				requestedWidth = 100,
				requestedHeight = 100
			};

			if (!webCam.isPlaying)
			{
				webCam.Play();
				currentTexture = new Texture2D(webCam.width, webCam.height, TextureFormat.RGBA32, true);
				IsRecording = true;
			}
			else
			{
				Debug.Log("Hay un dispositivo de vídeo actualmente en emisión");
			}

		}
		else
		{
			Debug.Log("Camara no encontrada o ya activada");
		}
	}

	// Desactiva la transmisión de la cámara....
	private void DesactivateCamera()
	{
		if (webCam.isPlaying)
			webCam.Stop();
	}

	private void CreateImageAndVoice()
	{
		getTexture = new Texture2D(MAX_WIDTH, MAX_HEIGHT, TextureFormat.RGBA32, true);
		speakers = AudioClipCreateEmpty("Voice", FREQUENCY_RATE * 2);
	}

	public void SendButtonPressed()
	{
		string msg = uMessageInput.text;

		if (mIsServer)
		{
			msg =  GetComponent<PlayerCode>().GetName()+": " + msg;
			Append(msg);
			SendString(msg);
		}
		else
		{
			msg = GetComponent<PlayerCode>().GetName() + ": " + msg;
			SendString(msg);
		}
		uMessageInput.text = "";

		//make sure the text box is in focus again so the user can continue typing without clicking it again
		//select another element first. without this the input field is in focus after return pressed
		uSend.Select();
		uMessageInput.Select();
	}

	public void InputOnEndEdit()
	{
		if (Input.GetKey(KeyCode.Return))
		{
			SendButtonPressed();
		}
	}

	// Función para pintar el mensaje obtenido de la red en el chat...
	private void Append(string text)
	{
		uOutput.AddTextEntry(text);
	}

	// Función para pasar un vector de float a otro de bytes
	private byte[] ToByteArray(float[] floatArray)
	{
		int len = floatArray.Length * 4;
		byte[] byteArray = new byte[len];
		int pos = 0;
		foreach (float f in floatArray)
		{
			byte[] data = System.BitConverter.GetBytes(f);
			System.Array.Copy(data, 0, byteArray, pos, 4);
			pos += 4;
		}
		return byteArray;
	}

	// Función para pasar un vector de bytes a otro de float
	private float[] ToFloatArray(byte[] byteArray)
	{
		int len = byteArray.Length / 4;
		float[] floatArray = new float[len];
		for (int i = 0; i < byteArray.Length; i += 4)
		{
			floatArray[i / 4] = System.BitConverter.ToSingle(byteArray, i);
		}
		return floatArray;
	}

	public bool GetTrasmitter()
	{
		return isTransmitter;
	}

	public bool GetSpeaking()
	{
		return isSpeaking;
	}

	public bool GetRecording()
	{
		return IsRecording;
	}
}