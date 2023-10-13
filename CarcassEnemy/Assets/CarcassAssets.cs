using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Assets
{
    public static class CarcassAssets
    {
        public static GameObject Carcass => Plugin.AssetLoader.LoadAsset<GameObject>("Carcass");
        public static GameObject CarcassPreview => Plugin.AssetLoader.LoadAsset<GameObject>("CarcassPreview");
        public static Sprite CarcassIcon => Plugin.AssetLoader.LoadAsset<Sprite>("carcass_icon");
        public static Material CarcassEyeMaterial => Plugin.AssetLoader.LoadAsset<Material>("CarcassEye_Material");

        //VFX
        public static GameObject HealFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassHeal");
        public static GameObject HealAuraFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassHealAura");
        public static GameObject GenericSpawnFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassSpawnGeneric");
        public static GameObject PsychosisFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_Psychosis");
        public static GameObject SigilActivateFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_SigilActivate");
        public static GameObject HookSnapFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_HookSnap");
        public static GameObject CarcassStunnedFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassStunned");
        public static GameObject BloodSprayFX => Plugin.AssetLoader.LoadAsset<GameObject>("BloodSpray");
        public static GameObject CarcassDeathFX => Plugin.AssetLoader.LoadAsset<GameObject>("vfx_CarcassDeath");
        public static GameObject CarcassScreamSFX => Plugin.AssetLoader.LoadAsset<GameObject>("sfx_CarcassScream");


        private static SpawnableObject carcassSpawnable;

        public static SpawnableObject GetCarcassSpawnableObject()
        {
            if (carcassSpawnable == null)
                carcassSpawnable = BuildCarcassObject();
            return carcassSpawnable;
        }

        private static SpawnableObject BuildCarcassObject()
        {
            SpawnableObject spawnable = ScriptableObject.CreateInstance<SpawnableObject>();
            spawnable.name = "Carcass_SpawnableObject";
            spawnable.identifier = "Carcass";
            spawnable.objectName = "Carcass";
            spawnable.gameObject = CarcassAssets.Carcass;
            spawnable.spawnableObjectType = SpawnableObject.SpawnableObjectDataType.Enemy;
            spawnable.enemyType = EnemyType.Filth; //EnemyType controls progress based sandbox unlocks. So this should be always unlocked.
            spawnable.iconKey = CarcassAssets.CarcassIcon.name;
            spawnable.gridIcon = CarcassAssets.CarcassIcon;
            spawnable.backgroundColor = new Color(0.349f, 0.349f, 0.349f, 1f);


            //Infopage stuff
            spawnable.type = "SUPREME HUSK/UNFINISHED PRIME";
            spawnable.description = "While it’s extremely rare to find a husk so powerful to have the energy to transform into a prime soul, it’s not uncommon for lesser husks to find their undoing in such power.\n\nA Carcass is what happens when a prime soul fails to fully develop, resulting in what can be called “a portable flesh prison”. Still much more powerful compared to lesser husks, but still very dependent on its physical body to the point that it would instantly disintegrate without it.\n\nSurgical markings on it's body shows that an attempt was made to contain their energy, it has been left in an irreversible berzerk state. However, trapped in such a situation, the Carcass has grown stronger, now able to mimic most of the flesh prison’s abilities as well as being able to partially manifest itself thanks to some type of summoning circles.";
            spawnable.strategy = "- Once unleashed, Carcass’s summoning circle will activate only after making contact with it's target at which point, an manifestation of Carcass will appear. Carcass manifestations are able to drain the life energy of any animate entity and absorb the freed energy. When the circle becomes active, the runes on the circle will change shape and a bright light will appear. The manifestations can be avoided by staying away from the ground.\n\n- Carcass will spawn eyes to assist them in battle, you may be tempted to destroy the eyes, however doing so will anger Carcass greatly. When enraged, Carcass will change behavior entirely: when most of the time it usually tries to avoid close range combat, now it will fully charge you with all its strength.";
            spawnable.preview = CarcassAssets.CarcassPreview;

            return spawnable;
        }
    }
}
