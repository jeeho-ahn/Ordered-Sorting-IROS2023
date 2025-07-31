using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ikResult
{
    private float _dist;
    private float[] _angleArr;
    public float distErr
    {
        get
        {
            return _dist;
        }
        set
        {
            _dist = value;
        }
    }

    public float[] angles
    {
        get
        {
            return _angleArr;
        }
        set
        {
            _angleArr = value;
        }
    }

    public ikResult()
    {
        _dist = 0;
        _angleArr = new float[0];
    }

    public ikResult(float distanceErr, float[] anglesRes)
    {
        _dist = distanceErr;
        _angleArr = anglesRes;
    }
}

public class moveResult
{
    private float _dist;
    private bool _is_ok;

    public float distErr
    {
        get
        {
            return _dist;
        }
        set
        {
            _dist = value;
        }
    }

    public bool ok
    {
        get
        {
            return _is_ok;
        }
        set
        {
            _is_ok = value;
        }
    }

    public moveResult(float d, bool b)
    {
        _dist = d;
        _is_ok = b;
    }
}

public class manipulation : MonoBehaviour
{
    public GameObject toolObj, testObj;
    private overseer oseer;
    private GameObject pickedObj;
    private metadata meta;

    private bool ik_triggered = false;
    private Vector3 latest_target_pos;
    private Quaternion latest_target_q;
    private ikResult latest_ik_res;
    private bool ik_res_updated = false;

    private string _latest_dropped_obj_name = "";

    [SerializeField] GameObject[] object_chain;
    [SerializeField] float[] home_pose = {0, -25, 75, 35, 90, 0 };
    //private float[] home_pose = {0, -25, 100, 15, 90, 0 };
    //[SerializeField] float[] home_pose = {0, -57, 112, 35, 90, 0 };
    //private float[] home_pose = {0, -20, 112, 0, 90, 0 };
    [SerializeField] float[] pick_pose = { -33.595f, 15.326f, 87.699f, -25.802f, 98.255f, 122.799f };
    [SerializeField] Vector3 tool_pose;
    [SerializeField] bool is_moving, was_moving;

    private ros_topic self_ros;

    private Thread thread;

    public void cubeEvent(string msg)
    {
        if (msg == "pick")
            move_btn_event();
        else if (msg == "home")
            to_home_pose();
    }

    public bool checkIfMoving()
    {
        if(object_chain == null)
        {
            UnityEngine.Debug.LogWarning("object chain is missing");
            return false;
        }

        bool _mving = false;
        foreach(var gObj in object_chain)
        {
            var is_running = gObj.GetComponent<hinge_PID>().is_running;
            if (is_running)
            {
                _mving = true;
                break;
            }
        }

        return _mving;
    }

    public void test_ik()
    {
        var t_pos_ros = ros_conversions.unity2ros(new Vector3(-0.416f, 0.605f, 0.45f));
        var t_rot_ros = ros_conversions.unity2ros(Quaternion.Euler(new Vector3(165, 0, 90)));
        ik_solver(t_pos_ros, t_rot_ros);
    }

    private void Start()
    {
        object_chain = toolObj.GetComponent<IKsolver_gradient>().get_object_chain();
        oseer = GameObject.Find("overseer").GetComponent<overseer>();

        pickedObj = null;

        tool_pose = new Vector3();
        is_moving = false;
        was_moving = false;

        //ik solver coroutine
        ik_triggered = false;
        ik_res_updated = false;
        latest_ik_res = new ikResult();

        self_ros = gameObject.GetComponent<ros_topic>();
        //thread = new Thread(ik_solver_thread);
        //thread.Start();
        meta = gameObject.GetComponent<metadata>();

        _latest_dropped_obj_name = "";
    }

    public void triggerOverseer(string target = "")
    {
        var rid = gameObject.GetComponent<metadata>().get_id();
        oseer.robotStopEvent(rid, "arm",target);
        //self_ros.callEvent(new robotEvent(rid, "arm"));
        //self_ros.pub_event("arm");
    }

    private void Update()
    {
        is_moving = checkIfMoving();

        if (was_moving == true && is_moving == false)
        {
            UnityEngine.Debug.Log("arm stopped id: " + gameObject.GetComponent<metadata>().get_id());
            //stop manipulator
            if(meta.is_eventcall_on())
                triggerOverseer();
        }
        was_moving = is_moving;

        //ik coroutine
        if(ik_res_updated)
        {
            StopCoroutine(ik_solver_coroutine());
            trigger_ik_move();
        }

        //for monitoring
        //tool_pose =
        //    ros_conversions.unity2ros(jeehoTools.rotationMatrixToEulerAngles(jeehoTools.quatToMatrix(toolObj.transform.rotation)));
    }

    public void move_btn_event()
    {
        move_manipulator(ros_conversions.unity2ros(new Vector3(-0.416f, 0.605f, 0.45f)),
            ros_conversions.unity2ros(Quaternion.Euler(new Vector3(165, 0, 0))));
    }

    public void to_pick_pose()
    {
        for (int j = 0; j < object_chain.Length; j++)
            object_chain[j].GetComponent<hinge_PID>().pid_start(pick_pose[j] * Mathf.Deg2Rad);
    }

    public void to_home_pose()
    {
        for (int j = 0; j < object_chain.Length; j++)
            object_chain[j].GetComponent<hinge_PID>().pid_start(home_pose[j] * Mathf.Deg2Rad);
    }

    public ikResult ik_solver(Vector3 t_pos, Quaternion t_rot, bool move_robot = true)
    {
        //convert to Unity coord
        var t_pos_unity = ros_conversions.ros2unity(t_pos);
        var t_rot_unity = ros_conversions.ros2unity(t_rot);

        var res = toolObj.GetComponent<IKsolver_gradient>().solve_ik(t_pos_unity, t_rot_unity, move_robot);
        return res;
    }

    public int trigger_ik_coroutine(Vector3 t_pos, Quaternion t_rot)
    {
        UnityEngine.Debug.Log("use corountine ik triggered");
        latest_target_pos = t_pos;
        latest_target_q = t_rot;
        ik_triggered = true;
        StartCoroutine(ik_solver_coroutine());
        return 0;
    }

    private IEnumerator ik_solver_coroutine()
    {
        if (ik_triggered)
        {
            yield return null;
            UnityEngine.Debug.Log("start corountine ik");
            //convert to Unity coord
            var t_pos_unity = ros_conversions.ros2unity(latest_target_pos);
            var t_rot_unity = ros_conversions.ros2unity(latest_target_q);
            latest_ik_res = toolObj.GetComponent<IKsolver_gradient>().solve_ik(t_pos_unity, t_rot_unity, false);
            ik_res_updated = true;
            ik_triggered = false;            
        }        
        else
        {
            yield return null;
        }
    }

    private void ik_solver_thread()
    {
        while(true)
        {
            if (ik_triggered)
            {
                //yield return null;
                UnityEngine.Debug.Log("start corountine ik");
                //convert to Unity coord
                var t_pos_unity = ros_conversions.ros2unity(latest_target_pos);
                var t_rot_unity = ros_conversions.ros2unity(latest_target_q);
                latest_ik_res = toolObj.GetComponent<IKsolver_gradient>().solve_ik(t_pos_unity, t_rot_unity, false);
                ik_res_updated = true;
                ik_triggered = false;
            }

            Thread.Sleep(25);
        }        
    }

    public moveResult move_manipulator(Vector3 t_pos_ros, Quaternion t_rot_ros, float m_tol=0.005f, bool use_coroutine = false)
    {
        if(use_coroutine)
        {
            UnityEngine.Debug.Log("use corountine ik");
            trigger_ik_coroutine(t_pos_ros, t_rot_ros);
            return new moveResult(0, true);
        }
        else
        {
            //var t_pos_ros = pose2d_ext.unity2ros(new Vector3(-0.416f, 0.605f, 0.45f));
            //var t_rot_ros = pose2d_ext.unity2ros(Quaternion.Euler(new Vector3(165, 0, 90)));
            //var angles = ik_solver(t_pos_ros, t_rot_ros, false);
            // todo: find ik in a separate thread
            var res = ik_solver(t_pos_ros, t_rot_ros, false);

            //move if distance error is within tolerance
            bool _mv = false;
            if (res.distErr < m_tol)
            {
                _mv = true;
                for (int j = 0; j < object_chain.Length; j++)
                    object_chain[j].GetComponent<hinge_PID>().pid_start(res.angles[j] * Mathf.Deg2Rad);
            }
            else
            {
                UnityEngine.Debug.Log("Manip. Planning Failed: " + res.distErr);
            }

            return new moveResult(res.distErr, _mv);
        }
        
    }

    private void trigger_ik_move()
    {
        UnityEngine.Debug.Log("move triggered");
        ik_res_updated = false;
        set_to_move_manipulator(latest_ik_res);
    }

    private void set_to_move_manipulator(ikResult res, float m_tol = 0.005f)
    {
        //move if distance error is within tolerance
        bool _mv = false;
        if (res.distErr < m_tol)
        {
            _mv = true;
            for (int j = 0; j < object_chain.Length; j++)
                object_chain[j].GetComponent<hinge_PID>().pid_start(res.angles[j] * Mathf.Deg2Rad);
        }
        else
        {
            UnityEngine.Debug.Log("Manip. Planning Failed: " + res.distErr);
        }
    }


    public void set_picked_object(GameObject obj)
    {
        pickedObj = obj;
    }

    public GameObject get_picked_object()
    {
        return pickedObj;
    }

    public void set_drop_object()
    {
        _latest_dropped_obj_name = pickedObj.GetComponent<object_metadata>().objName;
        pickedObj = null;
    }

    public object_metadata get_picked_obj_meta()
    {
        return pickedObj.GetComponent<object_metadata>();
    }

    public void pick_obj_test()
    {
        if(testObj!=null)
        {
            var H_tool = toolObj.GetComponent<tool_pose>().get_R_tool("down");
            var H_obj_pick = testObj.GetComponent<pick_pose>().getPickPose();
            //var H_obj_u = testObj.transform.localToWorldMatrix;

            var H_obj_u = jeehoTools.quatToMatrix(testObj.transform.rotation);
            var t_obj_u = testObj.transform.position;
            H_obj_u.SetColumn(3, new Vector4(t_obj_u.x, t_obj_u.y, t_obj_u.z, 1));


            //test
            var q_obj = ros_conversions.unity2ros(testObj.transform.rotation);
            var t_obj = ros_conversions.unity2ros(testObj.transform.position);
            var H_obj = jeehoTools.quatToMatrix(q_obj);
            H_obj.SetColumn(3, new Vector4(t_obj.x, t_obj.y, t_obj.z));

            //convert to ROS Coord
            //var H_obj = jeehoTools.unityH2rosH(H_obj_u);

            var tf = H_obj*H_obj_pick;

            var t_pos = tf * new Vector4(0, 0, 0, 1);
            //var t_rot = jeehoTools.QFromMat_unity(H_tool*H_obj);
            var t_rot = q_obj * jeehoTools.QFromMat_unity(H_tool);

            move_manipulator(t_pos, t_rot);
        }
    }

    public int approach(GameObject pickObj)
    {
        //todo: return error if not reachable
        if (pickObj != null)
        {
        var H_tool = toolObj.GetComponent<tool_pose>().get_R_tool("down");
        var H_obj_pick = pickObj.GetComponent<pick_pose>().getPickPose();
        //var H_obj_u = testObj.transform.localToWorldMatrix;

        var H_obj_u = jeehoTools.quatToMatrix(pickObj.transform.rotation);
        var t_obj_u = pickObj.transform.position;
        H_obj_u.SetColumn(3, new Vector4(t_obj_u.x, t_obj_u.y, t_obj_u.z, 1));


        //test
        var q_obj = ros_conversions.unity2ros(pickObj.transform.rotation);
        var t_obj = ros_conversions.unity2ros(pickObj.transform.position);
        var H_obj = jeehoTools.quatToMatrix(q_obj);
        H_obj.SetColumn(3, new Vector4(t_obj.x, t_obj.y, t_obj.z));

        //convert to ROS Coord
        //var H_obj = jeehoTools.unityH2rosH(H_obj_u);

        var tf = H_obj * H_obj_pick;

        var t_pos = tf * new Vector4(0, 0, 0, 1);
        //var t_rot = jeehoTools.QFromMat_unity(H_tool*H_obj);
        var t_rot = q_obj * jeehoTools.QFromMat_unity(H_tool);

        //move_manipulator(t_pos, t_rot);
        approach(t_pos, t_rot);
        return 0;
        }
        else
        {
            return -1;
        }
    }

    public int approach_drop(Vector3 dPos_u, GameObject pObj)
    {
        var H_tool = toolObj.GetComponent<tool_pose>().get_R_tool("down");
        var H_obj_pick = pObj.GetComponent<pick_pose>().getPickPose();
        //var H_obj_u = testObj.transform.localToWorldMatrix;

        var H_obj_u = jeehoTools.quatToMatrix(pObj.transform.rotation);
        //var t_obj_u = pickObj.transform.position;
        var t_obj_u = dPos_u;
        H_obj_u.SetColumn(3, new Vector4(t_obj_u.x, t_obj_u.y, t_obj_u.z, 1));


        //test
        var q_obj = ros_conversions.unity2ros(Quaternion.identity);
        var t_obj = ros_conversions.unity2ros(dPos_u);
        var H_obj = jeehoTools.quatToMatrix(q_obj);
        H_obj.SetColumn(3, new Vector4(t_obj.x, t_obj.y, t_obj.z));

        //convert to ROS Coord
        //var H_obj = jeehoTools.unityH2rosH(H_obj_u);

        var tf = H_obj * H_obj_pick;

        var t_pos = tf * new Vector4(0, 0, 0, 1);
        //var t_rot = jeehoTools.QFromMat_unity(H_tool*H_obj);
        var t_rot = q_obj * jeehoTools.QFromMat_unity(H_tool);

        //move_manipulator(t_pos, t_rot);
        approach(t_pos, t_rot);
        return 0;
    }

    public int approach(Vector3 tPos, Quaternion tRot)
    {
        for (int r = 0; r < 5; r++)
        {
           var res = move_manipulator(tPos, tRot);
            if (res.ok)
                break;
        }
        return 0;
    }

    public void set_latest_dropped_obj(string obj_name)
    {
        _latest_dropped_obj_name = obj_name;
    }
    public string get_latest_dropped_obj()
    {
        return _latest_dropped_obj_name;
    }
}
