using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quit_program : MonoBehaviour
{
    public void terminate_program()
    {
        UnityEngine.Debug.Log("Terminate");
        #if UNITY_STANDALONE
                Application.Quit();
        #endif
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
