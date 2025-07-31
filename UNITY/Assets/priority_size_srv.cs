using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;

public class priority_size_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/priority_size";
    private manageScene manager;

    // Start is called before the first frame update
    void Start()
    {
        ros = gameObject.GetComponent<ROSConnection>();
        ros.ImplementService<priSizeRegRequest, priSizeRegResponse>(rosSrvName, handleSrvReq);
        //link manageRobot obj
        manager = GameObject.Find("overseer").GetComponent<manageScene>();
    }

    priSizeRegResponse handleSrvReq(priSizeRegRequest req)
    {
        priSizeRegResponse out_res = new priSizeRegResponse();

        Dictionary<int, float> temp_map = new Dictionary<int, float>();
        foreach(var it in req.pairs)
        {
            temp_map.Add(it.priority.data, (float)it.size.data);
        }

        manager.set_pri_size(temp_map);

        return out_res;
    }
}
