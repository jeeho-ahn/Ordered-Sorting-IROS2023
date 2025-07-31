using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class gridmap_ros_bridge : MonoBehaviour
{
    private ROSConnection ros;
    private gen_gridmap gg;
    [SerializeField] string srvName;
    // Start is called before the first frame update
    void Start()
    {
        gg = gameObject.GetComponent<gen_gridmap>();
        ros = GameObject.Find("ROSconnector").GetComponent<ROSConnection>();
        srvName = "/unity/gridmapReq";
        ros.ImplementService<TriggerRequest, TriggerResponse>(srvName, handleSrvReq);
    }

    private TriggerResponse handleSrvReq(TriggerRequest trig)
    {
        gg.generate_and_pub();
        return new TriggerResponse();
    }
}
