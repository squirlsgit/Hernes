using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.Events;

[System.Serializable]
public class Register<T>
{
    public Dictionary<T, List<T>> register = new Dictionary<T, List<T>>();
    [SerializeField]
    private List<T> _debugRegistry = new List<T>();
    public bool Contains(T mode, T item)
    {
        return register.ContainsKey(mode) && register[mode].Contains(item);
    }
    public bool Contains(T item)
    {
        foreach (var kvp in register)
        {
            if (kvp.Value.Contains(item))
            {
                return true;
            }
        }
        return false;
    }
    public void Add(T mode, T item)
    {
        if (!register.ContainsKey(mode))
        {
            register[mode] = new List<T>();
        }
        if (!register[mode].Contains(item))
        {
            register[mode].Add(item);
            _debugRegistry.Add(item);
        }
    }
    public void Remove(T mode, T item)
    {
        if (Contains(mode, item))
        {
            register[mode].Remove(item);
            _debugRegistry.Remove(item);
        }
    }
    public void Remove(T item)
    {
        foreach(var kvp in register)
        {
            if (kvp.Value.Contains(item)) {
                register[kvp.Key].Remove(item);
            }
        }
        _debugRegistry.Remove(item);
    }
    public void Clean()
    {
        foreach (var kvp in register)
        {
            kvp.Value.RemoveAll((item) => item == null);
        }
    }
}

public class FirebaseStoreManager : MonoBehaviour
{
    //public static FirebaseStore instance;
    public Register<string> register = new Register<string>();
    public List<SpawnItemScriptableObject> prefabs = new List<SpawnItemScriptableObject>();
    public string path;
    // add vfx and sfx spawning
    // Start is called before the first frame update
    void Start()
    {
    }
    public void OnGameStart()
    {
        Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        SpawnAllFromFirebase();
        FirebaseDatabase.DefaultInstance.GetReference(path).ChildAdded += OnNewObject;
    }
    public void OnGameOver()
    {
        FirebaseDatabase.DefaultInstance.GetReference(path).ChildAdded -= OnNewObject;
        // Do some cleanup?
    }

    private void OnNewObject(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snapshot = args.Snapshot;
        FirebaseSpawn(snapshot);
    }

    private void FixedUpdate()
    {
        register.Clean();
    }

    void SpawnAllFromFirebase()
    {
        FirebaseDatabase.DefaultInstance.GetReference(path).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach(var childsnapshot in snapshot.Children)
                {
                    FirebaseSpawn(childsnapshot);
                }
            }
        });
    }
    public void FirebaseSpawn(DataSnapshot snapshot)
    {
        var name = snapshot.Key;
        var datum = snapshot.Value as Dictionary<string, object>;
        if (datum == null)
        {
            Debug.LogWarning("Reference is null");
            return;
        }
        var record = JsonUtility.FromJson<FirebaseRecord>(snapshot.GetRawJsonValue());
        if (datum.ContainsKey("type") && datum.ContainsKey("position") && datum.ContainsKey("rotation"))
        {
            if (IsSpawned(name, type: record.Type))
            {
                Debug.Log($"Object={name} of type {record.Type} already spawned");
                return;
            }
            if (prefabs.Find(prefab => prefab.type == record.Type) == null)
            {
                Debug.LogWarning($"Object={name} of type {record.Type} is not supported");
                return;
            }
            Debug.Log($"Spawning Object = {name} json={JsonUtility.ToJson(record)}");
            Spawn(record.Type, position: record.Position, rotation: record.Rotation, name: name);
        }
        else
        {
            Debug.LogWarning($"Object {name} is not readable. object={snapshot.GetRawJsonValue()}");
        }
    }

    public GameObject Spawn(int i, Vector3 position, string name = null, Quaternion? rotation = null)
    {
        return Spawn(prefabs[i], position, name, rotation);
    }
    public GameObject Spawn(SpawnItemScriptableObject so, Vector3 position, string name = null, Quaternion? rotation = null, Vector3? scale = null)
    {
        if (!rotation.HasValue)
        {
            rotation = Random.rotationUniform;
        }
        if (!scale.HasValue)
        {
            scale = Vector3.one;
        }
        GameObject go = Instantiate(so.prefab, position, rotation.Value, transform);
        if (name != null)
        {
            Debug.Log($"Adding Name name={name}");
            go.name = name;
        }
        go.transform.localScale = scale.Value;
        return go;
    }
    public GameObject Spawn(string type, Vector3 position, string name = null, Quaternion? rotation = null)
    {
        return Spawn(prefabs.FindIndex((prefab) => prefab.type == type), position, name, rotation);
    }
    public bool IsSpawned(string name, string type = null)
    {
        return transform.Find(name) != null || register.Contains(name, type);
    }
}
