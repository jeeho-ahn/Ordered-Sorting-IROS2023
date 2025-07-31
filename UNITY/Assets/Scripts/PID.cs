using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PID
{
    private float _kp,_ki,_kd;

    public float Kp
    {
        get
        {
            return _kp;
        }
        set
        {
            _kp = value;
        }
    }
    public float Ki
    {
        get
        {
            return _ki;
        }
        set
        {
            _ki = value;
        }
    }
    public float Kd
    {
        get
        {
            return _kd;
        }
        set
        {
            _kd = value;
        }
    }

    private float _p, _i, _d;
    private float prevErr;

    public PID(float p, float i, float d)
    {
        _kp = p;
        _ki = i;
        _kd = d;
    }

    public void reset_i()
    {
        _i = 0;
    }

    public float GetOutput(float curErr, float dTime)
    {
        _p = curErr;
        _i += _p * dTime;
        _d = (_p - prevErr) / dTime;
        prevErr = curErr;

        return _p * Kp + _i * Ki + _d * Kd;
    }
}
