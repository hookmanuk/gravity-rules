using System;
using System.Collections.Generic;
using UnityEngine;


namespace BangsPhysics
{
    class PhysicsManager : MonoBehaviour
    {
        public static PhysicsManager Instance { get; private set; }

        public List<RigidBody> rigidBodies { get; set; }
        public List<RigidBodyAttractor> attractors { get; set; }

        void Awake()
        {
            Instance = this;

            rigidBodies = new List<RigidBody>();
            attractors = new List<RigidBodyAttractor>();
        }

        public void OnRigidBodyAwake(RigidBody newRigidBody)
        {
            rigidBodies.Add(newRigidBody);
        }

        public void OnRigidBodyDestroy(RigidBody rigidBody)
        {
            rigidBodies.Remove(rigidBody);
        }

        private void FixedUpdate()
        {
            foreach (var rigidBody in rigidBodies)
            {
                foreach (var attractor in attractors)
                {
                    attractor.ApplyForce(rigidBody);
                }

                rigidBody.OnFixedUpdate();
            }
        }

        public List<Vector3> ForwardSimulate(RigidBody rigidBody, int totalPoints, int ticksBetweenPoints)
        {
            List<Vector3> futurePoints = new List<Vector3>();

            float timeStep = Time.fixedDeltaTime;

            RigidBody myClone = rigidBody.DeepCopy();

            int totalTicks = totalPoints * ticksBetweenPoints;
            for(int index = 0; index < totalTicks; ++index)
            {
                foreach (var attractor in attractors)
                {
                    attractor.ApplyForce(myClone);
                }

                myClone.OnFixedUpdate();

                if ((index) % ticksBetweenPoints == 0)
                {
                    futurePoints.Add(myClone.position);
                }
            }

            return futurePoints;
        }
    }
}
