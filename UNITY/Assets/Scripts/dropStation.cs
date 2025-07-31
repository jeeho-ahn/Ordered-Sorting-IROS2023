using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;

[Serializable]
public class dropStation
{
    private string _type;
    private Vector3 _pos_ros;
    private List<GameObject> _dropped_items;
    private float _drop_offset;
    

    public string Type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
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

    public float DropOffset
    {
        get
        {
            return _drop_offset;
        }
        set
        {
            _drop_offset = value;
        }
    }

    public void drop_item(GameObject gobj)
    {
        _dropped_items.Add(gobj);
    }



    public List<GameObject> get_items()
    {
        return _dropped_items;
    }

    public int get_lowest_priority()
    {
        if(_dropped_items.Count>0)
        {
            int piv = 0;
            foreach(var obj in _dropped_items)
            {
                int temp_id = obj.GetComponent<object_metadata>().priority;
                if (temp_id > piv)
                    piv = temp_id;
            }
            return piv;
        }
        else
        {
            //dropped items list is empty
            return -1;
        }
    }

    public dropStation()
    {
        _type = "";
        _pos_ros = new Vector3();
        _dropped_items = new List<GameObject>();
        _drop_offset = 0f;
        //_wait_queue = new List<GameObject>();
    }

    public dropStation(string type_in, Vector3 pos_in, bool is_ros_coord = true)
    {
        _type = type_in;
        _dropped_items = new List<GameObject>();
        _drop_offset = 0f;
        //_wait_queue = new List<GameObject>();
        if (is_ros_coord)
        {
            _pos_ros = pos_in;
        }
        else
        {
            _pos_ros = ros_conversions.unity2ros(pos_in);
        }
    }

    public dropStation(IcraObjectMsg objMsg)
    {
        _type = objMsg.type;
        //_wait_queue = new List<GameObject>();
        _dropped_items = new List<GameObject>();
        _drop_offset = 0f;
        _pos_ros = new Vector3((float)objMsg.x, (float)objMsg.y, 0);
    }
}
