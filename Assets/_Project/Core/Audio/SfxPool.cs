using UnityEngine;
using UnityEngine.Audio;

namespace DungeonBlade.Core.Audio
{
    public class SfxPool : MonoBehaviour
    {
        public static SfxPool Instance { get; private set; }

        [SerializeField] int poolSize = 12;
        [SerializeField] AudioMixerGroup sfxGroup;

        AudioSource[] _sources;
        int _next;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sources = new AudioSource[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"SfxSource_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.outputAudioMixerGroup = sfxGroup;
                src.spatialBlend = 0f;
                _sources[i] = src;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Play(AudioClip clip, float volume = 1f, float pitchVariation = 0f)
        {
            if (clip == null) return;
            var src = _sources[_next];
            _next = (_next + 1) % _sources.Length;

            src.pitch = pitchVariation > 0f ? 1f + Random.Range(-pitchVariation, pitchVariation) : 1f;
            src.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public static void TryPlay(AudioClip clip, float volume = 1f, float pitchVariation = 0f)
        {
            if (Instance != null) Instance.Play(clip, volume, pitchVariation);
        }
    }
}
