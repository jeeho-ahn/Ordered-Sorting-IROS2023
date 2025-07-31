using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vel_two_wheels
{
    float v_l, v_r=0.0f;

    public void set_v_l(float v_in)
    {
        v_l = v_in;
    }

    public void set_v_r(float v_in)
    {
        v_r = v_in;
    }

    public float get_v_l()
    {
        return v_l;
    }
    public float get_v_r()
    {
        return v_r;
    }
}
class vel_four_wheels
{
    float v_l1, v_l2=0.0f;
    float v_r1, v_r2=0.0f;
}

public class differential_drive_controller : MonoBehaviour
{
    // Start is called before the first frame update
    public float wheel_dist = 1.0f;
    public float wheel_radius = 1.0f;

    //void Start()
    //{
        
    //}

    public void parse_kinematics_params(string filename)
    {
        var diff_param_csv = csv_reader.read_csv(filename);
        wheel_dist = float.Parse(diff_param_csv["track"]);
        wheel_radius = float.Parse(diff_param_csv["wheel_radius"]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public vel_two_wheels cmd_to_two_wheels(float v, float w)
    {
        vel_two_wheels two_wh_vel = new vel_two_wheels();
        //common term
        float ct = w * (wheel_dist / 2);
        //two wheels
        // (v - w(l/2))/r
        two_wh_vel.set_v_l((v - ct) / wheel_radius);
        // (v + w(l/2))/r
        two_wh_vel.set_v_r((v + ct) / wheel_radius);

        return two_wh_vel;
    }
}
