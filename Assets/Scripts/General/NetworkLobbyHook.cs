using UnityEngine;
using Prototype.NetworkLobby;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook
{

	// Mediante esta fúnción, el nombre y el color que el jugaodr escoge en el Lobby será visualizado en la partida.
	public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
	{
		LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
		PlayerCode localPlayer = gamePlayer.GetComponent<PlayerCode>();

		// Se iguala el nombre y el color del Lobby al del avatar concreto. El color y el nombre se almacena en el script Player Code del jugador.
		localPlayer.pName = lobby.playerName;
		localPlayer.playerColor = lobby.playerColor;
	}
}
	
