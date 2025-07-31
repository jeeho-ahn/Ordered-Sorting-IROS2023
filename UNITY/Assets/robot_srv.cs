using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;

public class robot_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/manage_robot";
    private manageRobot manager;
    // Start is called before the first frame update
    void Start()
    {
        ros = gameObject.GetComponent<ROSConnection>();
        ros.ImplementService<mobileRobotRequest, mobileRobotResponse>(rosSrvName, handleSrvReq);
        //link manageRobot obj
        manager = GameObject.Find("overseer").GetComponent<manageRobot>();
    }

    mobileRobotResponse handleSrvReq(mobileRobotRequest req)
    {
        if (manager == null)
        {
            UnityEngine.Debug.LogError("manager object is missing");
            return new mobileRobotResponse();
        }
        return manager.handleSrvReq(req);
    }
}
