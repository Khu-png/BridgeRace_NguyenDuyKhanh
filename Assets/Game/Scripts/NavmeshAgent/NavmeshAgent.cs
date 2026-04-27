using UnityEngine;
using UnityEngine.AI;

namespace NavMesh
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavmeshAgent : MonoBehaviour
    {
        [SerializeField] private Transform tf;
        [SerializeField] private NavMeshAgent agent;

        private Vector3 destination;

        public bool IsDestionation => IsDestination;
        public bool IsDestination => Vector3.Distance(tf.position, destination + (tf.position.y - destination.y) * Vector3.up) < 0.1f;
        public bool IsEnabled => agent != null && agent.enabled;
        public bool IsStopped => agent == null || agent.isStopped;
        public bool PathPending => agent != null && agent.pathPending;
        public bool UpdatePosition => agent != null && agent.updatePosition;
        public float RemainingDistance => agent != null ? agent.remainingDistance : float.MaxValue;
        public float StoppingDistance => agent != null ? agent.stoppingDistance : 0f;
        public Vector3 Velocity => agent != null ? agent.velocity : Vector3.zero;

        public void ConfigureForEnemy()
        {
            agent.updateRotation = false;
            agent.baseOffset = 0f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        public void SetDestination(Vector3 destination)
        {
            if (!IsEnabled)
            {
                return;
            }

            this.destination = destination;
            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        public void StopAtCurrentPosition()
        {
            if (!IsEnabled)
            {
                return;
            }

            agent.ResetPath();
            agent.isStopped = true;
            SyncNextPosition();
        }

        public void PauseAtCurrentPosition()
        {
            if (!IsEnabled)
            {
                return;
            }

            agent.isStopped = true;
            SyncNextPosition();
        }

        public void ResumeAtCurrentPosition()
        {
            if (!IsEnabled)
            {
                return;
            }

            SyncNextPosition();
            agent.isStopped = false;
        }

        public void DisableMovement()
        {
            if (!IsEnabled)
            {
                return;
            }

            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
            SyncNextPosition();
        }

        public void EnableMovement()
        {
            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            agent.Warp(tf.position);
            agent.updatePosition = true;
            agent.updateRotation = false;
            agent.isStopped = false;
        }

        public void DisableAgent()
        {
            if (!IsEnabled)
            {
                return;
            }

            StopAtCurrentPosition();
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.enabled = false;
        }

        public void SyncNextPosition()
        {
            if (IsEnabled)
            {
                agent.nextPosition = tf.position;
            }
        }

        public bool CanReach(Vector3 point, float sampleDistance)
        {
            if (!TrySamplePosition(point, sampleDistance, out Vector3 sampledPosition))
            {
                return false;
            }

            NavMeshPath path = new NavMeshPath();
            return agent.CalculatePath(sampledPosition, path) && path.status == NavMeshPathStatus.PathComplete;
        }

        public bool TrySamplePosition(Vector3 point, float maxDistance, out Vector3 sampledPosition)
        {
            sampledPosition = point;
            if (!IsEnabled || !UnityEngine.AI.NavMesh.SamplePosition(point, out NavMeshHit hit, maxDistance, UnityEngine.AI.NavMesh.AllAreas))
            {
                return false;
            }

            sampledPosition = hit.position;
            return true;
        }

        public bool TrySnapToNavMesh(Vector3 point, float maxDistance)
        {
            if (!TrySamplePosition(point, maxDistance, out Vector3 sampledPosition))
            {
                return false;
            }

            tf.position = sampledPosition;
            SyncNextPosition();
            return true;
        }
    }
}
