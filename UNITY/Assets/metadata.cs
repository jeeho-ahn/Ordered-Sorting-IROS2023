using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class metadata : MonoBehaviour
{
    [SerializeField] int id = -1;
    [SerializeField] Transform pivotLink;
    public float x, y, th;
    [SerializeField] string latest_task = "";
    private NavMeshAgent navAgent;
    private Vector3 home_pos;
    private bool _event_call = true;

    // Start is called before the first frame update
    void Start()
    {
        pivotLink = this.gameObject.transform.Find("base_link").Find("chassis_link");
        latest_task = "";
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        //home_pos = new Vector3();
        _event_call = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        var p = get_pose2d();
        x = p.x;
        y = p.y;
        th = p.th;
    }

    public void set_task(string task_name)
    {
        latest_task = task_name;
    }

    public string get_task()
    {
        return latest_task;
    }

    public void set_id(int id_in)
    {
        id = id_in;
    }

    public int get_id()
    {
        return id;
    }

    public pose2d get_pose2d()
    {
        if (pivotLink == null)
            pivotLink = this.gameObject.transform.Find("base_link").Find("chassis_link");

        return new pose2d(pivotLink);
    }

    public void set_home_pos(Vector3 home_pos_in)
    {
        home_pos = home_pos_in;
    }

    public Vector3 get_home_pos()
    {
        return home_pos;
    }

    public void set_eventcall(bool onOff)
    {
        _event_call = onOff;
    }

    public bool is_eventcall_on()
    {
        return _event_call;
    }

    public void avoidance_off()
    {
        navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }
    public void avoidance_on(string q = "high")
    {
        if (q == "low")
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        else if (q == "medium")
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        else if (q == "good")
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
        else if (q == "high")
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        else
        {
            //??
        }
    }

    public void set_avoidance_priority(int avoid_priority)
    {
        navAgent.avoidancePriority = avoid_priority;
    }
    //set its id as priority if no value is given
    public void set_avoidance_priority(bool from_back = false)
    {
        if(!from_back)
            navAgent.avoidancePriority = get_id()+1; //to preserve higher priority
        else
            navAgent.avoidancePriority = 99-get_id()*9; //to preserve higher priority
    }

    
}
