using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    private const string LevelPrefKey = "Level";
    private const string FallBrickPoolKey = "FallBrick";
    private static readonly Vector3 PlayerSpawnPosition = new Vector3(0f, 0f, -1.2f);

    [SerializeField] private List<GameObject> levels;
    [SerializeField] private Transform mapHolder;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private Player playerInstance;
    private GameObject currentLevel;
    private int level;
    private readonly List<Character> characters = new List<Character>();
    private readonly List<Enemy> enemies = new List<Enemy>();
    private readonly List<Rewarded> rewardedAds = new List<Rewarded>();

    public int CurrentLevel => level;
    public Player CurrentPlayer => playerInstance;

    private void Start()
    {
        level = GetSavedLevelIndex();
        ReloadCurrentLevel();
    }

    public void OnInit()
    {
        if (!HasPrefabLevels())
        {
            return;
        }

        InitializePlayer();
        BindCameraToPlayer();
        ResetRewardedAdsForNewPlayer();
        UIManager.Instance?.OpenUI<LevelText>()?.SetLevel(level + 1);
    }

    public void OnDespawn()
    {
        SimplePool.DespawnAll(FallBrickPoolKey);
        DestroyAllCharacters();
        DestroyCurrentLevel();
    }

    public void OnLoadLevel(int index)
    {
        if (!HasPrefabLevels())
        {
            return;
        }

        DestroyCurrentLevel();
        Transform parent = mapHolder != null ? mapHolder : transform;
        currentLevel = Instantiate(levels[Mathf.Clamp(index, 0, levels.Count - 1)], parent);
    }

    public void OnNextLevel()
    {
        if (!HasPrefabLevels())
        {
            LoadNextScene();
            return;
        }

        level = (level + 1) % levels.Count;
        SaveCurrentLevel();
        ReloadCurrentLevel();
    }

    public void OnReplay()
    {
        if (HasPrefabLevels())
        {
            ReloadCurrentLevel();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetToLevel1()
    {
        if (!HasPrefabLevels())
        {
            PlayerPrefs.DeleteKey(LevelPrefKey);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        level = 0;
        SaveCurrentLevel();
        ReloadCurrentLevel();
    }

    public void OnWin() => GameManager.Instance.GameWin();
    public void OnLose() => GameManager.Instance.GameLose();

    public void RegisterCharacter(Character character)
    {
        if (character == null || characters.Contains(character))
        {
            return;
        }

        characters.Add(character);

        if (character is Player player)
        {
            playerInstance = player;
        }
        else if (character is Enemy enemy)
        {
            enemies.Add(enemy);
        }
    }

    public void UnregisterCharacter(Character character)
    {
        if (character == null)
        {
            return;
        }

        characters.Remove(character);

        if (character == playerInstance)
        {
            playerInstance = null;
        }
        else if (character is Enemy enemy)
        {
            enemies.Remove(enemy);
        }
    }

    public void RegisterRewardedAd(Rewarded rewardedAd)
    {
        if (rewardedAd != null && !rewardedAds.Contains(rewardedAd))
        {
            rewardedAds.Add(rewardedAd);
        }
    }

    public void UnregisterRewardedAd(Rewarded rewardedAd)
    {
        rewardedAds.Remove(rewardedAd);
    }

    public void SetGameplayActorsPaused(bool isPaused)
    {
        if (isPaused)
        {
            playerInstance?.ResetMovementState();
        }

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            if (isPaused)
            {
                enemy.PauseMovement();
            }
            else
            {
                enemy.ResumeMovement();
            }
        }
    }

    private int GetSavedLevelIndex()
    {
        return HasPrefabLevels() ? Mathf.Clamp(PlayerPrefs.GetInt(LevelPrefKey, 0), 0, levels.Count - 1) : 0;
    }

    private void SaveCurrentLevel()
    {
        PlayerPrefs.SetInt(LevelPrefKey, level);
        PlayerPrefs.Save();
    }

    private void ReloadCurrentLevel()
    {
        OnDespawn();
        OnLoadLevel(level);
        OnInit();
    }

    private void LoadNextScene()
    {
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int nextBuildIndex = Mathf.Min(currentBuildIndex + 1, SceneManager.sceneCountInBuildSettings - 1);
        SceneManager.LoadScene(nextBuildIndex);
    }

    private void InitializePlayer()
    {
        if (playerPrefab == null)
        {
            return;
        }

        if (playerInstance != null)
        {
            Destroy(playerInstance.gameObject);
        }

        playerInstance = Instantiate(playerPrefab, PlayerSpawnPosition, Quaternion.identity);
        playerInstance.transform.SetPositionAndRotation(PlayerSpawnPosition, Quaternion.identity);
        playerInstance.ResetMovementState();
    }

    private void BindCameraToPlayer()
    {
        if (playerInstance == null || cinemachineCamera == null)
        {
            return;
        }

        cinemachineCamera.Target.TrackingTarget = playerInstance.transform;
    }

    private void DestroyCurrentLevel()
    {
        if (currentLevel == null)
        {
            return;
        }

        Destroy(currentLevel);
        currentLevel = null;
    }

    private void DestroyAllCharacters()
    {
        Character[] currentCharacters = characters.ToArray();
        characters.Clear();
        enemies.Clear();
        playerInstance = null;

        foreach (Character character in currentCharacters)
        {
            if (character != null)
            {
                Destroy(character.gameObject);
            }
        }
    }

    private void ResetRewardedAdsForNewPlayer()
    {
        for (int i = rewardedAds.Count - 1; i >= 0; i--)
        {
            Rewarded rewardedAd = rewardedAds[i];
            if (rewardedAd == null)
            {
                rewardedAds.RemoveAt(i);
                continue;
            }

            rewardedAd.ResetRewardAvailability();
        }
    }

    private bool HasPrefabLevels() => levels != null && levels.Count > 0;
}
