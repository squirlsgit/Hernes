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
    public Dictionary<string, object> value = null;
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
            }
        }
    }
    public string _path;
    public bool isListening = false;
    public UnityEvent<object> OnValueUpdate = new UnityEvent<object>();
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
        isListening = false;
    }
    void OnFirstFrame()
    {
        _path = Path;
        Reference.ValueChanged += HandleUpdate;
        isListening = true;
    }
    void Get()
    {

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
    protected virtual void PushLocalValue(Dictionary<string, object> data)
    {
        Reference.Push();
    }
    public virtual void SetRemoteValue(Dictionary<string, object> data)
    {
        Reference.SetValueAsync(data);
    }
    public virtual void Remove()
    {
        Reference.RemoveValueAsync();
    }
}
