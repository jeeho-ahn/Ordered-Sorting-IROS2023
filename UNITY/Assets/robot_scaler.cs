using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class robot_scaler : MonoBehaviour
{
    public void apply_scale(float scale_in)
    {
        if(scale_in>0)
        {
            gameObject.transform.localScale = new Vector3(scale_in, scale_in, scale_in);
        }
    }
}
