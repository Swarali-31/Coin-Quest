using UnityEngine;

namespace CoinQuest
{
    public sealed class MagicProjectile : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float remaining;
        private bool spent;
        private Transform owner;
        public void Launch(Vector3 aim, float velocity, float lifetime, Transform shotOwner = null) { direction = aim.normalized; speed = velocity; remaining = lifetime; owner = shotOwner; }
        private void Update() { if (spent) return; transform.position += direction * speed * Time.deltaTime; remaining -= Time.deltaTime; if (remaining <= 0) Destroy(gameObject); }
        private void OnTriggerEnter(Collider other)
        {
            if (spent) return;
            if (owner != null && (other.transform == owner || other.transform.IsChildOf(owner))) return;
            var enemy = other.GetComponentInParent<EnemyController>(); if (enemy != null) enemy.TakeDamage(1);
            spent = true; Destroy(gameObject);
        }
    }
}
