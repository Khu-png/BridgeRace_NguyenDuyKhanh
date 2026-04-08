using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Brick Stack")]
    [SerializeField] private Transform brickHolder;
    [SerializeField] private string playerBrickKey = "PlayerBrick";
    [SerializeField] private float brickOffset;
    [SerializeField] private float moveSpeed;
    
    private Stack<GameObject> brickStack = new Stack<GameObject>();
    
    public void CollectBrick()
    {
        GameObject brick = SimplePool.Spawn(playerBrickKey, brickHolder.position, Quaternion.identity);

        brick.transform.SetParent(brickHolder, false);

        brickStack.Push(brick);
        
        StartCoroutine(MoveToStackPosition(brick, brickStack.Count - 1));
    }


    public GameObject RemoveBrick()
    {
        if (brickStack.Count == 0) return null;

        GameObject topBrick = brickStack.Pop();
        
        StartCoroutine(UpdateStackSmooth());

        return topBrick;
    }
    
    IEnumerator MoveToStackPosition(GameObject brick, int index)
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
    
    IEnumerator UpdateStackSmooth()
    {
        int i = 0;
        foreach (var brick in brickStack)
        {
            StartCoroutine(MoveToStackPosition(brick, i));
            i++;
        }

        yield return null;
    }
    
    public void ClearStack()
    {
        foreach (var brick in brickStack)
        {
            SimplePool.Despawn(brick);
        }
        brickStack.Clear();
    }
    
    public int GetStackCount()
    {
        return brickStack.Count;
    }
}