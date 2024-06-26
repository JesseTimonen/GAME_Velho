using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct SoundTrack
{
    public string name;
    public AudioClip clip;
    [Range(0, 1)] public float volume;
}

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private SoundTrack[] soundTracks;
    private AudioSource audioSource;
    public string currentlyPlaying = "";

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource.clip == null)
        {
            audioSource.volume = 0;
        }
    }

    private void Start()
    {
        ModifyVolumeLevels(PlayerPrefs.GetFloat("MusicVolume", 0f), PlayerPrefs.GetFloat("BackgroundVolume", 0f), PlayerPrefs.GetFloat("SpellVolume", 0f), PlayerPrefs.GetFloat("UIVolume", 0f));
    }

    public void PlayMusic(string trackName, float musicVolume = -1f, float fadeDuration = 2f)
    {
        AudioClip newTrack = GetClipFromName(trackName);

        if (musicVolume == -1f)
        {
            musicVolume = GetVolumeFromName(trackName);
        }

        if (newTrack != null)
        {
            currentlyPlaying = trackName;
            StartCoroutine(AnimateMusicCrossfade(newTrack, musicVolume, fadeDuration));
        }
        else
        {
            StopMusic();
        }
    }

    public void StopMusic(float fadeDuration = 2f)
    {
        StartCoroutine(AnimateMusicFadeOut(fadeDuration));
    }

    private IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float musicVolume, float fadeDuration)
    {
        if (audioSource.isPlaying)
        {
            yield return StartCoroutine(FadeMusicVolume(audioSource.volume, 0, fadeDuration));
        }

        audioSource.clip = nextTrack;
        audioSource.Play();
        yield return StartCoroutine(FadeMusicVolume(0, musicVolume, fadeDuration));
    }

    private IEnumerator AnimateMusicFadeOut(float fadeDuration)
    {
        yield return StartCoroutine(FadeMusicVolume(audioSource.volume, 0, fadeDuration));
        audioSource.Stop();
    }

    private IEnumerator FadeMusicVolume(float startVolume, float endVolume, float fadeDuration)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / fadeDuration);
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, percent);
            yield return null;
        }
        audioSource.volume = endVolume;
    }

    private AudioClip GetClipFromName(string name)
    {
        foreach (SoundTrack soundTrack in soundTracks)
        {
            if (soundTrack.name == name)
            {
                return soundTrack.clip;
            }
        }

        return null;
    }

    private float GetVolumeFromName(string name)
    {
        foreach (SoundTrack soundTrack in soundTracks)
        {
            if (soundTrack.name == name)
            {
                return soundTrack.volume;
            }
        }

        return 1f;
    }

    public void ModifyVolumeLevels(float musicVolume, float backgroundVolume, float spellVolume, float UIVolume)
    {
        // If volume was set to 0% (-60 slider value) mute audio as much as possible (I don't know a way to mute audio mixers, so -80db is best I can do for now)
        if (musicVolume <= -80) { musicVolume = -160; }
        if (backgroundVolume <= -80) { backgroundVolume = -160; }
        if (spellVolume <= -80) { spellVolume = -160; }
        if (UIVolume <= -80) { UIVolume = -160; }

        audioMixer.SetFloat("MusicVolume", musicVolume / 2f);
        audioMixer.SetFloat("BackgroundVolume", backgroundVolume / 2f);
        audioMixer.SetFloat("SpellVolume", spellVolume / 2f);
        audioMixer.SetFloat("UIVolume", UIVolume / 2f);
    }
}
