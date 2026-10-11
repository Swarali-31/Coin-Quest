using UnityEngine;

namespace CoinQuest
{
    /// <summary>Small procedural gait layered onto the existing player controller.</summary>
    public sealed class ExplorerVisualAnimator : MonoBehaviour
    {
        private Transform actor, leftArm, rightArm, leftLeg, rightLeg, head;
        private Vector3 previousPosition;
        private float gait;
        public void Configure(Transform playerRoot, Transform lArm, Transform rArm, Transform lLeg, Transform rLeg, Transform headTransform)
        {
            actor = playerRoot; leftArm = lArm; rightArm = rArm; leftLeg = lLeg; rightLeg = rLeg; head = headTransform;
            previousPosition = actor.position;
        }
        private void Update()
        {
            if (actor == null) return;
            Vector3 delta = actor.position - previousPosition; previousPosition = actor.position;
            float speed = new Vector3(delta.x, 0, delta.z).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            float amount = GameManager.Instance != null && GameManager.Instance.IsPlaying ? Mathf.Clamp01(speed / 2.8f) : 0;
            gait += Time.deltaTime * Mathf.Lerp(2f, 10f, amount);
            float swing = Mathf.Sin(gait) * 31f * amount;
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-swing, 0, -7f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(swing, 0, 7f);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(swing, 0, 0);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-swing, 0, 0);
            if (head != null) head.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 1.2f) * 2f, 0);
        }
    }
}
