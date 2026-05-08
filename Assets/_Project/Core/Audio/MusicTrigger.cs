using UnityEngine;

namespace DungeonBlade.Core.Audio
{
    // Drop one of these into a scene, assign a music clip, and that track
    // plays (and crossfades from any previous track) whenever the scene loads.
    public class MusicTrigger : MonoBehaviour
    {
        [SerializeField] AudioClip track;
        [Tooltip("0 = silent. Most music sits comfortably around 0.5-0.7.")]
        [Range(0f, 1f)]
        [SerializeField] float volume = 0.6f;
        [Tooltip("If true, plays only when the scene first loads. If false, plays every time this script's GameObject becomes active (useful for arena-only music).")]
        [SerializeField] bool playOnSceneLoadOnly = true;
        [Tooltip("If wired, replaces the main track when the boss arena seal triggers. Leave empty in non-boss scenes.")]
        [SerializeField] AudioClip arenaTrack;

        bool _arenaActive;

        void Start()
        {
            if (playOnSceneLoadOnly) PlayMain();
        }

        void OnEnable()
        {
            if (!playOnSceneLoadOnly) PlayMain();
        }

        void PlayMain()
        {
            if (track == null || _arenaActive) return;
            MusicPlayer.TryPlay(track, volume);
        }

        // Hook this up to BossArenaTrigger via a UnityEvent if you want
        // dynamic arena music. Or call from any script: trigger.SwitchToArena().
        public void SwitchToArena()
        {
            if (arenaTrack == null) return;
            _arenaActive = true;
            MusicPlayer.TryPlay(arenaTrack, volume);
        }

        public void RestoreMain()
        {
            _arenaActive = false;
            if (track != null) MusicPlayer.TryPlay(track, volume);
        }
    }
}
