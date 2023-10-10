using UnityEngine;

namespace CarcassEnemy.Assets
{
    public static class UKPrefabs
    {
        public static UKAsset<GameObject> LobbedProjectileExplosiveHH { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Explosive HH.prefab");
        public static UKAsset<GameObject> LobbedProjectileExplosive { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Explosive.prefab");
        public static UKAsset<GameObject> HomingProjectile { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Homing.prefab");
        public static UKAsset<GameObject> DroneFlesh { get; private set; } = new UKAsset<GameObject>("Assets/Prefabs/Enemies/DroneFlesh.prefab");
        public static UKAsset<GameObject> RageEffect { get; private set; } = new UKAsset<GameObject>("Assets/Particles/Enemies/RageEffect.prefab");
        public static UKAsset<GameObject> BreakParticle { get; private set; } = new UKAsset<GameObject>("Assets/Particles/Breaks/BreakParticle.prefab");
        public static UKAsset<GameObject> SpawnEffect { get; private set; } = new UKAsset<GameObject>("Assets/Particles/Spawn Effects/SpawnEffect 5.prefab");
        public static UKAsset<GameObject> LightShaft { get; private set; } = new UKAsset<GameObject>("Assets/Particles/LightShaft.prefab");

    }
}
