using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.AI;

public class preplan_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/preplan_request";
    private manageScene manager;
    private overseer oseer;
    public GameObject preplan_robot;

    private GameObject temp_robot;
    private List<icraObj> objects_list_arch;

    private bool wait_for_render;

    // Start is called before the first frame update
    void Start()
    {
        ros = gameObject.GetComponent<ROSConnection>();
        ros.ImplementService<object_srvRequest, object_srvResponse>(rosSrvName, handleSrvReq);
        //link manageRobot obj
        manager = GameObject.Find("overseer").GetComponent<manageScene>();
        oseer = GameObject.Find("overseer").GetComponent<overseer>();
        wait_for_render = false;
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    private IEnumerator remove_object_coroutine(string obj_name)
    {
        manager.remove_object(obj_name);
        //Debug.Log("Before end of frame:");
        yield return null;
        wait_for_render = false;
        //yield return new WaitForEndOfFrame();
        //Debug.Log("After end of frame");
    }

    private IEnumerator wait_for_next_frame()
    {
        yield return null;
        wait_for_render = false;
        //UnityEngine.Debug.Log("frame updated");
    }

    object_srvResponse handleSrvReq(object_srvRequest req)
    {
        object_srvResponse out_res = new object_srvResponse();

        if(req.cmd.data == "preplan_robot")
        {
            UnityEngine.Debug.Log("preplan robot req received");
            //var robot = req.objects[0];

            //instantiate robot
            //var pos_u = ros_conversions.ros2unity(new Vector3((float)robot.x, (float)robot.y, 0));
            //temp_robot = Instantiate(preplan_robot, pos_u, Quaternion.identity);

            //pick one of the presenting robots in the scene
            temp_robot = oseer.robots_list[0];

            objects_list_arch = new List<icraObj>();
            //save current objects list for restoration
            foreach(var it in oseer.objectList)
            {
                var objMeta = it.GetComponent<object_metadata>();
                icraObj tempObj = new icraObj();
                tempObj.Name = objMeta.objName;
                tempObj.PosRos = ros_conversions.unity2ros(it.transform.position);
                tempObj.RotationRos = ros_conversions.unity2ros(it.transform.rotation);
                tempObj.Priority = objMeta.priority;
                tempObj.Type = objMeta.type;
                objects_list_arch.Add(tempObj);
            }
        }

        else if(req.cmd.data=="is_ready")
        {
            if(wait_for_render)
            {
                out_res.result.data = -1;
            }
            else
            {
                out_res.result.data = 1;
            }
        }
        
        //receive one object to plan
        else if(req.cmd.data == "preplan")
        {
            if(temp_robot==null)
            {
                UnityEngine.Debug.Log("Preplan robot not initialized");
                out_res.result.data = -2;
                return out_res;
            }
            //UnityEngine.Debug.Log("preplan received");

            var obj = req.objects[0];
            //objects are already instantiated

            var tpos = new Vector3((float)obj.x,(float)obj.y,0);
            //find path to object
            var res = temp_robot.GetComponent<navigation>().planToPoint(tpos, true, false);

            if(res != null)
            {
                out_res.result.data = 1;
                wait_for_render = true;
                //set to remove the object
                manager.remove_object(obj.name);
                StartCoroutine(wait_for_next_frame());
                //while(wait_for_render) //wait until new frame is rendered
                //{
                //todo: figure out a way to wait properly
                //}
            }
            else
            {
                //plan failed
                out_res.result.data = -1;
            }
        }

        else if(req.cmd.data == "restore_obj")
        {
            //remove preplan robot
            //Destroy(temp_robot);
            manager.remove_objects();
            foreach (var it in objects_list_arch)
            {
                //todo: make sure the object is not in the list already                
                manager.add_obj(it,false);
            }
        }

        return out_res;
    }
}
