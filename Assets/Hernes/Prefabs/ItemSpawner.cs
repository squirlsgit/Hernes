using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner instance;
    public string spawningTag = "ItemSpawner";
    public AnimationCurve spawnRate = AnimationCurve.Linear(0, 1, 1, 1);
    public List<GameObject> spawners;
    public float spawnTime;
    public int maxItems = 100;
    public PrefabStore store;
    public List<SpawnItemScriptableObject> prefabs
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
        var rate = spawnRate.Evaluate(transform.childCount / maxItems);
        if (rate > 0f && Time.time - spawnTime > 1f / rate)
        {
            SpawnItem();
            spawnTime = Time.time;
        }
    }
    void SpawnItem(int i = -1, string name = null)
    {
        if (transform.childCount < maxItems)
        {
            if (i == -1)
            {
                i = Mathf.RoundToInt(Random.value * (prefabs.Count - 1));
            }
            int j = Mathf.RoundToInt(Random.value * (spawners.Count - 1));
            store.Spawn(i, spawners[j].transform.position, name: name, rotation: spawners[j].transform.rotation);
        }
    }
}
