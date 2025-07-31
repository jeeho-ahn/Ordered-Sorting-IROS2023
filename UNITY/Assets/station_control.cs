using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class station_control : MonoBehaviour
{
    private overseer oseer;
    private drop_station_metadata meta;
    private ros_topic self_ros;
    private Dictionary<string, int> buffMap;

    // Start is called before the first frame update
    void Start()
    {
        oseer = GameObject.Find("overseer").GetComponent<overseer>();
        meta = gameObject.GetComponent<drop_station_metadata>();
        self_ros = gameObject.GetComponent<ros_topic>();
        buffMap = new Dictionary<string, int>();
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    public void occupy_buffer(string obj_name, int ind)
    {
        meta.occupy_buff_ind(ind);
        buffMap.Add(obj_name, ind);
    }    

    public Vector3 get_target_pos(GameObject robot)
    {
        return meta.get_target_pos(robot);
    }

    public Vector3 get_buffer_pos(out int buffInd, bool robot_stop = false, bool return_unity = false)
    {
        //meta.set_buffer_vec(30);
        var buffer_vec = meta.get_buffer_vec();
        var init_vec = buffer_vec * 1.25f;

        int buff_ind = meta.find_empty_buff_ind();
        if(buff_ind == -1)
        {
            //buffer queue is full.. weird
            buffInd = -1;
            return new Vector3();
        }
      
        var piv_vec = ros_conversions.unity2ros(gameObject.transform.position) + init_vec + (float)buff_ind * 1.1f * buffer_vec;

        meta.occupy_buff_ind(buff_ind);

        buffInd = buff_ind;

        if (return_unity)
            return ros_conversions.ros2unity(piv_vec);
        else
            return piv_vec;        
    }

    public void free_buffer_ind(int ind)
    {
        meta.free_buff_ind(ind);
    }

    public bool get_approval(GameObject robot)
    {
        int next_pri = meta.get_lowest_priority_in_station() + 1;
        if(robot.GetComponent<manipulation>().get_picked_object().GetComponent<object_metadata>().priority == next_pri)
        {
            triggerOverseer_with_robotID(robot.GetComponent<metadata>().get_id());
            return true;
        }
        else
        {
            UnityEngine.Debug.Log("Robot needs to wait");
            return false;
        }
    }

    void triggerOverseer_with_robotID(int robot_id)
    {
        UnityEngine.Debug.Log("station approved id: " + robot_id);
        oseer.robotStopEvent(robot_id, "station",meta.name);
        //self_ros.pub_event("station",meta.name,robot_id);
    }

    public void drop_item(GameObject obj, bool trigger_gravity = true)
    {
        meta.add_obj_to_dropped_list(obj, trigger_gravity);
        //call drop event on overseer side
        oseer.object_finished_event(obj);

        //check if this was in buff
        var objName = obj.GetComponent<object_metadata>().name;
        if (buffMap.ContainsKey(objName))
        {
            free_buffer_ind(buffMap[objName]);
            buffMap.Remove(objName);
        }
       
        //check wait queue and call robot if needed
        if (meta.get_wait_queue_size() > 0)
        {
            //call if next priority is in wait queue
            int next_pri = obj.GetComponent<object_metadata>().priority + 1;
            var next_in_wq = meta.search_wait_queue_for_priority(next_pri);

            //next object found in wait queue
            if (next_in_wq != null)
            {
                var robot_id = next_in_wq.GetComponent<metadata>().get_id();
                UnityEngine.Debug.Log("Call next in line: " + robot_id.ToString());
                
                //call robot
                triggerOverseer_with_robotID(robot_id);
                //remove from wait queue
                meta.remove_robot_from_wait_queue(robot_id);
            }
        }
    }
}
