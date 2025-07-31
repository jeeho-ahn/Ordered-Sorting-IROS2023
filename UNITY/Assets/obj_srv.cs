using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;

public class obj_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/manage_object";
    private manageScene manager;

    // Start is called before the first frame update
    void Start()
    {
        ros = gameObject.GetComponent<ROSConnection>();
        ros.ImplementService<object_srvRequest, object_srvResponse>(rosSrvName, handleSrvReq);
        //link manageRobot obj
        manager = GameObject.Find("overseer").GetComponent<manageScene>();
    }

    object_srvResponse handleSrvReq(object_srvRequest req)
    {
        object_srvResponse out_res = new object_srvResponse();
        if (req.cmd.data == "add" || req.cmd.data == "rm" || req.cmd.data == "mv")
        {
            //add objects in the list
            for (int i = 0; i < req.objects.Length; i++)
            {
                if (req.cmd.data == "add")
                {
                    //convert to unity icraObject
                    manager.add_obj(new icraObj(req.objects[i]));
                }
                else if(req.cmd.data == "rm")
                {
                    manager.remove_object(new icraObj(req.objects[i]));
                }
                /* todo: fill below
                else if (req.cmd.data == "rm")
                    overseerObj.remove_robot(piv.id);
                else if (req.cmd.data == "mv")
                    overseerObj.path_plan(piv.id, temp_pose);
                */
            }
        }

        else if (req.cmd.data == "init")
        {
            UnityEngine.Debug.Log("Initialize Objects");
            //reset and init
            manager.reset_obj();
            //add objects
            //add objects in the list
            for (int i = 0; i < req.objects.Length; i++)
            {
                manager.add_obj(new icraObj(req.objects[i]));
            }
        }

        else if(req.cmd.data == "init_drop")
        {
            UnityEngine.Debug.Log("Initialize Drop Stations");
            //construct drop stations list
            List<dropStation> out_list = new List<dropStation>();
            for (int i = 0; i < req.objects.Length; i++)
            {
                out_list.Add(new dropStation(req.objects[i]));
            }

            //init new stations
            manager.set_drop_stations(out_list);
        }

        return out_res;
    }
}
