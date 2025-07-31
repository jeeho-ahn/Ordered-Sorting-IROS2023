using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showPos : MonoBehaviour
{
    public float x, y, z;
    // Start is called before the first frame update
    void Start()
    {
        update_pose();
    }

    // Update is called once per frame
    void Update()
    {
        update_pose();
    }

    void update_pose()
    {
        var tr = transform.position;
        x = tr.x;
        y = tr.y;
        z = tr.z;
    }
}
