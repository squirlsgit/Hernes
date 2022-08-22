using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public struct ItemData : IFirebaseRecord
{
    // Player name, can be null if not held.
    public string controller;
    public List<float> angularVelocity;
    public List<float> velocity;
    public List<float> position;
    public List<float> rotation;
    public List<float> scale;
    public string type;
    public string lastUpdatedAt;
    public string Type
    {
        get { return type; }
    }
    public DateTime LastUpdatedAt
    {
        get
        {
            return DateTime.Parse(lastUpdatedAt);
        }
        set
        {
            lastUpdatedAt = value.ToUniversalTime().ToString();
        }
    }
    public Vector3 AngularVelocity
    {
        get
        {
            return angularVelocity.GetVector();
        }
        set
        {
            if (value != null)
            {
                angularVelocity = new List<float>() { value.x, value.y, value.z };
            }
            else
            {
                angularVelocity = null;
            }
        }
    }
    public Vector3 Velocity
    {
        get
        {
            return velocity.GetVector();
        }
        set
        {
            if (value != null)
            {
                velocity = new List<float>() { value.x, value.y, value.z };
            }
            else
            {
                velocity = null;
            }
        }
    }
    public Vector3 Position
    {
        get
        {
            return position.GetVector();
        }
        set
        {
            if (value != null)
            {
                position = new List<float>() { value.x, value.y, value.z };
            }
            else
            {
                position = null;
            }
        }
    }
    public Quaternion Rotation
    {
        get
        {
            return rotation.GetEuler();
        }
        set
        {
            if (value != null)
            {
                rotation = new List<float>() { value.eulerAngles.x, value.eulerAngles.y, value.eulerAngles.z };
            }
            else
            {
                rotation = null;
            }
        }
    }
    public Vector3 Scale
    {
        get
        {
            return scale.GetVector();
        }
        set
        {
            if (value != null)
            {
                scale = new List<float>() { value.x, value.y, value.z };
            }
            else
            {
                scale = null;
            }
        }
    }
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    public static ItemData FromJson(string json)
    {
        if (json == null)
        {
            return new ItemData();
        }
        return JsonUtility.FromJson<ItemData>(json);
    }
    public Dictionary<string, object> Dictionary
    {
        get
        {
            return new Dictionary<string, object>()
            {
                { "type", type },
                { "lastUpdatedAt", lastUpdatedAt },
                { "controller", controller },
                { "angularVelocity", angularVelocity },
                { "velocity", velocity },
                { "position", position },
                { "rotation", rotation },
            };
        }
    }
}
public class WeaponStore : DataStore<ItemData>
{
}
