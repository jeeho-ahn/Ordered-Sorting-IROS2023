using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class light_control : MonoBehaviour
{
    public Light spotLight;
    private manageRobot robotManager;
    public bool light_on = true;

    // Start is called before the first frame update
    void Start()
    {
        robotManager = GameObject.Find("overseer").GetComponent<manageRobot>();
    }

    // Update is called once per frame
    void Update()
    {
        if(robotManager.get_light_toggle() && !light_on)
        {
            light_on = true;
            spotLight.enabled = light_on;
        }
        else if(robotManager.get_light_toggle() == false && light_on)
        {
            light_on = false;
            spotLight.enabled = light_on;
        }
    }
}
