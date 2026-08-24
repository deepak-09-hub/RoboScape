using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string SoundKey = "SoundEnabled";
    private const string MusicKey = "MusicEnabled";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Clips")]
    [SerializeField] private AudioClip buttonClickSound;

    public bool IsSoundOn { get; private set; }
    public bool IsMusicOn { get; private set; }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadSettings();
        ApplyMusicState();
    }

    private void LoadSettings()
    {
        // First install = both ON.
        IsSoundOn =
            PlayerPrefs.GetInt(
                SoundKey,
                1
            ) == 1;

        IsMusicOn =
            PlayerPrefs.GetInt(
                MusicKey,
                1
            ) == 1;
    }

    // ==================================================
    // SOUND
    // ==================================================

    public bool ToggleSound()
    {
        IsSoundOn = !IsSoundOn;

        PlayerPrefs.SetInt(
            SoundKey,
            IsSoundOn ? 1 : 0
        );

        PlayerPrefs.Save();

        return IsSoundOn;
    }

    public void PlayButtonSound()
    {
        PlaySound(buttonClickSound);
    }

    public void PlaySound(AudioClip clip)
    {
        if (!IsSoundOn ||
            soundSource == null ||
            clip == null)
        {
            return;
        }

        soundSource.PlayOneShot(clip);
    }

    // ==================================================
    // MUSIC
    // ==================================================

    public bool ToggleMusic()
    {
        IsMusicOn = !IsMusicOn;

        PlayerPrefs.SetInt(
            MusicKey,
            IsMusicOn ? 1 : 0
        );

        PlayerPrefs.Save();

        ApplyMusicState();

        return IsMusicOn;
    }

    private void ApplyMusicState()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.loop = true;

        if (IsMusicOn)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }
    }

    // ==================================================
    // VIBRATION
    // ==================================================

    public void VibrateOnLose()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        try
        {
            using AndroidJavaClass unityPlayer =
                new AndroidJavaClass(
                    "com.unity3d.player.UnityPlayer"
                );

            AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>(
                    "currentActivity"
                );

            AndroidJavaObject vibrator =
                activity.Call<AndroidJavaObject>(
                    "getSystemService",
                    "vibrator"
                );

            if (vibrator != null)
            {
                // Very short vibration: 40 milliseconds.
                vibrator.Call(
                    "vibrate",
                    40L
                );
            }
        }
        catch
        {
            Handheld.Vibrate();
        }

#elif UNITY_IOS && !UNITY_EDITOR

        Handheld.Vibrate();

#endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}