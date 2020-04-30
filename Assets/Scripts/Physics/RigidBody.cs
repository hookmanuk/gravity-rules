using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BangsPhysics
{


    public class RigidBody : MonoBehaviour
    {
        public Vector3 position { get; set; }
        public Vector3 velocity { get; set; }
        public Vector3 acceleration { get; private set; }

        public Quaternion rotation { get; set; }   //  TODO - is this needed?

        public bool isKinematic;

        public float mass;

        Vector3 totalForce;


        public RigidBody DeepCopy()
        {
            return (RigidBody)this.MemberwiseClone();
        }

        public void Start()
        {
            PhysicsManager.Instance.rigidBodies.Add(this);

            position = transform.position;
        }

        public void OnDestroy()
        {
            PhysicsManager.Instance.rigidBodies.Remove(this);
        }

        public void AddForce(Vector3 force)
        {
            totalForce += force;
        }

        public void Reset()
        {
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
            totalForce = Vector3.zero;
        }

        public void OnFixedUpdate()
        {
            if (!isKinematic)
            {
                acceleration = totalForce / mass;
                velocity += acceleration * Time.fixedDeltaTime;
                position += velocity * Time.fixedDeltaTime;
            }

            totalForce = Vector3.zero;
        }

        private void Update()
        {
            if (!isKinematic)
            {
                transform.position = position;
            }
        }
    }

}
