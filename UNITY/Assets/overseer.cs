using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public struct mobileRobotPose
{
    public int id;
    public pose2d pose;
}

public class robot_target_pair
{
    public GameObject robot;
    public GameObject target;

    public robot_target_pair()
    {
        robot = null;
        target = null;
    }

    public robot_target_pair(GameObject robot_in, GameObject target_in)
    {
        robot = robot_in;
        target = target_in;
    }
}

public class robotEvent
{
    public int id;
    public string target_name;
    public string event_detail;

    public robotEvent()
    {
        id = -1;
        event_detail = "";
        target_name = "";
    }

    public robotEvent(robotEvent copythis)
    {
        id = copythis.id;
        event_detail = copythis.event_detail;
        target_name = copythis.target_name;
    }

    public robotEvent(int id_in, string e_in, string target_name_in = "")
    {
        id = id_in;
        event_detail = e_in;
        target_name = target_name_in;
    }
}

public class overseer : MonoBehaviour
{
    //robot objects
    public List<GameObject> robots_list;
    //robots waiting for obstacles to be removed
    public List<robot_target_pair> held_pairs;
    //object objects
    public List<GameObject> objectList;
    //drop stations Map
    private Dictionary<string, GameObject> dropStationsMap;
    //stack of finished objects
    private List<GameObject> finished_objects;
    public Button testBtn, planBtn;
    public InputField x_field, y_field;
    public GameObject robot_prefab;

    //private ROSConnection ros;
    private robot_event_srv rosEventSrv;
    private robot_event_pub eventPublisher;
    public float init_scale = 0.8f;
    public InputField robot_scale_field;

    //event service stack
    private List<robotEvent> eventsStack;
    private List<robotEvent> openStack;
    private List<robotEvent> waitStack;
    private robotEvent failedEvent;

    //object lift event triggered
    private bool obj_lift_trig;
    //object drop event triggered
    private bool obj_drop_trig;

    // Start is called before the first frame update
    void Start()
    {
        rosEventSrv = GameObject.Find("ROSconnector").GetComponent<robot_event_srv>();
        eventPublisher = GameObject.Find("ROSconnector").GetComponent<robot_event_pub>();
        robots_list = new List<GameObject>();
        held_pairs = new List<robot_target_pair>();
        finished_objects = new List<GameObject>();
        testBtn.onClick.AddListener(testBtnPressed);
        planBtn.onClick.AddListener(planBtnEvent);
        //init object list
        objectList = new List<GameObject>();
        robot_scale_field.text = init_scale.ToString();
        eventsStack = new List<robotEvent>();
        openStack = new List<robotEvent>();
        waitStack = new List<robotEvent>();
        dropStationsMap = new Dictionary<string, GameObject>();
        failedEvent = null;
        obj_lift_trig = false;
        obj_drop_trig = false;
    }


    bool exists_in_eventStack(robotEvent re)
    {
        foreach(var it in eventsStack)
        {
            if (it.id == re.id)
                return true;
        }
        return false;
    }

    // Update is called once per frame
    /*
    void Update()
    {
        if(waitStack.Count>0)
        {
            for(int i=waitStack.Count-1; i>=0; i--)
            {
                var it = waitStack[i];
                if(!exists_in_eventStack(it))
                {
                    eventsStack.Add(new robotEvent(it));
                    //remove from stack 
                    waitStack.RemoveAt(i);
                }
            }
        }

        //call ROS srv if stack is not empty
        if(eventsStack.Count > 0)
        {
            if (rosEventSrv != null)
            {
                openStack = new List<robotEvent>(eventsStack);
                rosEventSrv.callEvent(eventsStack);                
            }
            eventsStack.Clear();
        }
        else //trigger failed event if there's any triggered
        {
            //when event stack is empty and there's failed event
            if(failedEvent!=null)
            {
                //shoot service
                rosEventSrv.callEvent(failedEvent);
                failedEvent = null;
            }
        }
    }
    */

    private void Update()
    {
        /*
        if (failedEvent != null)
        {
            //shoot service
            rosEventSrv.callEvent(failedEvent);
            failedEvent = null;
        }
        */
        if(obj_drop_trig)
        {
            //do something
            obj_drop_trig = false;
        }

        if (obj_lift_trig)
        {
            if(held_pairs.Count>0)
            {
                List<int> resolved_list = new List<int>();
                foreach (var it in held_pairs)
                {
                    //try planning again. if success, remove from held list
                    //target pos
                    var tpos_u = it.target.transform.position;
                    var res = it.robot.GetComponent<navigation>().planToPoint(tpos_u, false, true);
                    if(res != null) //plan ok
                    {
                        resolved_list.Add(it.robot.GetComponent<metadata>().get_id());
                        UnityEngine.Debug.Log("held robot resolved: " + it.robot.GetComponent<metadata>().get_id());
                    }
                }
                
                //remove resolved robots from held list
                foreach(var it in resolved_list)
                {
                    for(int i=0; i<held_pairs.Count; i++)//held robot
                    {
                        if(held_pairs[i].robot.GetComponent<metadata>().get_id() == it)
                        {
                            held_pairs.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            obj_lift_trig = false;
        }
    }

    public Dictionary<string, GameObject> get_dropStationsMap()
    {
        return dropStationsMap;
    }

    public void set_dropStationsMap(Dictionary<string, GameObject> dMap_in)
    {
        dropStationsMap = dMap_in;
    }

    public void clear_dropStationsMap()
    {
        dropStationsMap.Clear();
    }

    public void add_to_dropStationsMap(string type_in, GameObject ds)
    {
        dropStationsMap.Add(type_in, ds);
    }

    public void object_picked_event(GameObject obj)
    {
        //todo: add object to picked list
    }

    public void object_lift_event(GameObject obj)
    {
        obj_lift_trig = true;
    }

    public void object_finished_event(GameObject obj)
    {
        finished_objects.Add(obj);
        obj_drop_trig = true;
    }


    public int add_robot(pose2d init_pose, int new_id = -1)
    {
        if (new_id == -1)
        {
            new_id = robots_list.Count;
            //todo: check for id duplicate
        }
        //check collision and modify if needed
        NavMeshHit _hit;
        NavMesh.SamplePosition(ros_conversions.ros2unity(new Vector3(init_pose.x,init_pose.y,0)), out _hit, 5, NavMesh.AllAreas);

        if (_hit.hit == true)
        {
            // var robj = Instantiate(robot_prefab, new Vector3(0, 0, 0), Quaternion.identity);
            var robj = Instantiate(robot_prefab, _hit.position, Quaternion.identity);
            robj.GetComponent<metadata>().set_id(new_id);
            //apply scale
            //float scale_in = float.Parse(robot_scale_field.text);
            //robj.transform.localScale = new Vector3(scale_in, scale_in, scale_in);
            //set home pos
            robj.GetComponent<metadata>().set_home_pos(new Vector3(init_pose.x, init_pose.y, 0));
            robots_list.Add(robj);
        }
        else
        {
            UnityEngine.Debug.Log("Not a good position to place a robot");
            return -1;
        }
        return new_id;
    }

    int findRobotListInd(int id_in)
    {
        for (int i = 0; i < robots_list.Count; i++)
        {
            if (robots_list[i].GetComponent<metadata>().get_id() == id_in)
            {
                return i;
            }
        }
        return -1;
    }

    public GameObject getRobotObject(int id_in)
    {
        var rind = findRobotListInd(id_in);
        if(rind == -1)
        {
            UnityEngine.Debug.LogWarning("Robot id not found: " + id_in.ToString());
            return null;
        }
        return robots_list[rind];
    }

    public int remove_robot(int r_id)
    {
        int idx = findRobotListInd(r_id);
        if (idx != -1)
        {
            Destroy(robots_list[idx].gameObject);
            robots_list.RemoveAt(idx);
            return 0;
        }

        else
        {
            //id not found
            UnityEngine.Debug.LogWarning("id to remove not found in the list");
            return -1;
        }
    }

    public NavMeshPath path_plan(int robot_id, pose2d target_pose, bool move_robot=true)
    {
        int idx = findRobotListInd(robot_id);
        if(idx == -1)
        {
            //id not found
            UnityEngine.Debug.LogWarning("id to remove not found in the list");
            return new NavMeshPath();
        }
        var cur_robot = robots_list[idx];
        //Vector3 target_p = target_pose.pos_to_v3();
        //Quaternion target_r = target_pose.rot_to_quat();

        NavMeshPath out_path = 
            cur_robot.GetComponent<navigation>().planToPoint(target_pose.pos_to_v3(), true, move_robot);
        return out_path;
    }

    public void stop_robot(int robot_id)
    {
        int idx = findRobotListInd(robot_id);
        if (idx == -1)
        {
            //id not found
            UnityEngine.Debug.LogWarning("id to remove not found in the list");
            return;
        }
        robots_list[idx].GetComponent<navigation>().stop();
        return;
    }

    public pose2d get_robot_pose(int robot_id)
    {
        int idx = findRobotListInd(robot_id);
        if (idx == -1)
        {
            //id not found
            UnityEngine.Debug.LogWarning("id to remove not found in the list");
            return null;
        }
        return robots_list[idx].GetComponent<metadata>().get_pose2d();
    }

    public List<mobileRobotPose> get_robot_poses()
    {
        List<mobileRobotPose> out_list = new List<mobileRobotPose>();

        foreach (GameObject robot in robots_list)
        {
            mobileRobotPose r_tmp = new mobileRobotPose();
            var mdata = robot.GetComponent<metadata>();
            r_tmp.id = mdata.get_id();
            r_tmp.pose = mdata.get_pose2d();

            out_list.Add(r_tmp);
        }

        return out_list;
    }

    public List<GameObject> get_robot_list()
    {
        return robots_list;
    }

    void planBtnEvent()
    {
        Vector3 pos_in = new Vector3();
        pos_in.x = float.Parse(x_field.text);
        pos_in.y = float.Parse(y_field.text);
        pos_in.z = 0.0f;
        //planToPoint(pos_in, true);
        var last_robot = robots_list[robots_list.Count-1];
        last_robot.GetComponent<navigation>().planToPoint(pos_in);
    }

    void testBtnPressed()
    {
        /*
        //find collision-free position
        NavMeshHit _hit;
        NavMesh.SamplePosition(new Vector3(), out _hit, 5, NavMesh.AllAreas);

        if (_hit.hit == true)
        {
            var robj = Instantiate(robot_prefab, new Vector3(0, 0.8f, 0), Quaternion.identity);
            //var robj = Instantiate(robot_prefab, _hit.position, Quaternion.identity);
            int new_id = robots_list.Count;
            robj.GetComponent<metadata>().set_id(new_id);
            robots_list.Add(robj);
        }
        else
        {
            UnityEngine.Debug.Log("Not a good position to place a robot");
        }
        */
        add_robot(new pose2d(0.2f, 0.2f, 0f));
    }

    public void robotStopEvent(int robot_id, string type, string target_name="")
    {
        /*
         * Unity occasionally misses service call
         * suspectively because multiple call was made within a frame
         * modified to stack, then call once a frame
        if(rosEventSrv != null)
        {
            rosEventSrv.callEvent(robot_id, type); //arm or mobile
        }
        */
        UnityEngine.Debug.Log("eventTriggered: " + robot_id.ToString());
        /*
        //need to check for possible duplicate
        var new_event = new robotEvent(robot_id, type);
        if (!exists_in_eventStack(new_event))
            eventsStack.Add(new_event);
        else
        {
            UnityEngine.Debug.Log("to waiting stack: " + robot_id.ToString());
            waitStack.Add(new_event);
        }
        */
        //publish as ros topic
        eventPublisher.pub_event(type, target_name, robot_id);
    }

    /*
    public void handle_srv_dropout(List<int> id_list)
    {
        if(id_list.Count != openStack.Count)
        {
            //there's at least one dropout
            //remove accepted events from open stack
            foreach(var id in id_list)
            {
                int temp_ind = 0;
                for(; temp_ind < openStack.Count; temp_ind++)
                {
                    if (openStack[temp_ind].id == id)
                    {
                        openStack.RemoveAt(temp_ind);
                        break;
                    }
                }
                //todo: handle not found
            }

            //add back to event stack
            foreach(var it in openStack)
            {
                UnityEngine.Debug.Log("Dropout found: " + it.id.ToString());
                eventsStack.Add(new robotEvent(it));
            }
        }
        openStack.Clear();
    }
    */
    public void stop_all_robots(bool reset_path = true)
    {
        foreach(var r in robots_list)
        {
            //stop
            r.GetComponent<metadata>().set_eventcall(false);
            r.GetComponent<navigation>().stop(reset_path);
        }
    }

    public void planFailedEvent(GameObject robot, string object_name)
    {
        //stop all on-going tasks
        //stop robots
        stop_all_robots();
        /*
        //trigger replan request
        if(failedEvent==null) //trigger only when there no previously triggered failed event
        {
            failedEvent = new robotEvent(robot.GetComponent<metadata>().get_id(), "failed", object_name);
        }
        */
        robotStopEvent(robot.GetComponent<metadata>().get_id(), "failed", object_name);

        //robot_allocator will ask to reset some objects
    }

    public void remove_all_obj()
    {
        //remove instantiated objects
        foreach(var it in objectList)
        {
            Destroy(it);
        }
        //clear object list
        objectList.Clear();
    }

    public void remove_all_drop_stations()
    {
        //remove instantiated objects
        foreach (var it in dropStationsMap)
        {
            Destroy(it.Value);
        }
        //clear object list
        dropStationsMap.Clear();
    }

    public void remove_all_robots()
    {
        //remove instantiated objects
        foreach (var it in robots_list)
        {
            Destroy(it);
        }
        //clear object list
        robots_list.Clear();
    }

    public void reset_all()
    {
        //clear event stack
        eventsStack.Clear();
        //remove all obj
        remove_all_obj();

        //remove all drop stations
        remove_all_drop_stations();

        //remove all robots
        remove_all_robots();

        //reset scene manager
        gameObject.GetComponent<manageScene>().reset_manager();

        //reset robot manager
        gameObject.GetComponent<manageRobot>().reset_manager();
    }
}
