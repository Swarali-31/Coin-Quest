using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class HealingPickup : MonoBehaviour
    {
        private Vector3 origin;
        private void Start() { origin = transform.position; }
        private void Update() { transform.position = origin + Vector3.up * (Mathf.Sin(Time.time * 2.2f) * 0.12f); transform.Rotate(Vector3.up, 45f * Time.deltaTime, Space.World); }
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null || GameManager.Instance == null) return;
            int oldHealth = GameManager.Instance.Health;
            GameManager.Instance.CollectHealing(1);
            if (GameManager.Instance.Health > oldHealth) Destroy(gameObject);
        }
    }
}
