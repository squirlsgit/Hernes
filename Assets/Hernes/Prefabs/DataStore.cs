using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.Events;
//using Firebase.Extensions.TaskExtension;

public class DataStore : MonoBehaviour
{
    [System.Serializable]
    public enum StoreState {
        Waiting, Ready, Loaded, Empty,
    }
    [SerializeField]
    public StoreState State {
        get;
        protected set;
    }
    [Header("Settings to Target Remote")]
    public static string scenePath = "scene/";
    public string modulePath = "";
    [Header("Store of Value")]
    public Dictionary<string, object> value = null;
    public string jsonValue;
    public string type = null;

    [Header("Settings to Update Remote")]
    public string positionField = "position";
    public string rotationField = "rotation";
    public float syncRate;
    public bool partialUpdate = false;
    public string _path;
    public bool _isListening = false;
    public UnityEvent<object> OnValueUpdate = new UnityEvent<object>();
    public virtual Dictionary<string, object> Value
    {
        get
        {
            return value;
        }
        protected set
        {
            if (value != null)
            {
                State = StoreState.Loaded;
            }

            this.value = value;
            OnValueUpdate.Invoke(value);
            if (value == null)
            {
                State = StoreState.Empty;
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
    public virtual void SetValue(Dictionary<string, object> data, bool partial = false)
    {
        Value = data;
        if (!partial)
        {
            SetRemoteValue();
        } else
        {
            UpdateRemoteValue();
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
        if (State == StoreState.Ready)
        {
            Invoke("OnFirstFrame", 0);
        }
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
                if (snapshot.Value != null)
                {
                    UpdateLocalValue(snapshot);
                }
            }
        });
        Reference.ValueChanged += HandleUpdate;
        _isListening = true;
    }
    protected virtual IEnumerator Sync()
    {
        while(isActiveAndEnabled)
        {
            if (partialUpdate)
            {
                UpdateRemoteValue();
            } else
            {
                SetRemoteValue();
            }
            yield return new WaitForSeconds(syncRate);
        }
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
        Destroy(gameObject);
    }
    public virtual void UpdateRemoteValue()
    {
        var data = Value;
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
        Reference.UpdateChildrenAsync(data);
    }
    public virtual void SetRemoteValue()
    {
        var data = Value;
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
