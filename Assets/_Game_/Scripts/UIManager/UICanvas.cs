using UnityEngine;

public class UICanvas : MonoBehaviour
{
    [Header("Close Behavior")]
    [SerializeField] private bool isDestroyOnClose;

    [Header("Layout")]
    [SerializeField] private bool useSafeArea;
    [SerializeField] private bool useWidescreenProcessing;

    [Header("Popup Child")]
    [SerializeField] private UICanvas[] popups;

    private RectTransform rectTransformCache;
    private bool initialized;

    public UICanvas ParentsPopup { get; private set; }

    protected virtual void Awake()
    {
        OnInit();
    }

    protected virtual void OnInit()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        rectTransformCache = GetComponent<RectTransform>();

        ApplySafeArea();
        ApplyWidescreenProcessing();
        CachePopupParents();
    }

    public virtual void Setup()
    {
        OnInit();

        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.AddBackUI(this);
        UIManager.Instance.PushBackAction(this, BackKey);
    }

    public virtual void BackKey()
    {
        CloseDirectly();
    }

    public virtual void Open()
    {
        CancelInvoke(nameof(CloseDirectly));
        gameObject.SetActive(true);
    }

    public virtual void CloseDirectly()
    {
        CancelInvoke(nameof(CloseDirectly));

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveBackUI(this);
        }

        gameObject.SetActive(false);

        if (isDestroyOnClose)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Close(float delayTime)
    {
        CancelInvoke(nameof(CloseDirectly));
        Invoke(nameof(CloseDirectly), delayTime);
    }

    public T GetPopup<T>() where T : UICanvas
    {
        for (int i = 0; i < popups.Length; i++)
        {
            if (popups[i] is T popup)
            {
                return popup;
            }
        }

        Debug.LogError("Missing popup: " + typeof(T).Name, this);
        return null;
    }

    public T OpenPopup<T>() where T : UICanvas
    {
        T popup = GetPopup<T>();
        if (popup == null)
        {
            return null;
        }

        popup.Setup();
        popup.Open();
        return popup;
    }

    public bool IsOpenedPopup<T>() where T : UICanvas
    {
        T popup = GetPopup<T>();
        return popup != null && popup.gameObject.activeInHierarchy;
    }

    public void ClosePopup<T>(float delayTime) where T : UICanvas
    {
        T popup = GetPopup<T>();
        if (popup != null)
        {
            popup.Close(delayTime);
        }
    }

    public void ClosePopupDirect<T>() where T : UICanvas
    {
        T popup = GetPopup<T>();
        if (popup != null)
        {
            popup.CloseDirectly();
        }
    }

    public void CloseAllPopup()
    {
        for (int i = 0; i < popups.Length; i++)
        {
            if (popups[i] != null)
            {
                popups[i].CloseDirectly();
            }
        }
    }

    private void ApplySafeArea()
    {
        if (!useSafeArea || rectTransformCache == null)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransformCache.anchorMin = anchorMin;
        rectTransformCache.anchorMax = anchorMax;
    }

    private void ApplyWidescreenProcessing()
    {
        if (!useWidescreenProcessing || rectTransformCache == null)
        {
            return;
        }

        float ratio = (float)Screen.width / Screen.height;
        if (ratio >= 2.1f)
        {
            return;
        }

        const float ratioDefault = 850f / 1920f;
        float value = 1f - (ratio - ratioDefault);
        float width = rectTransformCache.rect.width * value;
        rectTransformCache.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    private void CachePopupParents()
    {
        for (int i = 0; i < popups.Length; i++)
        {
            if (popups[i] != null)
            {
                popups[i].ParentsPopup = this;
            }
        }
    }
}
