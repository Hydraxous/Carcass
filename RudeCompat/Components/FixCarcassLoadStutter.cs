using CarcassEnemy;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FixCarcassLoadStutter : MonoBehaviour
{
    [Header("This pre-loads Carcass assets so it doesnt stutter when the enemy is spawned. Place in root of scene")]
    private int _ = 0;

    private void Awake()
    {
        Patcher.QueryInstance();
    }
}
