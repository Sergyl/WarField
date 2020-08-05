using UnityEngine;
using UnityEngine.Networking;

public class EnemyPartisan : NetworkBehaviour
{
	private Transform player;
	private readonly float  MoveSpeed = 5.0f;
	private readonly float MinDist = 5.0f;

	void Update()
	{
		if (!isServer)
			return;

		// Metidante estte fragmento de código, al enemigo se le dota de cierta inteligencia artificial. 
		// Por cada vez que se ejecuta la función Update, el enemigo fijará al jugador más cercano y lo perseguirá para quitarle vida.
		try
		{
			//Se busca al jugador más cercano mediante las funciones que se encuentran debajo de esta.
			player = GetClosestEnemy(FindNearestTransform());
			//El enemigo mirará al jugador (el GameObject TargetBody es una parte del cuerpo del jugador que servirá para que el enemigo fije su objeto en el cuerpo del jugador)
			transform.LookAt(player.Find("TargetBody").transform);

			//Se evita que el enemigo vuele por el cielo
			if (transform.position.y <= 1.0f)
				transform.position += transform.up;

			//Si el enemigo está a una distancia muy alejada del jugador, lo perseguirá.
			if (Vector3.Distance(transform.position, player.position) >= MinDist)
			{
				transform.position += transform.forward * MoveSpeed * Time.deltaTime;
			}
		}
		catch (System.NullReferenceException)
		{
			return;
		}
	}



	// Esta función devuelve un vector del tipo Transform, el cual almacena las posiciones de todos los jugadores del mapa.
	public Transform[] FindNearestTransform()
	{
		GameObject[] enemies;
		Transform[] enemiesLocation;

		enemies = GameObject.FindGameObjectsWithTag("Player");
		enemiesLocation = new Transform[enemies.Length];

		for (int i = 0; i < enemies.Length; i++)
		{
			enemiesLocation[i] = enemies[i].transform;
		}

		return enemiesLocation;
	}

	// Esta función se encarga de recorger el vector de tipo Transform obtenido de la función FindNearestTransform(), y calcular cual es el jugador
	// que se encuentra más cerca
	Transform GetClosestEnemy(Transform[] enemies)
	{
		Transform tMin = null;
		float minDist = Mathf.Infinity;
		Vector3 currentPos = transform.position;
		foreach (Transform t in enemies)
		{
			float dist = Vector3.Distance(t.position, currentPos);
			if (dist < minDist)
			{
				tMin = t;
				minDist = dist;
			}
		}
		return tMin;
	}
}
