using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dupJackal : MonoBehaviour
{
    public GameObject robot_prefab;
    // Start is called before the first frame update
    void Start()
    {
        var robj = Instantiate(robot_prefab, new Vector3(3, 0, 3), Quaternion.identity);
        robj.GetComponent<metadata>().set_id(2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
