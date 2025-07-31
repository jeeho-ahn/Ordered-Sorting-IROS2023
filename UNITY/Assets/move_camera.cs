using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move_camera : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("c"))
        {
            gameObject.transform.position = new Vector3(-10.58408f, 7.677119f, - 3.743407f);
            var rot = new Quaternion();
            rot.eulerAngles = new Vector3(35.174f, 429.772f, 0);
            gameObject.transform.rotation = rot;
        }
    }
}
