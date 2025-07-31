using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csv_reader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Get the path of the Game data folder
        //var m_Path = Application.dataPath;
        //var params = read_csv(m_Path + "/robot_dim/jackal.csv");
    }

    //read csv
    public static IDictionary<string,string> read_csv(string filename, char delim = ',')
    {
        IDictionary<string, string> csv_dict = new Dictionary<string, string>();
        string[] lines = System.IO.File.ReadAllLines(filename);

        foreach (string line in lines)
        {
            string[] sp = line.Split(delim);

            if (sp.Length != 2)
            {
                UnityEngine.Debug.Log("parameter file error");
            }

            csv_dict.Add(sp[0], sp[1]);
        }

        return csv_dict;
    }
}
