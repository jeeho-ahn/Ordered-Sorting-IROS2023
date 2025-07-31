using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics;


public class control_motors : MonoBehaviour
{
    public Button controlBtn;
    public InputField valField;
    public ArticulationBody[] wheel_bodies;
    private ArticulationDrive[] wheel_drives;

    float increm = 1.0f;
    bool auto_stop = true;
    // Start is called before the first frame update
    void Start()
    {
        controlBtn.onClick.AddListener(setTargetAngles);
        //fwBtn.onClick.AddListener(goForward);        
        valField.text = increm.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w"))
        {
            //print("space key was pressed");
            goForward(1.0f);
        }
        else if (Input.GetKey("s"))
        {
            //print("space key was pressed");
            goForward(-1.0f);
        }
        else if (Input.GetKey("e"))
        {
            //print("space key was pressed");
            turn(1.0f);
        }
        else if (Input.GetKey("q"))
        {
            //print("space key was pressed");
            turn(-1.0f);
        }
        else
        {
            if(auto_stop)
                stop();
        }
    }

    void setTargetAngles()
    {
        if (auto_stop)
            auto_stop = false;
        else
            auto_stop = true;
        //get drives
        foreach (ArticulationBody body in wheel_bodies)
        {
            ArticulationDrive dr = body.xDrive;
            dr.target = 100;
            body.xDrive = dr;
        }
    }

    void goForward(float dir)
    {
        increm = float.Parse(valField.text);
        float inc = dir * increm;

        //get drives
        foreach (ArticulationBody body in wheel_bodies)
        {
            ArticulationDrive dr = body.xDrive;
            var curVal = dr.target;
            var newVal = curVal + inc;
            dr.target = newVal;
            body.xDrive = dr;
        }
    }

    void turn(float dir)
    {
        increm = float.Parse(valField.text);
        
        for(int i=0; i<4; i++)
        {
            float inc = dir * increm;
            if (i % 2 == 0)
                inc *= -1.0f;
            ArticulationDrive dr = wheel_bodies[i].xDrive;
            var curVal = dr.target;
            var curPos = wheel_bodies[i].jointPosition[0] * Mathf.Rad2Deg;
            //if (Mathf.Abs(curVal - curPos) < 100)
            //{
                var newVal = curVal + inc*0.5f;
                dr.target = newVal;
                wheel_bodies[i].xDrive = dr;
            //}
        }

    }

    void stop()
    {
        foreach (ArticulationBody body in wheel_bodies)
        {
            ArticulationDrive dr = body.xDrive;
            var curVal = dr.target;
            var curPos = body.jointPosition[0] * Mathf.Rad2Deg;
            var newVal = curPos;
            dr.target = newVal;
            body.xDrive = dr;
        }
    }
}
