#region Assembly Firebase.TaskExtension, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// E:\Hernes\Assets\Firebase\Plugins\Firebase.TaskExtension.dll
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core.Player;
using System.Linq;
using System;
using System.Threading.Tasks;
using Firebase.Database;
using Hernes;

[RequireComponent(typeof(HVRPlayerController))]
public class PlayerManager : MonoBehaviour
{
    [Header("Player Initialize Value")]
    [SerializeField]
    protected PlayerData InitPlayer;
    [Header("Synced at Runtime")]
    public PlayerData Player;
    public PlayerStore Store
    {
        get
        {
            return SceneManager.instance.PlayerDataStore;
        }
    }
    public FirebaseStore Score
    {
        get
        {
            return SceneManager.instance.PlayerScoreStore;
        }
    }
    public FirebaseStore Events
    {
        get
        {
            return SceneManager.instance.PlayerEventsStore;
        }
    }
    public Dictionary<string, object> Data
    {
        get
        {
            return SceneManager.instance.PlayerDataStore?.Data;
        }
    }
    protected string _localJsonData = null;
    public string JsonData
    {
        get
        {
            return SceneManager.instance.PlayerDataStore?.JsonValue;
        }
    }
    public string Name
    {
        get
        {
            return Store.gameObject.name;
        }
        set
        {
            Store.gameObject.name = value;
            Score.gameObject.name = value;
        }
    }
    public List<string> PlayerNames = new List<string>();
    protected HVRPlayerController pc;
    private void Start()
    {
        pc = GetComponent<HVRPlayerController>();
    }
    // Update is called once per frame
    void Update()
    {
        if (SceneManager.instance.State == SceneManager.GameState.Playing && (Store.Status == PlayerStore.StoreState.Loaded /*|| Store.State == DataStore.StoreState.Empty*/) && Player.health <= 0f)
        {
            DespawnPlayer();
        }
        // Sync position and rotation
        Store.transform.position = transform.position;
        Store.transform.rotation = transform.rotation;
    }
    public double attritionDuration =  1200;
    public double sleepingAttritionDuration = (24 * 3600) * 7;
    private void FixedUpdate()
    {
        //if (SceneManager.instance.State == SceneManager.GameState.Playing)
        //{
        //    double attrition = Time.fixedDeltaTime * (1 / attritionDuration);
        //    var health = (1000 * Player.health - 1000 * attrition) / 1000;
        //    Player.health = (float)health;
        //    Player.Velocity = new Vector3(pc.xzVelocity.x, pc.yVelocity, pc.xzVelocity.z);
        //    Store.SetValue(Player.Dictionary, partial: true);
        //}
    }
    public async System.Threading.Tasks.Task InitPlayerData(bool setRemote = false)
    {
        DataSnapshot snapshot = await Store.GetValue();
        Debug.Log($"Player Manager retrieved player data ={snapshot.GetRawJsonValue()}");
        if (snapshot.Value != null)
        {
            Player = PlayerData.FromJson(snapshot.GetRawJsonValue());
            if (Player.state == "sleeping")
            {
                var attrition = (DateTime.UtcNow.Subtract(Store.Value.LastUpdatedAt.ToUniversalTime()).TotalSeconds / sleepingAttritionDuration);
                Debug.Log($"Applying attrition to player health {attrition}");
                Player.health = (1000 * Player.health - (float)(attrition * 1000)) / 1000;
                Player.state = InitPlayer.state;
            }
        }
        else
        {
            //Initialize new Player Object
            Player = InitPlayer;
        }
        Player.controller = "player";
        Player.StartAt = DateTime.Now;
        // Select Player Name
        if (Name == null || Name.Length == 0)
        {
            Name = PlayerNames.Random();
        }
        if (setRemote)
        {
            Player.Position = transform.position;
            Player.Rotation = transform.rotation;
            Player.Scale = transform.lossyScale;
            // Set Remote Player Value
            Store.SetRawValueBypass(Player.ToJson());
        }
    }
    public async System.Threading.Tasks.Task SpawnPlayer()
    {
        await InitPlayerData();
        // Select Position and Rotation for Spawn
        var spawn = GameObject.FindGameObjectsWithTag("PlayerSpawn").Where((s) => s.activeInHierarchy).ToList().Random();
        transform.position = spawn.transform.position;
        transform.rotation = spawn.transform.rotation;

        // Sync position and rotation
        Store.transform.position = transform.position;
        Store.transform.rotation = transform.rotation;

        spawn.SetActive(false);

        // Set Remote Player Value
        Store.SetRawValueBypass(Player.ToJson());
        Store.OnValueUpdate.AddListener(OnRemoteUpdated);
        // Activate Data Store
        Store.gameObject.SetActive(true);
    }
    public void DespawnPlayer()
    {
        Store.gameObject.SetActive(false);
        var origin = GameObject.FindGameObjectWithTag("PlayerOrigin");
        transform.position = origin.transform.position;
        transform.rotation = origin.transform.rotation;
        Score.SetValue(new Dictionary<string, object>()
        {
            { "time", (DateTime.UtcNow.Subtract(Player.StartAt.ToUniversalTime())).ToString() },
            { "lastUpdatedAt", DateTime.Now.ToString() },
        });
        Store.Remove();
        Store.gameObject.SetActive(false);
    }
    public void OnNameChange(string name)
    {
        Name = name;
        Debug.Log($"Name changed to {name}");
    }
    public void OnRemoteUpdated()
    {
        Debug.Log($"Player Manager - On Remote Updated {Store.Path}");
        if (Store.JsonValue != null)
        {
            Player = PlayerData.FromJson(Store.JsonValue);
        }
    }
    public void OnHeadsetRemoved()
    {
        Store.SetValue(new Dictionary<string, object>()
        {
            { "state", "sleeping" },
        }, partial: true);
    }
}
 