using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class ros_service_bridge : MonoBehaviour
{
    public ROSConnection ros;
    public string rosSrvName;
    // Start is called before the first frame update
    void Start()
    {
        ros = GameObject.Find("ROSconnector").GetComponent<ROSConnection>();
        rosSrvName = "";
    }
}
