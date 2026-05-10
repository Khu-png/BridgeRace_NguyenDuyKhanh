using System;
using System.Collections;
using UnityEngine;

public class Counter : UICanvas
{
    [SerializeField] private GameObject three;
    [SerializeField] private GameObject two;
    [SerializeField] private GameObject one;
    [SerializeField] private GameObject go;
    [SerializeField] private float stepDuration = 1f;

    private Coroutine countRoutine;

    public static void PlayBeforeGameplay(Action onCompleted)
    {
        Counter counter = UIManager.Instance?.OpenUI<Counter>();
        if (counter == null)
        {
            onCompleted?.Invoke();
            return;
        }

        counter.Play(onCompleted);
    }

    protected override void OnInit()
    {
        base.OnInit();
        ResolveItems();
        HideAll();
    }

    public void Play(Action onCompleted)
    {
        if (countRoutine != null)
        {
            StopCoroutine(countRoutine);
        }

        countRoutine = StartCoroutine(PlayRoutine(onCompleted));
    }

    public override void CloseDirectly()
    {
        if (countRoutine != null)
        {
            StopCoroutine(countRoutine);
            countRoutine = null;
        }

        HideAll();
        base.CloseDirectly();
    }

    private IEnumerator PlayRoutine(Action onCompleted)
    {
        yield return ShowStep(three);
        yield return ShowStep(two);
        yield return ShowStep(one);
        yield return ShowStep(go);

        countRoutine = null;
        CloseDirectly();
        onCompleted?.Invoke();
    }

    private IEnumerator ShowStep(GameObject item)
    {
        HideAll();
        if (item != null)
        {
            item.SetActive(true);
        }

        yield return new WaitForSeconds(stepDuration);
    }

    private void HideAll()
    {
        SetActive(three, false);
        SetActive(two, false);
        SetActive(one, false);
        SetActive(go, false);
    }

    private void ResolveItems()
    {
        three = three != null ? three : transform.Find("3")?.gameObject;
        two = two != null ? two : transform.Find("2")?.gameObject;
        one = one != null ? one : transform.Find("1")?.gameObject;
        go = go != null ? go : transform.Find("Go!")?.gameObject;
    }

    private static void SetActive(GameObject item, bool isActive)
    {
        if (item != null)
        {
            item.SetActive(isActive);
        }
    }
}
