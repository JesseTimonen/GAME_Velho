using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


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
    public static MusicManager instance;
    [SerializeField] private SoundTrack[] soundTracks;
    private AudioSource audioSource;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource.clip == null)
        {
            audioSource.volume = 0;
        }
    }


    public void PlayMusic(string trackName, float musicVolume = -1f, float fadeDuration = 1f)
    {
        AudioClip newTrack = GetClipFromName(trackName);

        if (musicVolume == -1f)
        {
            musicVolume = GetVolumeFromName(trackName);
        }

        if (newTrack != null)
        {
            StartCoroutine(AnimateMusicCrossfade(newTrack, musicVolume, fadeDuration));
        }
        else
        {
            StopMusic();
        }
    }


    public void StopMusic(float fadeDuration = 1f)
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
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.unscaledTime / fadeDuration;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, percent);
            yield return null;
        }
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
}
