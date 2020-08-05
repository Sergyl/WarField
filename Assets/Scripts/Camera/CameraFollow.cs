using UnityEngine;

/*
Mediante ese script, la cámara seguirá en todo momento al jugador. El parámetro de entrada playerTrasnform posee la posicion del jugador.
La posición de la cámara se iguala a la posición exacta en donde la cámara se pondrá para crear una perspectiva de primera persona.
*/

public class CameraFollow : MonoBehaviour
{
	private Transform playerTransform;
	private float distanceX = 0.0f;
	private float distanceY = 0.0f;
	private float distanceZ = 0.0f;

	//Esta función es parecida a la de Update, pero es esta la que recomienda el manual de Unity para que realice el seguimiento de la cámara.
	void LateUpdate()
    {
        FollowCamera();
    }

	//Es la función que recogerá la posición del jugador en todo momento. La posición de la cámara luego se igualará a la posición del jugador.
    private void FollowCamera()
    {
        if (playerTransform != null)
        {
            Vector3 dir = new Vector3(distanceX, distanceY, distanceZ);
            transform.position = playerTransform.position + dir;
            transform.rotation = playerTransform.rotation;
        }
    }

	//Esta función permite especificar a qué jugador pertenecerá la posición recogida en la variable playerTransform.
    public void SetTarget(Transform target)
	{
		playerTransform = target;
	}
}