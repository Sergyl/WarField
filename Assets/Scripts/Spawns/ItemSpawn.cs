using UnityEngine;
using UnityEngine.Networking;
/*
 * Este script es muy similar a EnemySpawner. Mediante este script se gestiona la aparición de las manzanas. Las manzanas son items del juego
 * que curarán al jugador cuando ambos entren en contacto.
 */

public class ItemSpawn : NetworkBehaviour
{
    public GameObject itemPrefab;
    public int numberOfItems;

	// Cuando el server inicie...
	public override void OnStartServer()
    {
		// Se creará la primera oleada de manzanas...
		SpawnWave();
		//Mediante esta función, permitirá que la función SpawnWave se ejecute entre un intervalo de 30-35s aleatoriamente
		InvokeRepeating("SpawnWave", 30.0f, 35.0f);
    }

	// Esta función crea las manzanas en el mapa. El valor numberOfItems es un parámetro de entrada, e indica el número de manzanas que se crearán por oleada.
	void SpawnWave()
    {
        for (int i = 0; i < numberOfItems; i++)
        {
			// Se crea una posición aleatoria para cada aparición
			var spawnPosition = new Vector3(Random.Range(-60.0f, 60.0f), 0.0f, Random.Range(-60.0f, 60.0f));
            var spawnRotation = Quaternion.Euler(0.0f, Random.Range(0, 180), 0.0f);

			// Iguala la posición de aparición a la obtenida en las líneas anteriores.
			spawnPosition += transform.position;
            var item = (GameObject)Instantiate(itemPrefab, spawnPosition, spawnRotation);
			// Crea las manzas para todos los jugadores
			NetworkServer.Spawn(item);
        }
    }
}
