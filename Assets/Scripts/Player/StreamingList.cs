using UnityEngine;
using UnityEngine.Networking;

/*
 * Este script se encarga de generar la lista que mostrará a los jugadores que han abierto una sala de streaming, y están esperando que alguien se una a ellos.
 */

public class StreamingList : MonoBehaviour
{

	public GameObject Button;
	public GameObject selfPlayer;
	public GameObject StreamingPanel;
	private GameObject[] buttons = new GameObject[6];
	private Vector3[] SpawnButton = new Vector3[6];
	private GameObject nombre;

	public void Update()
	{
		// Mediante esta función la lista de streaming activos se actualizará incluso cuando esté abierta. De modo que en el momento de abrirla si alguna sala se cierra
		// por alguna razón, desaparecerá su botón correspondiente de la lista automáticamente.

		// Si la lista de streaming está abierta.....
		if (StreamingPanel.activeSelf)
		{
			// Se hace una comprobación de los nombres de los jugadores que tienen abierta su sala.
			GameObject CallManager = GameObject.FindGameObjectWithTag("CallManager");
			SyncListString namesFree = CallManager.GetComponent<CallManager>().GetListFree();
			for (int i = 0; i < buttons.Length; i++)
			{
				if(buttons[i] != null)
				{
					nombre = buttons[i].transform.Find("Nombre").gameObject;
					// Si el nombre de algún jugador de la lista ya no se encuentra dentro de la variable namesFree, desaparecerá su botón.
					if (!namesFree.Contains(nombre.GetComponent<UnityEngine.UI.Text>().text))
					Destroy(buttons[i]);
				}
			}
		}
	}

	// Esta función se ejecutará cuando aparezca la lista de streaming activos. Iniciará unos vectores de posición en donde colocará los posibles nombres de los jugadores
	// que hayan creado una sala.
	public void OnActivateSearch()
	{
		InitializeVectors();
		ClearButtons();
		CreateButtons();
	}

	// Inicializa las posiciones que tendrán los botones dentro de la lista de streaming abiertos. Hay 6 posiciones, ya que son 6 el número de jugadores que tendrá el juego como máximo.
	private void InitializeVectors()
	{
		SpawnButton[0] = new Vector3(-79.8f, -26.5f, 0.0f);
		SpawnButton[1] = new Vector3(-40.2f, -26.5f, 0.0f);
		SpawnButton[2] = new Vector3(-79.8f, -39.5f, 0.0f);
		SpawnButton[3] = new Vector3(-40.2f, -39.5f, 0.0f);
		SpawnButton[4] = new Vector3(-79.8f, -52.5f, 0.0f);
		SpawnButton[5] = new Vector3(-40.2f, -52.5f, 0.0f);
	}

	// Borrará todos los botones de los jugadores a los que se puede unir.
	private void ClearButtons()
	{
		for(int i=0; i<buttons.Length; i++)
		{
			Destroy(buttons[i]);
		}
	}

	// Ésta función crea botones con el nombre de las salas activas.
	private void CreateButtons()
	{
		// Buscará el GameObject CallManager en la escena. Ya que es el encargado de gestionar el tema de los nombres de los jugadores que tienen o no abierta una sala de streaming.
		GameObject CallManager = GameObject.FindGameObjectWithTag("CallManager");
		// Mediante la función GetListFree creada en el script CallManager del GameObject del mismo nombre, se obtiene una lista de nombres correspondientes a los jugadores que tienen una sala activada.
		SyncListString namesFree = CallManager.GetComponent<CallManager>().GetListFree();

		// Mediante este bucle for, se añade a cada jugador con su sala abierta, una posición en la lista de streaming abiertos.
		for (int i = 0; i < namesFree.Count; i++)
		{
			// Si el nombre obtenido de la lista, no es el propio, no aparecerá en la lista (ya que no tiene sentido meterse en una sala de streaming que ha creado uno mismo)
			if (!namesFree.Contains(selfPlayer.GetComponent<PlayerCode>().GetName()))
			{
				// Crea un botón con la una de las posiciones de los vectores inicializados anteriormente.
				buttons[i] = (GameObject)Instantiate(Button, SpawnButton[i], Quaternion.identity);
				// Asigna la posición del vector de manera relativa a la posición de la lista dentro de la lista de streaming activos
				buttons[i].transform.SetParent(StreamingPanel.transform, false);
				// Muestra el botón 
				buttons[i].SetActive(true);
				// El botoón tendrá el nombre del jugador que ha creado la sala. De modo que si se presione sobre ese bótón, la función que realizará la entrada a la sala
				// recibirá como parámetro de entrada el nombre del botón. Ya que, a la hora de crear las salas de streaming, estas se crean un nombre que las identifica de las demas. Este nombre no es otro que el nombre del jugador que crea la sala
				GameObject name = buttons[i].transform.Find("Nombre").gameObject;
				name.GetComponent<UnityEngine.UI.Text>().text = namesFree[i].ToString();
			}
		}
	}
}
