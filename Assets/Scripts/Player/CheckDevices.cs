using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CheckDevices : NetworkBehaviour
{
	/*
	 * Este script hará una comprobación en la que se verá si el jugador tiene su cámara y micrófono conectados al ordenador cuando este esté
	 * dentro de una sala de streaming o haya creado alguna. Si el jugador tiene su micróno y/o cámara conectada a su ordenador, aparecerá un icono al lado de su 
	 * nombre indicando que dispositivos detecta Unity. Ya que este icono deberá ser visto por todos los usuarios, se utilizará funcione RPC para que puedan ser
	 * enviadas a lo largo de la red.
	 */

    private WebCamDevice wcam;
	public GameObject cam;
	public GameObject microphone;

	private bool camActive = false;
	private bool micActive = false;
	private bool actualCam = false;
	private bool actualMic = false;
	private bool toggleChecking = true;
	private bool newChecking = true;
	private float timer = 0.0f;

    // Update is called once per frame
    void LateUpdate()
    {
		if (!isServer)
			return;

		//Para mejorar el rendimiento del juego, la comprobación de si el usuario tiene cámara y micrófono conectado se hará 
		//únicamente cuando este se encuentre en el modo Activo o en el modo Ocupado.
		if (GetComponent<CheckState>().GetStateGreen() || GetComponent<CheckState>().GetStateRed())
		{
			timer += Time.deltaTime;
			//La comprobación se hará cada 3 segundos, si no se hiciera este if la comprobación la haría cada menos de 0.2s, 
			//cargando excesivamente al procesador.
			if (timer > 3)
			{
				CheckBoolean();

				if (newChecking)
				{
					if (camActive)
						RpcEnabledCam();
					else
						RpcDisabledCam();


					if (micActive)
						RpcEnabledMicro();
					else
						RpcDisabledMicro();

					actualCam = camActive;
					actualMic = micActive;
					newChecking = false;
				}

				if ((actualCam != camActive) || (actualMic != micActive))
					newChecking = true;

				timer = 0;
				toggleChecking = true;
			}
		}else
		{
			if (toggleChecking)
			{
				RpcDisabledCam();
				RpcDisabledMicro();
				toggleChecking = false;
				newChecking = true;
			}
		}
	}

	private void CheckBoolean()
	{
		WebCamDevice[] devices = WebCamTexture.devices;
		if (devices.Length != 0)
			camActive = true;
		else
			camActive = false;



		if (Microphone.devices.Length != 0)
			micActive = true;
		else
			micActive = false;	
	}

	[ClientRpc]
	private void RpcEnabledCam()
	{
		cam.GetComponent<Image>().enabled = true;
	}

	[ClientRpc]
	private void RpcDisabledCam()
	{
		cam.GetComponent<Image>().enabled = false;
	}

	[ClientRpc]
	private void RpcEnabledMicro()
	{
		microphone.GetComponent<Image>().enabled = true;
	}

	[ClientRpc]
	private void RpcDisabledMicro()
	{
		microphone.GetComponent<Image>().enabled = false;
	}
}
