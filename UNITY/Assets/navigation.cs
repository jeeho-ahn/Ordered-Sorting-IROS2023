using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;


public class navigation : MonoBehaviour
{
    UnityEngine.AI.NavMeshAgent agent;
    private overseer oseer;
    private manageRobot robotManager;
    private metadata meta;
    //public Button navBtn;
    //public InputField x_field, y_field;
    [SerializeField] private bool is_moving;
    [SerializeField] private bool was_moving;
    [SerializeField] private bool has_goal;
    [SerializeField] private Vector2 cur_goal; // in ros coord.
    [SerializeField] private bool reached_goal;
    private Vector3[] unity_path;
    public LineRenderer LineRenderer;

    //show path
    public Toggle show_path_toggle;
    public bool show_path = true;

    private float reached_thres = 0.12f;
    private bool event_sent = false;
    private bool trigger_line_render;
    private ros_topic self_ros;
    private robot_event_pub event_publisher;
    private bool _is_planning;

    //radial search num
    private int rad_search_n = 8;

    private bool tricked = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        oseer = GameObject.Find("overseer").GetComponent<overseer>();
        robotManager = GameObject.Find("overseer").GetComponent<manageRobot>();
        is_moving = false;
        was_moving = false;
        has_goal = false;
        cur_goal = new Vector2();
        reached_goal = false;
        _is_planning = false;
        //navBtn.onClick.AddListener(gotoPoint);
        //navBtn.onClick.AddListener(planBtnEvent);
        //self_ros = gameObject.GetComponent<ros_srv>();
        //self_ros = gameObject.GetComponent<ros_topic>();
        meta = gameObject.GetComponent<metadata>();

        //line properites
        LineRenderer = gameObject.GetComponent<LineRenderer>();
        // set the color of the line
        LineRenderer.startColor = Color.red;
        LineRenderer.endColor = Color.red;

        // set width of the renderer
        LineRenderer.startWidth = 0.025f;
        LineRenderer.endWidth = 0.025f;
        trigger_line_render = false;
    }

    /*
    void planBtnEvent()
    {
        Vector3 pos_in = new Vector3();
        pos_in.x = float.Parse(x_field.text);
        pos_in.y = float.Parse(y_field.text);
        pos_in.z = 0.0f;
        planToPoint(pos_in, true);
    }
    */

    private UnityEngine.AI.NavMeshPath radial_search(float angle_step, float dist, Vector3 pivot_pos_u, int rad_search_size, 
                    out NavMeshHit _hit_out, out bool res, bool do_center = true)
    {
        var path_plan = new UnityEngine.AI.NavMeshPath();
        var _hit = new NavMeshHit();
        bool out_res = false;

        if(do_center)
        {
            res = NavMesh.SamplePosition(pivot_pos_u, out _hit, 0.4f, NavMesh.AllAreas);
            if (res)
            {
                agent.CalculatePath(_hit.position, path_plan);
                if (path_plan.status == NavMeshPathStatus.PathComplete)
                {
                   //UnityEngine.Debug.Log("center plan ok");
                    res = true;
                    _hit_out = _hit;
                    return path_plan;
                }
            }
        }

        bool rad_search_failed = true;
        for (int i = 0; i < rad_search_size; i++)
        {
            float th = angle_step * (float)i;
            float tx = dist * Mathf.Cos(th);
            float tz = dist * Mathf.Sin(th);
            Vector3 tPos = new Vector3(tx, 0, tz) + pivot_pos_u;

            res = NavMesh.SamplePosition(tPos, out _hit, 0.1f, NavMesh.AllAreas);
            if (res)
            {
                agent.CalculatePath(_hit.position, path_plan);
                if (path_plan.status == NavMeshPathStatus.PathComplete)
                {
                    //ok
                    //UnityEngine.Debug.Log("radial search ok");
                    //UnityEngine.Debug.Log(_hit.position);
                    rad_search_failed = false;
                    out_res = true;
                    break;
                }
            }
        }

        if (rad_search_failed)
        {
            //UnityEngine.Debug.Log("Radial search failed: " + meta.get_id().ToString());
            //return null;
            out_res = false;
        }

        _hit_out = _hit;
        res = out_res;
        return path_plan;
    }

    public NavMeshPath planToPoint(Vector3 pos, bool is_ros_coord = true, bool move_robot = true, bool is_sampled_point = false)
    {
        _is_planning = true;
        Vector3 pos_ros = pos;
        Vector3 pos_unity = pos;
        if (!is_ros_coord)
        {
            pos_ros = ros_conversions.unity2ros(pos);
        }
        else
        {
            pos_unity = ros_conversions.ros2unity(pos);
        }

        var path_plan = new NavMeshPath();
        NavMeshHit _hit = new NavMeshHit();
        //radial search before calling it failed
        float dist = 0.4f;
        float angle_step = 2 * Mathf.PI / (float)rad_search_n;
        bool rad_res = false;

        if (!is_sampled_point)
        {
            //UnityEngine.Debug.Log("Initial Radial Search: " + meta.get_id().ToString());
            
            path_plan = radial_search(angle_step, dist, pos_unity, rad_search_n, out _hit, out rad_res);
            if (!rad_res)
            {
                UnityEngine.Debug.Log("Radial search failed: " + meta.get_id().ToString());
                //return null;
                var res = NavMesh.SamplePosition(pos_unity, out _hit, 0.4f, NavMesh.AllAreas);

                if (!res)
                {
                    UnityEngine.Debug.Log("Sampling Position Failed: " + meta.get_id().ToString());
                    return null;
                }
                //var path_plan = new UnityEngine.AI.NavMeshPath();
                agent.CalculatePath(_hit.position, path_plan);
            }
        }
        else //target point is already a sampled point
        {
            agent.CalculatePath(pos_unity, path_plan);
        }
        /*
        var res = NavMesh.SamplePosition(pos_unity, out _hit, 0.4f, NavMesh.AllAreas);

        if(!res)
        {
            UnityEngine.Debug.Log("Sampling Position Failed: " + meta.get_id().ToString());
            return null;
        }
        */

        if(path_plan.status != NavMeshPathStatus.PathComplete && !is_sampled_point)
        {
            if (path_plan.status == NavMeshPathStatus.PathPartial)
            {
                UnityEngine.Debug.Log("Partial Plan found: " + meta.get_id().ToString());
                //target might be on navemesh island. sample again
                var furthest_pt = path_plan.corners[path_plan.corners.Length-1];
                //project to floor
                furthest_pt.y = 0;
                pos_unity.y = 0;
                var dir_vec = (furthest_pt - pos_unity);
                Vector3 dir_vec_n = dir_vec / dir_vec.magnitude;
                var adj_pos = pos_unity + 0.35f * dir_vec_n;
                var res = NavMesh.SamplePosition(adj_pos, out _hit, 0.1f, NavMesh.AllAreas); //todo: find better planning method

                //todo: re-organize this part to be either recursive or something
                if (!res)
                {
                    UnityEngine.Debug.Log("Sampling Point Failed: " + meta.get_id().ToString());
                    //return null;
                }
                else
                {
                    agent.CalculatePath(_hit.position, path_plan);
                }
                if (path_plan.status != NavMeshPathStatus.PathComplete)
                {
                    UnityEngine.Debug.Log("Attempting Radial Search: " + meta.get_id().ToString());
                    //radial search before calling it failed
                    //float dist = 0.4f;
                    //float angle_step = 2 * Mathf.PI / (float)rad_search_n;

                    //bool rad_res = false;
                    var path_plan_rad = radial_search(angle_step, dist, pos_unity, rad_search_n, out _hit, out rad_res);
                    if(rad_res)
                    {
                        //radial search ok
                        path_plan = path_plan_rad;
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Radial search failed: " + meta.get_id().ToString());
                        return null;
                    }

                    /*
                    bool rad_search_failed = true;
                    for (int i = 0; i < rad_search_n; i++)
                    {
                        float th = angle_step * (float)i;
                        float tx = dist * Mathf.Cos(th);
                        float tz = dist * Mathf.Sin(th);
                        Vector3 tPos = new Vector3(tx, 0, tz) + pos_unity;

                        res = NavMesh.SamplePosition(tPos, out _hit, 0.1f, NavMesh.AllAreas);
                        if (res)
                        {
                            agent.CalculatePath(_hit.position, path_plan);
                            if (path_plan.status == NavMeshPathStatus.PathComplete)
                            {
                                //ok
                                rad_search_failed = false;
                                break;
                            }
                        }
                    }

                    if (rad_search_failed)
                    {
                        UnityEngine.Debug.Log("Radial search failed: " + meta.get_id().ToString());
                        return null;
                    }
                    */
                }
            }

            else
            {
                UnityEngine.Debug.Log("Path Plan Failed: " + meta.get_id().ToString());
                return null;
            }
        }

        unity_path = path_plan.corners;
        trigger_line_render = true;
        if (move_robot)
        {
            //agent.SetDestination(pos_unity);
            agent.SetPath(path_plan);
            //set robot goal
            has_goal = true;
            if(!is_sampled_point)
                set_goal(ros_conversions.unity2ros(_hit.position));
            //set_goal(ros_conversions.unity2ros(pos_unity));
            event_sent = false;
        }

        _is_planning = false;
        return path_plan;
    }

    public void stop(bool reset_path = true)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (reset_path)
            agent.ResetPath();
    }

    public void unlock(bool reset_path = true)
    {
        if (reset_path)
            agent.ResetPath();
        agent.isStopped = false;
    }

    public bool ReachedDestinationOrGaveUp()
    {

        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void set_goal(Vector2 goal_in_ros)
    {
        cur_goal = goal_in_ros;
        has_goal = true;
    }

    public void set_goal(Vector3 goal_in_ros)
    {
        set_goal(new Vector2(goal_in_ros.x, goal_in_ros.y));
    }

    public void set_reached()
    {
        has_goal = false;
        event_sent = true;
    }

    bool check_reached(float thres)
    {
        var robotPos_ros = ros_conversions.unity2ros(this.transform.position);
        var robotPos2d_ros = new Vector2(robotPos_ros.x, robotPos_ros.y);
        if (Vector2.Distance(robotPos2d_ros, cur_goal) < thres)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool goal_reached()
    {
        if(has_goal == true)
        {
            if(check_reached(reached_thres))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            UnityEngine.Debug.Log("goal not set");
            return true;
        }
    }

    void triggerOverseer()
    {
        var rid = gameObject.GetComponent<metadata>().get_id();
        UnityEngine.Debug.Log("mobile stopped id: " + rid);
        oseer.robotStopEvent(rid,"mobile");
        //self_ros.callEvent(new robotEvent(rid, "mobile"));
        //self_ros.pub_event("mobile");
    }

    void stop_event()
    {
        set_reached();
        if (meta.is_eventcall_on())
            triggerOverseer();
    }

    public void toggle_show_path()
    {
        if (show_path)
            show_path = false;
        else
            show_path = true;
    }

    private IEnumerator trick()
    {
        UnityEngine.Debug.Log("trick activated");
        yield return null;
        var cur_pos = gameObject.transform.position;
        if (tricked)
        {
            gameObject.transform.position = new Vector3(cur_pos.x + 0.0001f, cur_pos.y, cur_pos.z);
            tricked = false;
        }

        else
        {
            gameObject.transform.position = new Vector3(cur_pos.x + 0.0001f, cur_pos.y, cur_pos.z);
            tricked = true;
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        is_moving = !ReachedDestinationOrGaveUp();
        reached_goal = check_reached(reached_thres);
        /*
        if(is_moving)
        {
            //check if replanning is needed
            NavMeshHit _hit;
            if (!NavMesh.SamplePosition(ros_conversions.ros2unity(cur_goal), out _hit, 0.001f, NavMesh.AllAreas))
            {
                //replan
                //UnityEngine.Debug.Log("Replanning!");
                //planToPoint(cur_goal);
            }
        }
        */


        if (robotManager.get_show_path() && !show_path)
            show_path = true;
        else if (!robotManager.get_show_path() && show_path)
            show_path = false;                
        

        //is_moving = !agent.is;
        //if(was_moving == true && is_moving == false)
        if (is_moving==false)
        {
            //UnityEngine.Debug.Log("robot stopped");
            if(reached_goal && event_sent == false && has_goal)
            {
                stop_event();
            }
            else if(!reached_goal && has_goal && !_is_planning)
            {
                //replan
                UnityEngine.Debug.Log("Replan");
                planToPoint(new Vector3(cur_goal.x, cur_goal.y, 0), true, true,true);
                //StartCoroutine(trick());
            }
            
        }
        was_moving = is_moving;

        //draw line
        if(has_goal)
        {
            if (trigger_line_render && show_path)
            {
                LineRenderer.enabled = true;
                // set the position
                LineRenderer.positionCount = unity_path.Length;
                LineRenderer.SetPositions(unity_path);
                trigger_line_render = false;
            }
            
        }
        else
        {
            LineRenderer.enabled = false;
        }
    }
    
}
