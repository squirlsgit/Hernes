using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static List<float> GetPosition(this Transform t)
    {
        return new List<float>() { t.position.x, t.position.y, t.position.z };
    }
    public static List<float> GetRotation(this Transform t)
    {
        var euler = t.rotation.eulerAngles;
        return new List<float>() { euler.x, euler.y, euler.z };
    }
    public static Vector3 GetVector(this List<float> l)
    {
        return new Vector3(l[0], l[1], l[2]);
    }
    public static Quaternion GetEuler(this List<float> l)
    {
        return Quaternion.Euler(l[0], l[1], l[2]);
    }
}
