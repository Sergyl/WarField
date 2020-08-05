using UnityEngine.Networking;
using UnityEngine.UI;

/*
 * Este script permite mostrar en pantalla el número de vidas del jugador. Además, permite actualizar el valor de las vidas cuando estan cambien.
 */
public class Lives : NetworkBehaviour
{
    public Text life;
    private int vida;

    void Update()
	{
        if (!isLocalPlayer)
            return;

		// Se guarda en una variable el componente script Health
        Health health = GetComponent<Health>();

		//Mediante la función GetLive construida en el script Health, se extra el valor de las vidas restantes del jugador.
        vida = health.GetLive();

		// Se muestran las vidas en la interfaz de usuario del jugador.
        life.text = (vida.ToString());
    }
}