using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    private const string LevelPrefKey = "Level";
    private const string FallBrickPoolKey = "FallBrick";
    private const string TransitionObjectName = "LevelLoader";
    private const string TransitionStartTrigger = "Start";
    private const string TransitionEndTrigger = "End";
    private static readonly Vector3 PlayerSpawnPosition = new Vector3(0f, 0f, -1.2f);

    [SerializeField] private List<GameObject> levels;
    [SerializeField] private Transform mapHolder;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionDuration = 2f;

    private Player playerInstance;
    private GameObject currentLevel;
    private int level;
    private Coroutine transitionRoutine;
    private readonly List<Enemy> enemies = new List<Enemy>();
    private readonly List<Rewarded> rewardedAds = new List<Rewarded>();
    private readonly HashSet<ColorType> usedCharacterColors = new HashSet<ColorType>();

    public int CurrentLevel => level;
    public Player CurrentPlayer => playerInstance;

    private void Start()
    {
        ApplyApplicationSettings();
        level = GetSavedLevelIndex();
        ReloadCurrentLevel();
        PlayTransitionEnd();
        GameManager.Instance?.GameBegin();
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
    }

    public void OnDespawn()
    {
        SimplePool.ReturnAll(FallBrickPoolKey);
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
        currentLevel = Instantiate(levels[Mathf.Clamp(index, 0, levels.Count - 1)], mapHolder);
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

    public void PlayTransition(Action middleAction, Action finishedAction = null)
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        ResolveTransitionAnimator();
        if (transitionAnimator == null)
        {
            middleAction?.Invoke();
            finishedAction?.Invoke();
            return;
        }

        transitionRoutine = StartCoroutine(TransitionRoutine(middleAction, finishedAction));
    }

    public void RegisterCharacter(Character character)
    {
        if (character == null)
        {
            return;
        }

        if (character is Player player)
        {
            playerInstance = player;
        }
        else if (character is Enemy enemy && !enemies.Contains(enemy))
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

    public ColorType GetUniqueCharacterColorType(CharacterDataSO characterData)
    {
        if (characterData == null)
        {
            return ColorType.None;
        }

        ColorType colorType = characterData.GetRandomColorTypeExcept(usedCharacterColors);
        usedCharacterColors.Add(colorType);
        return colorType;
    }

    public void SetGameplayActorsPaused(bool isPaused)
    {
        if (playerInstance != null)
        {
            if (isPaused || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                playerInstance.PauseMovement();
            }
            else
            {
                playerInstance.ResumeMovement();
            }
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
        usedCharacterColors.Clear();
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
        if (playerInstance != null)
        {
            Destroy(playerInstance.gameObject);
        }

        playerInstance = Instantiate(playerPrefab, PlayerSpawnPosition, Quaternion.identity);
        playerInstance.transform.SetPositionAndRotation(PlayerSpawnPosition, Quaternion.identity);
        playerInstance.ResetForSpawn();
    }

    private void BindCameraToPlayer()
    {
        cameraFollow.SetTarget(playerInstance.transform);
    }

    private void DestroyCurrentLevel()
    {
        if (currentLevel == null)
        {
            return;
        }

        currentLevel.SetActive(false);
        Destroy(currentLevel);
        currentLevel = null;
    }

    private void DestroyAllCharacters()
    {
        if (playerInstance != null)
        {
            Player player = playerInstance;
            playerInstance = null;
            player.OnDespawn();
            Destroy(player.gameObject);
        }

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            enemies.RemoveAt(i);

            if (enemy != null)
            {
                enemy.OnDespawn();
                Destroy(enemy.gameObject);
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

    public void GrantPendingRewardedAds()
    {
        for (int i = rewardedAds.Count - 1; i >= 0; i--)
        {
            Rewarded rewardedAd = rewardedAds[i];
            if (rewardedAd == null)
            {
                rewardedAds.RemoveAt(i);
                continue;
            }

            rewardedAd.GrantPendingRewardBricks();
        }
    }

    private IEnumerator TransitionRoutine(Action middleAction, Action finishedAction)
    {
        PlayTransitionStart();
        yield return new WaitForSecondsRealtime(transitionDuration);

        middleAction?.Invoke();

        PlayTransitionEnd();
        yield return new WaitForSecondsRealtime(transitionDuration);

        transitionRoutine = null;
        finishedAction?.Invoke();
    }

    private void PlayTransitionStart()
    {
        ResolveTransitionAnimator();
        if (transitionAnimator == null)
        {
            return;
        }

        transitionAnimator.ResetTrigger(TransitionEndTrigger);
        transitionAnimator.SetTrigger(TransitionStartTrigger);
    }

    private void PlayTransitionEnd()
    {
        ResolveTransitionAnimator();
        if (transitionAnimator == null)
        {
            return;
        }

        transitionAnimator.ResetTrigger(TransitionStartTrigger);
        transitionAnimator.SetTrigger(TransitionEndTrigger);
    }

    private void ResolveTransitionAnimator()
    {
        if (transitionAnimator != null)
        {
            transitionAnimator.gameObject.SetActive(true);
            return;
        }

        GameObject transitionObject = GameObject.Find(TransitionObjectName);
        if (transitionObject == null)
        {
            return;
        }

        transitionObject.SetActive(true);
        transitionAnimator = transitionObject.GetComponent<Animator>();
    }

    private void ApplyApplicationSettings()
    {
        Input.multiTouchEnabled = false;
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        const int maxScreenHeight = 1280;
        float ratio = (float)Screen.currentResolution.width / Screen.currentResolution.height;
        if (Screen.currentResolution.height > maxScreenHeight)
        {
            Screen.SetResolution(Mathf.RoundToInt(ratio * maxScreenHeight), maxScreenHeight, true);
        }
    }

    private bool HasPrefabLevels() => levels.Count > 0;
}
