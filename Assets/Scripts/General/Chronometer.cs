using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

/*
 * Este script se encargá de hacer funcionar el temporizador de la partida. Este temporizador funcionará en el servidor, y el servidor enviará el valor de su
 * reloj al resto de jugadores. Es más justo y es más justo que todos los jugadores se rijan por el mismo reloj.
 */

public class Chronometer : NetworkBehaviour
{
	private int secondsA = 0;
	private int secondsB = 0;
	private int minutesA = 0;
	private int minutesB = 3;
	public Text chronometer;

	// Inicia la coroutine de la función AddTime. Mediante la función StartCoroutine se avisa a Unity de que la función especificada entre paréntesis puede 
	// ejecutarsa periódicamente.
	void Start()
	{
		if (isServer)
			StartCoroutine("AddTime");
	}

	// Se encarga de actualizar el cronómetro de la pantalla
	void Update()
	{
		chronometer.text = (+ minutesA+ ""+ minutesB+ ":" + secondsA +""+ secondsB);

		if (minutesA == 0 && minutesB == 0 && secondsA == 0 && secondsB == 0)
			StopCoroutine("AddTime");
	}

	//Esta función realiza la función de ser un reloj digital. Espera cada segundo para añadir un valor a los segundos...etc.
	IEnumerator AddTime()
	{
		while (true)
		{
			yield return new WaitForSeconds(1);
			secondsB--;

			if(secondsA == 0 && secondsB < 0)
			{
				secondsB = 9;
				secondsA = 5;
				minutesB -= 1;
			}

			if(secondsA != 0 && secondsB < 0)
			{
				secondsB = 9;
				secondsA -= 1;
			}

			if (minutesA == 0 && minutesB == 0 && secondsA == 0 && secondsB == 0)
			{
				secondsB = 0;
				secondsA = 0;
				minutesB = 0;
				minutesA = 0;
			}

			RpcSetChronometer(secondsA, secondsB, minutesA, minutesB);

			if(!isClient)
				SetChronometer(secondsA, secondsB, minutesA, minutesB);

		}
	}

	//Mediante esta función RPC el servidor podrá actualizar su reloj al de todos para que todos sean iguales.
	[ClientRpc]
	public void RpcSetChronometer(int secA, int secB, int minA, int minB)
	{
		secondsA = secA;
		secondsB = secB;
		minutesA = minA;
		minutesB = minB;
	}

	public void SetChronometer(int secA, int secB, int minA, int minB)
	{
		secondsA = secA;
		secondsB = secB;
		minutesA = minA;
		minutesB = minB;
	}

	public int GetSecA()
	{
		return secondsA;
	}

	public int GetSecB()
	{
		return secondsB;
	}

	public int GetMinA()
	{
		return minutesA;
	}

	public int GetMinB()
	{
		return minutesB;
	}
}
