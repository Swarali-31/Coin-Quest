using UnityEngine;

namespace CoinQuest
{
    public sealed class AudioManager : MonoBehaviour
    {
        private AudioSource source;
        private bool enabledAudio = true;
        public void Build() { source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0; }
        public void Toggle() { enabledAudio = !enabledAudio; }
        public void Play(string sound)
        {
            if (!enabledAudio || source == null) return;
            float frequency = sound == "coin" ? 880 : sound == "rune" || sound == "pulse" ? 620 : sound == "damage" ? 180 : sound == "victory" ? 980 : 440;
            int rate = 22050, count = sound == "victory" ? rate / 3 : rate / 12;
            var clip = AudioClip.Create("Quest " + sound, count, 1, rate, false);
            var data = new float[count];
            for (int i = 0; i < count; i++) data[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / rate) * (1f - i / (float)count) * 0.12f;
            clip.SetData(data, 0); source.PlayOneShot(clip); Destroy(clip, count / (float)rate + 0.1f);
        }
    }
}
