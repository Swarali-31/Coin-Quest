using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class WingPickup : MonoBehaviour
    {
        [SerializeField] private float flightSeconds = 20f;
        private Vector3 origin;
        private void Start() { origin = transform.position; }
        private void Update()
        {
            transform.position = origin + Vector3.up * (0.18f + Mathf.Sin(Time.time * 2f) * 0.18f);
            transform.Rotate(Vector3.up, 65f * Time.deltaTime, Space.World);
        }
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;
            player.ActivateFlight(flightSeconds);
            Destroy(gameObject);
        }
    }
}
