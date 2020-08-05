using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/*
 * Este script gestionará todo lo referente al control de salud y vidas del jugador/enemigo.
 */

public class Health : NetworkBehaviour
{
    public bool destroyOnDeath;
    public const int maxHealth = 100;
	public RectTransform healthBar;
    public Text numberOfLives;
    public const int maxLive = 5;
    private NetworkStartPosition[] spawnPoints;

	// Esta variable sincronizada es especial, cuando el valor de la variable currentHealth cambie su valor se ejecutará la función OnChangeHealth
	[SyncVar(hook = "OnChangeHealth")]
    public int currentHealth = maxHealth;

    [SyncVar]
    public int currentLive = maxLive;

    void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }
		// Mostrará en la interfaz de usuario el número de vidas con las que empieza el jugador al momento de iniciar la partida. El valor es el mismo que maxLive
        numberOfLives.text = (maxLive.ToString());
		// Se guarda en una variable aquellos GameObject que tengan el componente NetworkStarPosition, utilizado para señalar el punto del mapa donde aparecerán los jugadores.
        spawnPoints = FindObjectsOfType<NetworkStartPosition>();
    }

	//Función que se encarga de restar vida al jugador.
	public void TakeDamage(int amount)
    {
        if (!isServer)
            return;

        currentHealth -= amount;

		//Si la vida llega a 0...
        if (currentHealth <= 0)
        {
			// Si es un GameObject destruible (es decir, un enemigo)
            if (destroyOnDeath)
            {
				// El GameObject del enemigo desaparecerá cuando pierda toda su salud.
                Destroy(gameObject);
            }
            else
            {
				//En cambio si es un GameObject del tipo jugador, y este pierde su salud.
                currentHealth = maxHealth;

				// El jugador reaparecerá cuando este muera
				if (currentLive > 0)
				{
					// Llamada a una función RPC para que reste una vida al jugador.
					RpcTakeAwayLive();

					//Este if se coloca para que le quite una vida cuando el jugador muera, en el caso de que sea una conexión por servidor dedicado o MatchMaker
					if (!isClient)
						TakeAwayLive();

					//Esta función es la de reaparición del jugador.
					RpcRespawn();
				}
            }
        }
    }

	// Mediante esta función, el jugador se senará la cantidad especificada como parámetro de entrada
    public void Heal(int amount)
    {
		// Se le suma la cantidad especificada.
        currentHealth += amount;
		
		//Si el jugador ya tiene su salud al máximo o la supera, se fijará a su salud máximo. De esta manera se evita que un jugador tenga 120% de salud.
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

	//Esta función se ejecutará cuando la variable currentHealth varíe. Mediante esta función se ajusta la barra de control de salud a la salud que posee el jugador.
    void OnChangeHealth(int health)
    {
        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
	}

	//Esta función inicializa la salud del jugador a su máxima salud posible, en esta caso, al de la variable maxLive.
    public void SetLiveMax()
    {
        if (!isServer)
            return;

        RpcSetLiveMax();
    }

	/*
	 * Diferentes funciones RPC para que los cambios sean vistos por todos los jugadores.
	 */

	// Función para restar una vida al jugador. Esta función se llama cuando el jugador pierde toda su salud. Es una función RPC
    [ClientRpc]
    void RpcTakeAwayLive()
    {
        currentLive -= 1;
        numberOfLives.text = (currentLive.ToString());
    }

	// Función para restar una vida al jugador. Esta función se llama cuando el jugador pierde toda su salud.
	void TakeAwayLive()
	{
		currentLive -= 1;
		numberOfLives.text = (currentLive.ToString());
	}

	//Esta función inicializa la salud del jugador a su máxima salud posible, en esta caso, al de la variable maxLive. Es una función RPC
	[ClientRpc]
    private void RpcSetLiveMax()
    {
        currentLive = maxLive;
    }

	// Mediante esta función, el servidor será capaz de hacer que un jugador reaparezca en uno de los puntos marcados en el mapa cuando este muera.
    [ClientRpc]
    void RpcRespawn()
    {
		if (!isLocalPlayer)
			return;

        Vector3 spawnPoint = Vector3.zero;

		if (spawnPoints != null && spawnPoints.Length > 0)
			spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

        transform.position = spawnPoint;
    }

    public int GetMaxLife()
    {
        return maxLive;
    }

    public int GetLive()
    {
        return currentLive;
    }
}