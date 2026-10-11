using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class CoinCollectible : MonoBehaviour
    {
        private Vector3 start;
        private float phase;
        private bool collected;
        private void Start() { start = transform.position; phase = Random.value * 5f; }
        private void Update()
        {
            if (collected || (GameManager.Instance != null && !GameManager.Instance.IsPlaying)) return;
            transform.Rotate(Vector3.up, 100f * Time.deltaTime, Space.World);
            transform.position = start + Vector3.up * (Mathf.Sin(Time.time * 2.3f + phase) * 0.14f);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (collected || other.GetComponentInParent<PlayerController>() == null || GameManager.Instance == null) return;
            collected = true; GameManager.Instance.CollectCoin(); Destroy(gameObject);
        }
    }
}
