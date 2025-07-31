using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reserve_buffer : MonoBehaviour
{
    public GameObject reserve_mesh;
    private GameObject reserved_obj = null;
    private int buff_ind = 0;
    // Start is called before the first frame update
    //void Start()
    //{

    //}

    public int get_buff_ind()
    {
        return buff_ind;
    }
    public void set_buff_ind(int ind)
    {
        buff_ind = ind;
    }

    public void reserve_pos(Vector3 pos_ros,Vector3 sizev)
    {
        var pos_u = ros_conversions.ros2unity(pos_ros);

        //height
        var h = sizev.y; //unity coord.
        pos_u.y += h / 2;

        reserved_obj = Instantiate(reserve_mesh, pos_u, Quaternion.identity);

        //collision,gravity off
        /*
        reserved_obj.GetComponent<MeshCollider>().enabled = false;
        var rigidBodyComp = reserved_obj.GetComponent<Rigidbody>();
        if (rigidBodyComp != null)
        {
            rigidBodyComp.useGravity = false;
        }
        */

        reserved_obj.transform.localScale = sizev;
    }

    public void free_reservation()
    {
        if (reserved_obj != null)
        {
            //UnityEngine.Debug.Log("Destroy");
            Destroy(reserved_obj);
            reserved_obj = null;
        }
    }

    public Vector3 get_reserved_pos()
    {
        return ros_conversions.unity2ros(reserved_obj.transform.position);
    }
}
