using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "Hernes/ScriptableObjects/Item", order = 1)]
public class SpawnItemScriptableObject : ScriptableObject
{
    public string type;
    public GameObject prefab;
}
