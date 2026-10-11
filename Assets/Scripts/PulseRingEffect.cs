using UnityEngine;

namespace CoinQuest
{
    public sealed class PulseRingEffect : MonoBehaviour
    {
        private LineRenderer line;
        private float radius;
        private float age;
        private const float Duration = 0.8f;
        public static void Create(Vector3 position, float maxRadius)
        {
            var obj = new GameObject("Echo Pulse Ring"); obj.transform.position = position;
            obj.AddComponent<PulseRingEffect>().radius = maxRadius;
        }
        private void Awake()
        {
            line = gameObject.AddComponent<LineRenderer>(); line.useWorldSpace = false; line.loop = true; line.positionCount = 65; line.widthMultiplier = 0.14f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = new Color(0.18f, 0.75f, 1f, 0.9f);
        }
        private void Update()
        {
            age += Time.deltaTime; float t = Mathf.Clamp01(age / Duration); float r = radius * t;
            for (int i = 0; i < line.positionCount; i++) { float a = i / 64f * Mathf.PI * 2; line.SetPosition(i, new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r)); }
            line.startColor = line.endColor = new Color(0.18f, 0.75f, 1f, 1f - t);
            if (age >= Duration) Destroy(gameObject);
        }
    }
}
