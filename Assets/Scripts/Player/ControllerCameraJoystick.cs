using UnityEngine;
using UnityEngine.Networking;

/*
 * Este script permite traducir el movimiento obtenido del joystick izquierdo a un movimiento en la cámara del avatar. Este script solo realizará su función
 * cuando el proyecto se esté ejecutando en un dispositivo móvil Android.
 */

public class ControllerCameraJoystick : NetworkBehaviour
{
	[SerializeField] private JoystickFunction Joystick;//Joystick reference for assign in inspector

	private CharacterController controller;
	private float vertical;
	private float horizontal;
	private bool isAndroid = false;

	private void Start()
	{
		if (!isLocalPlayer)
			return;

		// Se guarda en una variable el componente Character Controller que se encuentre adherido al GameObject del jugador, este componente sirve para mover el avatar.
		controller = GetComponent<CharacterController>();

		// Se comprueba que este script se ejecuta desde una plataforma Android o no, en función de ello, la variable isAndroid adquirirá un valor u otro.
		if (Application.platform == RuntimePlatform.Android)
			isAndroid = true;
		else
			isAndroid = false;
	}

	void Update()
	{
		if (!isAndroid)
			return;

			//Mediante estas líneas se recoge la información del movimiento pertenecientes a los joysticks
			//Change Input.GetAxis (or the input that you using) to Joystick.Vertical or Joystick.Horizontal
			float v = Joystick.Vertical; //get the vertical value of joystick
			float h = Joystick.Horizontal; //get the horizontal value of joystick

			//Se divide el valor obtenido para que el movimiento de la cámara del avatar no sea tan agresivo como el de los joysticks.
			vertical += v/4;
			horizontal += h/4;

			//Mediante el componente controller, podemos hacer que el avatar rote en la dirección que se desea. Utilizando las variables obtenidas anteriormente desde los joystick, podemos mover la cámara.
			controller.transform.eulerAngles = new Vector3(-vertical, horizontal, 0.0f);

	}
}