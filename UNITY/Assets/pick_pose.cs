using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pick_pose : MonoBehaviour
{
    private Matrix4x4 _pick_pose_local;
    // Start is called before the first frame update
    void Start()
    {
        _pick_pose_local = Matrix4x4.identity;
        //_pick_pose_local.SetColumn(3, new Vector4(0, 0, 0.05f,1)); //ROS Coord

        //get object local scale
        var scale_vec3 = transform.localScale;
        _pick_pose_local.SetColumn(3, new Vector4(0, 0, scale_vec3.y/2, 1)); //ROS Coord
    }

    public Matrix4x4 getPickPose()
    {
        return _pick_pose_local;
    }   
}
