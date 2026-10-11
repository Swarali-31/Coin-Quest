using UnityEngine;

namespace CoinQuest
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private float distance = 6.6f;
        [SerializeField] private float sensitivity = 2.3f;
        [SerializeField] private float followSpeed = 14f;
        private Transform target;
        private float yaw;
        private float baseYaw;
        [SerializeField] private float focusHeight = 1.18f;
        private float pitch = 16f;
        private bool inputEnabled;
        public void SetTarget(Transform value)
        {
            target = value;
            if (value == null) return;
            yaw = value.eulerAngles.y;
            baseYaw = yaw;
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0);
            Vector3 focus = target.position + Vector3.up * focusHeight;
            transform.position = focus + orbit * Vector3.back * distance;
            transform.rotation = Quaternion.LookRotation(focus + Vector3.up * 0.22f - transform.position, Vector3.up);
        }
        public void SetInputEnabled(bool value) { inputEnabled = value; }
        private void LateUpdate()
        {
            if (target == null) return;
            if (inputEnabled && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                yaw = Mathf.Clamp(yaw + Input.GetAxis("Mouse X") * sensitivity * 4f, baseYaw - 45f, baseYaw + 45f);
                pitch -= Input.GetAxis("Mouse Y") * sensitivity * 3f;
                pitch = Mathf.Clamp(pitch, -8f, 48f);
            }
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0);
            Vector3 focus = target.position + Vector3.up * focusHeight;
            Vector3 offset = orbit * Vector3.back * distance;
            Vector3 desired = focus + offset;
            RaycastHit[] hits = Physics.RaycastAll(focus, offset.normalized, distance, ~0, QueryTriggerInteraction.Ignore);
            float closest = distance;
            foreach (var hit in hits)
                if (hit.transform != target && !hit.transform.IsChildOf(target)) closest = Mathf.Min(closest, hit.distance);
            if (closest < distance) desired = focus + offset.normalized * Mathf.Max(0.5f, closest - 0.25f);
            transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.unscaledDeltaTime);
            transform.rotation = Quaternion.LookRotation(focus + Vector3.up * 0.22f - transform.position, Vector3.up);
        }
    }
}



