using System.Collections;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    private const string StartTriggerName = "Start";
    private const string EndTriggerName = "End";

    [SerializeField] private Animator animator;
    [SerializeField] private float animationDuration = 0.85f;

    private static LevelLoader instance;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        instance = this;
    }

    public static void PlayEnd()
    {
        LevelLoader loader = GetOrCreateInstance();
        if (loader == null) return;

        loader.gameObject.SetActive(true);
        loader.ResolveAnimator();
        loader.TriggerEnd();
    }

    public static void PlayMenuTransition(System.Action middleAction = null, System.Action finishedAction = null)
    {
        LevelLoader loader = GetOrCreateInstance();
        if (loader == null) return;

        loader.gameObject.SetActive(true);
        loader.ResolveAnimator();

        if (loader.transitionRoutine != null)
        {
            loader.StopCoroutine(loader.transitionRoutine);
        }

        loader.transitionRoutine = loader.StartCoroutine(loader.PlayMenuTransitionRoutine(middleAction, finishedAction));
    }

    private IEnumerator PlayMenuTransitionRoutine(System.Action middleAction, System.Action finishedAction)
    {
        TriggerStart();
        yield return new WaitForSecondsRealtime(animationDuration);
        middleAction?.Invoke();
        TriggerEnd();
        yield return new WaitForSecondsRealtime(animationDuration);
        transitionRoutine = null;
        finishedAction?.Invoke();
    }

    private void TriggerStart()
    {
        ResolveAnimator();

        if (animator != null)
        {
            animator.ResetTrigger(EndTriggerName);
            animator.SetTrigger(StartTriggerName);
        }
    }

    private void TriggerEnd()
    {
        ResolveAnimator();

        if (animator != null)
        {
            animator.ResetTrigger(StartTriggerName);
            animator.SetTrigger(EndTriggerName);
        }
    }

    private void ResolveAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private static LevelLoader GetOrCreateInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        LevelLoader[] loaders = Resources.FindObjectsOfTypeAll<LevelLoader>();
        foreach (LevelLoader loader in loaders)
        {
            if (loader == null || !loader.gameObject.scene.IsValid()) continue;

            instance = loader;
            return instance;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform target in transforms)
        {
            if (target == null || !target.gameObject.scene.IsValid()) continue;
            if (target.name != nameof(LevelLoader)) continue;

            instance = target.GetComponent<LevelLoader>() ?? target.gameObject.AddComponent<LevelLoader>();
            return instance;
        }

        return null;
    }
}
