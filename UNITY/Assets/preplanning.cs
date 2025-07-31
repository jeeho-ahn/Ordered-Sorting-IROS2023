using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class preplanning : MonoBehaviour
{
    public GameObject robot;
    public GameObject object_prefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator WaitForNextFrameCoroutine()
    {
        Debug.Log("Before end of frame:");
        yield return null; ;
        //yield return new WaitForEndOfFrame();
        Debug.Log("After end of frame");

        NavMeshHit _hit;
        var tpos = new Vector3(1f, 0.0f, 1f);
        //find path to object
        var res2 = NavMesh.SamplePosition(tpos, out _hit, 0.1f, NavMesh.AllAreas);
        UnityEngine.Debug.Log(res2);
    }

    IEnumerator wait_for_one_frame()
    {
        UnityEngine.Debug.Log("wait");
        //yield return null;
        yield return new WaitForSeconds(2);
        UnityEngine.Debug.Log("done");
    }

    public void plan_test()
    {
        /*
        var tpos = new Vector3(3f, 0f, 3f);
        NavMeshHit _hit;
        var res = NavMesh.SamplePosition(tpos, out _hit, 0.1f, NavMesh.AllAreas);
        UnityEngine.Debug.Log(res);
        */
        for (int i=0; i<5; i++)
        {
            UnityEngine.Debug.Log("here1");
            StartCoroutine(wait_for_one_frame());
            UnityEngine.Debug.Log("here2");
        }
    }

    public void test_preplan1()
    {
        var tpos = new Vector3(1f, 0.0f, 1f);

        NavMeshHit _hit;
        var res = NavMesh.SamplePosition(tpos, out _hit, 0.1f, NavMesh.AllAreas);
        UnityEngine.Debug.Log(res);
        
        //place object to empty space
        var temp = Instantiate(object_prefab, tpos+new Vector3(0,0.51f,0), Quaternion.identity);

        //wait
        StartCoroutine(WaitForNextFrameCoroutine());

        
    }
    void test_preplan2()
    {
        //remove existing obj

        //find path to object space


    }
}
