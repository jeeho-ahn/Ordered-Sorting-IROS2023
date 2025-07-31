using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IKsolver_gradient : MonoBehaviour
{
    [SerializeField] GameObject[] object_chain;
    public int chain_length = 7;

    public GameObject ikTarget;
    public bool is_ee = true;
    [SerializeField] int actual_length=0;

    /*
    public float DistanceFromTarget(Vector3 target, float[] angles)
    {
        Vector3 point = ForwardKinematics(angles);
        return Vector3.Distance(point, target);
    }
    */

    public float SamplingDistance = 0.1f;
    public float LearningRate = 0.1f;
    public int max_it = 5000;
    public float tol_m = 0.0005f;
    public float tol_deg = 0.1f;
    //public bool move_robot = true;


    public static float getAnglefromAxis(Vector3 euler, Vector3 axis)
    {
        //dot product
        //return Vector3.Dot(euler, axis);

        //get axis val
        var cur_raw = Vector3.Dot(euler, axis);
        float cur = 0;
        //bring it within range 0 ~ 2pi
        if (cur_raw >= 360 || cur_raw <= -1 * 360)
        {
            var arg = (int)cur_raw / (int)360;
            cur = cur_raw - (arg * 360);
        }
        else
            cur = cur_raw;

        Vector3[] axis_mask = new Vector3[3];
        axis_mask[0] = new Vector3(1, 0, 0);
        axis_mask[1] = new Vector3(0, 1, 0);
        axis_mask[2] = new Vector3(0, 0, 1);
        //get values from other axes
        List<float> other_two = new List<float>();
        foreach(Vector3 m in axis_mask)
        {
            var temp_ang = Vector3.Dot(euler, m);
            if (temp_ang != cur_raw || cur == 0)
                other_two.Add(temp_ang);                
        }

        //for debug
        if (other_two.Count < 2)
            UnityEngine.Debug.LogWarning("??");

        // assume they are same
        if (other_two[0] == 0)
        {
            if (cur >= 0)
                return cur;
            else
                return cur + 360;
        }
        else if (other_two[0] == 180 || other_two[0] == -180)
            return 90 - cur + 90;
        else
            return cur; //don't know what could end up here
    }

    public static Vector3 getEulerVecfromAxis(float angle, Vector3 axis)
    {
        return Vector3.Scale(new Vector3(angle, angle, angle), axis);
    }

    Vector2 getMinMaxAngle(GameObject jointObj)
    {
        var lims = jointObj.GetComponent<HingeJoint>().limits;
        return new Vector2(lims.min, lims.max);
    }


    // Start is called before the first frame update
    void Start()
    {
        if (object_chain == null)
            get_object_chain();
    }

    public ikResult solve_ik(bool move_robot = true)
    {
        if(ikTarget==null)
        {
            UnityEngine.Debug.LogWarning("IK target object is missing");
            return null;
        }
        Vector3 target_pos = ikTarget.transform.position;
        Quaternion target_rot = ikTarget.transform.rotation;

        return solve_ik(target_pos, target_rot, move_robot);
    }

    public ikResult solve_ik(Vector3 target_pos, Quaternion target_rot, bool move_robot = true)
    {
        
        float[] angles = new float[actual_length];
        for(int j=0; j<actual_length; j++)
        {
            //object_chain[j].GetComponent<HingeJoint>().axis
            //angles[j] = object_chain[j].transform.localEulerAngles.x;
            angles[j] = getAnglefromAxis(object_chain[j].transform.localEulerAngles, object_chain[j].GetComponent<HingeJoint>().axis);
        }

        float[] angles_cp = new float[actual_length];
        System.Array.Copy(angles, angles_cp, actual_length);

        int it_count = 0;
        float mDist = 0;
        for (int it=0; it < max_it; it++)
        {
            it_count++;
            InverseKinematics(target_pos,target_rot, angles);
            //apply angles
            for (int j = 0; j < actual_length; j++)
                object_chain[j].transform.localEulerAngles = getEulerVecfromAxis(angles[j], object_chain[j].GetComponent<HingeJoint>().axis);
            mDist = Vector3.Distance(target_pos, this.transform.position);
            //if (mDist < tol_m && Quaternion.Angle(target_rot,this.transform.rotation)<tol_deg)
            if (mDist < tol_m && Mathf.Abs(Quaternion.Angle(target_rot, this.transform.rotation)) < tol_deg)
            {
                break;
            }
        }
        //return angles;
        //restore if not applying rightaway
        if (!move_robot)
        {
            for (int j = 0; j < actual_length; j++)
                object_chain[j].transform.localEulerAngles = getEulerVecfromAxis(angles_cp[j], object_chain[j].GetComponent<HingeJoint>().axis);
        }
        //UnityEngine.Debug.Log(it_count);

        //return angles;
        return new ikResult(mDist, angles);
    }

    
    public float PartialGradient(Vector3 target_pose,Quaternion target_rot, float[] angles, int i)
    {
        // Saves the angle,
        // it will be restored later
        float angle = angles[i];

        // Gradient : [F(x+SamplingDistance) - F(x)] / h
        //float f_x = DistanceFromTarget(target, angles);
        //get distance directly from unity
        float f_x = Vector3.Distance(target_pose, this.transform.position);
        float q1 = Quaternion.Angle(target_rot, this.transform.rotation);
        //var q1 = Quaternion.Inverse(target_rot) * this.transform.rotation;
        


        //angles[i] += SamplingDistance;
        var cur_angle = angle;
        var new_angle = cur_angle + SamplingDistance;
        object_chain[i].transform.localEulerAngles = getEulerVecfromAxis(new_angle, object_chain[i].GetComponent<HingeJoint>().axis);

        //float f_x_plus_d = DistanceFromTarget(target, angles);
        float f_x_plus_d = Vector3.Distance(target_pose, this.transform.position);
        float q2 = Quaternion.Angle(target_rot, this.transform.rotation);
        //var q2 = Quaternion.Inverse(target_rot) * this.transform.rotation;

        //var Qe1 = jeehoTools.rotationMatrixToEulerAngles(jeehoTools.quatToMatrix(pose2d_ext.unity2ros(q1)));
        //var Qe2 = jeehoTools.rotationMatrixToEulerAngles(jeehoTools.quatToMatrix(pose2d_ext.unity2ros(q2)));

        //var dq = Qe2 - Qe1;


        float gradient = (f_x_plus_d - f_x) / SamplingDistance;
        float grad_rot = (q2 - q1)/180f / SamplingDistance;
        //float grad_rot = (dq.x+dq.y+dq.z) / SamplingDistance;


        // Restores
        angles[i] = angle;
        object_chain[i].transform.localEulerAngles = getEulerVecfromAxis(angle, object_chain[i].GetComponent<HingeJoint>().axis);

        //return gradient;
        return gradient + 0.2f*grad_rot;
    }

    public void InverseKinematics (Vector3 target_pos, Quaternion target_rot, float [] angles)
    {
        for (int i = 0; i < actual_length; i ++)
        {
            // Gradient descent
            // Update : Solution -= LearningRate * Gradient
            float gradient = PartialGradient(target_pos,target_rot, angles, i);
            angles[i] -= LearningRate * gradient;

            //clamp into limits
            //var minMax = getMinMaxAngle(object_chain[i]);
            //angles[i] = Mathf.Clamp(angles[i], minMax.x, minMax.y);
        }
    }

    public GameObject[] get_object_chain()
    {
        //init chain array
        if (is_ee)
        {
            actual_length = chain_length - 1;
            object_chain = new GameObject[chain_length - 1];
            //end_effector
            object_chain[chain_length - 2] = this.gameObject.transform.parent.gameObject;
            for (int i = chain_length - 3; i >= 0; i--)
            {
                object_chain[i] = object_chain[i + 1].transform.parent.gameObject;
            }
        }
        else
        {
            actual_length = chain_length;
            object_chain = new GameObject[chain_length];
            //end_effector
            object_chain[chain_length - 1] = this.gameObject;
            for (int i = chain_length - 2; i >= 0; i--)
            {
                object_chain[i] = object_chain[i + 1].transform.parent.gameObject;
            }
        }
        return object_chain;
    }
}
