using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public static class ros_conversions
{
    // Start is called before the first frame update
    public static Vector3 geoMsgP2UnityVec3(RosMessageTypes.Geometry.PointMsg pos_ros)
    {
        return new Vector3((float)pos_ros.x, (float)pos_ros.y, (float)pos_ros.z);
    }

    public static Quaternion geoMsgQ2UnityQ(RosMessageTypes.Geometry.QuaternionMsg q_ros)
    {
        return new Quaternion((float)q_ros.x, (float)q_ros.y, (float)q_ros.z, (float)q_ros.w);
    }
    public static Vector3 unity2ros(this Vector3 unityVec)
    {
        return new Vector3(unityVec.z, -unityVec.x, unityVec.y);
    }
    public static Vector3 ros2unity(this Vector3 vector3)
    {
        return new Vector3(-vector3.y, vector3.z, vector3.x);
    }
    public static Quaternion ros2unity(this Quaternion quaternion)
    {
        return new Quaternion(quaternion.y, -quaternion.z, -quaternion.x, quaternion.w);
    }
    public static Quaternion unity2ros(this Quaternion quaternion)
    {
        return new Quaternion(-quaternion.z, quaternion.x, -quaternion.y, quaternion.w);
    }
}
