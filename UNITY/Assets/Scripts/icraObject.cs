using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;

[Serializable]
public class icraObj
{
    private string _name;
    Vector3 _pos_ros;
    Quaternion _rot_ros;
    string _ty;
    int _pri;

    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public Vector3 PosRos
    {
        get
        {
            return _pos_ros;
        }
        set
        {
            _pos_ros = value;
        }
    }

    public Quaternion RotationRos
    {
        get
        {
            return _rot_ros;
        }
        set
        {
            _rot_ros = value;
        }
    }

    public string Type
    {
        get
        {
            return _ty;
        }
        set
        {
            _ty = value;
        }
    }

    public int Priority
    {
        get
        {
            return _pri;
        }
        set
        {
            _pri = value;
        }
    }

    public icraObj()
    {
        _name = "";
        _pos_ros = new Vector3();
        _rot_ros = Quaternion.identity;
        _ty = "";
        _pri = -1;
    }

    public icraObj(string name_in, Vector3 pos_in,
        Quaternion rot_in, string type_in, int priority_in)
    {
        _name = name_in;
        _pos_ros = pos_in;
        _rot_ros = rot_in;
        _ty = type_in;
        _pri = priority_in;
    }

    public icraObj(IcraObjectMsg obj_msg)
    {
        _name = obj_msg.name;
        //_pos_ros = ros_conversions.geoMsgP2UnityVec3(obj_msg.pose.position);
        //_rot_ros = ros_conversions.geoMsgQ2UnityQ(obj_msg.pose.orientation);
        _pos_ros = new Vector3((float)obj_msg.x, (float)obj_msg.y, 0); //z will be overrided
        _rot_ros = Quaternion.identity;
        _ty = obj_msg.type;
        _pri = obj_msg.priority;
    }               
}
