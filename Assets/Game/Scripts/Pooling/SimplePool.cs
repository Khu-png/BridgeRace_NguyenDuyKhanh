using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    public static GameObject Spawn(string key, Vector3 pos, Quaternion rot)
    {
        return PoolManager.Instance.Get(key, pos, rot);
    }

    public static void Despawn(GameObject obj)
    {
        PoolManager.Instance.Return(obj);
    }

    public static void DespawnAll(string key)
    {
        PoolManager.Instance.ReturnAllActive(key);
    }
}
