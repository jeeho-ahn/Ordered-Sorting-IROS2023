using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;

public class actions_alloc_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/actions_alloc_srv";
    private manageRobot manager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
