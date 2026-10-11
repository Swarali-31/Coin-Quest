using UnityEngine;

namespace CoinQuest
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float runSpeed = 5.4f;
        [SerializeField] private float walkSpeed = 5.4f;
        [SerializeField] private float jumpHeight = 1.8f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float dodgeSpeed = 12f;
        [SerializeField] private float dodgeDuration = 0.2f;
        [SerializeField] private float dodgeCooldown = 0.9f;
        [SerializeField] private float flightRiseSpeed = 3.8f;
        [SerializeField] private float maxFlightHeight = 4.2f;
        [SerializeField] private float flightLandingWindow = 1.15f;
        private CharacterController motor;
        private ThirdPersonCamera cameraRig;
        private Transform visual;
        private float verticalVelocity;
        private float dodgeUntil;
        private float nextDodge;
        private Vector3 dodgeDirection;
        private float flightRemaining;
        private float flightBaseHeight;
        private bool safeLanding;
        private bool bossEncounterNotified;

        public bool IsFlying { get { return flightRemaining > 0f; } }
        public float FlightRemaining { get { return Mathf.Max(0f, flightRemaining); } }
        public bool IsAirborne { get { return flightRemaining > 0f || verticalVelocity > 0.05f || (motor != null && !motor.isGrounded); } }
        private void Awake() { motor = GetComponent<CharacterController>(); }
        public void Configure(ThirdPersonCamera rig, Transform model) { cameraRig = rig; visual = model; }
        public void ActivateFlight(float duration)
        {
            flightRemaining = Mathf.Clamp(duration, 0f, 20f);
            flightBaseHeight = transform.position.y;
            verticalVelocity = flightRiseSpeed;
            safeLanding = false;
            if (GameManager.Instance != null && GameManager.Instance.UI != null) GameManager.Instance.UI.ShowNotification("GOLDEN WINGS! Auto-flight for 20 seconds, then you land.");
        }
        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || motor == null || !motor.enabled) return;
            float dt = Time.deltaTime;
            Transform cam = cameraRig != null ? cameraRig.transform : transform;
            Vector3 cameraForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            if (cameraForward.sqrMagnitude < 0.001f) cameraForward = Vector3.forward;
            Vector3 cameraRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            if (cameraRight.sqrMagnitude < 0.001f) cameraRight = Vector3.right;

            // Temple-run pacing: the route carries the explorer forward; A/D steer across the safe track.
            float steer = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                        - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            if ((transform.position.x > 4.5f && steer > 0f) || (transform.position.x < -4.5f && steer < 0f)) steer = 0f;
            bool arenaFight = GameManager.Instance.FinalBoss != null && GameManager.Instance.FinalBoss.gameObject.activeInHierarchy &&
                              Mathf.Abs(transform.position.z - GameManager.Instance.FinalBoss.transform.position.z) < 24f;
            if (arenaFight && !bossEncounterNotified)
            {
                bossEncounterNotified = true;
                GameManager.Instance.UI.ShowNotification("FINAL ARENA: W/S move, A/D dodge, Left Click attacks, Q stuns.");
            }
            float pace = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? walkSpeed * 0.55f : runSpeed;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) pace = runSpeed * 1.15f;
            Vector3 move;
            if (arenaFight)
            {
                float forwardInput = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) -
                                     (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
                move = cameraForward * forwardInput * walkSpeed + cameraRight * steer * walkSpeed;
                if (move.sqrMagnitude > walkSpeed * walkSpeed) move = move.normalized * walkSpeed;
            }
            else move = Vector3.forward * pace + cameraRight * (steer * walkSpeed * 0.9f);
            move = Vector3.ProjectOnPlane(move, Vector3.up);
            if (move.sqrMagnitude > pace * pace) move = move.normalized * pace;

            bool grounded = motor.isGrounded;
            if (grounded && verticalVelocity < 0f) { if (safeLanding) flightRemaining = 0f; verticalVelocity = -2f; safeLanding = false; }
            if (flightRemaining > 0f)
            {
                flightRemaining = Mathf.Max(0f, flightRemaining - dt);
                if (flightRemaining <= flightLandingWindow) safeLanding = true;
                if (safeLanding) verticalVelocity = -flightRiseSpeed;
                else
                {
                    if (Input.GetKey(KeyCode.LeftControl)) { flightRemaining = 0f; safeLanding = true; verticalVelocity = -2.5f; }
                    else verticalVelocity = transform.position.y < flightBaseHeight + maxFlightHeight ? flightRiseSpeed : 0f;


                }
            }
            else if (!safeLanding)
            {
                if (grounded && Input.GetKeyDown(KeyCode.Space)) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                verticalVelocity = Mathf.Max(verticalVelocity + gravity * dt, -30f);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDodge)
            {
                nextDodge = Time.time + dodgeCooldown;
                dodgeUntil = Time.time + dodgeDuration;
                dodgeDirection = steer != 0f ? cameraRight * Mathf.Sign(steer) : Vector3.forward;
                dodgeDirection.y = 0f;
                dodgeDirection.Normalize();
            }
            Vector3 horizontal = move;
            if (Time.time < dodgeUntil) horizontal = Vector3.forward * pace + dodgeDirection * dodgeSpeed;
            motor.Move((horizontal + Vector3.up * verticalVelocity) * dt);

            Vector3 facing = Vector3.ProjectOnPlane(horizontal, Vector3.up);
            if (facing.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(facing), 12f * dt);
            if (visual != null) visual.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 12f) * 0.008f);
        }
        public void Teleport(Vector3 position)
        {
            if (motor == null) motor = GetComponent<CharacterController>();
            motor.enabled = false; transform.position = position; motor.enabled = true;
            verticalVelocity = 0f; flightRemaining = 0f; safeLanding = false;
        }
        public Vector3 AimDirection(Camera camera)
        {
            if (camera == null) return transform.forward;
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit[] hits = Physics.RaycastAll(ray, 80f, ~0, QueryTriggerInteraction.Ignore);
            float closest = float.MaxValue;
            Vector3 aimPoint = ray.origin + ray.direction * 80f;
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                if (hit.distance >= closest) continue;
                closest = hit.distance;
                aimPoint = hit.point;
            }
            Vector3 muzzle = transform.position + Vector3.up * 1.25f + transform.forward * 0.65f;
            return (aimPoint - muzzle).normalized;
        }
    }
}
