using UnityEngine;

namespace CoinQuest
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float radius = 2.8f;
        private IWorldInteractable current;
        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) { SetCurrent(null); return; }
            Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.up, radius, ~0, QueryTriggerInteraction.Collide);
            float nearestDistance = float.MaxValue;
            IWorldInteractable nearest = null;
            foreach (var hit in hits)
            {
                foreach (var component in hit.GetComponentsInParent<MonoBehaviour>())
                {
                    if (!(component is IWorldInteractable interactable)) continue;
                    float distance = (hit.ClosestPoint(transform.position + Vector3.up) - transform.position).sqrMagnitude;
                    if (distance < nearestDistance) { nearestDistance = distance; nearest = interactable; }
                }
            }
            SetCurrent(nearest);
            if (current != null && Input.GetKeyDown(KeyCode.E)) GameManager.Instance.Interact(current);
        }
        private void SetCurrent(IWorldInteractable value)
        {
            current = value;
            if (GameManager.Instance != null && GameManager.Instance.UI != null)
                GameManager.Instance.UI.SetPrompt(value != null ? "E  " + value.InteractionPrompt : string.Empty);
        }
    }
}
