
using UnityEngine;
public static class MathExtensions {

    /*
    Returns a random numbers between -range and range whose values around zero are more likely
    */
    public static float RandomBinomial(float range = 1.0f) {
        return Random.Range(-range, range) - Random.Range(-range, range);
    }

    /*
    Converts an orientation to vector, it assumes that cos = 1 is the positive z axis
    */
    public static Vector3 OrientationToVector(float orientation) {
        return new Vector3(Mathf.Sin(orientation), 0.0f, Mathf.Cos(orientation));
    }

    /*
    Converts from vector to an orientation, ignores the y component assumes that position z
    is forward 
    */
    public static float ToOrientation(this Vector3 direction) {
        return Mathf.Atan2(direction.x, direction.z);
    }


    public static float DegreesToRadians(float angleDegrees) {
        return (angleDegrees * Mathf.PI) / 180.0f;
    }

    public static float RadiansToDegrees(float angleRadians) {
        return (angleRadians * 180.0f) / Mathf.PI;
    }

}