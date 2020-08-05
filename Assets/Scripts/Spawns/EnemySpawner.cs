using UnityEngine;
using UnityEngine.Networking;

/*
 * Mediante este script se gestionará las oleadas de los enemigos.
 */

public class EnemySpawner : NetworkBehaviour
{
	public GameObject enemyPrefab;
    public int numberOfEnemies;

	// Cuando el server inicie...
	public override void OnStartServer()
	{
		// Se creará la primera oleada de enemigos....
		SpawnWave();
		//Mediante esta función, permitirá que la función SpawnWave se ejecute entre un intervalo de 30-40s aleatoriamente
		InvokeRepeating("SpawnWave", 30.0f, 40.0f);
	}

	private void Update()
	{
		// Este if hace una comprobación del objeto GameManager. Si el objeto detecta que la partida ha finalizado, ya no se apacerán más oleadas.
		if (GameObject.FindWithTag("GameManager") != null)
		{
			if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameControl>().GetResult())
			{
				// Destroye todos los enemigos del mapa cuando la partida termina.
				DeleteEnemy();
				// Detiene la función SpawnWave para que no vuelva a ejectuarse.
				CancelInvoke();
			}
		}
	}

	// Esta función crea a los enemigos en el mapa. El valor numberOfEnemies es un parámetro de entrada, e indica el número de enemigos que se crearán por oleada para cada tipo de enemigo.
	void SpawnWave()
	{
		for (int i = 0; i < numberOfEnemies; i++)
		{
			// Se crea una posición aleatoria para cada aparición
			var spawnPosition = new Vector3(
				Random.Range(-40.0f, 40.0f),
				0.0f,
				Random.Range(-40.0f, 40.0f));

			var spawnRotation = Quaternion.Euler(
				0.0f,
				Random.Range(0, 180),
				0.0f);

			// Iguala la posición de aparición a la obtenida en las líneas anteriores.
			spawnPosition += transform.position;
			var enemy = (GameObject)Instantiate(enemyPrefab, spawnPosition, spawnRotation);
			// Crea el enemigo para todos los jugadores
			NetworkServer.Spawn(enemy);
		}
	}

	// Elimina a todos los enemigos de la escena. Ocurre cuando finaliza la partida.
	void DeleteEnemy()
	{
		if(GameObject.FindWithTag("Enemy") != null)
		{
			foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
			{
				// Función para eliminar todos los enemigos de la escena en todos los jugadores.
				NetworkServer.Destroy(enemy);
			}
		}
	}
}