using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class trajectory_runner : MonoBehaviour
{
    private trajectory_tracking kanayama_ctrl;
    public Button run_traj_btn;
    private List<GameObject> traj_arrows;
    public GameObject arrow_prefab;
    // Start is called before the first frame update
    void Start()
    {
        kanayama_ctrl = gameObject.GetComponent(typeof(trajectory_tracking)) as trajectory_tracking;

        traj_arrows = new List<GameObject>();

        run_traj_btn.onClick.AddListener(run_traj);

        //GameObject test_arrow = Instantiate(arrow_prefab, new Vector3(1.0f,0.0f,1.0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void show_traj(List<pose2d> p)
    {
        traj_arrows.Clear();
        for(int i=0; i<p.Count; i++)
        {
            var position_unity = pose2d_ext.ros2unity(new Vector3(p[i].x, p[i].y, 0.0f));
            var orientation_unity = pose2d_ext.ros2unity(Quaternion.Euler(new Vector3(0, 0, p[i].th/3.141592f*180)));
            GameObject temp_arr = Instantiate(arrow_prefab, position_unity, orientation_unity);
            traj_arrows.Add(temp_arr);
        }
    }

    void run_traj()
    {
        // traj, vel, ms
        List<pose2d> test_pos = new List<pose2d> { new pose2d(0.5f, 0, 0), new pose2d(1.0f, 0, 0), new pose2d(1.2f, 0.2f, 0.785f), new pose2d(2.0f, 1.0f, 0.785f), new pose2d(3.0f, 2.0f, 0.785f), new pose2d(3.0f, 2.0f, 0.785f) };
        List<velCmd> test_vel = new List<velCmd> { new velCmd(0.05f, 0.0f), new velCmd(0.2f, 0.0f), new velCmd(0.2f, 0.52f), new velCmd(0.2f, 0.0f), new velCmd(0.15f, 0.0f), new velCmd(0.1f, 0.0f) };
        List<int> test_ms = new List<int> { 0, 5000, 10000, 13000, 16000,28000 };

        show_traj(test_pos);
        kanayama_ctrl.run_trajectory(test_pos, test_vel, test_ms);
    }
}
