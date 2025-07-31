using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_coroutine : MonoBehaviour
{
    private int counter = 0;

    private void Start()
    {
        //counter = counter + 1;

        //Debug.Log("Update: " + counter);
        StartCoroutine(WaitForEndOfFrameCoroutine(counter));
        StartCoroutine(WaitForEndOfFrameCoroutine(counter));
        StartCoroutine(WaitForEndOfFrameCoroutine(counter));
    }

    private void Update()
    {
        counter = counter + 1;
        Debug.Log("Update");
    }

    private IEnumerator WaitForEndOfFrameCoroutine(int counter)
    {
        Debug.Log("Before end of frame: " + counter);
        yield return null; ;
        Debug.Log("After end of frame: " + counter);
    }
}
