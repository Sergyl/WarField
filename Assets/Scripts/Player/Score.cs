using UnityEngine.Networking;
using UnityEngine.UI;

/*
 * Este script realiza la suma de los puntos y los muestra en la interfaz de usuario.
 */

public class Score : NetworkBehaviour {

	public Text PanelScore;

	[SyncVar]
	private int score = 0;

    // Update is called once per frame
    void Update ()
    {
        if (!isLocalPlayer)
            return;

		// Cada vez que se ejecuta la función Update, actualiza los puntos en la interfaz de usuario.
		PanelScore.text = ("Score: " + score.ToString());
	}

	/*
	 * Funciones para sumar puntos y un Get para sacar el valor de score.
	 */
	
	[ClientRpc]
	public void RpcAddScore(int point)
	{
		score += point;
	}

	public void AddScore(int point)
	{
		score += point;
	}

	public int GetScore()
    {
        return score;
    }
}
