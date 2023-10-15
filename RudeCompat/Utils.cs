using UnityEngine;

namespace CarcassEnemy
{
    public static class Utils
    {
        public static Vector3 XZ(this Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }
    }
}
