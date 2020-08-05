using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CallManager : NetworkBehaviour {

	// Se declara como variable una lista sincronizada de string.
	public SyncListString playerFree;

	public void Update()
	{
		if (!isServer)
			return;

		// Se publica la lista para que todos los jugadores tengan acceso a ella en todo momento mediante su componente CallManager correspondiente.
		RpcPlayersFree();		
	}


	// Función Get para obtener la lista de jugadores.
	public SyncListString GetListFree()
	{
		return playerFree;
	}

	// Esta función publica la lista de jugadores que han creado una sala de streaming. Esta función será ejecutada continuamente en el juego.
	[ClientRpc]
	private void RpcPlayersFree()
	{
		// Se guarda en una variable los jugadores que hay en el mapa.
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

		// Recorre cada jugador del vector de jugadores....
		foreach(GameObject player in players)
		{
			// Lee las variables isSending y isBusy de cada jugador... si es un jugador en modo activo, se añade a la lista.
			if (player.GetComponent<PlayerCode>().IsSending() && !player.GetComponent<PlayerCode>().IsBusy())
			{
				if (!playerFree.Contains(player.GetComponent<PlayerCode>().GetName()))
				{
					// Se añade a la lista....
					playerFree.Add(player.GetComponent<PlayerCode>().GetName());
				}
			}else
			// Lee las variables isSending y isBusy de cada jugador... si es un jugador en modo ocupado o desocupado, se elimina de la lista.
			{
				if (playerFree.Contains(player.GetComponent<PlayerCode>().GetName()))
					// Se elimina de la lista....
					playerFree.Remove(player.GetComponent<PlayerCode>().GetName());
			}
		}
	}
}
