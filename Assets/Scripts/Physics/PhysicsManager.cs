using System;
using System.Collections.Generic;
using UnityEngine;


namespace BangsPhysics
{
    class PhysicsManager : MonoBehaviour
    {
        public bool Started { get; set; }
        public bool IsRewinding { get; set; }
        public static PhysicsManager Instance { get; private set; }

        public List<RigidBody> rigidBodies { get; set; }
        public List<RigidBodyAttractor> attractors { get; set; }

        public Checkpoint ActiveCheckpoint { get; set; }

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
            if (Started && !IsRewinding)
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
        }

        public PathResult ForwardSimulate(RigidBody rigidBody, int totalPoints, int ticksBetweenPoints, Vector3 shipOffset)
        {
            PathResult pathResult = new PathResult();
            List<Vector3> futurePoints = new List<Vector3>();

            pathResult.FuturePoints = futurePoints;
            pathResult.State = FutureState.OK;

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

                foreach(var attractor in attractors)
                {
                    if (Vector3.Distance(attractor.transform.position, myClone.position) < attractor.GetComponent<SphereCollider>().radius)
                    {
                        //we've hit this attractor
                        pathResult.State = FutureState.PlanetHit;
                        break;
                    }
                }

                if (Vector3.Distance(ActiveCheckpoint.transform.position, myClone.position) < ActiveCheckpoint.GetComponent<SphereCollider>().radius*10)
                {
                    pathResult.State = FutureState.CheckpointHit;
                }

                if (pathResult.State != FutureState.OK)
                {
                    break;
                }
               
                if ((index) % ticksBetweenPoints == 0)
                {
                    futurePoints.Add(myClone.position + shipOffset);
                }
            }

            return pathResult;
        }
    }

    public class PathResult
    {
        public FutureState State { get; set; }
        public List<Vector3> FuturePoints { get; set; }
    }

    public enum FutureState
    {
        OK,
        PlanetHit,
        CheckpointHit
    }
}
