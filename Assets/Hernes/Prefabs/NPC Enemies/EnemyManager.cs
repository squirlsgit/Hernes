using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : FirebaseManager
{

    public string type;
    public Vector3 velocity = Vector3.zero;
    public string state;
    public float health;
    public Vector3 position = Vector3.zero;

    public float syncDelay = 1f;
    public Rigidbody rb;
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
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StopAllCoroutines();
        StartCoroutine(SyncDatabase());
    }

    // Update is called once per frame
    void Update()
    {

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
            { "velocity", new List<float>() { rb.velocity.x, rb.velocity.y, rb.velocity.z } },
            { "state", state },
            { "health", health },
        };
        SetRemoteValue(dict);
    }

    private void OnDestroy()
    {
        Remove();
    }
}
