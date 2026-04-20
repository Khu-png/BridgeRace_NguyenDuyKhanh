using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    private const string LevelPrefKey = "Level";
    private static readonly Vector3 PlayerSpawnPosition = new Vector3(0f, 0f, -1.2f);

    [SerializeField] private List<GameObject> levels;
    [SerializeField] private Transform mapHolder;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private Player playerInstance;
    private GameObject currentLevel;
    private int level;

    public int CurrentLevel => level;

    private void Start()
    {
        level = GetSavedLevelIndex();
        LoadCurrentLevel();
    }

    private int GetSavedLevelIndex()
    {
        if (!HasPrefabLevels())
        {
            return 0;
        }

        int savedIndex = PlayerPrefs.GetInt(LevelPrefKey, 0);
        return Mathf.Clamp(savedIndex, 0, levels.Count - 1);
    }

    private void SaveCurrentLevel()
    {
        PlayerPrefs.SetInt(LevelPrefKey, level);
        PlayerPrefs.Save();
    }

    private void LoadCurrentLevel()
    {
        if (!HasPrefabLevels())
        {
            return;
        }

        LoadLevel(level);
        InitializePlayer();
        MovePlayerToSpawn(PlayerSpawnPosition);
        BindCameraToPlayer();

        UIManager.Instance?.UpdateLevelText(level + 1);
    }

    public void LoadLevel(int index)
    {
        if (!HasPrefabLevels())
        {
            return;
        }

        if (index >= levels.Count)
        {
            index = 0;
        }

        if (currentLevel != null)
        {
            currentLevel.SetActive(false);
            Destroy(currentLevel);
            currentLevel = null;
        }

        Transform parent = mapHolder != null ? mapHolder : transform;
        currentLevel = Instantiate(levels[index], parent);
    }

    private void InitializePlayer()
    {
        if (playerPrefab == null)
        {
            return;
        }

        if (playerInstance != null)
        {
            playerInstance.gameObject.SetActive(false);
            Destroy(playerInstance.gameObject);
            playerInstance = null;
        }

        GameObject playerObject = Instantiate(playerPrefab, PlayerSpawnPosition, Quaternion.identity);
        playerInstance = playerObject.GetComponent<Player>();
    }

    private void MovePlayerToSpawn(Vector3 position)
    {
        if (playerInstance == null)
        {
            return;
        }

        playerInstance.transform.SetPositionAndRotation(position, Quaternion.identity);
        playerInstance.ResetMovementState();

        Rigidbody playerRigidbody = playerInstance.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void BindCameraToPlayer()
    {
        if (playerInstance == null)
        {
            return;
        }

        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Target.TrackingTarget = playerInstance.transform;
        }
    }

    public void OnNextLevel()
    {
        if (HasPrefabLevels())
        {
            level++;
            if (level >= levels.Count)
            {
                level = 0;
            }

            SaveCurrentLevel();
            ReloadCurrentLevel();
            return;
        }

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int nextBuildIndex = currentBuildIndex + 1;

        if (nextBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextBuildIndex = currentBuildIndex;
        }

        SceneManager.LoadScene(nextBuildIndex);
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

    private void ReloadCurrentLevel()
    {
        DestroyAllCharacters();

        if (currentLevel != null)
        {
            currentLevel.SetActive(false);
            Destroy(currentLevel);
            currentLevel = null;
        }

        LoadCurrentLevel();
    }

    private void DestroyAllCharacters()
    {
        Character[] characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach (Character character in characters)
        {
            if (character == null)
            {
                continue;
            }

            character.gameObject.SetActive(false);
            Destroy(character.gameObject);
        }

        playerInstance = null;
    }

    public void OnWin() => GameManager.Instance.GameWin();
    public void OnLose() => GameManager.Instance.GameLose();

    private bool HasPrefabLevels() => levels != null && levels.Count > 0;
}
