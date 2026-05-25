using System.Collections.Generic;
using UnityEngine;

namespace DungeonBlade.Bank
{
    /// <summary>
    /// Drives the lobby ambient soundscape:
    /// - 2D wind/ambient bed loop playing constantly at low volume.
    /// - 3D fire crackle loops auto-spawned on every Light tagged as a brazier/fire pit
    ///   (any Light component with a child object whose name contains "Brazier" or "FirePit").
    /// - Random day-time bird chirps occasionally play (Sparrow Sounds) when sunlight is bright.
    /// - Optional night ambience cross-fades in when sun is down.
    ///
    /// Attach to any GameObject (typically the same one with AudioListener / Environment).
    /// Drag clips into the slots, hit Play.
    /// </summary>
    [DisallowMultipleComponent]
    public class AmbientAudioController : MonoBehaviour
    {
        [Header("Wind / outdoor bed (2D, plays constantly)")]
        [Tooltip("Looped 2D ambient bed — wind through trees, generic outdoor.")]
        [SerializeField] AudioClip windAmbience;
        [Range(0f, 1f)] [SerializeField] float windVolume = 0.25f;

        [Header("Fire crackle (3D, spawned on each brazier + fire pit)")]
        [Tooltip("Looped fire crackle / burning sound. Played in 3D at every brazier light position.")]
        [SerializeField] AudioClip fireCrackle;
        [Range(0f, 1f)] [SerializeField] float fireVolume = 0.7f;
        [Tooltip("Inner 3D distance — at or below this, fire plays at full volume.")]
        [SerializeField] float fireMinDistance = 1.0f;
        [Tooltip("Outer 3D distance — beyond this, fire is fully silent.")]
        [SerializeField] float fireMaxDistance = 9f;
        [Tooltip("Custom rolloff curve mode for fire 3D falloff. Linear is more dramatic and predictable; Logarithmic feels more natural but quieter at distance.")]
        [SerializeField] AudioRolloffMode fireRolloffMode = AudioRolloffMode.Linear;
        [Tooltip("Doppler effect strength on fire sources (0 = no pitch shift from movement, 1 = full doppler).")]
        [Range(0f, 1f)] [SerializeField] float fireDopplerLevel = 0f;

        [Header("Day birds (random, sunny hours)")]
        [Tooltip("Pool of bird chirp clips. One is randomly picked between intervals.")]
        [SerializeField] AudioClip[] dayBirds;
        [Range(0f, 1f)] [SerializeField] float birdsVolume = 0.4f;
        [Tooltip("Average seconds between bird chirps.")]
        [SerializeField] float birdIntervalSeconds = 8f;
        [Tooltip("Random jitter added to interval (0..N seconds).")]
        [SerializeField] float birdIntervalJitter = 6f;

        [Header("Night ambience (optional)")]
        [Tooltip("Looped night ambience clip — crickets, owls, wind. Cross-fades in when sun is down.")]
        [SerializeField] AudioClip nightAmbience;
        [Range(0f, 1f)] [SerializeField] float nightVolume = 0.25f;

        [Header("Time of day reference (optional)")]
        [Tooltip("If assigned, day birds only play when sun is up; night ambience cross-fades in at night. If null, day audio plays constantly and night is disabled.")]
        [SerializeField] TimeOfDayCycle timeOfDay;

        AudioSource _windSource;
        AudioSource _nightSource;
        AudioSource _birdSource;
        readonly List<AudioSource> _fireSources = new List<AudioSource>();
        float _nextBirdAt;

        void Start()
        {
            // 2D wind bed
            if (windAmbience != null)
            {
                _windSource = CreateSource("Audio_Wind", spatial: false);
                _windSource.clip = windAmbience;
                _windSource.loop = true;
                _windSource.volume = windVolume;
                _windSource.Play();
            }

            // Optional night bed (starts silent, blended via Update if timeOfDay set)
            if (nightAmbience != null)
            {
                _nightSource = CreateSource("Audio_Night", spatial: false);
                _nightSource.clip = nightAmbience;
                _nightSource.loop = true;
                _nightSource.volume = 0f;
                _nightSource.Play();
            }

            // Bird oneshot player
            if (dayBirds != null && dayBirds.Length > 0)
            {
                _birdSource = CreateSource("Audio_Birds", spatial: false);
                _birdSource.loop = false;
                _birdSource.volume = birdsVolume;
                ScheduleNextBird();
            }

            // 3D fire sources on every Light that's a child of a Brazier/FirePit
            if (fireCrackle != null)
            {
                SpawnFireSources();
            }
        }

        void Update()
        {
            // Day/night cross-fade — use the directional light's current intensity as the proxy.
            float dayness = ComputeDayness();
            if (_windSource != null) _windSource.volume = windVolume * Mathf.Lerp(0.55f, 1f, dayness);
            if (_nightSource != null) _nightSource.volume = nightVolume * (1f - dayness);
            if (_birdSource != null)
            {
                if (Time.time >= _nextBirdAt && dayBirds != null && dayBirds.Length > 0 && dayness > 0.3f)
                {
                    _birdSource.PlayOneShot(dayBirds[Random.Range(0, dayBirds.Length)], birdsVolume * dayness);
                    ScheduleNextBird();
                }
            }
        }

        // 1 = bright day, 0 = night. Read from the directional light's intensity if available.
        float ComputeDayness()
        {
            Light sun = null;
            if (timeOfDay != null)
            {
                // TimeOfDayCycle has a private directionalLight field; grab via component.
                var dl = timeOfDay.GetComponent<Light>();
                if (dl != null && dl.type == LightType.Directional) sun = dl;
            }
            if (sun == null)
            {
                foreach (var l in FindObjectsOfType<Light>())
                {
                    if (l.type == LightType.Directional) { sun = l; break; }
                }
            }
            if (sun == null) return 1f;
            // Map intensity 0..2.5 → 0..1.
            return Mathf.Clamp01(sun.intensity / 2.5f);
        }

        void SpawnFireSources()
        {
            // Find every Light in the scene that's a child of an object named like a brazier/fire.
            foreach (var l in FindObjectsOfType<Light>())
            {
                if (l.type != LightType.Point) continue;
                var n = (l.transform.parent != null ? l.transform.parent.name : "").ToLowerInvariant();
                bool isFire = n.Contains("brazier") || n.Contains("firepit") || n.Contains("lantern");
                if (!isFire) continue;

                var go = new GameObject($"Audio_Fire_{l.transform.parent.name}");
                go.transform.SetParent(l.transform, false);
                go.transform.localPosition = Vector3.zero;
                var src = go.AddComponent<AudioSource>();
                src.clip = fireCrackle;
                src.loop = true;
                src.volume = fireVolume;
                src.spatialBlend = 1f;                  // fully 3D
                src.spread = 60f;                       // 3D sound cone spread for more directional feel
                src.rolloffMode = fireRolloffMode;
                src.minDistance = fireMinDistance;
                src.maxDistance = fireMaxDistance;
                src.dopplerLevel = fireDopplerLevel;
                src.playOnAwake = false;
                // Random start time so all 8+ fires don't crackle in unison.
                src.Play();
                src.time = Random.Range(0f, fireCrackle.length);
                _fireSources.Add(src);
            }
        }

        AudioSource CreateSource(string name, bool spatial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.spatialBlend = spatial ? 1f : 0f;
            src.playOnAwake = false;
            return src;
        }

        void ScheduleNextBird()
        {
            _nextBirdAt = Time.time + birdIntervalSeconds + Random.Range(0f, Mathf.Max(0f, birdIntervalJitter));
        }
    }
}
