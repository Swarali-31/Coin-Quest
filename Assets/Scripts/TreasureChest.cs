using System.Collections;
using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class TreasureChest : MonoBehaviour, IWorldInteractable
    {
        private Transform lid;
        private bool claimed;
        public string InteractionPrompt { get { return claimed ? "Relic claimed" : "Open the treasure chest"; } }
        public void Configure(Transform lidTransform) { lid = lidTransform; }
        public void Interact(GameManager manager) { if (!claimed) manager.TryOpenChest(this); }
        public void Claim()
        {
            if (claimed) return;
            claimed = true;
            StartCoroutine(OpenRoutine());
        }
        private IEnumerator OpenRoutine()
        {
            if (lid != null)
            {
                Quaternion start = lid.localRotation;
                Quaternion end = Quaternion.Euler(-70, 0, 0);
                float t = 0;
                while (t < 0.65f) { t += Time.deltaTime; lid.localRotation = Quaternion.Slerp(start, end, t / 0.65f); yield return null; }
            }
            yield return new WaitForSeconds(0.25f);
            if (GameManager.Instance != null) GameManager.Instance.Win();
        }
    }
}
