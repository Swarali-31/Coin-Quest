using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class RuneCollectible : MonoBehaviour, IWorldInteractable
    {
        [SerializeField] private int runeId;
        [SerializeField] private string runeName = "Blue";
        private Vector3 start;
        public string InteractionPrompt { get { return "Collect the " + runeName + " Rune"; } }
        public void Configure(int id, string label) { runeId = id; runeName = label; }
        private void Start() { start = transform.position; }
        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            transform.Rotate(Vector3.up, 36f * Time.deltaTime, Space.World);
            transform.position = start + Vector3.up * (0.18f + Mathf.Sin(Time.time * 1.8f) * 0.12f);
        }
        public void Interact(GameManager manager)
        {
            if (manager.HasRune(runeId)) return;
            manager.CollectRune(runeId, runeName); Destroy(gameObject);
        }
    }
}
