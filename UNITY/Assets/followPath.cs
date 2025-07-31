using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class followPath : MonoBehaviour
{
    UnityEngine.AI.NavMeshAgent agent;
    public Button navBtn;
    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        navBtn.onClick.AddListener(gotoPoint);
    }

    public void gotoPoint()
    {
        var path_plan = new UnityEngine.AI.NavMeshPath();
        agent.CalculatePath(new Vector3(3.0f, 0.65f, 3.0f), path_plan);
        
        NavMeshHit _hit;
        var res = NavMesh.SamplePosition(new Vector3(2.0f, 0, 2.0f), out _hit, 0.35f, NavMesh.AllAreas);

        agent.SetDestination(new Vector3(3.0f, 0.65f, 3.0f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
