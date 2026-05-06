using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace DungeonBlade.Core.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        [SerializeField] AudioMixerGroup musicGroup;
        [SerializeField] float crossfadeDuration = 1.5f;
        [SerializeField] float defaultVolume = 0.6f;

        AudioSource _a;
        AudioSource _b;
        AudioSource _active;
        AudioClip _currentClip;
        Coroutine _routine;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _a = MakeSource("MusicSourceA");
            _b = MakeSource("MusicSourceB");
            _active = _a;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        AudioSource MakeSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.outputAudioMixerGroup = musicGroup;
            src.volume = 0f;
            return src;
        }

        public void Play(AudioClip clip, float volume = -1f)
        {
            if (clip == null) { Stop(); return; }
            if (clip == _currentClip && _active.isPlaying) return;

            float v = volume >= 0f ? volume : defaultVolume;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Crossfade(clip, v));
        }

        public void Stop()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FadeOut());
        }

        IEnumerator Crossfade(AudioClip newClip, float targetVolume)
        {
            var prev = _active;
            var next = _active == _a ? _b : _a;

            next.clip = newClip;
            next.volume = 0f;
            next.Play();
            _active = next;
            _currentClip = newClip;

            float t = 0f;
            float startPrevVol = prev.volume;
            while (t < crossfadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / crossfadeDuration;
                next.volume = Mathf.Lerp(0f, targetVolume, k);
                prev.volume = Mathf.Lerp(startPrevVol, 0f, k);
                yield return null;
            }
            next.volume = targetVolume;
            prev.volume = 0f;
            prev.Stop();
            prev.clip = null;
        }

        IEnumerator FadeOut()
        {
            float startVol = _active.volume;
            float t = 0f;
            while (t < crossfadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _active.volume = Mathf.Lerp(startVol, 0f, t / crossfadeDuration);
                yield return null;
            }
            _active.volume = 0f;
            _active.Stop();
            _active.clip = null;
            _currentClip = null;
        }

        public static void TryPlay(AudioClip clip, float volume = -1f)
        {
            if (Instance != null) Instance.Play(clip, volume);
        }
    }
}
