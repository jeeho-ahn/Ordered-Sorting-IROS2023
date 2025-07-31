using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.AI;
using UnityEngine.UI;

public class manageRobot : MonoBehaviour
{
    private overseer overseerObj;
    private manageScene sceneManager;
    //public string rosSrvName = "/unity/manage_robot";
    //private ROSConnection ros;
    public InputField robot_scale_field;
    [SerializeField] float robot_scale;

    //show path
    public Toggle show_path_toggle;
    //public bool show_path;

    //light toggle
    public Toggle light_toggle;

    private List<robotEvent> _event_queue;


    private IEnumerator call_overseer_event_on_next_frame(GameObject obj, string event_detail = "obj_lift")
    {
        yield return null;
        if (event_detail == "obj_lift")
        {
            //overseer event
            overseerObj.object_lift_event(obj);
        }        
    }

    public bool get_show_path()
    {
        if (show_path_toggle)
            return show_path_toggle.isOn;
        else
            return false;
    }

    public bool get_light_toggle()
    {
        if (light_toggle)        
            return light_toggle.isOn;
        
        else
            return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        overseerObj = gameObject.GetComponent<overseer>();
        sceneManager = gameObject.GetComponent<manageScene>();
        //rosSrvName = "/unity/manage_robot";
        //ros = GameObject.Find("ROSconnector").GetComponent<ROSConnection>();
        //ros = gameObject.GetComponent<ROSConnection>();
        //ros.ImplementService<mobileRobotRequest>(rosSrvName, handleSrvReq);
        //ros.ImplementService<mobileRobotRequest, mobileRobotResponse>(rosSrvName, handleSrvReq);
        //robot_scale = 1.0f;
        //robot_scale_field.text = init_scale.ToString();
        _event_queue = new List<robotEvent>();
    }
    

    /*
    // Update is called once per frame
    void Update()
    {
        if(_event_queue.Count>0)
        {
            foreach(var e in _event_queue)
            {
                var robot = find_robot(e.id);
                robot.GetComponent<ros_srv>().callEvent(e);
            }
        }
        _event_queue.Clear();
    }
    */

    public void scale_robots(float scale_in)
    {
        var rlist = overseerObj.robots_list;
        foreach(var r in rlist)
        {
            r.GetComponent<robot_scaler>().apply_scale(scale_in);
        }
    }

    GameObject find_robot(int robot_id)
    {
        //find robot with input id
        var robot = overseerObj.getRobotObject(robot_id);
        if (robot == null)
        {
            UnityEngine.Debug.LogWarning("robot id not found");
        }
        return robot;
    }

    GameObject find_station_for_robot(GameObject robot)
    {
        //get object type
        var objType = robot.GetComponent<manipulation>().get_picked_obj_meta().type;
        //find station
        return sceneManager.get_drop_station(objType);
    }

    public int task_assign_handler(int robot_id, string target_name, string task_name)
    {
        /*
        //find robot with input id
        var robot = overseerObj.getRobotObject(robot_id);
        if (robot == null)
        {
            UnityEngine.Debug.LogWarning("robot id not found");
            return -1;
        }
        */
        //find (or check) robot
        var robot = find_robot(robot_id);
        if(robot == null) { return -1; };

        robot.GetComponent<metadata>().set_task(task_name);

        if (task_name == "move")
        {
            robot.GetComponent<metadata>().set_eventcall(true);
            robot.GetComponent<metadata>().set_avoidance_priority(true);
            move_robot(robot, target_name);
        }
        else if (task_name == "pre_grasp")
        {
            robot.GetComponent<metadata>().set_avoidance_priority(1);
            manip(robot, target_name);
        }
        else if (task_name == "grasp")
        {
            grasp(robot, target_name);
        }
        else if (task_name == "lift")
        {
            lift(robot_id);
        }
        else if (task_name == "move_st" || task_name == "move_buffer") //ask station and get target pos //move to station
        {
            //find targeting drop station
            robot.GetComponent<metadata>().set_avoidance_priority();
            var station = find_station_for_robot(robot);
            Vector3 tPos;
            if (task_name == "move_st")
                tPos = station.GetComponent<station_control>().get_target_pos(robot);
            else
            { 
                var obj = robot.GetComponent<manipulation>().get_picked_object();
                var obj_size = obj.transform.localScale.x;
                int buffInd;
                //assume buffInd is not negative
                tPos = station.GetComponent<station_control>().get_buffer_pos(out buffInd);
                
                //reserve buffer
                robot.GetComponent<reserve_buffer>().reserve_pos(tPos, obj.transform.localScale);
                station.GetComponent<station_control>().occupy_buffer(obj.GetComponent<object_metadata>().name, buffInd);

                //doesn't seem to occupy the space immediately within a frame
                NavMeshHit _hit;
                //var res = NavMesh.SamplePosition(ros_conversions.ros2unity(tPos) + new Vector3(obj_size/2 + 0.1f,0,0), out _hit, 0.5f, NavMesh.AllAreas);
                var res = NavMesh.SamplePosition(ros_conversions.ros2unity(tPos) + new Vector3(0.4f,0,0), out _hit, 0.5f, NavMesh.AllAreas);
                //var res = NavMesh.SamplePosition(ros_conversions.ros2unity(tPos), out _hit, 0.5f, NavMesh.AllAreas);
                //assume it always works
                tPos = ros_conversions.unity2ros(_hit.position); //todo: handle failed here
            }

            move_robot(robot, tPos);
        }

        else if (task_name == "wait_app") //wait for approval
        {
            //set lowest avoidance
            robot.GetComponent<metadata>().set_avoidance_priority(true);
            //find targeting drop station
            var station = find_station_for_robot(robot);
            //ask for approval
            station.GetComponent<station_control>().get_approval(robot);
        }
        else if (task_name == "move_drop")
        {
            robot.GetComponent<metadata>().set_avoidance_priority();
            //move_robot(robot, target_name, false);
            //find targeting drop station
            var station = find_station_for_robot(robot);
            var tPos = station.GetComponent<station_control>().get_target_pos(robot);
            //todo: verify it is approved and the station is expection this robot

            move_robot(robot, tPos);
        }
        else if (task_name == "pre_drop" || task_name == "pre_drop_buffer")
        {
            //turn off obstacle avoidance
            robot.GetComponent<metadata>().set_avoidance_priority(1);
            if(task_name == "pre_drop")
                manip(robot, target_name, true);//todo: turn off robot's avoidance
            else
            {
                //get reserved pos
                var reserved_pos = robot.GetComponent<reserve_buffer>().get_reserved_pos();
                //remove reservation mesh
                robot.GetComponent<reserve_buffer>().free_reservation();
                robot.GetComponent<manipulation>().approach_drop(ros_conversions.ros2unity(reserved_pos),
                                                            robot.GetComponent<manipulation>().get_picked_object());
            }
        }
        else if (task_name == "drop" || task_name == "drop_buffer")
        {
            if (task_name == "drop")
                drop(robot);
            else
                drop(robot, true,false);
            //set least priority
            robot.GetComponent<metadata>().avoidance_on();
            robot.GetComponent<metadata>().set_avoidance_priority(true);
        }
        else if(task_name == "home")
        {
            robot.GetComponent<metadata>().set_avoidance_priority(true);
            robot.GetComponent<metadata>().set_eventcall(false);
            move_robot(robot);
        }
        else
        {
            UnityEngine.Debug.LogWarning("invalid task name: " + task_name);
        }

        return 0;
    }

    //move to home
    public int move_robot(GameObject robot)
    {
        var home = robot.GetComponent<metadata>().get_home_pos();
        //UnityEngine.Debug.Log(home);
        return move_robot(robot, home);
    }

    public int move_robot(int id, string target_name, bool is_object = true)
    {
        //find (or check) robot
        var robot = find_robot(id);
        if (robot == null) { return -1; };

        return move_robot(robot, target_name, is_object);
    }
    public int move_robot(GameObject robot, string target_name, bool is_object = true)
    {
       Vector3 t_pos_u = new Vector3();

        GameObject obj = null;
        if (is_object)
        {
            //get object pose
            obj = gameObject.GetComponent<manageScene>().find_obj(target_name);
            if (obj == null)
                UnityEngine.Debug.LogWarning("object not found");
            //var obj_pos_u = obj.transform.position;
            t_pos_u = obj.transform.position;

            //set obstacle avoidance
            //robot.GetComponent<metadata>().set_avoidance_priority(obj.GetComponent<object_metadata>().priority);
            //robot.GetComponent<metadata>().set_avoidance_priority();
        }
        
        else //drop position target
        {
            //test drop pos
            //t_pos_u = ros_conversions.ros2unity(new Vector3(-1f, -1f, 0));
            //type of currently holding object
            obj = robot.GetComponent<manipulation>().get_picked_object();
            //get drop station gameobject
            var ds = sceneManager.get_drop_station(obj.GetComponent<object_metadata>().type);
            t_pos_u = ds.transform.position;
        }

        var plan_res = robot.GetComponent<navigation>().planToPoint(t_pos_u, false, true); // in Unity coord

        if(plan_res == null && is_object)
        {
            //shoot plan failed
            //overseerObj.planFailedEvent(robot, target_name);
            //publish topic from the robot object
            //overseerObj.stop_all_robots();
            //robot.GetComponent<ros_topic>().pub_event("failed", target_name);
            //var obj = robot.GetComponent<manipulation>().get_picked_object();
            //overseerObj.planFailedEvent(robot, target_name);

            //push to hold list
            overseerObj.held_pairs.Add(new robot_target_pair(robot, obj));
            UnityEngine.Debug.Log("robot held: " + robot.GetComponent<metadata>().get_id());
        }

        //todo: handle plan err
        return 0;
    }

    public int move_robot(GameObject robot, Vector3 t_pos, bool is_ros = true)
    {
        robot.GetComponent<navigation>().planToPoint(t_pos, is_ros, true); 

        //todo: handle plan err
        return 0;
    }

    public int manip(int robot_id, string obj_name, bool to_drop = false)
    {
        //find (or check) robot
        var robot = find_robot(robot_id);
        if (robot == null) { return -1; };

        return manip(robot, obj_name, to_drop);
    }

    public int manip(GameObject robot, string obj_name, bool to_drop = false)
    {
        //stop mobile
        robot.GetComponent<navigation>().stop();
        if (to_drop)
        {
            //test pre-drop
            var obj = robot.GetComponent<manipulation>().get_picked_object();
            //drop station
            var ds = sceneManager.get_drop_station(obj.GetComponent<object_metadata>().type);
            //currently presenting objects at the station
            var objs = ds.GetComponent<drop_station_metadata>().get_objs();
            //calculate dropping height
            float drop_h = ds.GetComponent<drop_station_metadata>().get_drop_offset();
            foreach(var it in objs)
            {
                //var size = sceneManager.get_pri_size()[it.GetComponent<object_metadata>().priority];
                float t_height = it.GetComponent<object_metadata>().get_height();
                drop_h += t_height;
            }
            //unity h : y axis
            //Vector3 drop_vec = new Vector3(0, drop_h + sceneManager.get_pri_size()[obj.GetComponent<object_metadata>().priority], 0);
            Vector3 drop_vec = new Vector3(0, drop_h + obj.GetComponent<object_metadata>().get_height()/2, 0);
            robot.GetComponent<manipulation>().approach_drop(ds.transform.position+drop_vec, obj);
        }
        else
        {
            //find obj object
            var obj = sceneManager.find_obj(obj_name);
            
            //pick
            robot.GetComponent<manipulation>().approach(obj);
        }
        
        //todo: handle error
        return 0;
    }

    public int grasp(int robot_id, string obj_name)
    {
        //find (or check) robot
        var robot = find_robot(robot_id);
        if (robot == null) { return -1; };

        return grasp(robot, obj_name);
    }

    public int grasp(GameObject robot, string obj_name, bool remove_obj_collision = true)
    {
        //find obj object
        var obj = sceneManager.find_obj(obj_name);
        //disable nav obstacle
        obj.GetComponent<NavMeshObstacle>().enabled = false;

        //lock tf between end-eff. and object
        var fjoint = obj.AddComponent<FixedJoint>();
        fjoint.connectedBody = robot.GetComponent<manipulation>().toolObj.GetComponent<Rigidbody>();
        robot.GetComponent<manipulation>().set_picked_object(obj);

        if(remove_obj_collision)
        {
            obj.GetComponent<MeshCollider>().enabled = false;
        }

        //trigger stop event right away
        //overseerObj.robotStopEvent(robot.GetComponent<metadata>().get_id(), "arm");
        //_event_queue.Add(new robotEvent(robot.GetComponent<metadata>().get_id(), "arm"));
        //robot.GetComponent<ros_topic>().pub_event("arm");
        robot.GetComponent<manipulation>().triggerOverseer();

        //todo: add error handle while attaching fixed joint
        return 0;
    }

    public int lift(int robot_id)
    {
        //find (or check) robot
        var robot = find_robot(robot_id);
        if (robot == null) { return -1; };

        return lift(robot);
    }

    public int lift(GameObject robot)
    {
        //to home pose
        robot.GetComponent<manipulation>().to_home_pose();
        //if it has an object attached on tool, call overseer event for lifting on next frame
        var obj = robot.GetComponent<manipulation>().get_picked_object();
        if (obj != null)
        {
            overseerObj.object_lift_event(obj);
        }

        //todo: handle possible error here
        return 0;
    }

    public int drop(int robot_id, bool reset_nav_obst = false)
    {
        //find (or check) robot
        var robot = find_robot(robot_id);
        if (robot == null) { return -1; };

        return drop(robot, reset_nav_obst);
    }

    public int drop(GameObject robot, bool reset_nav_obst = false, bool add_to_dropped = true)
    {
        var manipComp = robot.GetComponent<manipulation>();
        var obj = manipComp.get_picked_object();

        //enable nav mesh obstacle back
        if(reset_nav_obst)
            obj.GetComponent<NavMeshObstacle>().enabled = true;

        // restore obj mesh collider
        obj.GetComponent<MeshCollider>().enabled = true;

        //remove fixed joint
        Destroy(obj.GetComponent<FixedJoint>());
        robot.GetComponent<manipulation>().set_drop_object();
        //unity bug: doesn't drop on UI
        //obj.GetComponent<Rigidbody>().useGravity = false;
        //obj.GetComponent<Rigidbody>().useGravity = true;

        if (add_to_dropped)
        {
            //add to dropped objs
            var ds = sceneManager.get_drop_station(obj.GetComponent<object_metadata>().type);
            //ds.GetComponent<drop_station_metadata>().add_obj_to_dropped_list(obj);
            ds.GetComponent<station_control>().drop_item(obj,false);
            obj.GetComponent<MeshCollider>().enabled = false;
            obj.GetComponent<Rigidbody>().useGravity = false;
            //it sometimes slips out of the pile
            obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }

        //trigger stop event right away
        //overseerObj.robotStopEvent(robot.GetComponent<metadata>().get_id(), "arm");
        //_event_queue.Add(new robotEvent(robot.GetComponent<metadata>().get_id(), "arm"));
        //robot.GetComponent<ros_topic>().pub_event("arm");
        manipComp.triggerOverseer(obj.GetComponent<object_metadata>().objName);

        //todo: handle possible error here
        return 0;
    }

    public mobileRobotResponse handleSrvReq(mobileRobotRequest req)
    {
        mobileRobotResponse out_res = new mobileRobotResponse();
        if(req.cmd.data == "add" || req.cmd.data == "rm" || req.cmd.data == "mv")
        {
            //add robots in the list
            for(int i=0; i<req.robots.Length; i++)
            {
                var piv = req.robots[i];
                pose2d temp_pose = new pose2d((float)piv.x, (float)piv.y, (float)piv.th);
                if (req.cmd.data == "add")
                    overseerObj.add_robot(temp_pose, piv.id);
                else if (req.cmd.data == "rm")
                    overseerObj.remove_robot(piv.id);
                else if (req.cmd.data == "mv")
                    overseerObj.path_plan(piv.id, temp_pose);
            }

            //todo: add 'relocation' functionality
            //todo: handle exceptions for failures

            out_res.result.data = 1;
        }

        else
        {
            //wrong cmd
            out_res.result.data = -1;
        }
        return out_res;
    }

    public void reset_manager()
    {
        //nothing to do
    }
}

