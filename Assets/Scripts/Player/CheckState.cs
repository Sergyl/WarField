using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CheckState : NetworkBehaviour {

	/*
	 * Este script se encarga de visualizr en cual de los tres estados se encuentra el relación en relacion al streaming
	 * - Modo Activo: Ha iniciado una sala de streaming (is Sending = true)
	 * - Modo Ocupado: Está comunicándose con otro jugador dentro de otra sala de streaming (isBusy = true)
	 * - Modo Desocupado: No ha iniciado ninguna sala, ni entra dentro de ninguna. (isSending = false && isBusy = false)
	 */ 

	public GameObject pointRed;
	public GameObject pointGreen;
	public GameObject pointYellow;

	private bool redActive = false;
	private bool redActual = false;
	private bool greenActive = false;
	private bool greenActual = false;

	void Start ()
	{
		// Se comprueban las variables isSending y isBusy en el primer momento y se guardan en una variable booleana el estado actual
		CheckBoolean();
		redActual = redActive;
		greenActual = greenActive;

	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!isServer)
			return;

		// Se van comprobando las variables cada vez que se ejecuta la función Update se actualiza, si alguna variable cambia también cambiará la 
		// variable redActual o greenActual
		CheckBoolean();

		// Estos ifs se encarngan de comprobar si algunas de las dos variables cambian (eso significaría que habría un cambio de estado)
		if (redActual != redActive)
		{
			if (redActive)
				RpcEnabledRed();
			else
				RpcDisabledRed();

			redActual = redActive;
		}


		if (greenActual!= greenActive)
		{
			if (greenActive)
				RpcEnabledGreen();
			else
				RpcDisabledGreen();

			greenActual = greenActive;
		}

		// En caso de que las dos variables indiquen que no se está en el estado ocupado ni en el activo, entonces significa que está en el desocupado.
		if (!redActive && !greenActive)
		{
			RpcEnabledYellow();
		}
		else
		{
			RpcDisabledYellow();
		}
	}

	/*
		Esta función se encarga de comprobar las variables isSeding y isBusy que se encuentran en el script Player Code. En función de los valores que tengan
		cada una de ellas se aplicará un estado u otro
	*/
	private void CheckBoolean()
	{
		// Si isSending es true y IsBusy es false
		if (GetComponent<PlayerCode>().IsSending() == true && GetComponent<PlayerCode>().IsBusy() == false)
		{
			//Estamos ante el estado activo, el jugador ha iniciado una sala de streaming y espera que alguien se una.
			redActive = false;
			greenActive = true;
		}
		// Si isSending es true y IsBusy es true
		else if (GetComponent<PlayerCode>().IsSending() == true && GetComponent<PlayerCode>().IsBusy() == true)
		{
			// Estamos ante el estado ocupado, ya que la variable isBusy está activada, y eso significa que está comunicandose con otro jugador.
			redActive = true;
			greenActive = false;
		}
		// Si isSending es false y IsBusy es true
		else if (!GetComponent<PlayerCode>().IsSending() == true && GetComponent<PlayerCode>().IsBusy() == true)
		{
			// La variable isSending a false  y la isBusy indica que el no ha creado la sala, pero está dentro de una.
			// A efectos prácticos sigue tratándose del modo Ocupado, ya que se está comunicando con otro jugador
			redActive = true;
			greenActive = false;
		}
		else
		// Si isSending es false y IsBusy es false
		{
			// Estamos ante el estado  desocupado, ya que ni está en el estado ocupado ni en el activo
			redActive = false;
			greenActive = false;
		}
	}

	public bool GetStateRed()
	{
		return redActive;
	}

	public bool GetStateGreen()
	{
		return greenActive;
	}

	/*
	 * Estos estados deben ser visibles para todos los jugadores, así sabrán si el jugador está disponible o no para la comunicación. Por lo tanto, se hace uso
	 * de funciones RPC, para que esta información viaje a través de la red.
	 */

	[ClientRpc]
	private void RpcEnabledRed()
	{
		pointRed.GetComponent<Image>().enabled = true;
	}

	[ClientRpc]
	private void RpcDisabledRed()
	{
		pointRed.GetComponent<Image>().enabled = false;
	}

	[ClientRpc]
	private void RpcEnabledGreen()
	{
		pointGreen.GetComponent<Image>().enabled = true;
	}

	[ClientRpc]
	private void RpcDisabledGreen()
	{
		pointGreen.GetComponent<Image>().enabled = false;
	}

	[ClientRpc]
	private void RpcEnabledYellow()
	{
		pointYellow.GetComponent<Image>().enabled = true;
	}

	[ClientRpc]
	private void RpcDisabledYellow()
	{
		pointYellow.GetComponent<Image>().enabled = false;
	}
}
