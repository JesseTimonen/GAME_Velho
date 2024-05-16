using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DemoAudio : MonoBehaviour
{
    [SerializeField] private MusicManager musicManager;
    private string[] musicTracks = new string[] { "Main Menu", "Theme 1", "Theme 2", "Boss prelude", "Boss Phase 1", "Boss Phase 2", "Ending" };
    private int index = -1;


    private void Start()
    {
        // index = UnityEngine.Random.Range(0, musicTracks.Length);
        SwitchMusic();
    }


    private void SwitchMusic()
    {
        if (musicTracks.Length <= 1)
        {
            return;
        }

        List<int> possibleIndices = Enumerable.Range(0, musicTracks.Length).ToList();
        possibleIndices.Remove(index);
        // index = possibleIndices[UnityEngine.Random.Range(0, possibleIndices.Count)];
        index++;

        if (index >= musicTracks.Length)
        {
            index = 0;
        }

        musicManager.PlayMusic(musicTracks[index]);

        Invoke("SwitchMusic", 10f);
    }
}
