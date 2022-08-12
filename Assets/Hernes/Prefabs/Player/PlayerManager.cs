using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Player;
using System.Linq;
public class PlayerManager : DataStore
{
    public GameObject enemyPrefab;
    // Make enemy prefab which tracks enemy movement
    public GameObject enemyAIPrefab;
    // Make enemy prefab which updates enemy prefab and wanders, and shoots at vfx detection with delay.
    public Dictionary<string, object> enemies = null;

    // Hit effect needs to spawn vfx that spawns vfx remotely
    public GameObject hitEffect;

    [Header("---------------Player-----------------")]

    public string playerName;
    public string type;
    public Vector3 velocity = Vector3.zero;
    public string state;
    public float health;
    public Vector3 position = Vector3.zero;

    public float syncDelay = 1f;
    public HVRPlayerController pc;
    public override Dictionary<string, object> Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
            OnValueUpdate.Invoke(value);
            if (value == null)
            {
                OnEmpty();
            }
            if (value.ContainsKey("type"))
            {
                type = value["type"] as string;
            }
            if (value.ContainsKey("state"))
            {
                state = value["state"] as string;
            }
            if (value.ContainsKey("health"))
            {
                health = (float)value["health"];
            }
            if (value.TryGetValue("pos", out var p))
            {
                var pos = p as List<float>;
                transform.position = new Vector3(pos[0], pos[1], pos[2]);
            }
            if (value.TryGetValue("velocity", out var v))
            {
                var pos = v as List<float>;
                transform.position = new Vector3(pos[0], pos[1], pos[2]);
            }
        }
    }
    public override string Path
    {
        get
        {
            return scenePath + modulePath + playerName;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0f)
        {
            DespawnPlayer();
        }
    }
    public void SpawnPlayer()
    {
        if (name == null)
        {
            // select random name
        }
        // Spawn enemies
        
        // Track enemy added and spawn enemies



        playerName = name;
        List<GameObject> spawns = GameObject.FindGameObjectsWithTag("SpawnPlayer").ToList();
        int i = Mathf.RoundToInt(Random.value * (spawns.Count - 1));
        transform.position = spawns[i].transform.position;
        transform.rotation = spawns[i].transform.rotation;
        Destroy(spawns[i]);
        spawns.RemoveAt(i);
        StopAllCoroutines();
        StartCoroutine(SyncDatabase());
    }
    public void DespawnPlayer()
    {
        var origin = GameObject.FindGameObjectWithTag("PlayerOrigin");
        transform.position = origin.transform.position;
        transform.rotation = origin.transform.rotation;


        // Remove enemies
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(var enemy in enemies)
        {
            Destroy(enemy);
        }
    }
    public void OnHit(Vector3 pos, Vector3 dir, GameObject gO)
    {
        health -= 0.4f;
        Instantiate(hitEffect, pos, Quaternion.LookRotation(Vector3.up, dir));
    }
    protected virtual IEnumerator SyncDatabase()
    {
        while (isActiveAndEnabled)
        {
            yield return new WaitForSeconds(syncDelay);
            SyncDBValue();
        }
    }
    public void SyncDBValue()
    {
        var dict = new Dictionary<string, object>
        {
            { "type", type },
            { "pos", new List<float>() { transform.position.x, transform.position.y, transform.position.z } },
            { "velocity", new List<float>() { pc.xzVelocity.x, pc.yVelocity, pc.xzVelocity.z } },
            { "state", state },
            { "health", health },
        };
        SetRemoteValue(dict);
    }
}
