using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ring_fx : MonoBehaviour
{

    private Dictionary<string, int> colorMap;
    public GameObject ring_white;
    //must be stored in order from Editor
    public List<GameObject> ringList;

    private GameObject activated_ring = null;

    void initialize_colorMap()
    {
        colorMap = new Dictionary<string, int>();
        colorMap.Add("r", 0);
        colorMap.Add("g", 1);
        colorMap.Add("b", 2);
        colorMap.Add("y", 3);
        colorMap.Add("c", 4);
    }

    public void turn_on_ring(string c, bool init_colorMap = true)
    {
        if (init_colorMap)
            initialize_colorMap();
        GameObject ring;
        if (colorMap.ContainsKey(c))
            ring = ringList[colorMap[c]];
        else
            ring = ring_white;

        ring.SetActive(true);
        activated_ring = ring;
    }

    public void turn_off_ring()
    {
        if (activated_ring)
            activated_ring.SetActive(false);
    }
}
