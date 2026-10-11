using UnityEngine;

namespace CoinQuest
{
    /// <summary>Breathing, gait and attack swing for the assembled stone guardian.</summary>
    public sealed class GuardianVisualAnimator : MonoBehaviour
    {
        private Transform actor, leftArm, rightArm, leftLeg, rightLeg, body;
        private Vector3 previousPosition;
        private float attackStarted = -10f;
        private float gait;
        public void Configure(Transform root, Transform lArm, Transform rArm, Transform lLeg, Transform rLeg, Transform model)
        {
            actor = root; leftArm = lArm; rightArm = rArm; leftLeg = lLeg; rightLeg = rLeg; body = model; previousPosition = root.position;
        }
        public void TriggerAttack() { attackStarted = Time.time; }
        private void Update()
        {
            if (actor == null || body == null) return;
            Vector3 delta = actor.position - previousPosition; previousPosition = actor.position;
            float speed = new Vector3(delta.x, 0, delta.z).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            float moving = Mathf.Clamp01(speed / 1.6f);
            gait += Time.deltaTime * Mathf.Lerp(2f, 6f, moving);
            float walk = Mathf.Sin(gait) * 24f * moving;
            float attackAge = Time.time - attackStarted;
            float attack = attackAge >= 0 && attackAge < 0.55f ? Mathf.Sin(attackAge / 0.55f * Mathf.PI) * 78f : 0;
            body.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 2.3f) * 0.012f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-walk, 0, -8f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(walk - attack, 0, 8f);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(walk * 0.6f, 0, 0);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-walk * 0.6f, 0, 0);
        }
    }
}
