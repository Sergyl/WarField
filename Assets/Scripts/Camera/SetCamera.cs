using UnityEngine;
using UnityEngine.Networking;

/*
 * Cada cliente ejecuta su escena del juego, en ella hay un GameObject llamado MainCamara. Mediante este script ese GameObject de la escena
 * se añade al jugador correspondiente
 */

public class SetCamera : NetworkBehaviour
{
	public Transform CameraSet;

	//La función Start se ejecuta en el primer fotograma en el que inicia el GameObject asociado al script.
	void Start()
	{
		//Si el GameObject del jugador es el jugador de esta maquina
		if (!isLocalPlayer)
			return;

		//Se agrega la cámara que hay en la escena al jugador correspondiente.
		Camera.main.GetComponent<CameraFollow>().SetTarget(CameraSet);
	}
}
