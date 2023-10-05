using UnityEngine;

namespace CarcassEnemy.Assets
{
    public static class UKPrefabs
    {
        public static UKAsset<GameObject> LobbedProjectileExplosiveHH { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Explosive HH.prefab");
        public static UKAsset<GameObject> LobbedProjectileExplosive { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Explosive.prefab");
        public static UKAsset<GameObject> HomingProjectile { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Homing.prefab");
    }
}
