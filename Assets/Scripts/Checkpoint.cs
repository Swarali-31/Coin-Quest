using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class Checkpoint : MonoBehaviour, IWorldInteractable
    {
        private bool activeCheckpoint;
        private Renderer crystal;
        public string InteractionPrompt { get { return activeCheckpoint ? "Checkpoint active" : "Activate checkpoint"; } }
        public void Configure(Renderer gem) { crystal = gem; }
        public void Interact(GameManager manager)
        {
            if (activeCheckpoint) return;
            activeCheckpoint = true;
            if (crystal != null) crystal.material.color = new Color(0.3f, 1f, 0.55f);
            manager.ActivateCheckpoint(transform.position);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (activeCheckpoint || other.GetComponentInParent<PlayerController>() == null) return;
            var game = GameManager.Instance;
            if (game != null) Interact(game);
        }
    }
}
