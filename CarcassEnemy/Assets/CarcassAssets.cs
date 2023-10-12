﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Assets
{
    public static class CarcassAssets
    {
        public static GameObject Carcass => Plugin.AssetLoader.LoadAsset<GameObject>("Carcass");
        public static Sprite CarcassIcon => Plugin.AssetLoader.LoadAsset<Sprite>("carcass_icon");
        public static Material CarcassEyeMaterial => Plugin.AssetLoader.LoadAsset<Material>("CarcassEye_Material");

        //VFX
        public static GameObject HealFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassHeal");
        public static GameObject GenericSpawnFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassSpawnGeneric");
        public static GameObject PsychosisFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_Psychosis");
        public static GameObject SigilActivateFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_SigilActivate");
        public static GameObject HookSnapFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_HookSnap");
        public static GameObject CarcassStunnedFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassStunned");
        public static GameObject BloodSprayFX => Plugin.AssetLoader.LoadAsset<GameObject>("BloodSpray");
        public static GameObject CarcassDeathFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassDeath");
    }
}