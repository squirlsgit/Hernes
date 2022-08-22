using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;
using System.Threading.Tasks;
using System.Linq;
using System;
//using Firebase.Extensions.TaskExtension;

public interface IFirebaseRecord
{
    string ToJson();
    Dictionary<string, object> Dictionary { get; }
    DateTime LastUpdatedAt { get; set; }
    string Type { get; }
    Vector3 Position { get; set; }
    Quaternion Rotation { get; set; }
    Vector3 Scale { get; set; }
}

[System.Serializable]
public struct FirebaseRecord : IFirebaseRecord
{
    public List<float> rotation;
    public List<float> position;
    public List<float> scale;
    public string type;
    public string lastUpdatedAt;
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
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
    public Dictionary<string, object> Dictionary
    {
        get
        {
            return new Dictionary<string, object>()
            {
                { "position", position },
                { "rotation", rotation },
                { "scale", scale },
                { "type", type },
                { "lastUpdatedAt", lastUpdatedAt },
            };
        }
    }
    public string Type
    {
        get
        {
            if (type == null || type.Length == 0)
            {
                return null;
            }
            return type;
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
                Debug.Log($"SET POSITION {position[0]} {position[1]} value={value}");
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
}
public class DataStore<T> : MonoBehaviour where T : IFirebaseRecord
{
    public enum StoreSetting
    {
        DestroyOnEmpty = 1 << 0,
        DisableOnEmpty = 1 << 1,
        EmptyOnDestroy = 1 << 2,
        EmptyOnDisable = 1 << 3,
        PartialUpdate = 1 << 4,
        ReadyOnEnable = 1 << 5,
        PushOnEnable = 1 << 6,
    }
    [System.Serializable]
    public enum StoreState {
        Waiting, Ready, Loading, Loaded, Empty,
    }
    public StoreState Status {
        get
        {
            return _Status;
        }
        protected set
        {
            _Status = value;
        }
    }
    [SerializeField]
    protected StoreState _Status = StoreState.Ready;
    [Header("Store of Value")]
    [Tooltip("Synced at runtime. Used for initial push.")]
    [SerializeField]
    protected T _Value;
    public virtual T Value
    {
        get
        {
            return _Value;
        }
        protected set
        {
            _Value = value;
        }
    }
    public string JsonValue
    {
        get;
        protected set;
    }

    [Header("Settings to Update Remote")]
    public bool _isListening = false;


    [Header("General Settings")]
    [EnumFlags]
    [SerializeField]
    protected StoreSetting Settings = StoreSetting.PartialUpdate | StoreSetting.ReadyOnEnable;
    public bool HasSetting(StoreSetting setting)
    {
        return (setting & Settings) == setting;
    }
    protected Dictionary<string, object> _Data = null;
    public virtual Dictionary<string, object> Data
    {
        get
        {
            return _Data;
        }
        protected set
        {
            if (value != null)
            {
                OnLoaded();
            }

            if (value == null)
            {
                OnEmpty();
            }
            this._Data = value;
        }
    }
    public string _previousPath = "";
    public string modulePathFallback = "";
    public string PathOverride = null;
    public virtual string Path
    {
        get
        {
            if (PathOverride != null && PathOverride.Length > 0)
            {
                return PathOverride;
            }
            return ModulePath + "/" + gameObject.name;
        }
    }
    public virtual string ModulePath
    {
        get
        {
            if (ParentStore != null)
            {
                return ParentStore.path;
            } else
            {
                return modulePathFallback;
            }
        }
    }
    public virtual DatabaseReference Reference
    {
        get
        {
            return FirebaseDatabase.DefaultInstance.GetReference(Path);
        }
    }
    public virtual DatabaseReference ModuleReference
    {
        get
        {
            return FirebaseDatabase.DefaultInstance.GetReference(ModulePath);
        }
    }
    [Header("Settings to Target Remote")]
    [SerializeField]
    protected FirebaseStoreManager ParentStore;
    [SerializeField]
    protected string ParentStoreTag;
    private DatabaseReference UsingReference;
    private async void OnEnable()
    {
        if (ParentStore == null && transform.parent != null)
        {
            ParentStore = transform.parent.GetComponent<FirebaseStoreManager>();
        }
        if (ParentStore == null && ParentStoreTag != null && ParentStoreTag.Length > 0)
        {
            ParentStore = GameObject.FindGameObjectWithTag(ParentStoreTag)?.GetComponent<FirebaseStoreManager>();
        }
        if (HasSetting(StoreSetting.PushOnEnable))
        {
            Status = StoreState.Waiting;
            await Push();
        }
        if (HasSetting(StoreSetting.ReadyOnEnable))
        {
            Status = StoreState.Ready;
        }
    }
    private void OnDisable()
    {
        Reference.ValueChanged -= HandleUpdate;
        UsingReference = null;
        _isListening = false;
        if (Data != null && HasSetting(StoreSetting.EmptyOnDisable))
        {
            Remove();
        }
    }
    private void Update()
    {
        if (Status == StoreState.Ready || ((Status == StoreState.Loaded || Status == StoreState.Loading) && _previousPath != Path))
        {
            OnFirstFrame();
        }
        _previousPath = Path;
    }
    private void OnDestroy()
    {
        FirebaseDatabase.DefaultInstance.GetReference(Path).ValueChanged -= HandleUpdate;
        _isListening = false;
        if (Data != null && HasSetting(StoreSetting.EmptyOnDestroy))
        {
            Debug.Log("Empty on Destroy");
            Remove();
        }
    }
    private async void OnFirstFrame()
    {
        StopAllCoroutines();
        if (UsingReference != null)
        {
            Debug.LogWarning("Listerner already exists. Canceling listener");
            UsingReference.ValueChanged -= HandleUpdate;
        }
        Debug.Log($"Object={gameObject.name} Adding Listener to Path {Path}");
        Status = StoreState.Loading;
        Reference.ValueChanged += HandleUpdate;
        UsingReference = Reference;
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
        //Debug.Log($"Retrieved {Path}: {snapshot.GetRawJsonValue()}");
        Data = snapshot.Value as Dictionary<string, object>;
        JsonValue = snapshot.GetRawJsonValue();
        //var oldValue = Value;
        if (JsonValue != null)
        {
            dynamic newValue = JsonUtility.FromJson<T>(JsonValue);
            Value = newValue;
            //SetFromJson(JsonValue);
        }
        OnValueUpdate.Invoke();
    }
    protected virtual void SetFromJson(string json)
    {
        object v = JsonUtility.FromJson<T>(json);
        Value = (T)v;
    }
    protected virtual void OnEmpty()
    {
        Debug.LogWarning($"FirebaseManager {gameObject.name} {Path} is Empty. CurrentState = {Status.ToString()} --{Status}");
        if (HasSetting(StoreSetting.DestroyOnEmpty) && Status == StoreState.Loaded)
        {
            Destroy(gameObject);
        } else if (HasSetting(StoreSetting.DisableOnEmpty) && Status == StoreState.Loaded)
        {
            gameObject.SetActive(false);
        }
        Status = StoreState.Empty;
    }
    protected virtual void OnLoaded()
    {
        if (StoreState.Loaded != Status)
        {
            Register();
            Status = StoreState.Loaded;
            gameObject.SendMessage("OnLoaded", SendMessageOptions.DontRequireReceiver);
        }
    }
    protected virtual void Register(bool setName = false)
    {
        if (setName)
        {
            var name = System.Guid.NewGuid().ToString();
            Debug.Log($"Object = {gameObject.name} setting new name = {name}");
            gameObject.name = name;
        }
        if (ParentStore != null && ParentStore.gameObject != transform.parent.gameObject)
        {
            ParentStore.register.Add(Value.Type, gameObject.name);
        }
    }

    #region DB Accessors
    public System.Threading.Tasks.Task Push()
    {
        var val = Value;
        val.Position = transform.position;
        val.Rotation = transform.rotation;
        val.Scale = transform.lossyScale;
        Debug.Log($"VALUES AS JSON = {val.ToJson()}");
        return Push(val.Dictionary);
    }
    public System.Threading.Tasks.Task Push(Dictionary<string, object> data)
    {
        Register(setName: true);
        Debug.Log($"Pushing object={gameObject.name} value={JsonUtility.ToJson(Value)}");
        Debug.Log($"Dictionary Value object={gameObject.name} position={((dynamic)Value.Dictionary["position"]).Count}");
        return SetValue(data, partial: true);
    }
    public System.Threading.Tasks.Task Push(string json)
    {
        Register(setName: true);
        return SetRawValueBypass(json);
    }
    public virtual async System.Threading.Tasks.Task<DataSnapshot> GetValue()
    {
        var snapshot = await Reference.GetValueAsync();
        UpdateLocalValue(snapshot);
        return snapshot;
    }
    public virtual System.Threading.Tasks.Task SetValue(Dictionary<string, object> data, bool partial = false)
    {
        data["lastUpdatedAt"] = DateTime.UtcNow.ToString();
        if (partial)
        {
            return Reference.UpdateChildrenAsync(data);
        }
        else
        {
            return Reference.SetValueAsync(data);
        }
    }
    public virtual System.Threading.Tasks.Task SetRawValueBypass(string value)
    {
        return Reference.SetRawJsonValueAsync(value);
    }
    public virtual System.Threading.Tasks.Task Remove()
    {
        return Reference.RemoveValueAsync();
    }
#endregion

    public UnityEvent OnValueUpdate = new UnityEvent();
}
