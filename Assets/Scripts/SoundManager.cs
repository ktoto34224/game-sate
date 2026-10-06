using UnityEngine;

namespace PeakClimber
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource heartbeatSource;
        private AudioSource ambientSource;

        private AudioClip jumpClip;
        private AudioClip landClip;
        private AudioClip grabClip;
        private AudioClip pitonClip;
        private AudioClip heartbeatClip;
        private AudioClip bounceClip;
        private AudioClip campfireClip;
        private AudioClip berryClip;
        private AudioClip badgeClip;
        private AudioClip fallClip;
        private AudioClip victoryClip;

        public bool IsMuted { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.loop = true;
            heartbeatSource.playOnAwake = false;

            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            ambientSource.volume = 0.25f;

            GenerateAudioClips();
            StartAmbientWind();
        }

        private void GenerateAudioClips()
        {
            jumpClip = CreateToneClip("Jump", 0.15f, t => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(250f, 600f, t) * t));
            landClip = CreateToneClip("Land", 0.12f, t => (Mathf.PerlinNoise(t * 80f, 0f) * 2f - 1f) * (1f - t) * 0.8f);
            grabClip = CreateToneClip("Grab", 0.08f, t => Mathf.Sin(2f * Mathf.PI * 400f * t) * (1f - t) + (Mathf.PerlinNoise(t * 120f, 0f) - 0.5f) * 0.5f);
            pitonClip = CreateToneClip("Piton", 0.22f, t => (Mathf.Sin(2f * Mathf.PI * 1400f * t) + Mathf.Sin(2f * Mathf.PI * 2200f * t) * 0.5f) * Mathf.Exp(-t * 18f));
            heartbeatClip = CreateToneClip("Heartbeat", 0.6f, t =>
            {
                float p1 = Mathf.Sin(2f * Mathf.PI * 65f * t) * Mathf.Clamp01(1f - Mathf.Abs(t - 0.15f) / 0.12f);
                float p2 = Mathf.Sin(2f * Mathf.PI * 55f * t) * Mathf.Clamp01(1f - Mathf.Abs(t - 0.35f) / 0.12f) * 0.7f;
                return p1 + p2;
            });
            bounceClip = CreateToneClip("Bounce", 0.35f, t => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(180f, 720f, Mathf.Sin(t * Mathf.PI * 0.5f)) * t) * (1f - t));
            campfireClip = CreateToneClip("Campfire", 0.6f, t =>
            {
                float c1 = Mathf.Sin(2f * Mathf.PI * 523.25f * t); // C5
                float c2 = Mathf.Sin(2f * Mathf.PI * 659.25f * t); // E5
                float c3 = Mathf.Sin(2f * Mathf.PI * 783.99f * t); // G5
                return (c1 + c2 + c3) / 3f * Mathf.Exp(-t * 4f);
            });
            berryClip = CreateToneClip("Berry", 0.18f, t =>
            {
                float freq = t < 0.09f ? 880f : 1320f;
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t * 4f);
            });
            badgeClip = CreateToneClip("Badge", 0.75f, t =>
            {
                float freq = t < 0.2f ? 587.33f : (t < 0.4f ? 739.99f : 880f);
                return Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Clamp01(1f - (t - 0.4f) * 1.5f);
            });
            fallClip = CreateToneClip("Fall", 0.4f, t => (Mathf.PerlinNoise(t * 50f, 2f) * 2f - 1f) * Mathf.Lerp(0.2f, 0.9f, t));
            victoryClip = CreateToneClip("Victory", 1.8f, t =>
            {
                float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                int idx = Mathf.Min(3, (int)(t / 0.4f));
                return Mathf.Sin(2f * Mathf.PI * notes[idx] * t) * Mathf.Clamp01(1.8f - t);
            });
        }

        private AudioClip CreateToneClip(string name, float duration, System.Func<float, float> waveFunc)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                samples[i] = Mathf.Clamp(waveFunc(t) * 0.7f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void StartAmbientWind()
        {
            if (ambientSource == null) return;
            AudioClip wind = CreateToneClip("WindLoop", 2.0f, t =>
            {
                float n = Mathf.PerlinNoise(t * 15f, 3.5f) * 2f - 1f;
                return n * 0.35f;
            });
            ambientSource.clip = wind;
            if (!IsMuted) ambientSource.Play();
        }

        public void PlayJump() => PlaySfx(jumpClip, 0.7f);
        public void PlayLand() => PlaySfx(landClip, 0.5f);
        public void PlayGrab() => PlaySfx(grabClip, 0.6f);
        public void PlayPiton() => PlaySfx(pitonClip, 0.9f);
        public void PlayBounce() => PlaySfx(bounceClip, 0.9f);
        public void PlayCampfire() => PlaySfx(campfireClip, 0.85f);
        public void PlayBerry() => PlaySfx(berryClip, 0.8f);
        public void PlayBadge() => PlaySfx(badgeClip, 1.0f);
        public void PlayFall() => PlaySfx(fallClip, 0.8f);
        public void PlayVictory() => PlaySfx(victoryClip, 1.0f);

        public void SetHeartbeat(bool active)
        {
            if (heartbeatSource == null || IsMuted) return;
            if (active && !heartbeatSource.isPlaying)
            {
                heartbeatSource.clip = heartbeatClip;
                heartbeatSource.Play();
            }
            else if (!active && heartbeatSource.isPlaying)
            {
                heartbeatSource.Stop();
            }
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            if (IsMuted)
            {
                sfxSource.Stop();
                heartbeatSource.Stop();
                ambientSource.Stop();
            }
            else
            {
                ambientSource.Play();
            }
        }

        private void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (IsMuted || clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
