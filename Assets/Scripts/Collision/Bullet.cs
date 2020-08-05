using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Bullet : NetworkBehaviour
{
    private GameObject playerCreator;

	/* Mediante esta función Start, se realiza una comproboación de los GameObjects del tipo Player, y aquel cuyo nombre sea el mismo que el de la bala
	 * será el jugador que la haya disparado.
	 */
    private void Start()
    {
		foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (player.GetComponent<PlayerCode>().GetName() == gameObject.name)
				playerCreator = player;
		}
	}

	//Esta función es proporcionada por Unity y se ejecuta cada vez que el GameObject detecte que ha colisionado con otro elemento.
	//Mediante esta función se hace la comprobación para ver si el objeto con el que colisiona es un jugador o un enemigo
    void OnCollisionEnter(Collision collision)
	{
		if (!isServer)
			return;

        var hit = collision.gameObject;
        var health = hit.GetComponent<Health>();

		//Si colisiona con un enemigo, robará la vida del enemigo mediante la función TakeDamage y a continuación sumará 10 puntos al marcador mediante la función RPCAddScore
		//Como el score es un valor que debe ser sincronizado en todos los jugadores para saber cuando termina la partida, se utiliza un RPC.
		if (health != null && health.CompareTag("Enemy"))
		{
			health.TakeDamage(25);
			playerCreator.GetComponent<Score>().RpcAddScore(10);

			//Este if se coloca para que funcionen la suma de los puntos tambien cuando se usan servidores dedicados o MatchMaker, ya que ningún jugador será servidor.
			if (!isClient)
				playerCreator.GetComponent<Score>().AddScore(10);
		}
		//Si se dispara a un jugador no se sumará ningún punto, simplemente restará un poco de vida al jugador que haya recibido el disparo.
		else if (health != null && health.CompareTag("Player"))
		{
			health.TakeDamage(5);
		}

        Destroy(gameObject);
    }
}