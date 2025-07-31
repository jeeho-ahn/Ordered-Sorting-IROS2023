using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;

public class robot_pose_pub : MonoBehaviour
{
    // Start is called before the first frame update
    public string refrence_frame = "map";
    public string topic_name = "/unity/robot_poses";
    private overseer overseerObj;
    private ROSConnection ros;

    //publish frequency
    public int pub_freq = 2;
    int pub_per;
    DateTime time_lastpub;

    void Start()
    {
        //ros = GameObject.Find("ROSconnector").GetComponent<ROSConnection>();
        ros = gameObject.GetComponent<ROSConnection>();
        ros.RegisterPublisher<MobileRobotPoseArrayMsg>(topic_name);
        //overseerObj = gameObject.GetComponent<overseer>();
        overseerObj = GameObject.Find("overseer").GetComponent<overseer>();

        pub_per = 1 * 1000 / pub_freq;
        time_lastpub = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        if (DateTime.Now.Subtract(time_lastpub).TotalMilliseconds > pub_per)
        {
            var robot_poses = overseerObj.get_robot_poses();
            if (robot_poses.Count > 0)
            {
                //faild to find a way to publish tf directly
                //toss the list to ros, then publish tf from ros end
                MobileRobotPoseArrayMsg out_arr = new MobileRobotPoseArrayMsg();
                out_arr.poses = new MobileRobotPoseMsg[robot_poses.Count];

                for (int i = 0; i < robot_poses.Count; i++)
                {
                    var piv = robot_poses[i];
                    out_arr.poses[i] = new MobileRobotPoseMsg(piv.id, piv.pose.x, piv.pose.y, piv.pose.th);
                }

                //publish
                ros.Publish(topic_name, out_arr);
                time_lastpub = DateTime.Now;
            }
        }
    }
}
