using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;

public class unitTask_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/unit_task_alloc";
    private manageRobot manager;
    // Start is called before the first frame update
    void Start()
    {
        ros = gameObject.GetComponent<ROSConnection>();
        ros.ImplementService<unitTaskAllocRequest, unitTaskAllocResponse>(rosSrvName, handleSrvReq);
        //link manageRobot obj
        manager = GameObject.Find("overseer").GetComponent<manageRobot>();
    }

    unitTaskAllocResponse handleSrvReq(unitTaskAllocRequest req)
    {
        unitTaskAllocResponse out_res = new unitTaskAllocResponse();

        foreach (var it in req.task)
        {
            // move/pick/place
            var task_name = it.task_name.data;
            var robot_id = it.robot_id.data;
            var target_name = it.target.name.data;

            UnityEngine.Debug.Log("Task Received: " + task_name + " id: " + robot_id + " target: " + target_name);

            manager.task_assign_handler(robot_id, target_name, task_name);            
        }

        return out_res;
    }
}
