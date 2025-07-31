using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class object_metadata : MonoBehaviour
{
    [SerializeField] string _name = "";
    [SerializeField] string _type = "";
    [SerializeField] int _pri = -1;
    [SerializeField] float _h_adj = 1f;
    [SerializeField] float _size = 1f;

    private Dictionary<string, Color> colorMap;
    void init_colorMap()
    {
        colorMap = new Dictionary<string, Color>();
        colorMap.Add("r", Color.red);
        colorMap.Add("g", Color.green);
        colorMap.Add("b", Color.blue);
        colorMap.Add("y", Color.yellow);
        colorMap.Add("c", Color.cyan);
    }

    public string objName
    {
        get
        {
            if (_name == "")
                return gameObject.name;
            else
                return _name;
        }
        set
        {
            _name = value;
        }
    }

    public string type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
            //set color
            set_color();
        }
    }
    public int priority
    {
        get
        {
            return _pri;
        }
        set
        {
            _pri = value;
        }
    }

    //height adjustment i.r.t. pri-size
    public float height_adj
    {
        get
        {
            return _h_adj;
        }
        set
        {
            _h_adj = value;
        }
    }

    public bool is_match(string type_in, int priority_in)
    {
        if(type_in == type && priority_in == priority)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float size
    {
        get
        {
            return _size;
        }
        set
        {
            //todo: apply to local scale
            _size = value;
        }        
    }

    void set_color()
    {
        if (colorMap == null)
            init_colorMap();

        if (colorMap.ContainsKey(_type))
        {
            var meshR = gameObject.GetComponent<MeshRenderer>();
            meshR.material.color = colorMap[_type];
        }
        else
        {
            UnityEngine.Debug.Log("type not found in Color Map");
        }
    }

    public float get_height()
    {
        return _size * _h_adj;
    }
   
    // Start is called before the first frame update
    void Start()
    {
        name = gameObject.name;
        init_colorMap();
    }
}
