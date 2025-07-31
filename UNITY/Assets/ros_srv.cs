using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;

public class ros_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/robot_event";
    private overseer oseer;
    public string objType = "robot";

    [SerializeField] bool is_waiting_res;
    [SerializeField] int exp_id;
    [SerializeField] int rec_id;
    private int this_id;
    private string this_type;
    private robotEvent latestEvent;

    // Start is called before the first frame update
    void Start()
    {
        ros = GameObject.Find("ROSconnector").GetComponent<ROSConnection>();
        ros.RegisterRosService<robotEventsSrvRequest, robotEventsSrvResponse>(rosSrvName);
        oseer = GameObject.Find("overseer").GetComponent<overseer>();
        is_waiting_res = false;
        exp_id = -1;
        rec_id = -1;
        if(objType == "robot")
            this_id = gameObject.GetComponent<metadata>().get_id();
    }

    private void Update()
    {
        /*
        if(!is_waiting_res && (exp_id!=rec_id))
        {
            UnityEngine.Debug.Log("tyring again: " + exp_id.ToString());
            var out_srv = new robotEventsSrvRequest();
            out_srv.events = new RobotEventMsg[1];
            RobotEventMsg tempMsg = new RobotEventMsg();
            tempMsg.robot_id.data = latestEvent.id;
            tempMsg.event_detail.data = latestEvent.event_detail;
            tempMsg.target_name.data = latestEvent.target_name;
            out_srv.events[0] = tempMsg;
            ros.SendServiceMessage<robotEventsSrvResponse>(rosSrvName, out_srv, robotEventsResponseCallback);
        }
        */
    }

    /*
    public void callEvent(int robot_id, string event_detail)
    {
        
        var out_srv = new robotEventRequest();
        out_srv.robot_id.data = robot_id;
        out_srv.event_detail.data = event_detail;
        ros.SendServiceMessage<robotEventResponse>(rosSrvName, out_srv, robotEventResponseCallback);
        

    }
    */

    //wrap with list then call
    public void callEvent(robotEvent singleEvent)
    {
        callEvent(new List<robotEvent> { singleEvent });
    }

    public void callEvent(List<robotEvent> eventsList)
    {
        var out_srv = new robotEventsSrvRequest();
        out_srv.events = new RobotEventMsg[eventsList.Count];

        List<int> ids = new List<int>();
        for (int i = 0; i < 1; i++) //todo: modify to have one data at a time
        {
            RobotEventMsg tempMsg = new RobotEventMsg();
            tempMsg.robot_id.data = eventsList[i].id;
            //for debug
            ids.Add(eventsList[i].id);
            tempMsg.event_detail.data = eventsList[i].event_detail;
            tempMsg.target_name.data = eventsList[i].target_name;
            out_srv.events[i] = tempMsg;
        }
        string id_str = "";
        foreach (var id in ids)
        {
            id_str += id.ToString();
            id_str += " ";
        }
        //UnityEngine.Debug.Log("events: " + id_str);
        is_waiting_res = true;
        exp_id = eventsList[0].id;


        if (objType == "robot")
        {
            if (exp_id == this_id)
            {
                UnityEngine.Debug.Log("robot" + this_id + " called: " + exp_id);
                latestEvent = new robotEvent(eventsList[0]);
                ros.SendServiceMessage<robotEventsSrvResponse>(rosSrvName, out_srv, robotEventsResponseCallback);
            }
            else
            {
                UnityEngine.Debug.LogWarning("!!");
            }
        }

        else
        {
            latestEvent = new robotEvent(eventsList[0]);
            ros.SendServiceMessage<robotEventsSrvResponse>(rosSrvName, out_srv, robotEventsResponseCallback);
        }
        
    }

    void robotEventsResponseCallback(robotEventsSrvResponse res)
    {
        //check if there's next action
        //currently not expecting new task here
        //UnityEngine.Debug.Log("Event Accepted: " + res.new_task.robot_id.data);

        //var ids = new List<int>();
        //foreach (var r in res.new_tasks)
        //{
            UnityEngine.Debug.Log("Event Accepted: " + res.new_tasks[0].robot_id.data);
            //ids.Add(r.robot_id.data);
        //}
        //oseer.handle_srv_dropout(ids);

        is_waiting_res = false;
        rec_id = res.new_tasks[0].robot_id.data;
    }
}
