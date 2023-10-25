using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace CarcassEnemy.Components.CustomEnemy
{
    public abstract class CustomEnemyBase : MonoBehaviour, IEnemy
    {
        [SerializeField] protected EnemyIdentifier enemyIdentifier;
        protected float health;

        public EnemyIdentifier GetEnemyIdentifier()
        {
            return enemyIdentifier;
        }

        public bool IsAlive()
        {
            return IsAliveCore();
        }

        protected virtual bool IsAliveCore()
        {
            return GetHealth() > 0f;
        }


        public float GetHealth()
        {
            return GetHealthCore();
        }
        
        protected virtual float GetHealthCore()
        {
            return health;
        }


        public void GetHurt(HurtEventData hurtEventData)
        {
            GetHurtCore(hurtEventData);
            ResolveHitStyle(hurtEventData);
            ResolveHitGore(hurtEventData);
        }

        protected virtual void GetHurtCore(HurtEventData hurtEventData)
        {
            health -= hurtEventData.multiplier;
        }


        protected virtual void ResolveHitStyle(HurtEventData hurtData)
        {
            if (hurtData.hitter == "enemy")
                return;

            StyleCalculator scalc = StyleCalculator.Instance;
            float health = GetHealth();

            EnemyIdentifier enemyIdentifier = GetEnemyIdentifier();

            if (health <= 0f)
            {
                if (hurtData.hitter == "explosion" || hurtData.hitter == "ffexplosion" || hurtData.hitter == "railcannon")
                {
                    scalc.shud.AddPoints(120, "ultrakill.fireworks", hurtData.sourceWeapon, enemyIdentifier, -1, "", "");
                }
                else if (hurtData.hitter == "ground slam")
                {
                    scalc.shud.AddPoints(160, "ultrakill.airslam", hurtData.sourceWeapon, enemyIdentifier, -1, "", "");
                }
                else if (hurtData.hitter != "deathzone")
                {
                    scalc.shud.AddPoints(50, "ultrakill.airshot", hurtData.sourceWeapon, enemyIdentifier, -1, "", "");
                }
            }

            if (hurtData.hitter == "secret")
                return;

            scalc.HitCalculator(hurtData.hitter, "machine", hurtData.target.tag, !IsAlive(), enemyIdentifier, hurtData.sourceWeapon);
        }

        protected virtual void ResolveHitGore(HurtEventData hurtData)
        {

            GameObject gore = null;

            float damage = CalculateDamage(hurtData);

            if (damage <= 0f)
                return;

            GoreType? goreType = null;
            float health = GetHealth();

            if (hurtData.target.tag == "Head" && (damage >= 1f || health <= 0f))
            {
                goreType = GoreType.Head;
            }
            else if (((damage >= 1f || health <= 0f) && hurtData.hitter != "explosion") || (hurtData.hitter == "explosion" && hurtData.target.tag == "EndLimb"))
            {
                if (hurtData.target.CompareTag("Body"))
                    goreType = GoreType.Body;
                else
                    goreType = GoreType.Limb;
            }
            else if (hurtData.hitter != "explosion")
                goreType = GoreType.Small;

            EnemyIdentifier eid = GetEnemyIdentifier();

            if (eid == null)
                return;

            if (goreType != null)
                gore = BloodsplatterManager.Instance.GetGore(goreType.Value, eid.underwater, eid.sandified, eid.blessed);

            GoreZone goreZone = null;

            if (gore == null)
                return;

            goreZone = (goreZone == null) ? GoreZone.ResolveGoreZone(transform) : goreZone;

            gore.transform.position = hurtData.target.transform.position;
            if (hurtData.hitter == "drill")
            {
                gore.transform.localScale *= 2f;
            }

            if (goreZone != null && goreZone.goreZone != null)
            {
                gore.transform.SetParent(goreZone.goreZone, true);
            }

            if (!gore.TryGetComponent<Bloodsplatter>(out Bloodsplatter bloodSplatter))
                return;

            ParticleSystem.CollisionModule collision = bloodSplatter.GetComponent<ParticleSystem>().collision;
            if (hurtData.hitter == "shotgun" || hurtData.hitter == "shotgunzone" || hurtData.hitter == "explosion")
            {
                if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                {
                    collision.enabled = false;
                }
                bloodSplatter.hpAmount = 3;
            }
            else if (hurtData.hitter == "nail")
            {
                bloodSplatter.hpAmount = 1;
                bloodSplatter.GetComponent<AudioSource>().volume *= 0.8f;
            }

            bloodSplatter.GetReady();
        }
        
        protected virtual float CalculateDamage(HurtEventData hurtData)
        {
            return hurtData.multiplier + GetLocationCritDamageMultiplier(hurtData.target.tag) * hurtData.multiplier * hurtData.critMultiplier;
        }

        public void Instakill()
        {
            InstakillCore();
        }

        protected virtual void InstakillCore()
        {
            health = 0f;
        }


        public void Knockback(Vector3 force)
        {
            KnockbackCore(force);
        }

        protected virtual void KnockbackCore(Vector3 force) {}


        public float GetLocationCritDamageMultiplier(string location)
        {
            return GetLocationCritMultiplierCore(location);
        }

        protected virtual float GetLocationCritMultiplierCore(string location)
        {
            if (string.IsNullOrEmpty(location))
                return 0f;

            if (location == "Head")
                return 1f;

            if (location == "Limb" || location == "EndLimb")
                return 0.5f;

            return 0f;
        }
    }
}
