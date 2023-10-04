using CarcassEnemy.AI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class Carcass : AIStateMachine, ICustomEnemy
    {
        //References
        [SerializeField] private CarcassComponents components;
        public CarcassComponents Components => components;

        [SerializeField] private Transform target;
        public Transform Target => target;

        [SerializeField] private CarcassParametersAsset serializedParameters;

        private CarcassParameters parameters;
        public CarcassParameters Parameters
        {
            get
            {
                if(parameters == null)
                {
                    if (serializedParameters != null)
                        parameters = serializedParameters.Parameters;
                    else
                        parameters = new CarcassParameters(); //L
                }
                return parameters;
            }
        }


        //internal state
        private CarcassState defaultState => Idle;

        private float health;
        public float Health => health;

        //conk-creet states
        public CarcassFloatAround FloatAround { get; } = new CarcassFloatAround();
        public CarcassIdle Idle { get; } = new CarcassIdle();

        private void Awake()
        {
            if (components != null)
            {
                health = Parameters.maxHealth;
                components.Machine.health = Parameters.maxHealth;
                foreach(GameObject go in components.Hitboxes)
                {
                    Rigidbody rb = go.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }

        private void Start()
        {
            DefaultState();
            SetTarget(NewMovement.Instance.transform);
        }

        public void DefaultState()
        {
            SetState(defaultState);
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        //Meh
        public void TurnTowards(Vector3 point, float speed)
        {
            Vector3 pos = transform.position;
            Vector3 direction = point - pos;
            Quaternion rot = Quaternion.LookRotation(direction.XZ());
            Quaternion currentRot = transform.rotation;
            Quaternion newRotation = Quaternion.RotateTowards(currentRot, rot, Time.deltaTime * speed);
            transform.rotation = newRotation;
        }

        public float CalculateVerticalMoveDirection(Vector3 position, float desiredHeight)
        {
            if (!Physics.Raycast(position, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
                return -1f;

            float targetHeight = hit.point.y + desiredHeight;


            if (target != null)
            {
                targetHeight = Mathf.Max(targetHeight, target.position.y);
            }


            float distanceToDesiredHeight = targetHeight - position.y;
            return Mathf.Sign(distanceToDesiredHeight);
        }

        private void OnCollisionEnter(Collision col)
        {
            if (!typeof(CarcassState).IsAssignableFrom(currentState.GetType()))
                return;

            ((CarcassState)currentState)?.OnCollisionEnter(this, col);
        }

        //Shows state on screen
        private void OnGUI()
        {
            GUI.skin.box.fontSize = 35;
            GUI.skin.box.normal.textColor = Color.white;
            GUI.skin.box.alignment = TextAnchor.UpperLeft;

            string stateName = (currentState == null) ? "NO_STATE" : currentState.GetType().Name;
            stateName += $"\nHP:{health}";
            GUILayout.Box(stateName.TrimEnd('\n', '\r'));
        }

        public void Instakill()
        {
            if (dead)
                return;

            dead = true;
            StartCoroutine(DeathCoroutine());
        }
        

        public void GetHurt(HurtEventData hurtEventData)
        {
            string hurtDbg = $"Carcass hurt for {hurtEventData.multiplier} * {hurtEventData.critMultiplier}";
            if(hurtEventData.sourceWeapon != null)
            {
                hurtDbg += $" from {hurtEventData.sourceWeapon.name}";
            }

            if(hurtEventData.target != null)
            {
                hurtDbg += $" at location {hurtEventData.target.name}";
            }

            Debug.Log(hurtDbg);

            //Impl: Crit damage
            health = Mathf.Max(0f, health-hurtEventData.multiplier);

            if (health <= 0f && !dead)
            {
                dead = true;
                StartCoroutine(DeathCoroutine());
            }

        }

        private bool dead;

        //Placeholder stuff
        private IEnumerator DeathCoroutine()
        {
            Vector3 scale = transform.localScale;

            float time = 0.75f;
            float timer = time;

            while (timer > 0f)
            {
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;

                float interval = 1-(timer / time);
                Vector3 newScale = Vector3.Lerp(scale,new Vector3(0,scale.y,0),interval);
                transform.localScale = newScale;
            }

            transform.localScale = Vector3.zero;
            Debug.Log("Carcass died.");
            GameObject.Destroy(gameObject);
        }


        public Vector3 ExternalForce { get; private set; }

        public void Knockback(Vector3 force)
        {
            ExternalForce += force;
        }
    }
}
