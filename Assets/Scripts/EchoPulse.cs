using UnityEngine;

namespace CoinQuest
{
    public sealed class EchoPulse : MonoBehaviour
    {
        [SerializeField] private float cooldown = 5f;
        [SerializeField] private float radius = 11f;
        [SerializeField] private float stunSeconds = 2f;
        private float readyAt;
        public float CooldownRemaining { get { return Mathf.Max(0, readyAt - Time.time); } }
        public bool IsReady { get { return Time.time >= readyAt; } }
        public void Activate()
        {
            if (!IsReady || GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            readyAt = Time.time + cooldown;
            PulseRingEffect.Create(transform.position + Vector3.up * 0.22f, radius);
            var game = GameManager.Instance;
            var guardian = game.FinalBoss;
            bool guardianInRange = guardian != null && guardian.gameObject.activeInHierarchy &&
                                   Vector3.Distance(transform.position, guardian.transform.position) <= 32f;
            if (guardianInRange) guardian.Stun(stunSeconds);
            foreach (var hidden in FindObjectsOfType<EchoReveal>()) hidden.Reveal(4f);
            game.Audio.Play("pulse");
            game.UI.ShowNotification(guardianInRange ? "Guardian stunned! Attack with Left Click." : "No guardian in range. Move closer, then press Q to stun it.");
        }
    }
}
