using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Wave
{
    public List<EnemyType> enemyTypes;
    public List<CustomSpawn> customSpawns;
    public float spawnDuration;
    public float waveDuration;
    public bool activateFear;
    public string playSong = "";
}

[System.Serializable]
public class EnemyType
{
    public GameObject enemyPrefab;
    public int count;
}

[System.Serializable]
public class CustomSpawn
{
    public GameObject gameObject;
    public float activationTime;
    public bool keepUnactive;
    [HideInInspector] public bool hasBeenActivated;
}

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Animator waveAnnouncement;
    [SerializeField] private Leaderboards leaderboards;
    [SerializeField] private TextMeshProUGUI waveDurationText;
    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private float spawnRadius = 40f;
    [SerializeField] private List<Wave> waves;
    [SerializeField] private Wave survivalWave;

    [SerializeField] private int currentWave = 0;
    private float waveTimer = 0f;
    private string currentSong = "";

    private void Start()
    {
        StartNextWave();
    }

    private void Update()
    {
        waveTimer += Time.deltaTime;

        UpdateWaveDurationText();

        Wave wave = currentWave <= waves.Count ? waves[currentWave - 1] : survivalWave;

        // Start next wave if time ends or all enemies have been killed
        if (waveTimer >= wave.waveDuration || (enemyParent.childCount == 0 && waveTimer >= wave.spawnDuration))
        {
            StartNextWave();
        }

        // Custom spawns
        foreach (var customSpawn in wave.customSpawns)
        {
            if (!customSpawn.hasBeenActivated)
            {
                if (waveTimer >= customSpawn.activationTime)
                {
                    if (!customSpawn.keepUnactive)
                    {
                        customSpawn.gameObject.SetActive(true);
                    }

                    customSpawn.gameObject.transform.parent = enemyParent;
                    customSpawn.hasBeenActivated = true;
                }
            }
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        waveTimer = 0f;
        UpdateWaveNumberText();

        if (!GameManager.Instance.hasPlayerDied)
        {
            leaderboards.PostRoundScore(currentWave);
        }

        if (currentWave == 1)
        {
            leaderboards.StartSpeedRunTimer();
        }

        if (currentWave >= 2)
        {
            waveAnnouncement.SetTrigger("Show");
        }

        if (currentWave <= waves.Count)
        {
            Wave wave = waves[currentWave - 1];
            StartCoroutine(SpawnWave(wave));
            GameManager.Instance.ToggleFear(wave.activateFear);

            if (!string.IsNullOrEmpty(wave.playSong) && wave.playSong != currentSong)
            {
                musicManager.PlayMusic(wave.playSong);
            }
        }
        else
        {
            leaderboards.EndSpeedRunTimer();
            GameManager.Instance.SetSurvivalModifier(currentWave - waves.Count);
            StartCoroutine(SpawnWave(survivalWave));
            GameManager.Instance.ToggleFear(survivalWave.activateFear);

            if (!string.IsNullOrEmpty(survivalWave.playSong) && survivalWave.playSong != currentSong)
            {
                musicManager.PlayMusic(survivalWave.playSong);
            }
        }
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        List<EnemySpawnTask> spawnTasks = new List<EnemySpawnTask>();

        // Gather all enemies to be spawned and assign random spawn times
        foreach (var enemyType in wave.enemyTypes)
        {
            for (int i = 0; i < enemyType.count; i++)
            {
                spawnTasks.Add(new EnemySpawnTask
                {
                    enemyPrefab = enemyType.enemyPrefab,
                    spawnTime = Random.Range(0, wave.spawnDuration) // Assign random spawn time within spawnDuration
                });
            }
        }

        // Sort by spawn time to ensure randomized but ordered timing
        spawnTasks.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));

        // Spawn enemies according to the sorted spawn times
        float previousSpawnTime = 0f;
        foreach (var task in spawnTasks)
        {
            float delay = task.spawnTime - previousSpawnTime;
            yield return new WaitForSeconds(delay);
            Vector3 spawnPosition = GetRandomPointOnCircleEdge(spawnRadius);
            GameObject enemy = Instantiate(task.enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.transform.parent = enemyParent;
            previousSpawnTime = task.spawnTime;
        }
    }

    private Vector3 GetRandomPointOnCircleEdge(float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        return new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
    }

    private void UpdateWaveDurationText()
    {
        float remainingTime = (currentWave <= waves.Count ? waves[currentWave - 1].waveDuration : survivalWave.waveDuration) - waveTimer;

        if (remainingTime < 0) remainingTime = 0;

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        waveDurationText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateWaveNumberText()
    {
        waveNumberText.text = currentWave.ToString();
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
    #endif
}

// Helper class to store enemy spawn tasks
public class EnemySpawnTask
{
    public GameObject enemyPrefab;
    public float spawnTime;
}

// Extension method for shuffling a list
public static class ListExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
