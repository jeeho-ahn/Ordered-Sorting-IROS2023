using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pose2d
{
    public float x,y,th;

    public pose2d(float x_in, float y_in, float th_in)
    {
        x = x_in;
        y = y_in;
        th = th_in;
    }

    public pose2d(Transform tr)
    {
        var Pos = pose2d_ext.unity2ros(tr.position);
        var Rot = pose2d_ext.unity2ros(tr.rotation);
        
        x = Pos.x;
        y = Pos.y;
        //th = pose2d_ext.quat2euler2d(Rot);
        th = pose2d_ext.quat2euler(Rot).z;
        //var th2 = pose2d_ext.quat2euler(Rot);
        //UnityEngine.Debug.Log("check");
    }

    public static pose2d operator +(pose2d p1, pose2d p2)
    {
        var out_pose = new pose2d(0, 0, 0);
        out_pose.x = p1.x + p2.x;
        out_pose.y = p1.y + p2.y;
        out_pose.th = p1.th + p2.th;
        return out_pose;
    }
    public static pose2d operator -(pose2d p1, pose2d p2)
    {
        var out_pose = new pose2d(0, 0, 0);
        out_pose.x = p1.x - p2.x;
        out_pose.y = p1.y - p2.y;
        out_pose.th = p1.th - p2.th;
        return out_pose;
    }

    public float get_x(){
        return x;
    }
    public float get_y(){
        return y;
    }
    public float get_th(){
        return th;
    }
    public void set_x(float x_in)
    {
        x = x_in;
    }
    public void set_y(float y_in)
    {
        y = y_in;
    }
    public void set_th(float th_in)
    {
        th = th_in;
    }

    public Vector3 pos_to_v3()
    {
        return new Vector3(x, y, 0);
    }

    public Quaternion rot_to_quat()
    {
        return pose2d_ext.eurler2quat(new Vector3(0, 0, th));
    }
}

public static class pose2d_ext
{    
    public static float quat2euler2d(this UnityEngine.Quaternion q)
    {
        double numerator = 2.0 * (q.w * q.z);
        double denominator = 1.0 - (2.0 * (q.z * q.z));
        return Mathf.Atan2((float)numerator, (float)denominator);
    }


    public static Vector3 quat2euler(this UnityEngine.Quaternion q)
    {
        Vector3 angles;

        // roll (x-axis rotation)
        float sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
        float cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
        angles.x = Mathf.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        float sinp = 2 * (q.w * q.y - q.z * q.x);
        if (Mathf.Abs(sinp) >= 1)
            if (sinp > 0)
                angles.y = Mathf.PI / 2; // use 90 degrees if out of range
            else
                angles.y = Mathf.PI * -1 / 2;
        else
            angles.y = Mathf.Asin(sinp);

        // yaw (z-axis rotation)
        float siny_cosp = 2 * (q.w * q.z + q.x * q.y);
        float cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
        angles.z = Mathf.Atan2(siny_cosp, cosy_cosp);

        return angles;
    }

    public static UnityEngine.Quaternion eurler2quat(this UnityEngine.Vector3 e)
    {
        float roll = e.x;
        float pitch = e.y;
        float yaw = e.z;
        // Abbreviations for the various angular functions
        float cy = Mathf.Cos(yaw * 0.5f);
        float sy = Mathf.Sin(yaw * 0.5f);
        float cp = Mathf.Cos(pitch * 0.5f);
        float sp = Mathf.Sin(pitch * 0.5f);
        float cr = Mathf.Cos(roll * 0.5f);
        float sr = Mathf.Sin(roll * 0.5f);

        UnityEngine.Quaternion q;
        q.w = cr * cp * cy + sr * sp * sy;
        q.x = sr * cp * cy - cr * sp * sy;
        q.y = cr * sp * cy + sr * cp * sy;
        q.z = cr * cp * sy - sr * sp * cy;

        return q;
    }

    [Obsolete("use ros_conversion instead of pose2d_ext")]
    public static Vector3 unity2ros(this Vector3 unityVec)
    {
        return new Vector3(unityVec.z, -unityVec.x, unityVec.y);
    }
    [Obsolete("use ros_conversion instead of pose2d_ext")]
    public static Vector3 ros2unity(this Vector3 vector3)
    {
        return new Vector3(-vector3.y, vector3.z, vector3.x);
    }
    [Obsolete("use ros_conversion instead of pose2d_ext")]
    public static Quaternion ros2unity(this Quaternion quaternion)
    {
        return new Quaternion(quaternion.y, -quaternion.z, -quaternion.x, quaternion.w);
    }
    [Obsolete("use ros_conversion instead of pose2d_ext")]
    public static Quaternion unity2ros(this Quaternion quaternion)
    {
        return new Quaternion(-quaternion.z, quaternion.x, -quaternion.y, quaternion.w);
    }
}
public class velCmd
{
    public float v,w;
    public velCmd(float v_in, float w_in)
    {
        v = v_in;
        w = w_in;
    }
    public float get_v()
    {
        return v;
    }
    public float get_w()
    {
        return w;
    }
    public void set_v(float v_in)
    {
        v = v_in;
    }
    public void set_w(float w_in)
    {
        w = w_in;
    }
}

public class kanayama_params
{
    public float kx, ky, kth;
    public kanayama_params()
    {
        kx=ky=kth = 0.0f;
    }
}

public class trajectory_set
{
    public List<pose2d> pos;
    public List<velCmd> vel;
    public List<int> ms;

    public trajectory_set()
    {
        pos = new List<pose2d>();
        vel = new List<velCmd>();
        ms = new List<int>();
    }

    public trajectory_set(List<pose2d> pos_in, List<velCmd> vel_in, List<int> time_ms_in)
    {
        pos = pos_in;
        vel = vel_in;
        ms = time_ms_in;
    }

    public int pivot_search(int cur_ms, int start_pivot=0)
    {
        if(ms.Count == 0)
        {
            UnityEngine.Debug.Log("Trajectory is Empty");
            return -1;
        }

        if(start_pivot >= ms.Count)
        {
            UnityEngine.Debug.Log("Out of Traj. Range");
            return -2;
        }
        else if(start_pivot < 0)
        {
            UnityEngine.Debug.Log("Out of Traj. Range (neg)");
            return -2;
        }

        if(start_pivot == ms.Count-1)
        {
            UnityEngine.Debug.Log("End of Trajectory");
            return -3;
        }

        int cur_pivot = start_pivot;

        for (int i = start_pivot; i < ms.Count-1; i++)
        {
            var t_n = ms[cur_pivot];
            var t_n_ = ms[cur_pivot + 1];

            if (cur_ms >= t_n && cur_ms < t_n_)
                return cur_pivot;
            else
                cur_pivot++;
        }

        //not found
        UnityEngine.Debug.Log("Index not found");
        UnityEngine.Debug.Log("dTime: " + cur_ms);
        return ms.Count-1;
    }
}

public class trajectory_tracking : MonoBehaviour
{
    private jackal_controller jackal_ctrl;
    
    public string tracking_model = "kanayama";
    public float linear_vel_limit = 0.5f;
    public float angular_vel_limit = 100f;
    
    // robot frame
    public Transform robotBody;

    private trajectory_set traj_to_run;
    private bool run_traj;
    int pivot_ind;
    DateTime t_start;

    // Start is called before the first frame update
    void Start()
    {
        jackal_ctrl = gameObject.GetComponent(typeof(jackal_controller)) as jackal_controller;
        traj_to_run = new trajectory_set();
        run_traj = false;
        pivot_ind = 0;
        
        //var parent = gameObject.transform.parent;
        //var base_obj = gameObject.transform.parent.transform.Find("base_link");
        robotBody = gameObject.transform.parent.transform.Find("base_link").transform.Find("chassis_link");
    }

    // Update is called once per frame
    void Update()
    {
        if (run_traj)
            trajectory_iterations();
    }

    float sq(float d)
    {
        return d * d;
    }

    kanayama_params update_kanayama_parameters(velCmd ref_vel, float sample_per = 0.1f)
    {
        var param = new kanayama_params();
        param.kx = 1.2f; // fix. unit = sec^-1
        //param->kth = -1*(current_vel->linear_vel * sin(quat2euler(pose_error->angle))) / (0.25 * param->kx * pow(pose_error->pose_x,2));
        //param->ky = pow(param->kth,2) / (4);
        //param->ky = pow((2*reference_vel->linear_vel*sin(quat2euler(pose_error->angle))),2) / pow((param->kx*pow(pose_error->pose_x,2)),2); ??????
        param.ky = (sq(4 / (ref_vel.v * sample_per + 0.5f * Mathf.Exp(-1.0f * param.kx * sample_per)))) / 2 / 2;
        param.kth = 2.2f * Mathf.Sqrt(param.ky) / 2;
        // param->ky = 80;
        //param->kth = 0.277721;
        return param;
    }

    velCmd kanayama_controller(pose2d p_ref, pose2d p_cur, velCmd ref_vel)
    {
        //init fixed params
        //float kx = 10f;
        //float ky = 0.64f;
        //float kth = 0.16f;
        var param = update_kanayama_parameters(ref_vel);
        //1. get reference pose
        //2. get current pose
        //3. calculate pose error
        var dPose = p_ref - p_cur;
        // Pose error
        // dx*cos(th) + dy*sin(th)
        // dx*-sin(th) + dy*cos(th)
        // dth
        var p_err = new pose2d(0, 0, 0);
        p_err.x = dPose.x * Mathf.Cos(p_cur.th) + dPose.y * Mathf.Sin(p_cur.th);
        p_err.y = dPose.x * -1 * Mathf.Sin(p_cur.th) + dPose.y * Mathf.Cos(p_cur.th);
        p_err.th = dPose.th;
        //4. calculate new q=(v,w)
        var q_out = new velCmd(0,0);
        q_out.v = ref_vel.v * Mathf.Cos(p_err.th) + param.kx * p_err.x;
        q_out.w = ref_vel.w + ref_vel.v * (param.ky * p_err.y + param.kth * Mathf.Sin(p_err.th));

        return q_out;
    }

    velCmd apply_vel_limits(velCmd rawVelCmd, velCmd velLim)
    {
        velCmd out_vel = new velCmd(rawVelCmd.v,rawVelCmd.w);

        if (rawVelCmd.v > velLim.v)
            out_vel.v = velLim.v;
        //if (rawVelCmd.w > velLim.w)
        //    out_vel.w = velLim.w;

        return out_vel;
    }

    private void trajectory_iterations()
    {
        //control period
        float ctrl_freq = 10.0f;
        float ctrl_per = 1 / ctrl_freq;
        
        //count time and play
        //var t_start = DateTime.Now;
        if (pivot_ind < traj_to_run.ms.Count)
        {
            var t_delta = DateTime.Now.Subtract(t_start).TotalMilliseconds;
            /*
            if (t_delta >= traj_to_run.ms[pivot_ind]) //todo: search for cur ind
            {
                UnityEngine.Debug.Log("delta time: " + t_delta);
                UnityEngine.Debug.Log("Currnet traj. index: " + pivot_ind);
                pivot_ind++;
            } 
            */
            var prev_pivot = pivot_ind;
            pivot_ind = traj_to_run.pivot_search((int)t_delta, pivot_ind);
            if(prev_pivot != pivot_ind)
                UnityEngine.Debug.Log("New Pivot: " + pivot_ind);

            if (pivot_ind >= 0)
            {
                //apply pointed pose
                var ref_pose = traj_to_run.pos[pivot_ind];
                //get current robot pose                
                var cur_pose = new pose2d(robotBody.transform);
                //UnityEngine.Debug.Log(cur_pose.get_x());
                //trajectory tracking
                var cmd_to_go = kanayama_controller(ref_pose, cur_pose, traj_to_run.vel[pivot_ind]);

                //apply velocity limits
                cmd_to_go = apply_vel_limits(cmd_to_go, new velCmd(linear_vel_limit, angular_vel_limit));

                //command robot
                jackal_ctrl.apply_vel_cmd(cmd_to_go.v, cmd_to_go.w);
                UnityEngine.Debug.Log("v: " + cmd_to_go.v + " w: " + cmd_to_go.w);
                //UnityEngine.Debug.Log("v: " + cmd_to_go_lim.v + " w: " + cmd_to_go_lim.w);
            }            
        }
        if(pivot_ind == -3)
        {
            UnityEngine.Debug.Log("Running Trajectory Done");
            //stop robot
            jackal_ctrl.apply_vel_cmd(0.0f, 0.0f);
            run_traj = false;
        }
    }

    public void run_trajectory(List<pose2d> traj_p, List<velCmd> traj_vel, List<int> traj_t_ms)
    {
        //check if p and t have identical size
        if (traj_p.Count != traj_t_ms.Count)
        {
            UnityEngine.Debug.Log("trajectory pose and time size mismatch");
            return;
        }
        
        if (robotBody == null)
        {
            UnityEngine.Debug.Log("Robot Body not found");
            return;
        }

        t_start = DateTime.Now;
        run_traj = true;
        pivot_ind = 0;
        traj_to_run = new trajectory_set(traj_p, traj_vel, traj_t_ms);
        UnityEngine.Debug.Log("num of trj.: " + traj_to_run.pos.Count);
        trajectory_iterations();
    }
}
