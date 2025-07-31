using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class axes_vis : MonoBehaviour
{
    public Toggle show_toggle;
    bool is_on = true;
    public void toggle()
    {
        if(is_on)
        {
            hide();
        }
        else
        {
            show();
        }
    }
    void show()
    {
        gameObject.SetActive(true);
        is_on = true;
    }
    void hide()
    {
        gameObject.SetActive(false);
        is_on = false;
    }

    void apply()
    {
        if(is_on)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }

    private void Start()
    {
        is_on = show_toggle.isOn;
        apply();
    }
}
