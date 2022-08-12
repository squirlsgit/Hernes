using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : DataStore
{
    [SerializeField]
    GameObject spawnItem;
    public float syncDelay = 1f;
    public string type;

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
            if(value.ContainsKey("type"))
            {
                type = value["type"] as string;
            }
            if (value.TryGetValue("pos", out var p))
            {
                var pos = p as List<float>;
                transform.position = new Vector3(pos[0], pos[1], pos[2]);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StopAllCoroutines();
        StartCoroutine(SyncDatabase());
    }

    protected virtual IEnumerator SyncDatabase()
    {
        while(isActiveAndEnabled)
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
            { "pos", new List<float>() { transform.position.x, transform.position.y, transform.position.z } }
        };
        SetRemoteValue(dict);
    }

    private void OnDestroy()
    {
        if (spawnItem != null)
        {
            Instantiate(spawnItem, transform.position, transform.rotation, transform.parent);
            Remove();
        }
    }
}
