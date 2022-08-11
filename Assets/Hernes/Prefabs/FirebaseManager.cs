using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.Events;
//using Firebase.Extensions.TaskExtension;

public class FirebaseManager : MonoBehaviour
{
    public static string scenePath = "scene/";
    public string modulePath = "";
    public string positionField = "position";
    public string rotationField = "rotation";
    public Dictionary<string, object> value = null;
    public string type = null;
    public string _path;
    public bool pushIfEmptyOnInit = true;
    public bool _isListening = false;
    public UnityEvent<object> OnValueUpdate = new UnityEvent<object>();
    public virtual Dictionary<string, object> Value
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
            } else
            {
                if (value.TryGetValue(positionField, out var p))
                {
                    var pos = (List<float>)p;
                    transform.position = pos.GetVector();
                }
                if (value.TryGetValue(rotationField, out var r))
                {
                    var rot = (List<float>)r;
                    transform.rotation = rot.GetEuler();
                }
            }
        }
    }
    public virtual string Path
    {
        get
        {
            return scenePath + modulePath + gameObject.name;
        }
    }
    public virtual DatabaseReference Reference
    {
        get
        {
            return FirebaseDatabase.DefaultInstance.GetReference(Path);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Invoke("OnFirstFrame", 0);
    }

    private void OnDestroy()
    {
        FirebaseDatabase.DefaultInstance.GetReference(Path).ValueChanged -= HandleUpdate;
        _isListening = false;
    }
    void OnFirstFrame()
    {
        _path = Path;
        Reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Value == null)
                {
                    if (pushIfEmptyOnInit)
                    {
                        SetRemoteValue();
                    }
                } else
                {
                    UpdateLocalValue(snapshot);
                }
            }
        });
        Reference.ValueChanged += HandleUpdate;
        _isListening = true;
    }
    private void HandleUpdate(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        UpdateLocalValue(args.Snapshot);
    }
    protected virtual void UpdateLocalValue(DataSnapshot snapshot)
    {
        Value = snapshot.Value as Dictionary<string, object>;
    }
    protected virtual void OnEmpty()
    {
        Debug.LogWarning($"FirebaseManager {gameObject.name} {Path} is Empty");
    }
    public virtual void SetRemoteValue(Dictionary<string, object> data = null)
    {
        if (data == null)
        {
            data = new Dictionary<string, object>();
        }
        data[positionField] = transform.GetPosition();
        data[rotationField] = transform.GetRotation();
        data["type"] = type;
        if (type == null)
        {
            Debug.LogWarning($"{Path} has no type.");
        }
        Reference.SetValueAsync(data);
    }
    public virtual void Remove()
    {
        Reference.RemoveValueAsync();
    }
}
