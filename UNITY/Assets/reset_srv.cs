using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;

public class reset_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/reset_scene";
    private overseer oseer;

    // Start is called before the first frame update
    void Start()
    {
        ros = gameObject.GetComponent<ROSConnection>();
        ros.ImplementService<TriggerRequest, TriggerResponse>(rosSrvName, handleSrvReq);
        //link manageRobot obj
        oseer = GameObject.Find("overseer").GetComponent<overseer>();
    }

    TriggerResponse handleSrvReq(TriggerRequest req)
    {
        TriggerResponse out_res = new TriggerResponse();

        //trigger reset to overseer
        oseer.reset_all();

        out_res.message = "reset";
        return out_res;
    }
}
