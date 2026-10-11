using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class Hazard : MonoBehaviour
    {
        private float nextHit;
        private void OnTriggerStay(Collider other)
        {
            if (Time.time < nextHit || other.GetComponentInParent<PlayerController>() == null || GameManager.Instance == null) return;
            nextHit = Time.time + 1.5f;
            GameManager.Instance.DamagePlayer(1, "A jungle trap hurt you!");
        }
    }
}
