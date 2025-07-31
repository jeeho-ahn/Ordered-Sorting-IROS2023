using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class jackal_controller : differential_drive_controller
{
    public Button test_btn, stop_btn;
    public ArticulationBody[] left_wheels, right_wheels;
    public List<float> left_target_vel, right_target_vel;
    // Start is called before the first frame update
    void Start()
    {
        var m_Path = Application.dataPath;
        //var params = read_csv(m_Path + "/robot_dim/jackal.csv");
        parse_kinematics_params(m_Path + "/robot_dim/jackal.csv");
        test_btn.onClick.AddListener(test_drive);
        stop_btn.onClick.AddListener(stop_drive);

        //construct target velocity lists
        left_target_vel = new List<float>();
        right_target_vel = new List<float>();
        foreach(ArticulationBody wh in left_wheels){
            left_target_vel.Add(0.0f);
        }
        foreach(ArticulationBody wh in right_wheels){
            right_target_vel.Add(0.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //move motors
        //count delta time
        var dTime = Time.deltaTime;
        for (int i=0; i<left_target_vel.Count;i++)
        {
            var body = left_wheels[i];
            ArticulationDrive dr = body.xDrive;
            float tvel = left_target_vel[i];
            float dpos = tvel * dTime;
            var curVal = dr.target;
            var newVal = curVal + dpos;
            dr.target = newVal;
            body.xDrive = dr;
        }
        for (int i = 0; i < right_target_vel.Count; i++)
        {
            var body = right_wheels[i];
            ArticulationDrive dr = body.xDrive;
            float tvel = right_target_vel[i];
            float dpos = tvel * dTime;
            var curVal = dr.target;
            var newVal = curVal + dpos;
            dr.target = newVal;
            body.xDrive = dr;
        }
    }

    void test_drive() //in m/s rad/s
    {
        apply_vel_cmd(0.1f, 1.57f);
    }

    void stop_drive()
    {
        apply_vel_cmd(0.0f, 0.0f);
    }

    void update_target_vel(vel_two_wheels two_wheel)
    {
        for(int i=0;i<left_target_vel.Count;i++)
        {
            left_target_vel[i] = two_wheel.get_v_l()*Mathf.Rad2Deg; //to deg
        }
        for(int i = 0; i < right_target_vel.Count; i++)
        {
            right_target_vel[i] = two_wheel.get_v_r()*Mathf.Rad2Deg; //to deg
        }
    }

    //velocity command
    public void apply_vel_cmd(float v, float w)
    {
        var two_wheel = cmd_to_two_wheels(v, w);
        update_target_vel(two_wheel);
        //to four wheels
        /*
        foreach (ArticulationBody body in left_wheels){
            ArticulationDrive dr = body.xDrive;
            dr.target = two_wheel.get_v_l();
            body.xDrive = dr;
        }
        foreach (ArticulationBody body in right_wheels){
            ArticulationDrive dr = body.xDrive;
            dr.target = two_wheel.get_v_r();
            body.xDrive = dr;
        }
        */
    }
}
