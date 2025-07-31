using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drop_station_metadata : MonoBehaviour
{
    private dropStation meta;
    private bool trigger_drop = false;

    //test waiting queue vector
    [SerializeField] Vector3 _waiting_vec_ros;
    [SerializeField] Vector3 _buffer_vec_ros;
    // waiting_queue
    [SerializeField] List<GameObject> _wait_queue;
    [SerializeField] List<bool> _buff_queue;

    public void init_station(dropStation ds_in)
    {
        meta = ds_in;
        //move object
        gameObject.transform.position = ros_conversions.ros2unity(ds_in.PosRos);
    }

    public List<GameObject> get_objs()
    {
        return meta.get_items();
    }

    public float get_drop_offset()
    {
        return meta.DropOffset; 
    }

    public void add_obj_to_dropped_list(GameObject obj, bool trigger_gravity = true)
    {
        meta.drop_item(obj);
        trigger_drop = trigger_gravity;
    }

    public void add_to_wait_queue(GameObject robot_in)
    {
        _wait_queue.Add(robot_in);
    }

    public void remove_from_wait_queue(int robot_id)
    {
        for (int i = 0; i < _wait_queue.Count; i++)
        {
            if (_wait_queue[i].GetComponent<metadata>().get_id() == robot_id)
            {
                _wait_queue.RemoveAt(i);
                break;
            }
        }

        //not found if reached here
        UnityEngine.Debug.Log("Failed: rm from wait queue. Robot not in queue");
    }

    /**
     * Search for robot gameObject with designated object priority in wait queue
     * returns robot GameObject if found.
     * returns null if not found.
     */
    public GameObject wait_queue_search(int obj_priority)
    {
        for (int i = 0; i < _wait_queue.Count; i++)
        {
            if (_wait_queue[i].GetComponent<manipulation>().get_picked_object().GetComponent<object_metadata>().priority == obj_priority)
            {
                return _wait_queue[i];
            }
        }

        //not found
        return null;
    }

    public int wait_queue_size()
    {
        return _wait_queue.Count;
    }

    public GameObject search_wait_queue_for_priority(int priority)
    {
        return wait_queue_search(priority);
    }

    public void remove_robot_from_wait_queue(int robot_id)
    {
        remove_from_wait_queue(robot_id);
    }

    public int get_wait_queue_size()
    {
        return wait_queue_size();
    }

    public Vector3 get_waiting_vec()
    {
        return _waiting_vec_ros;
    }

    public void set_waiting_vec(Vector3 vec_in, bool is_ros = true)
    {
        if (is_ros)
            _waiting_vec_ros = vec_in;

        else
            _waiting_vec_ros = ros_conversions.unity2ros(vec_in);
    }

    public Vector3 set_auto_waiting_vec()
    {
        // origin to station unit vector
        var disp = gameObject.transform.position;
        var disp_ros = ros_conversions.unity2ros(disp);
        _waiting_vec_ros = disp_ros.normalized;
        return _waiting_vec_ros;
    }

    public Vector3 set_buffer_vec(float rz_deg)
    {
        var rz_rad = Mathf.Deg2Rad*rz_deg;
        var pvec_n = ros_conversions.unity2ros(gameObject.transform.position).normalized;
        var bvec_2d = jeehoTools.rot2d(new Vector2(pvec_n.x, pvec_n.y),rz_rad);

        _buffer_vec_ros = new Vector3(bvec_2d.x,bvec_2d.y,0);
        return _buffer_vec_ros;
    }

    public Vector3 get_buffer_vec()
    {
        return _buffer_vec_ros;
    }

    public int get_lowest_priority_in_station()
    {
        return meta.get_lowest_priority();
    }

    public Vector3 get_drop_pos()
    {
        //cur position
        var cur_pos_ros = ros_conversions.unity2ros(gameObject.transform.position);
        //assuming unity will find closest walkable point
        var dir = Vector3.zero - gameObject.transform.position;
        var dir_ros = ros_conversions.unity2ros(dir);

        var dir_ros_unit = dir_ros.normalized;

        return (cur_pos_ros - 0.19f * dir_ros_unit);
    }

    public Vector3 get_next_wait_pos()
    {
        //fixed for now
        //assuming avoidance priority is temporarily set to 99
        return 0.75f * (_wait_queue.Count + 1) * _waiting_vec_ros + ros_conversions.unity2ros(gameObject.transform.position);
    }

    public Vector3 get_target_pos(GameObject robot)
    {
        //return drop pos if empty
        //return wait pos if robot needs to wait for another
        var objStack = meta.get_items();
        var latest_priority = meta.get_lowest_priority();
        //next object should be one with +1 priority
        var cur_obj_pri = robot.GetComponent<manipulation>().get_picked_object().GetComponent<object_metadata>().priority;
        if (cur_obj_pri == latest_priority + 1)
        {
            //to drop pos
            return get_drop_pos();
        }
        else
        {
            //add to wait queue
            add_to_wait_queue(robot);
            //to wait pos
            return get_next_wait_pos();
        }
    }

    public int find_empty_buff_ind()
    {
        for(int i=0; i<_buff_queue.Count; i++)
        {
            if (_buff_queue[i] == false)
                return i;
        }

        // buffer queue is full ...??
        return -1;
    }

    public void occupy_buff_ind(int ind)
    {
        _buff_queue[ind] = true;
    }

    public void free_buff_ind(int ind)
    {
        _buff_queue[ind] = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        meta = new dropStation();
        trigger_drop = false;

        //default vec
        //_waiting_vec_ros = new Vector3(-1, 0, 0);
        set_auto_waiting_vec();
        set_buffer_vec(30);
        _wait_queue = new List<GameObject>();
        _buff_queue = new List<bool>();
        for (int i = 0; i < 25; i++)
            _buff_queue.Add(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(trigger_drop)
        {
            var olist = meta.get_items();
            var rgby = olist[olist.Count - 1].GetComponent<Rigidbody>();
            if(rgby.useGravity == true)
            {
                rgby.useGravity = false;
            }    
            else
            {
                rgby.useGravity = true;
                trigger_drop = false;
            }
        }        
    }
}
