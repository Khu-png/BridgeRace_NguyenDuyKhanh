using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    [Header("Brick Stack")]
    [SerializeField] protected Transform brickHolder;
    [SerializeField] protected string brickKey = "PlayerBrick";
    [SerializeField] protected float brickOffset = 0.5f;
    [SerializeField] protected float moveSpeed = 5f;

    [Header("Color")]
    [SerializeField] public Color characterColor = Color.white;

    protected Stack<GameObject> brickStack = new Stack<GameObject>();
    
    public void CollectBrick()
    {
        BrickSpawner brickSpawner = FindFirstObjectByType<BrickSpawner>();
        GameObject brick = SimplePool.Spawn(brickKey, brickHolder.position, Quaternion.identity);
        
        brick.transform.SetParent(brickHolder, false);
        MeshRenderer[] renderers = brick.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in renderers)
        {
            mr.material = new Material(mr.material);
            mr.material.color = characterColor;
        }
        
        brickStack.Push(brick);
        
        StartCoroutine(MoveToStackPosition(brick, brickStack.Count - 1));
    }

    public virtual GameObject RemoveBrick()
    {
        if (brickStack.Count == 0) return null;

        GameObject topBrick = brickStack.Pop();

        StartCoroutine(UpdateStackSmooth());

        return topBrick;
    }
    
    protected IEnumerator MoveToStackPosition(GameObject brick, int index)
    {
        Vector3 start = brick.transform.localPosition;
        Vector3 target = new Vector3(0, index * brickOffset, -0.5f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            brick.transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        brick.transform.localPosition = target;
    }
    
    protected IEnumerator UpdateStackSmooth()
    {
        int i = 0;
        foreach (var brick in brickStack)
        {
            yield return StartCoroutine(MoveToStackPosition(brick, i));
            i++;
        }
    }

    public int GetStackCount()
    {
        return brickStack.Count;
    }
}