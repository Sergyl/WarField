using UnityEngine;

/*
 * Este script pertenece al asset UJoystick, y no se ha modificado nada de él. 
 * Este script solo tiene sentido en la versión Android.
 */

public static class JoystickUtils
{

    public static Vector3 TouchPosition(this Canvas _Canvas,int touchID)
    {
        Vector3 Return = Vector3.zero;
		Vector3 pos = Vector3.zero;

		if (_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				Return = Input.GetTouch(touchID).position;
			}
		}
		else if (_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			Vector2 tempVector = Vector2.zero;
			if (Application.platform == RuntimePlatform.Android)
			{
				pos = Input.GetTouch(touchID).position;
			}

			RectTransformUtility.ScreenPointToLocalPointInRectangle(_Canvas.transform as RectTransform, pos, _Canvas.worldCamera, out tempVector);
			Return = _Canvas.transform.TransformPoint(tempVector);
		}

        return Return;
    }
}