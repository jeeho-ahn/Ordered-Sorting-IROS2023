using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Icra2023Pkg;
using Unity.Robotics.ROSTCPConnector;

public class robot_event_srv : MonoBehaviour
{
    private ROSConnection ros;
    public string rosSrvName = "/unity/robot_event";
    private overseer oseer;
    private bool _is_busy;
    private List<List<robotEvent>> _waitQueue;

    // Start is called before the first frame update
    void Start()
    {
        ros = gameObject.GetComponent<ROSConnection>();
        ros.RegisterRosService<robotEventsSrvRequest, robotEventsSrvResponse>(rosSrvName);
        oseer = GameObject.Find("overseer").GetComponent<overseer>();
        _is_busy = false;
        _waitQueue = new List<List<robotEvent>>();
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

    private void Update()
    {
        if (!_is_busy)
        {
            if (_waitQueue.Count>0)
            {
                callEvent(_waitQueue[0]);
                _waitQueue.RemoveAt(0);
            }
        }                
    }

    //wrap with list then call
    public void callEvent(robotEvent singleEvent)
    {
        callEvent(new List<robotEvent>{ singleEvent });
    }

    public void callEvent(List<robotEvent> eventsList)
    {
        if (_is_busy)
        {
            _waitQueue.Add(eventsList);
        }
        else
        {
            _is_busy = true;
            var out_srv = new robotEventsSrvRequest();
            out_srv.events = new RobotEventMsg[eventsList.Count];

            List<int> ids = new List<int>();
            for (int i = 0; i < eventsList.Count; i++)
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
            UnityEngine.Debug.Log("events: " + id_str);
            
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

        //is_waiting_res = false;
        //rec_id = res.new_tasks[0].robot_id.data;
    }
}
