using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySimplifier : MonoBehaviour
{
    public bool neverOutlineAndRemoveSimplifier;
    public bool enemyScriptHandlesEnrage;
    public Transform enemyRootTransform;
    public List<int> radiantSubmeshesToIgnore = new List<int>();
    public Material enragedMaterial;
    public Material simplifiedMaterial;
    public Material simplifiedMaterial2;
    public Material simplifiedMaterial3;
    public Material enragedSimplifiedMaterial;
    public bool ignoreCustomColor;
    public Material[] matList;
}
