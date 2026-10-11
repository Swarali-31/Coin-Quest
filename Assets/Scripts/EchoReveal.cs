using UnityEngine;

namespace CoinQuest
{
    public sealed class EchoReveal : MonoBehaviour
    {
        private Renderer[] renderers;
        private Collider[] colliders;
        private float hideAt;
        private void Awake() { renderers = GetComponentsInChildren<Renderer>(true); colliders = GetComponentsInChildren<Collider>(true); SetVisible(false); }
        public void Reveal(float seconds) { hideAt = Time.time + seconds; SetVisible(true); }
        private void Update() { if (hideAt > 0 && Time.time >= hideAt) { hideAt = 0; SetVisible(false); } }
        private void SetVisible(bool visible) { foreach (var r in renderers) r.enabled = visible; foreach (var c in colliders) c.enabled = visible; }
    }
}
