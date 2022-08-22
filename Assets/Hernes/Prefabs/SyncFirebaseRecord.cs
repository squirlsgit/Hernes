using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SyncFirebaseRecord : MonoBehaviour
{
    [Range(0.1f, 100f)]
    [SerializeField]
    protected float SyncRate = 1f;

    [SerializeField]
    protected float LastUpdatedTime;

    [SerializeField]
    protected List<DataStore<FirebaseRecord>> Stores = null;

    private void Awake()
    {
        LastUpdatedTime = Time.time;
    }
    private void Start()
    {
        if (Stores == null)
        {
            Stores = GetComponents<DataStore<FirebaseRecord>>().ToList();
        }
    }
    private void FixedUpdate()
    {
        if (Hernes.SceneManager.instance.State == Hernes.SceneManager.GameState.Playing)
        {
            if (LastUpdatedTime + SyncRate < Time.time)
            {
                foreach (var store in Stores)
                {
                    LastUpdatedTime = Time.time;
                    var Data = store.Value;
                    Data.Position = transform.position;
                    Data.Rotation = transform.rotation;
                    Data.LastUpdatedAt = DateTime.UtcNow;
                    store.SetValue(Data.Dictionary, partial: true);
                }
            }
        }
    }
}
