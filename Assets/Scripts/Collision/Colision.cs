using UnityEngine;

public class Colision : MonoBehaviour {

	//Esta función se detectará cuando un enemigo colisione cuerpo a cuerpo con un jugador. Esta función a diferencia de OnCollisionEnter se ejecutará
	//cada fotograma que el GameObject esté chochando con algo.
	void OnCollisionStay(Collision collision)
	{
		var hit = collision.gameObject;
		//Si colisiona con un GameObject de tipo Player
		if (hit.CompareTag("Player"))
		{
			var health = hit.GetComponent<Health>();
			if (health != null)
			{
				//Se resta 1 vida al jugador con el que haya chocado.
				health.TakeDamage(1);
			}
		}

	}
}
