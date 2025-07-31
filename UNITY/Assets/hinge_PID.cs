using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class hinge_PID : MonoBehaviour
{
    private HingeJoint joint;
    private Rigidbody body;
    private rotate_body rBody;

    private PID pid_controller;

    public float P=1, I=1, D=1;

   // public float kc = 10;
    //public float tau_i = 0.05f, tau_d = 0.01f;
    //public float kp=0.5f, ki, kd;

    //public float max_v = 0.5f;
    public float tol_deg = 0.1f;

    [SerializeField] float target = 0;
    [SerializeField] float target_deg = 0;

    //[SerializeField] float ierr = 0;
    [SerializeField] float err_stamp = 0;
    [SerializeField] float latest_deg = 0;
    [SerializeField] float cur_ang = 0;
    [SerializeField] Vector3 cur_angles;
    [SerializeField] float cqw,cqx,cqy,cqz;
    //[SerializeField] float pid_out = 0;

    private DateTime latest_time;

    public bool is_running = false;
    // Start is called before the first frame update
    void Start()
    {
        //ki = kc / tau_i;
        //kd = kc * tau_d;
        joint = GetComponent<HingeJoint>();
        //body = this.gameObject.GetComponent<Rigidbody>();
        rBody = GetComponent<rotate_body>();
        latest_time = DateTime.Now;
        //joint.useMotor = true;
        //var mot = joint.motor;
        //mot.force = 10000;
        //joint.motor = mot;

        pid_controller = new PID(P,I,D);
    }

    private float get_current_angle()
    {
        //Unity's Quaternion to euler flips unwantedly
        //var mat = jeehoTools.quatToMatrix(this.transform.localRotation);
        //var eul = jeehoTools.rotationMatrixToEulerAngles(mat);
        var eul = jeehoTools.quat2Eul(this.transform.localRotation);
        return IKsolver_gradient.getAnglefromAxis(eul*Mathf.Rad2Deg, joint.axis);

        //return IKsolver_gradient.getAnglefromAxis(this.transform.localRotation.eulerAngles, joint.axis);
        //return IKsolver_gradient.getAnglefromAxis(UnityEditor.TransformUtils.GetInspectorRotation(transform), joint.axis);
    }

    public void pid_start(float t_angle_rad)
    {
        //ierr = 0;
        latest_deg = get_current_angle();
        //pid_out = 0;
        target = t_angle_rad;
        err_stamp = target - latest_deg;
        target_deg = (target * Mathf.Rad2Deg)%360.0f;
        is_running = true;
        pid_controller.reset_i();
    }

    private float dAngle(float target, float source)
    {
        /*
        var a = target - source;
        a += (a > 180) ? -360 : (a < -180) ? 360 : 0;

        */
        var a = Mathf.DeltaAngle(source, target);
        float negpos = 1;
        //if (target > source)
        //    negpos = -1;

        return a*negpos;
    }

    public void pid_iteration(float t_angle_deg, float dtime)
    {
        /*
        float cur_angle_deg = get_current_angle();
        float err = t_angle_deg - cur_angle_deg;
        err_stamp = err;

        ierr = ierr + ki * err * dtime;
        var dpv = (cur_angle_deg - latest_deg) / dtime;

        var P = kp * err;
        var I = ierr;
        var D = -1 * kd * dpv;

        pid_out = pid_out + P + I + D;

        if (pid_out > max_v)
            pid_out = max_v;
        else if (pid_out < -1 * max_v)
            pid_out = -1 * max_v;

        latest_deg = cur_angle_deg;

        //command
        //var mot = joint.motor;
        //mot.targetVelocity = pid_out*Mathf.Rad2Deg;
        //joint.motor = mot;
        rBody.set_goal_vel(pid_out);
        */
        float cur_angle_deg = get_current_angle();
        //float err = t_angle_deg - cur_angle_deg;
        //float err = Mathf.DeltaAngle(cur_angle_deg, t_angle_deg);
        float err = dAngle(t_angle_deg, cur_angle_deg);
        var corr = pid_controller.GetOutput(err, dtime);
        var curVel = rBody.get_cur_vel();
        rBody.set_goal_vel(curVel + corr);
        err_stamp = err;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (is_running)
        {
            if (Mathf.Abs(err_stamp) < Mathf.Abs(tol_deg))
            {
                //finish
                is_running = false;
                //pid_out = 0;
                rBody.set_goal_vel(0);
            }

            //var dtime_mm = DateTime.Now.Subtract(latest_time).TotalMilliseconds;
            pid_iteration(target_deg, Time.fixedDeltaTime);
        }

        //Vector3 eulVel = IKsolver_gradient.getEulerVecfromAxis(pid_out*Mathf.Rad2Deg, joint.axis);
        //Quaternion dRotation = Quaternion.Euler(eulVel * Time.fixedDeltaTime);
        //body.MoveRotation(body.rotation * dRotation);
        else
        {
            rBody.set_goal_vel(0);
        }

        //for monitoring
#if UNITY_EDITOR
        var cur_q = this.transform.localRotation;
        cqw = cur_q.w; cqx = cur_q.x; cqy = cur_q.y; cqz = cur_q.z;

        //test
        //var mat = jeehoTools.quatToMatrix(cur_q);
        //var eul = jeehoTools.rotationMatrixToEulerAngles(mat);
        var eul = jeehoTools.quat2Eul(cur_q);
        cur_ang = get_current_angle();
        //cur_angles = cur_q.eulerAngles;
        cur_angles = eul*Mathf.Rad2Deg;
        
#endif
    }
}
