using UnityEngine;

//Esta función se ejecutará cuando los GameObject Apple detecten una colision del tipo jugador. Estos GameObject se encargan de sanar al jugador cuando
// este entre el contacto con ellos.
public class ColisionLife : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        var hit = collision.gameObject;
        var health = hit.GetComponent<Health>();
        if (health != null)
        {
			//Si el GameObject con el que ha colisionado es del tipo Player...
            if (hit.CompareTag("Player"))
            {
				//Se cura 50 puntos al jugador y se destruye la manzana.
                health.Heal(50);
                Destroy(gameObject);
            }
        }

    }
}
