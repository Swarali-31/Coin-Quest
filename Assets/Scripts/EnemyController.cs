using System.Collections;
using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(Collider))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private int health = 3;
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float damageRange = 1.9f;
        [SerializeField] private float attackCooldown = 1.9f;
        [SerializeField] private float attackWindupSeconds = 0.9f;
        [SerializeField] private bool flying;
        private Transform player;
        private Renderer[] renderers;
        private Color[] originalColors;
        private float stunnedUntil;
        private float nextAttack;
        private float attackWindupEnds;
        private bool attackWindingUp;
        private Vector3 patrolOrigin;
        private int maxHealth;
        private bool finalBoss;
        private float patrolPhase;
        public bool IsFinalBoss { get { return finalBoss; } }
        public float HealthFraction { get { return maxHealth <= 0 ? 0f : Mathf.Clamp01((float)health / maxHealth); } }
        public int MaxHealth { get { return maxHealth; } }
        public int CurrentHealth { get { return Mathf.Max(0, health); } }
        public void Configure(int hitPoints, bool isFlying, float awareness, float speed)
        {
            health = hitPoints; maxHealth = hitPoints; flying = isFlying; detectionRange = awareness; moveSpeed = speed;
        }
        public void SetFinalBoss(bool value)
        {
            finalBoss = value;
            if (value) { attackCooldown = 2.4f; attackWindupSeconds = 1.4f; }
        }
        private void Start()
        {
            maxHealth = health; patrolOrigin = transform.position; patrolPhase = Random.value * 6.28f; renderers = GetComponentsInChildren<Renderer>(); originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) originalColors[i] = renderers[i].sharedMaterial.color;
            if (GameManager.Instance != null) player = GameManager.Instance.Player;
        }
        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || health <= 0) return;
            if (player == null) player = GameManager.Instance.Player;
            if (Time.time < stunnedUntil) { transform.Rotate(0, 60 * Time.deltaTime, 0); return; }
            if (player == null) return;
            Vector3 delta = player.position - transform.position;
            delta.y = 0;
            float distance = delta.magnitude;
            var playerController = player.GetComponent<PlayerController>();
            bool playerGrounded = playerController == null || !playerController.IsAirborne;
            bool canDamagePlayer = flying || playerGrounded;
            if (distance <= damageRange)
            {
                if (!canDamagePlayer)
                {
                    attackWindingUp = false;
                    return;
                }
                if (!attackWindingUp && Time.time >= nextAttack)
                {
                    attackWindingUp = true;
                    attackWindupEnds = Time.time + attackWindupSeconds;
                    var visual = GetComponent<GuardianVisualAnimator>(); if (visual != null) visual.TriggerAttack();
                    if (finalBoss) GameManager.Instance.Notify("GUARDIAN WINDING UP! Dodge with Shift or stun with Q.");
                }
                else if (attackWindingUp && Time.time >= attackWindupEnds)
                {
                    attackWindingUp = false;
                    nextAttack = Time.time + attackCooldown;
                    GameManager.Instance.DamagePlayer(1, flying ? "A shadow bat struck you!" : "A guardian struck you!");
                }
            }
            else
            {
                attackWindingUp = false;
                if (distance <= detectionRange && distance > damageRange * 0.8f)
                {
                    Vector3 direction = delta.normalized;
                    transform.position += direction * moveSpeed * Time.deltaTime;
                    if (direction.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 5f * Time.deltaTime);
                    if (flying) transform.position = new Vector3(transform.position.x, patrolOrigin.y + Mathf.Sin(Time.time * 3f) * 0.45f, transform.position.z);
                }
                else if (distance > detectionRange && !finalBoss)
                {
                    Vector3 patrol = patrolOrigin + Vector3.right * Mathf.Sin(Time.time * 0.65f + patrolPhase) * 1.5f;
                    transform.position = Vector3.MoveTowards(transform.position, patrol, moveSpeed * 0.35f * Time.deltaTime);
                }
            }
        }
        public void Stun(float seconds) { stunnedUntil = Mathf.Max(stunnedUntil, Time.time + seconds); attackWindingUp = false; nextAttack = Mathf.Max(nextAttack, Time.time + seconds); }
        public void TakeDamage(int amount)
        {
            if (health <= 0) return;
            health -= Mathf.Max(1, amount);
            if (finalBoss && GameManager.Instance != null && GameManager.Instance.UI != null) GameManager.Instance.UI.UpdateBossHealth(this);
            StopAllCoroutines(); StartCoroutine(HitFlash());
            if (health <= 0)
            {
                if (GameManager.Instance != null)
                {
                    if (finalBoss) GameManager.Instance.OnFinalBossDefeated(this);
                    else GameManager.Instance.Notify(gameObject.name == "Shadow Bat" ? "The shadow bat fades into the dusk." : "The stone guardian crumbles!");
                }
                foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
                StartCoroutine(FallAway());
            }
        }
        private IEnumerator HitFlash()
        {
            foreach (var r in renderers) if (r != null) r.material.color = Color.white;
            yield return new WaitForSeconds(0.12f);
            for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].material.color = originalColors[i];
        }
        private IEnumerator FallAway()
        {
            yield return new WaitForSeconds(0.35f);
            float t = 0; Vector3 start = transform.localScale;
            while (t < 0.5f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(start, Vector3.zero, t / 0.5f); yield return null; }
            gameObject.SetActive(false);
        }
    }
}
