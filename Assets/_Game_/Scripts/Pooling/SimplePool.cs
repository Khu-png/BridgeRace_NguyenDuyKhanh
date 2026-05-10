using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    public static GameObject Get(string key, Vector3 pos, Quaternion rot)
    {
        PoolManager poolManager = PoolManager.Instance;
        return poolManager != null ? poolManager.Get(key, pos, rot) : null;
    }

    public static T Get<T>(string key, Vector3 pos, Quaternion rot) where T : PoolObject
    {
        GameObject obj = Get(key, pos, rot);
        return obj != null ? obj.GetComponent<T>() : null;
    }

    public static void Return(GameObject obj)
    {
        if (obj == null) return;

        PoolObject poolObject = obj.GetComponent<PoolObject>();
        if (poolObject == null) return;

        Return(poolObject);
    }

    public static void Return(PoolObject obj)
    {
        if (obj == null) return;

        PoolManager poolManager = PoolManager.Instance;
        if (poolManager == null) return;

        poolManager.Return(obj.gameObject);
    }

    public static void ReturnAll(string key)
    {
        PoolManager poolManager = PoolManager.Instance;
        if (poolManager == null) return;

        poolManager.ReturnAllActive(key);
    }

    public static GameObject Spawn(string key, Vector3 pos, Quaternion rot) => Get(key, pos, rot);
    public static T Spawn<T>(string key, Vector3 pos, Quaternion rot) where T : PoolObject => Get<T>(key, pos, rot);
    public static void Despawn(GameObject obj) => Return(obj);
    public static void Despawn(PoolObject obj) => Return(obj);
    public static void DespawnAll(string key) => ReturnAll(key);
}
