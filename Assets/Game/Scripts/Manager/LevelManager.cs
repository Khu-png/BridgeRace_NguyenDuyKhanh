using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] private List<GameObject> levels;
    [SerializeField] private Transform mapHolder;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CameraFollow cameraFollow;

    private GameObject playerInstance;
    private GameObject currentLevel;
    private int level;

    public int CurrentLevel => level;

    private void Start()
    {
        level = PlayerPrefs.GetInt("Level", 0);

        if (HasPrefabLevels())
        {
            LoadLevel(level);
            OnInit();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLevelText(level + 1);
        }
    }

    private void OnInit()
    {
        if (playerPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = new Vector3(0f, 1f, 0f);
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(playerInstance.transform);
        }
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
            Destroy(currentLevel);
        }

        Transform parent = mapHolder != null ? mapHolder : transform;
        currentLevel = Instantiate(levels[index], parent);
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

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLevelText(level + 1);
            }

            PlayerPrefs.SetInt("Level", level);
            PlayerPrefs.Save();

            OnReplay();
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
            Despawn();
            LoadLevel(level);
            OnInit();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnWin() => GameManager.Instance.GameWin();
    public void OnLose() => GameManager.Instance.GameLose();

    private void Despawn()
    {
        if (playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }

        if (currentLevel != null)
        {
            Destroy(currentLevel);
            currentLevel = null;
        }
    }

    private bool HasPrefabLevels()
    {
        return levels != null && levels.Count > 0;
    }
}
