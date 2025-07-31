using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;


public class ros_topic : MonoBehaviour
{
    // Start is called before the first frame update
    public string topic_name = "/unity/robot_event";
    private overseer overseerObj;
    private ROSConnection ros;
    public string objType = "robot";
    private int my_id;

    void Start()
    {
        //ros = GameObject.Find("ROSconnector").GetComponent<ROSConnection>();
        ros = GameObject.Find("ROSconnector").gameObject.GetComponent<ROSConnection>();
        ros.RegisterPublisher<RobotEventMsg>(topic_name);
        //overseerObj = gameObject.GetComponent<overseer>();
        if(objType == "robot")
            my_id = gameObject.GetComponent<metadata>().get_id();
    }

    public void pub_event(string event_detail, string target_name = "", int robot_id = -1)
    {
        RobotEventMsg tempMsg = new RobotEventMsg();
        tempMsg.event_detail.data = event_detail;
        tempMsg.target_name.data = target_name;
        if (objType == "robot")
            tempMsg.robot_id.data = my_id;
        else
            tempMsg.robot_id.data = robot_id;

        ros.Publish(topic_name, tempMsg);
    }
}
