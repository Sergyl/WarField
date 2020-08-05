using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
	//Esta función se ejecutará cuando los GameObject invisible alrededor del río detecten una colisión del tipo Enemy o Bullet, las ignoren.
	//De esta manera los enemigos podrán cruzar el río, y los disparos del jugador también.
	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Bullet")
		{
			//Esta es la función encargada de ignorar la colisión.
			Physics.IgnoreCollision(collision.collider, GetComponent<BoxCollider>());
		}
	}

}