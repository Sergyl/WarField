using UnityEngine;

public class BulletFire : MonoBehaviour
{

	//Este script se adhiere al GameObject de las bolas de fuego que lanzan los enemigos dragones.
	//Cuando el GameObject de la bola de fuego detecta colisión, restará 45 puntos de vida a la salud de su víctima
	void OnCollisionEnter(Collision collision)
	{
		var hit = collision.gameObject;
		var health = hit.GetComponent<Health>();
		if (health != null)
		{
			//Se le resta 45 puntos de salud.
			health.TakeDamage(45);
		}
		//El GameObject de la bola de dragón será destruido cuando colisione con algo.
		Destroy(gameObject);
	}
}