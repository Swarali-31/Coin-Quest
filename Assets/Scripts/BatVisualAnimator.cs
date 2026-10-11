using UnityEngine;

namespace CoinQuest
{
    /// <summary>Flaps the bat's broad membrane wings while its enemy root hovers and pursues.</summary>
    public sealed class BatVisualAnimator : MonoBehaviour
    {
        private Transform leftWing, rightWing;
        public void Configure(Transform left, Transform right) { leftWing = left; rightWing = right; }
        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            float flap = Mathf.Sin(Time.time * 13f) * 42f;
            if (leftWing != null) leftWing.localRotation = Quaternion.Euler(flap, 0, -flap * 0.15f);
            if (rightWing != null) rightWing.localRotation = Quaternion.Euler(-flap, 0, flap * 0.15f);
        }
    }
}
