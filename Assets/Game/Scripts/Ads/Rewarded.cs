using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System.Collections;

public class Rewarded : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
  [SerializeField] Button _showAdButton;
  [SerializeField] string _androidAdUnitId = "Rewarded_Android";
  [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
  [SerializeField] int _rewardBrickCount = 5;
  string _adUnitId = null; // This will remain null for unsupported platforms
  bool _isAdLoaded;
  bool _hasRewardedThisPlayer;
  int _pendingRewardBrickCount;

  void Awake()
  {   
    // Get the Ad Unit ID for the current platform:
    _adUnitId = Application.platform == RuntimePlatform.IPhonePlayer
      ? _iOSAdUnitId
      : _androidAdUnitId;

    // Hide the button until the ad is ready to show:
    if (_showAdButton != null)
    {
      _showAdButton.gameObject.SetActive(false);
    }
  }

  void Start()
  {
    StartCoroutine(LoadAdWhenInitialized());
  }

  IEnumerator LoadAdWhenInitialized()
  {
    yield return new WaitUntil(() => Advertisement.isInitialized);
    LoadAd();
  }

  // Call this public method when you want to get an ad ready to show.
  public void LoadAd()
  {
    if (_hasRewardedThisPlayer)
    {
      if (_showAdButton != null)
      {
        _showAdButton.gameObject.SetActive(false);
      }

      return;
    }

    if (string.IsNullOrEmpty(_adUnitId))
    {
      Debug.LogWarning("Rewarded Ad Unit ID is missing.");
      return;
    }

    if (!Advertisement.isInitialized)
    {
      return;
    }

    _isAdLoaded = false;
    if (_showAdButton != null)
    {
      _showAdButton.gameObject.SetActive(false);
    }

    // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
    Debug.Log("Loading Ad: " + _adUnitId);
    Advertisement.Load(_adUnitId, this);
  }

  // If the ad successfully loads, add a listener to the button and enable it:
  public void OnUnityAdsAdLoaded(string adUnitId)
  {
    Debug.Log("Ad Loaded: " + adUnitId);

    if (adUnitId.Equals(_adUnitId))
    {
      _isAdLoaded = true;

      // Show the button for users to click:
      if (_showAdButton != null && !_hasRewardedThisPlayer)
      {
        _showAdButton.gameObject.SetActive(true);
      }
    }
  }

  // Implement a method to execute when the user clicks the button:
  public void ShowAd()
  {
    if (_hasRewardedThisPlayer)
    {
      Debug.Log("Rewarded ad already used for this player.");
      return;
    }

    if (!_isAdLoaded)
    {
      Debug.Log("Rewarded ad is not ready yet.");
      LoadAd();
      return;
    }

    // Hide the button:
    if (_showAdButton != null)
    {
      _showAdButton.gameObject.SetActive(false);
    }

    _isAdLoaded = false;

    // Then show the ad:
    Advertisement.Show(_adUnitId, this);
  }

  // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
  public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
  {
    if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
    {
      _hasRewardedThisPlayer = true;
      Debug.Log("rewarded");
      GrantRewardBricks();
    }

    LoadAd();
  }

  // Implement Load and Show Listener error callbacks:
  public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
  {
    Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
    // Use the error details to determine whether to try to load another ad.
    _isAdLoaded = false;
  }

  public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
  {
    Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
    // Use the error details to determine whether to try to load another ad.
    LoadAd();
  }

  public void OnUnityAdsShowStart(string adUnitId) { }
  public void OnUnityAdsShowClick(string adUnitId) { }

  public void ResetRewardAvailability()
  {
    _hasRewardedThisPlayer = false;
    GrantPendingRewardBricks();
    LoadAd();
  }

  private void GrantRewardBricks()
  {
    if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.MainMenu)
    {
      _pendingRewardBrickCount += _rewardBrickCount;
      Debug.Log("Rewarded bricks will be granted when the player starts.");
      return;
    }

    if (!GrantPlayerBricks(_rewardBrickCount))
    {
      _pendingRewardBrickCount += _rewardBrickCount;
    }
  }

  private void GrantPendingRewardBricks()
  {
    if (_pendingRewardBrickCount <= 0)
    {
      return;
    }

    if (GrantPlayerBricks(_pendingRewardBrickCount))
    {
      _pendingRewardBrickCount = 0;
    }
  }

  private bool GrantPlayerBricks(int brickCount)
  {
    Player player = FindFirstObjectByType<Player>();
    if (player == null)
    {
      Debug.LogWarning("Cannot grant rewarded bricks because Player was not found.");
      return false;
    }

    Vector3 pickupPosition = player.transform.position;
    for (int i = 0; i < brickCount; i++)
    {
      player.CollectBrick(pickupPosition);
    }

    return true;
  }
}
