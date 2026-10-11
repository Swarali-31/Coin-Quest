using UnityEngine;
using UnityEngine.EventSystems;

namespace CoinQuest
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private float shotCooldown = 0.42f;
        [SerializeField] private float attackRange = 80f;
        [SerializeField] private float aimAssistRadius = 0.45f;
        private float readyAt;
        private EchoPulse pulse;
        private Camera aimCamera;

        public void Configure(PlayerController player, EchoPulse echo, Camera camera)
        {
            pulse = echo;
            aimCamera = camera;
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
            if (Input.GetKeyDown(KeyCode.Q) && pulse != null) pulse.Activate();
            if (Input.GetMouseButtonDown(0) && Time.time >= readyAt &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())) Shoot();
        }

        private void Shoot()
        {
            readyAt = Time.time + shotCooldown;
            if (aimCamera == null) aimCamera = Camera.main;
            if (aimCamera == null) return;

            GameManager game = GameManager.Instance;
            EnemyController target = game.FinalBoss;
            bool lockedToGuardian = target != null && target.gameObject.activeInHierarchy &&
                                    !game.FinalBossDefeated && Vector3.Distance(transform.position, target.transform.position) <= 32f;
            Vector3 impact = lockedToGuardian ? target.transform.position + Vector3.up * 2.4f : Vector3.zero;

            if (!lockedToGuardian)
            {
                Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                RaycastHit[] hits = Physics.SphereCastAll(ray, aimAssistRadius, attackRange, ~0, QueryTriggerInteraction.Ignore);
                float nearestDistance = float.MaxValue;
                Collider nearestCollider = null;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform) || hit.distance >= nearestDistance) continue;
                    nearestDistance = hit.distance;
                    nearestCollider = hit.collider;
                    impact = hit.point;
                }
                target = nearestCollider != null ? nearestCollider.GetComponentInParent<EnemyController>() : null;
            }

            if (target == null)
            {
                game.UI.ShowNotification("Aim at an enemy and Left Click to attack.");
                return;
            }

            target.TakeDamage(1);
            CreateHitFlash(impact);
            game.Audio.Play("shot");
            if (target.IsFinalBoss && target.CurrentHealth > 0) game.UI.ShowNotification("Guardian hit! " + target.CurrentHealth + " hits left. Press Q to stun.");
        }

        private void CreateHitFlash(Vector3 position)
        {
            var flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "Magic Hit Flash";
            flash.transform.position = position;
            flash.transform.localScale = Vector3.one * 0.8f;
            var collider = flash.GetComponent<Collider>(); if (collider != null) collider.enabled = false;
            flash.GetComponent<Renderer>().material = GameManager.Instance.Builder.GlowMaterial(new Color(0.15f, 0.72f, 1f));
            Destroy(flash, 0.16f);
        }
    }
}