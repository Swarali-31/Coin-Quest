using System.Collections;
using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class TempleDoor : MonoBehaviour, IWorldInteractable
    {
        private Transform slab;
        private Collider barrier;
        private bool unlocked;
        private bool opening;
        private bool open;
        public bool IsOpen { get { return open; } }
        public string InteractionPrompt { get { return open ? "Temple unlocked" : unlocked ? "Open the temple door" : "Temple door locked"; } }
        public void Configure(Transform doorSlab, Collider doorCollider) { slab = doorSlab; barrier = doorCollider; }
        public void Unlock() { unlocked = true; }
        public void Interact(GameManager manager)
        {
            if (open || opening) return;
            if (!unlocked) { manager.TryOpenDoor(this); return; }
            StartCoroutine(OpenRoutine());
        }
        public void Open() { if (!open && !opening) StartCoroutine(OpenRoutine()); }
        private IEnumerator OpenRoutine()
        {
            opening = true;
            if (barrier != null) barrier.enabled = false;
            Vector3 origin = slab.localPosition;
            float time = 0;
            while (time < 1.2f)
            {
                time += Time.deltaTime;
                slab.localPosition = origin + Vector3.up * (3.8f * Mathf.SmoothStep(0, 1, time / 1.2f));
                yield return null;
            }
            open = true; opening = false;
            if (GameManager.Instance != null) { GameManager.Instance.Notify("The Lost Temple is open!"); GameManager.Instance.Audio.Play("rune"); }
        }
    }
}
