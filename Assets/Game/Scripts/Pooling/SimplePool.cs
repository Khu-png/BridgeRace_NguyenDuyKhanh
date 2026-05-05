using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    public static GameObject Spawn(string key, Vector3 pos, Quaternion rot)
    {
        PoolManager poolManager = PoolManager.Instance;
        return poolManager != null ? poolManager.Get(key, pos, rot) : null;
    }

    public static void Despawn(GameObject obj)
    {
        if (obj == null) return;

        PoolManager poolManager = PoolManager.Instance;
        if (poolManager == null) return;

        poolManager.Return(obj);
    }

    public static void DespawnAll(string key)
    {
        PoolManager poolManager = PoolManager.Instance;
        if (poolManager == null) return;

        poolManager.ReturnAllActive(key);
    }
}
