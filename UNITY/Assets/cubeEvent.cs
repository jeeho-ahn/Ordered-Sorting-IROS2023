using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeEvent : MonoBehaviour
{
    public string eventMsg = "";
    public manipulation man;
    // Start is called before the first frame update
    void Start()
    {
        man = GetComponentInParent<manipulation>();
    }

    public void eventFunc()
    {
        man.cubeEvent(eventMsg);
    }
}
