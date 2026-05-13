using System;
using UnityEngine;

namespace DungeonBlade.Dungeon
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] string checkpointId = "checkpoint_1";
        [SerializeField] Transform spawnPoint;
        [SerializeField] float healOnReach = 20f;

        public string Id => checkpointId;
        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;
        public Quaternion SpawnRotation => spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        public float HealAmount => healOnReach;

        public bool Activated { get; private set; }

        // Static event so audio / UI can listen without per-instance wiring
        // (any newly-activated checkpoint anywhere fires this).
        public static event Action<Checkpoint> AnyCheckpointActivated;

        void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (Activated) return;

            var respawn = other.GetComponentInParent<RespawnManager>();
            if (respawn == null) return;

            Activated = true;
            respawn.RegisterCheckpoint(this);
            AnyCheckpointActivated?.Invoke(this);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Activated ? new Color(0.2f, 1f, 0.4f, 0.4f) : new Color(0.4f, 0.6f, 1f, 0.3f);
            var col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
        }
    }
}
