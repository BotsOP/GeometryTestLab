using UnityEngine;

public class MathLibrary : MonoBehaviour
{
    public static float TAU = 6.28318530718f;
    public static Vector2 GetVectorByAngle(float angRad)
    {
        return new Vector2(
            Mathf.Cos(angRad),
            Mathf.Sin(angRad)
        );
    }
}
