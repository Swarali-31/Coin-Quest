using System.Collections;
using UnityEngine;

namespace CoinQuest
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float invulnerabilitySeconds = 1.75f;
        private float nextDamageTime;
        private Renderer[] renderers;
        public int CurrentHealth { get; private set; } = 5;
        private void Awake() { renderers = GetComponentsInChildren<Renderer>(); }
        public bool TryTakeDamage(int amount)
        {
            if (Time.time < nextDamageTime || CurrentHealth <= 0) return false;
            nextDamageTime = Time.time + invulnerabilitySeconds;
            CurrentHealth = Mathf.Max(0, CurrentHealth - Mathf.Max(1, amount));
            StopAllCoroutines(); StartCoroutine(Flash()); return true;
        }
        public void Restore(int amount) { CurrentHealth = Mathf.Min(5, CurrentHealth + Mathf.Max(0, amount)); }
        private IEnumerator Flash()
        {
            for (int i = 0; i < 6; i++) { foreach (var r in renderers) if (r != null) r.enabled = (i % 2 == 0); yield return new WaitForSeconds(0.075f); }
            foreach (var r in renderers) if (r != null) r.enabled = true;
        }
    }
}
