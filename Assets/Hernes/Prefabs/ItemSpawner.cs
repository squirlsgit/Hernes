using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ItemSpawner))]
public class TestItemSpawner : Editor
{
    public string interfaceName = "squirrel";
    public override void OnInspectorGUI()
    {
        var itemSpawner = (ItemSpawner)target;
        GUILayout.Space(20f);
        GUILayout.Label("Test Item Spawning");
        if (EditorApplication.isPlaying)
        {
            foreach(var item in itemSpawner.Prefabs)
            {
                if (GUILayout.Button($"Spawn {item.type}"))
                {
                    itemSpawner.SpawnItem(item);
                }
            }
        }
        base.OnInspectorGUI();
    }
}
#endif
public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner instance;
    public string spawningTag = "ItemSpawn";
    public AnimationCurve spawnRate = AnimationCurve.Linear(0, 1, 1, 1);
    public List<GameObject> spawners;
    public float spawnTime;
    public int maxItems = 100;
    public FirebaseStoreManager store;
    public List<SpawnItemScriptableObject> Prefabs
    {
        get
        {
            return store.prefabs;
        }
    }
    private void Awake()
    {
        instance = this;
        spawnTime = Time.time;
    }
    // Start is called before the first frame update
    void Start()
    {
        spawners = GameObject.FindGameObjectsWithTag(spawningTag).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        //var rate = spawnRate.Evaluate(transform.childCount / maxItems);
        //if (rate > 0f && Time.time - spawnTime > 1f / rate)
        //{
        //    SpawnItem();
        //    spawnTime = Time.time;
        //}
    }
    public void SpawnItem(SpawnItemScriptableObject so = null)
    {
        if (transform.childCount < maxItems)
        {
            if (so == null)
            {
                so = store.prefabs.Random();
            }
            var spawner = spawners.Random();
            store.Spawn(so, spawner.transform.position, rotation: spawner.transform.rotation);
        }
    }
}
