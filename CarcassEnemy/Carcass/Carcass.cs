using CarcassEnemy.AI;
using System;
using System.Collections.Generic;
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

        //conk-creet States
        public CarcassFloatAround FloatAround { get; } = new CarcassFloatAround();
        public CarcassIdle Idle { get; } = new CarcassIdle();

        private void Awake()
        {
            if (components != null)
            {
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

        public void TurnTowards(Vector3 point, float speed)
        {
            Vector3 pos = transform.position;
            Vector3 direction = point - pos;
            Quaternion rot = Quaternion.LookRotation(direction.XZ());
            Quaternion currentRot = transform.rotation;
            Quaternion newRotation = Quaternion.RotateTowards(currentRot, rot, Time.deltaTime * speed);
            transform.rotation = newRotation;
        }

        public void MoveTowards(Vector3 point, float speed)
        {

        }

        public void Move(Vector3 delta)
        {

        }

        public float CalculateVerticalMoveDirection(Vector3 position, float desiredHeight)
        {
            if (!Physics.Raycast(position, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
                return -1f;

            float distanceToDesiredHeight = (hit.point.y + desiredHeight) - position.y;
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
            GUILayout.Box(stateName.TrimEnd('\n', '\r'));
        }

        public void Instakill()
        {
            //Switch to state dying
            Destroy(gameObject);
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
        }


        public Vector3 ExternalForce { get; private set; }

        public void Knockback(Vector3 force)
        {
            ExternalForce += force;
        }
    }
}
