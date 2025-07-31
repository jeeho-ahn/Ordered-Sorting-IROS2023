using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.AI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;

public class gen_gridmap : MonoBehaviour
{
    public float width_m = 20;
    public float height_m = 20;
    public float row_off = -10;
    public float col_off = -10;

    public float step_m = 0.05f;

    public int pub_freq = 2;
    int pub_per;

    public Button pub_map_btn;

    OccupancyGridMsg occu_map;
    public ROSConnection ros;
    public string topic_name = "unity_gridmap";
    public string frame_name = "map";
    bool map_ready = false;

    DateTime time_lastpub;

    // Start is called before the first frame update
    void Start()
    {        
        //ROS gridmap type
        occu_map = new OccupancyGridMsg();
        //pub period
        pub_per = 1 * 1000 / pub_freq;
        //register publisher        
        //ros = ROSConnection.GetOrCreateInstance();
        ros = GameObject.Find("ROSconnector").GetComponent<ROSConnection>();
        ros.RegisterPublisher<OccupancyGridMsg>(topic_name);

        pub_map_btn.onClick.AddListener(generate_gridmap);
    }

    void generate_gridmap()
    {
        int num_row = (int)(height_m / step_m);
        int num_col = (int)(width_m / step_m);

        //offset
        PoseMsg map_offset = new PoseMsg();
        map_offset.position.x = col_off;
        map_offset.position.y = row_off;
        occu_map.info.origin = map_offset;

        //freq
        time_lastpub = DateTime.Now;
        // map meta data
        occu_map.header.frame_id = frame_name;
        occu_map.info.height = (uint)num_row;
        occu_map.info.width = (uint)num_col;
        occu_map.info.resolution = step_m;
        occu_map.data = new sbyte[num_row * num_col];

        DateTime t_start = DateTime.Now;

        int nFalse = 0;

        for (int i = 0; i < num_row; i++)
        {
            for (int j = 0; j < num_col; j++)
            {
                //get position
                float x = col_off + step_m * j;
                float y = row_off + step_m * i;
                //Vector3 p = new Vector3(x, 0, y);
                Vector3 p = ros_conversions.ros2unity(new Vector3(x,y,0));
                NavMeshHit _hit;
                var res = NavMesh.SamplePosition(p, out _hit, step_m*1.01f, NavMesh.AllAreas);
                int ser_ind = j + i * num_col;
                if (res == false)
                {
                    nFalse++;
                    occu_map.data[ser_ind] = 100;
                }
                else
                {
                    occu_map.data[ser_ind] = 0;
                }

            }
        }

        DateTime t_end = DateTime.Now;
        map_ready = true;

        UnityEngine.Debug.Log("Time took to generate gridmap: " + (t_end - t_start).TotalMilliseconds);
        UnityEngine.Debug.Log("nFalse: " + nFalse);
    }

    public void generate_and_pub()
    {
        generate_gridmap();
        ros.Publish(topic_name, occu_map);
    }

    /*
    // Update is called once per frame
    void Update()
    {
        if (DateTime.Now.Subtract(time_lastpub).TotalMilliseconds > pub_per
            && map_ready==true)
        {
            ros.Publish(topic_name, occu_map);
            time_lastpub = DateTime.Now;
        }
    }
    */
}
