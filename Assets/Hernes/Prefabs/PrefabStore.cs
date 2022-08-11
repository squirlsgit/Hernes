using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.Events;
public class PrefabStore : MonoBehaviour
{
    public static PrefabStore instance;
    public List<SpawnItemScriptableObject> prefabs = new List<SpawnItemScriptableObject>();
    public string path;
    // add vfx and sfx spawning
    // Start is called before the first frame update
    void Start()
    {
    }
    public void OnGameStart()
    {
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

    // Update is called once per frame
    void Update()
    {
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
                FirebaseSpawn(snapshot);
            }
        });
    }
    public void FirebaseSpawn(DataSnapshot snapshot)
    {
        var data = snapshot.Value as Dictionary<string, object>;
        if (data == null)
        {
            Debug.LogWarning("Reference is null");
            return;
        }
        // Do something with snapshot...
        foreach (KeyValuePair<string, object> kvp in data)
        {
            var datum = (Dictionary<string, object>)kvp.Value;
            if (datum.TryGetValue("type", out var t) && datum.TryGetValue("pos", out var p) && datum.TryGetValue("rotation", out var r))
            {
                var pos = (List<float>)p;
                Vector3 position = new Vector3(pos[0], pos[1], pos[2]);
                var rot = (List<float>)r;
                Quaternion rotation = Quaternion.Euler(rot[0], rot[1], rot[2]);
                Spawn((string)t, position, rotation: rotation, name: kvp.Key);
            }
            else
            {
                Debug.LogWarning($"Object {kvp.Key} is not readable. object={snapshot.GetRawJsonValue()}");
            }
        }
    }

    public GameObject Spawn(int i, Vector3 position, string name = null, Quaternion? rotation = null)
    {
        if (name == null)
        {
            name = System.Guid.NewGuid().ToString();
        }
        if (transform.Find(name) != null)
        {
            Debug.Log($"Child {name} already exists. Preventing duplicate spawn.");
            return null;
        }
        if (!rotation.HasValue)
        {
            rotation = Random.rotationUniform;
        }
        GameObject go = Instantiate(prefabs[i].prefab, position, rotation.Value, transform);
        go.name = name;
        return go;
    }
    public GameObject Spawn(string type, Vector3 position, string name = null, Quaternion? rotation = null)
    {
        return Spawn(prefabs.FindIndex((prefab) => prefab.type == type), position, name, rotation);
    }

}
