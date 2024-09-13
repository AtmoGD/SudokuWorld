using System.Runtime.InteropServices;
using UnityEngine;

public class Vibration : MonoBehaviour
{
    public static Vibration Manager { get; private set; }

    [DllImport("__Internal")]
    private static extern void Vibrate(int ms);

    public void PlopVibration()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        Vibrate(1000);
#else
        Debug.Log("Simulate vibration");
#endif
    }
}