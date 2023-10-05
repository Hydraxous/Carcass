namespace CarcassEnemy
{
    public class CarcassParameters
    {
        public float lateralFlySpeed = 30f;
        public float strafeSpeed = 15f;
        public float verticalFlySpeed = 10f;

        public float movementSmoothing = 8f;

        public float minTargetDistance = 16f;
        public float maxTargetDistance = 18f;
        
        public float desiredHeight = 5.5f;

        public float attackDelay = 3f;

        public float speedWhileAttackingMultiplier = 0.25f;

        //Timer
        public float directionChangeDelay = 2f;

        //VerticalLookRange;
        public float maxVerticalLookRange = 5f;

        //Blue projectile attack params
        public float shake_ProjectileOriginRadius = 4f;
        public float shake_ProjectileBurstLengthInSeconds = 2.2f;
        public int shake_ProjectileCount = 3;
        public int shake_ProjectileGroup = 3;


        public int maxHealth = 65;

       

    }
}
