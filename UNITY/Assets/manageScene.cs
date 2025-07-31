using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class manageScene : MonoBehaviour
{
    // add on Editor
    public GameObject objPrefab = null;
    // add on Editor
    public GameObject dropStationPrefab = null;
    // size table
    Dictionary<int, float> sizeMap;
    

    private overseer oseer;


    public void set_drop_stations(List<dropStation> dsList_in)
    {
        //clear current map
        //dropStationsMap.Clear();
        oseer.clear_dropStationsMap();
        foreach(var it in dsList_in)
        {
            add_drop_station(it.Type, it.PosRos);
        }
    }

    void add_drop_station(string type_in, Vector3 pos_ros, bool use_fx = true)
    {
        //if (dropStationsMap != null)
       // {
        var temp = Instantiate(dropStationPrefab, ros_conversions.ros2unity(pos_ros), Quaternion.identity);
        temp.GetComponent<drop_station_metadata>().init_station(new dropStation(type_in, pos_ros));
        if (use_fx)
            temp.GetComponent<ring_fx>().turn_on_ring(type_in);

        //dropStationsMap.Add(type_in, temp);
        oseer.add_to_dropStationsMap(type_in, temp);
        //}
    }

    public GameObject get_drop_station(string type)
    {
        var dropStationsMap = oseer.get_dropStationsMap();
        if(dropStationsMap.ContainsKey(type))
        {
            return dropStationsMap[type];
        }
        else
        {
            UnityEngine.Debug.Log("drop station not found");
            return null;
        }
    }

    private void init_sizeMap()
    {
        //fill object size map by priority
        sizeMap = new Dictionary<int, float>();
        sizeMap.Add(1, 0.3f);
        sizeMap.Add(2, 0.45f);
        sizeMap.Add(3, 0.6f);
    }

    // Start is called before the first frame update
    void Start()
    {
        if(objPrefab == null)
        {
            UnityEngine.Debug.LogError("ICRA Object Prefab is missing");
        }

        oseer = gameObject.GetComponent<overseer>();

        init_sizeMap();
        //init dropstations (will be initialized by python later)        
        //add_drop_station("r", new Vector3(-1.45f, -1.45f, 0f));
        //add_drop_station("g", new Vector3(-1.45f, 1.45f, 0f));
        //add_drop_station("b", new Vector3(-3f, 0f, 0f));
    }

    // Update is called once per frame
    void Update()
    {
        NavMesh.avoidancePredictionTime = 2;
        NavMesh.pathfindingIterationsPerFrame = 50;
    }

    public void set_pri_size(Dictionary<int, float> new_size_map)
    {
        sizeMap = new_size_map;
    }

    public Dictionary<int,float> get_pri_size()
    {
        return sizeMap;
    }

    public void reset_obj()
    {
        //remove from scene
        oseer.remove_all_obj();
    }

    public void add_obj(icraObj obj_in, bool adjust_z = true)
    {
        //object dimension by priority
        var size = sizeMap[obj_in.Priority];
        // set height squeeze
        var h_sqeeze = 0.15f; //todo: make this a variable
        var height = size * h_sqeeze;
        //convert to Unity Coord
        var pos_u = ros_conversions.ros2unity(obj_in.PosRos);
        if(adjust_z)
            pos_u.y = height/2; 
        var rot_u = ros_conversions.ros2unity(obj_in.RotationRos);
        var obj2add = Instantiate(objPrefab, pos_u, rot_u);
        
        obj2add.transform.localScale = new Vector3(size, height, size);
        //obj2add.name = obj_in.Type + obj_in.ToString();
        obj2add.name = obj_in.Name;
        var objMeta = obj2add.GetComponent<object_metadata>();
        objMeta.objName = obj_in.Name;
        objMeta.type = obj_in.Type;
        objMeta.priority = obj_in.Priority;
        objMeta.height_adj = h_sqeeze;
        objMeta.size = size;

        //obj2add.GetComponent<object_metadata>().objName = obj_in.Name;

        oseer.objectList.Add(obj2add);
    }

    public void remove_object(icraObj obj_in)
    {
        remove_object(obj_in.Name);
    }

    public void remove_object(string obj_name)
    {
        //remove from list
        for (int i = 0; i < oseer.objectList.Count; i++)
        {
            if (oseer.objectList[i].GetComponent<object_metadata>().objName == obj_name)
            {
                GameObject o = oseer.objectList[i];
                //remove from list
                oseer.objectList.RemoveAt(i);
                //destroy instance
                Destroy(o);
            }
        }
    }

    public void remove_objects(icraObj[] objs_in)
    {
        foreach(var o in objs_in)
        {
            remove_object(o);
        }
    }
    public void remove_objects(List<icraObj> objs_in)
    {
        foreach (var o in objs_in)
        {
            remove_object(o);
        }
    }

    //remove all objects
    public void remove_objects()
    {
        oseer.remove_all_obj();
    }


    public GameObject find_obj(string obj_name)
    {
        GameObject out_obj = null;
        foreach(GameObject o in oseer.objectList)
        {
            if (o.GetComponent<object_metadata>().objName == obj_name)
            {
                out_obj = o;
                break;
            }
        }

        if (out_obj == null)
            UnityEngine.Debug.LogWarning("object name not found: " + obj_name);

        return out_obj;
    }

    public GameObject find_obj_by_type_pri(string type, int priority)
    {
        GameObject out_obj = null;
        foreach (var o in oseer.objectList)
        {
            if (o.GetComponent<object_metadata>().is_match(type,priority))
            {
                out_obj = o;
                break;
            }
        }

        if (out_obj == null)
        {
            UnityEngine.Debug.LogWarning("object match not found: "
                + type + " " + priority.ToString());
        }

        return out_obj;
    }

    public void reset_manager()
    {
        init_sizeMap();
    }
}
