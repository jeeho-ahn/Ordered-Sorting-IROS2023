using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class tool_pose : MonoBehaviour
{
    private Matrix4x4 _r_tool_front,_r_tool_down;
    // Start is called before the first frame update
    void Start()
    {
        //tool is 90 deg rotated i.r.t. base in ROS
        _r_tool_front = jeehoTools.eul2R(new Vector3(-1*90*Mathf.Deg2Rad, 90 * Mathf.Deg2Rad, 0 * Mathf.Deg2Rad),"XYZ");
        //facing down means additional ry = 90 deg
        _r_tool_down = jeehoTools.eul2R(new Vector3(-0*Mathf.Deg2Rad, 90 * Mathf.Deg2Rad, 0))*_r_tool_front;
    }

    public Matrix4x4 get_R_tool(string toward="front")
    {
        if (toward == "front")
            return _r_tool_front;
        else if (toward == "down")
            return _r_tool_down;
        
        else
            UnityEngine.Debug.LogWarning("Invalid tool direction: " + toward);
        
        return Matrix4x4.identity;
    }
}
