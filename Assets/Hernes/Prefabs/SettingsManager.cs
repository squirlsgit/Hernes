using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
public class SettingsManager : FirebaseManager
{
    public float _fog;
    public float Fog
    {
        get
        {
            return _fog;
        }
        set
        {
            if (value != _fog)
            {
                FogVolume.maximumHeight.value = value;
            }
            _fog = value;
        }
    }
    public string _time;
    public string Time
    {
        get
        {
            return _time;
        }
        set
        {
            _time = value;
        }
    }
    public Volume volume;
    public Fog FogVolume
    {
        get
        {
            if (volume.sharedProfile.TryGet<Fog>(out var f))
            {
                return f;
            }
            else return null;
        }
    }
    protected override void UpdateLocalValue(DataSnapshot snapshot)
    {
        base.UpdateLocalValue(snapshot);
        Fog = (float)value["fog"];
        Time = (string)value["time"];
    }

    // Update is called once per frame
    void Update()
    {
       // Swap videos if time of day changed?

       // Blend Fog Heights with Animation Curve.
    }
}
