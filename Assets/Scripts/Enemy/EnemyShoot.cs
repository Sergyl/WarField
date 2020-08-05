using UnityEngine;
using UnityEngine.Networking;

public class EnemyShoot : NetworkBehaviour
{

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	private Transform player;
	private readonly float MoveSpeed = 5.0f;
	private readonly float MaxDist = 75.0f;
	private readonly float MinDist = 5.0f;
	private bool onRange = false;

	void Start()
	{
		// Se especifica la cadencia de disparo del dragón.
		InvokeRepeating("Shoot", 2.0f, 5.0f);
	}

	void Update()
	{
		if (!isServer)
			return;

		// Metidante estte fragmento de código, al enemigo se le dota de cierta inteligencia artificial. 
		// Por cada vez que se ejecuta la función Update, el enemigo fijará al jugador más cercano y lo perseguirá para quitarle vida.
		// Además, al ser este un enemigo de tipo dragón, cuando detecte que el enemigo está cerca, establecerá a True una variable llamada onRange, indicando que empezará a disparar.
		try
		{
			//Se busca al jugador más cercano mediante las funciones que se encuentran debajo de esta.
			player = GetClosestEnemy(FindNearestTransform());
			//El enemigo mirará al jugador (el GameObject TargetBody es una parte del cuerpo del jugador que servirá para que el enemigo fije su objeto en el cuerpo del jugador)
			transform.LookAt(player.Find("TargetBody").transform);

			//Si el enemigo está a una distancia muy alejada del jugador, lo perseguirá.
			if (Vector3.Distance(transform.position, player.position) >= MinDist)
			{
				transform.position += transform.forward * MoveSpeed * Time.deltaTime;

				//Si el jugador se encuentra a una distancia óptima para el dragon, el dragón podrá comenzar a disparar sus bolas de fuego
				if (Vector3.Distance(transform.position, player.position) <= MaxDist)
				{
					onRange = true;
				}
			}
		}
		catch (System.NullReferenceException)
		{
			return;
		}
	}

	void Shoot()
	{
		if (onRange)
		{
			Fire();
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


	void Fire()
	{
		// Create the Bullet from the Bullet Prefab
		var bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

		// Add velocity to the bullet
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 75;

		// Spawn the bullet on the Clients
		NetworkServer.Spawn(bullet);

		// Destroy the bullet after 2 seconds
		Destroy(bullet, 6.0f);
	}
}