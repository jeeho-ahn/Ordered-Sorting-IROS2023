using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate_body : MonoBehaviour
{
    public float target_vel;
    //public float target_pos; //deg
    private HingeJoint joint;
    public float max_w = 30.0f; //deg/s

    // Start is called before the first frame update
    void Start()
    {
        joint = GetComponent<HingeJoint>();
        target_vel = 0;
        //target_pos = 0;
    }

    // position in m, velocity in deg/s
    public void set_goal_vel(float target_velocity)
    {
        //target_pos = target_position;
        target_vel = target_velocity;
        if (target_vel > max_w)
            target_vel = max_w;
        else if (target_vel < max_w * -1)
            target_vel = max_w * -1;
    }

    public float get_cur_vel()
    { return target_vel; }

    // Update is called once per frame
    void FixedUpdate()
    {
        var dTime = Time.fixedDeltaTime;
        var dDeg = target_vel * dTime;

        //float cur_angle = IKsolver_gradient.getAnglefromAxis(this.transform.localEulerAngles, joint.axis);
        //float next_angle = cur_angle + dDeg;

        //transform.RotateAround(joint.anchor,joint.axis, dDeg);
        transform.Rotate(IKsolver_gradient.getEulerVecfromAxis(dDeg, joint.axis), Space.Self);
    }
}
