using UnityEngine;
using UnityEngine.UI;

/*
 * Este script se encarga de mostrar los textos que aparecen en la parte izquierda de la pantalla de juego. Estos textos tienen como finalidad informar
 * al jugador de en que estado se encuentra en referencia a las salas de streaming.
 */
public class ScreenWarn : MonoBehaviour {

	public GameObject StreamingText;
	public GameObject StreamingTextCam;
	public GameObject StreamingTextMicro;
	private float timer = 0.0f;
	private bool isAndroid = false;

	private void Start()
	{
		// Si la aplicación se ejecuta en Android, los textos se mostrarán en otra posición. Por ello, se hace esta comprobración.
		if(Application.platform == RuntimePlatform.Android)
		{
			isAndroid = true;
		} 
		else
		{
			isAndroid = false;
		}
	}

	// Update is called once per frame
	void Update ()
	{

		timer += Time.deltaTime;

		if (timer > 1)
		{
			//Texto de estado de conexión en la versión de PC
			if (!isAndroid)
			{
				if (GetComponent<PlayerCode>().IsSending() && !GetComponent<PlayerCode>().IsBusy())
					StreamingText.GetComponent<Text>().text = "Sala creada. Esperando jugadores ...";
				else if ((GetComponent<PlayerCode>().IsSending() || !GetComponent<PlayerCode>().IsSending()) && GetComponent<PlayerCode>().IsBusy())
					StreamingText.GetComponent<Text>().text = "Estás unido a una sala de streaming";
				else if (!GetComponent<PlayerCode>().IsSending() && !GetComponent<PlayerCode>().IsBusy())
					StreamingText.GetComponent<Text>().text = "Puedes unirte a una sala de streaming (pulsa Ctrl) o crear una (pulsa F1)";
			}

			//Texto de estado de dispositivos. Mediante estos textos el jugador puede asegurarse de que su cámara/micrófono estan emitiendo o no.
			// Este texto aparece en la parte superior de la pantalla.
			if( (GetComponent<PlayerCode>().IsSending() || !GetComponent<PlayerCode>().IsSending()) && GetComponent<PlayerCode>().IsBusy())
			{
				if (GetComponent<CallApp>().GetRecording())
					StreamingTextCam.GetComponent<Text>().text = "Cam: Emitiendo";
				else
					StreamingTextCam.GetComponent<Text>().text = "Cam: Desactivada";

				if (GetComponent<CallApp>().GetSpeaking())
					StreamingTextMicro.GetComponent<Text>().text = "Micro: Emitiendo";
				else
					StreamingTextMicro.GetComponent<Text>().text = "Micro: Desactivado";
			}
			else
			{
				StreamingTextCam.GetComponent<Text>().text = " ";
				StreamingTextMicro.GetComponent<Text>().text = " ";
			}

			timer = 0;
		}
	}
}
