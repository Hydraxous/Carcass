using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class HurtZone : MonoBehaviour
{
    public EnviroDamageType damageType;
    public bool trigger;
    public float hurtCooldown = 1f;
    public float setDamage;
}

public enum EnviroDamageType
{
    Normal,
    Burn,
    Acid,
    WeakBurn
}
