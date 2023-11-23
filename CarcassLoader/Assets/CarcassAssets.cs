using UnityEngine;

namespace CarcassLoader.Assets
{
    public static class CarcassAssets
    {
        public static GameObject Carcass => Plugin.AssetLoader.LoadAsset<GameObject>("Carcass");
        public static GameObject CarcassPreview => Plugin.AssetLoader.LoadAsset<GameObject>("CarcassPreview");
        public static GameObject CarcassFunko => Plugin.AssetLoader.LoadAsset<GameObject>("CarcassFunko");
        public static GameObject CarcassFunkoMesh => Plugin.AssetLoader.LoadAsset<GameObject>("CarcassFunkoMesh");
        public static Sprite CarcassIcon => Plugin.AssetLoader.LoadAsset<Sprite>("carcass_icon");
        public static Sprite FunkoSpawnIcon => Plugin.AssetLoader.LoadAsset<Sprite>("CarcassFunkoPopSpawnIcon");
        public static Material CarcassEyeMaterial => Plugin.AssetLoader.LoadAsset<Material>("CarcassEye_Material");

        private static SpawnableObject carcassSpawnable;
        private static SpawnableObject funkoSpawnable;

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
            spawnable.objectName = "CARCASS";
            spawnable.gameObject = CarcassAssets.Carcass;
            spawnable.spawnableObjectType = SpawnableObject.SpawnableObjectDataType.Enemy;
            spawnable.enemyType = EnemyType.Filth; //EnemyType controls progress based sandbox unlocks. So this should be always unlocked.
            spawnable.iconKey = CarcassAssets.CarcassIcon.name;
            spawnable.gridIcon = CarcassAssets.CarcassIcon;
            spawnable.backgroundColor = new Color(0.349f, 0.349f, 0.349f, 1f);

            //Infopage stuff
            spawnable.type = "SUPREME HUSK/UNFINISHED PRIME";
            spawnable.description = "While it’s extremely rare to find a husk powerful enough to become a prime soul, it’s not uncommon for lesser husks to find their undoing in such power.\n\nA Carcass is the result of a prime soul that has failed to fully develop, resulting in what can be called “a portable flesh prison”. While its power greatly surpasses that of lesser husks, it is still very dependent on its physical body to the point that it would instantly disintegrate without it.\n\nThe surgical markings on its body shows that an attempt was made to contain its energy, instead it is left in an irreversible berzerk state. However, trapped in such a situation, the Carcass has adapted to its power and is able to mimic most of a normal flesh prison’s abilities. Additionally it is able to partially manifest itself at a distance by using a ritualistic pentagram.";
            spawnable.strategy = "- Once unleashed, Carcass’s summoning circle will activate after making contact with its target. Upon the circle’s activation, Carcass will attempt to drain and re-purpose the life energy of the target by manifesting part of itself below them. When the circle becomes active, it will emit a bright light and its symbols will change shape. The manifestations can be avoided by remaining in the air, out of their reach.\n\n- Carcass will manifest eyes to assist them in battle. While the eyes may serve well as healing, if all of the eyes are destroyed it will cause Carcass to become enraged. Typically Carcass will avoid close range combat. When enraged, it will charge and pursue you with all of its strength.";
            spawnable.preview = CarcassAssets.CarcassPreview;

            return spawnable;
        }

        public static SpawnableObject GetCarcassFunkoSpawnableObject()
        {
            if (funkoSpawnable == null)
                funkoSpawnable = BuildFunkoObject();
            return funkoSpawnable;
        }

        private static SpawnableObject BuildFunkoObject()
        {
            SpawnableObject spawnable = ScriptableObject.CreateInstance<SpawnableObject>();
            spawnable.name = "CarcassFunko_SpawnableObject";
            spawnable.identifier = "Carcass Funko";
            spawnable.objectName = "Carcass Funko";
            spawnable.gameObject = CarcassAssets.CarcassFunko;
            spawnable.spawnableObjectType = SpawnableObject.SpawnableObjectDataType.Object;
            spawnable.iconKey = CarcassAssets.FunkoSpawnIcon.name;
            spawnable.gridIcon = CarcassAssets.FunkoSpawnIcon;
            spawnable.backgroundColor = new Color(0.349f, 0.349f, 0.349f, 1f); //TODO
            return spawnable;
        }
    }
}
