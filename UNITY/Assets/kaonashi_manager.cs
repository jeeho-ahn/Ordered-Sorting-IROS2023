using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kaonashi_manager : MonoBehaviour
{
    public GameObject kaonashi;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            //toggle kaonashi
            if (kaonashi.activeSelf == true)
                kaonashi.SetActive(false);
            else
                kaonashi.SetActive(true);
        }
    }
}
