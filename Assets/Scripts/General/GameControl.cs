using UnityEngine;
using UnityEngine.Networking;

/*
 * Este script se encarga de supervisar que la partida siga en juego o no. Para ganar la partida hay que intentar no morir y conseguir el máximo de puntos posibles
 * - Cuando el temporizador llega a 0, se hará una comprobación de los jugadores que aun no hayan perdidos sus 5 vidas, aquel jugador que tenga más puntos de entre
 *   los que continuan vivos, ganará la partida.
 * - Si mueren todos los jugadores antes de terminar el temporizador, nadie gana
 * - Si por ejemplo mueren 5 de los 6 jugadores de la partida, ganará aquel jugador que siga viva, independientemente de sus puntos.
 */

public class GameControl : NetworkBehaviour {

    private bool playerWarned = false;

	[SyncVar]
	private bool result = false;

	public void Update()
    {
		// Si los IsGameEnded es true (significa partida temrinada) y si playerWarned es false (significa que aun no ha terminado)
		// La variable playerWarned permite que esta función solo se ejecute una vez, así se evitaría que se siguiera ejecutando infinitamente después de perder la partida.
		if (IsGameEnded() && !playerWarned)
        {
			// La función Warn avisa a los jugadores de que la partida ha terminado poniendo la variable Finish a true.
			WarnToMe();
			playerWarned = true;
		}
    }

	// Esta hace la comprobación que se ha comentado en el comentario inicial del script.
    public bool IsGameEnded()
    {
		int playersLose = 0;
		int numberLosers = 0;

		// Si hay jugadores en el mapa...
		if(GameObject.FindGameObjectWithTag("Player") != null)
		{
			// Guarda en una variable el número de jugadores necesario que deben perder sus vidas para que el otro gana la partida (ej: si hay 5 jugadores, la partida terminarían si pierden 4...etc)
			playersLose = GameObject.FindGameObjectsWithTag("Player").Length - 1;
		}

		// El bucle recorrer cada jugador que hay en el mapa, para cada uno...
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
			// Si el valor de playerLose es 0, simboliza que solo hay 1 jugador en la partida, por lo tanto solo comprueba que ese jugador no muera.
			if (playersLose == 0)
			{
				if (player.GetComponent<Health>().GetLive() == 0)
					result = true;
			}
			// Si hay más jugadores en la partida...
			else
			{
				// Si este jugador ha perdido todas sus vidas....
				if (player.GetComponent<Health>().GetLive() == 0)
					// Se introduce en la variable de jugadores perdeddores...
					numberLosers++;

				// Si el número de jugadores perdedoras es igual al mínimo de jugadores necesarios que deben perder para terminar la partida..
				if (playersLose != 0 && (numberLosers == playersLose))
					// La partida termina...
					result = true;
			}

			// Si el temporizador de partida llega a 0...
			if (player.GetComponent<Chronometer>().GetSecA() == 0 && player.GetComponent<Chronometer>().GetSecB() == 0 && player.GetComponent<Chronometer>().GetMinA() == 0 && player.GetComponent<Chronometer>().GetMinB() == 0)
				// La partida termina...
				result = true;
        }

        return result;
    }

    public void WarnToMe()
	{
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
			player.GetComponent<PlayerCode>().SetFinish(true);
		}
    }

	public bool GetPlayerWarned()
	{
		return playerWarned;
	}

	public bool GetResult()
	{
		return result;
	}
}
