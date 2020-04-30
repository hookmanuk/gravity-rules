using System;
using System.Collections.Generic;
using UnityEngine;

namespace BangsPhysics
{
    class RigidBodyAttractor : MonoBehaviour
    {
        const float G = 667.4f;

        public float mass = 100.0f;

        public void Start()
        {
            PhysicsManager.Instance.attractors.Add(this);
        }

        public void OnDestroy()
        {
            PhysicsManager.Instance.attractors.Remove(this);
        }

        public void ApplyForce(RigidBody otherBody)
        {
            otherBody.AddForce(GetForce(otherBody));
        }

        public Vector3 GetForce(RigidBody otherBody)
        {
            Vector3 force = new Vector3();
            Vector3 direction = transform.position - otherBody.position;

            float forceMagnitude = G * (mass * otherBody.mass) / direction.sqrMagnitude;

            force = direction.normalized * forceMagnitude;

            return force;
        }
    }
}
