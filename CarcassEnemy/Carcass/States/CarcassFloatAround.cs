using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class CarcassFloatAround : CarcassState
    {
        private Transform target;
        private Rigidbody rb;

        private float direction;
        private float lateralGlideSpeed = 11f;
        private float verticalSpeed = 10f;

        private float movementSmoothing = 8f;

        private float desiredTargetDistance = 16f;
        private float maxTargetDistance = 18f;
        private float desiredHeight = 5.5f;

        private float actionDelay = 3f;
        private float timeUntilNextAction;

        public override void OnEnter(Carcass carcass)
        {
            timeUntilNextAction = actionDelay;
            target = carcass.Target;
            rb = carcass.Components.Rigidbody;
            direction = Mathf.Sign((UnityEngine.Random.value - 0.5f) * 2f);

            //Only do this state when target is provided.
            if (target == null)
                carcass.DefaultState();
        }

        public override void OnUpdate(Carcass carcass)
        {
            carcass.TurnTowards(target.position, 1000f);

            timeUntilNextAction -= Time.deltaTime;
            if (timeUntilNextAction <= 0f)
                carcass.SetState(this);//Restart state bc nothing else exists rn.
        }

        private float ResolveTargetDistance(Vector3 position, Vector3 targetPosition)
        {
            return Mathf.Sign(desiredTargetDistance - (targetPosition - position).XZ().magnitude);
        }

        private Vector3 velocity;

        public override void OnFixedUpdate(Carcass carcass)
        {
            Vector3 travelVector = carcass.transform.right * direction;

            Vector3 targetPos = target.position;
            Vector3 pos = carcass.transform.position;

            Vector3 toTarget = targetPos - pos;
            Vector3 toTargetXZ = toTarget.XZ();

            float lateralDistance = toTargetXZ.magnitude;
            
            if(lateralDistance < desiredTargetDistance)
            {
                travelVector = -toTargetXZ.normalized;
            }

            if(lateralDistance > maxTargetDistance)
            {
                travelVector = toTargetXZ.normalized;
            }

            Vector3 moveDelta = travelVector * lateralGlideSpeed;
            moveDelta.y += carcass.CalculateVerticalMoveDirection(carcass.transform.position, desiredHeight) * verticalSpeed;

            velocity = Vector3.MoveTowards(velocity, moveDelta, Time.deltaTime * movementSmoothing);
            rb.velocity = velocity;
        }

        public override void OnLateUpdate(Carcass carcass) {}
        public override void OnExit(Carcass carcass) { }

        public override void OnCollisionEnter(Carcass carcass, Collision col)
        {

        }
    }
}
