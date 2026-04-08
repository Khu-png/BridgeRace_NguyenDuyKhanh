using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [System.Serializable]
    public class PoolElement
    {
        public string key;
        public GameObject prefab;
        public int amount;
    }
    
    public class PoolObject : MonoBehaviour
    {
        public string key;
    }

    public List<PoolElement> elements;

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Transform> parentDictionary;

    protected override void Awake()
    {
        base.Awake();

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        parentDictionary = new Dictionary<string, Transform>();

        InitPool();
    }

    void InitPool()
    {
        foreach (var element in elements)
        {
            GameObject parentObj = new GameObject(element.key + "_Pool");
            parentObj.transform.SetParent(transform);

            parentDictionary.Add(element.key, parentObj.transform);

            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < element.amount; i++)
            {
                GameObject obj = Instantiate(element.prefab, parentObj.transform);
                
                PoolObject poolObj = obj.GetComponent<PoolObject>();
                if (poolObj == null)
                    poolObj = obj.AddComponent<PoolObject>();

                poolObj.key = element.key;

                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            poolDictionary.Add(element.key, queue);
        }
    }

    public GameObject Get(string key, Vector3 pos, Quaternion rot)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogError("Không có pool: " + key);
            return null;
        }

        Queue<GameObject> pool = poolDictionary[key];
        GameObject obj;

        if (pool.Count == 0)
        {
            PoolElement element = elements.Find(e => e.key == key);

            obj = Instantiate(element.prefab);

            PoolObject poolObj = obj.GetComponent<PoolObject>();
            if (poolObj == null)
                poolObj = obj.AddComponent<PoolObject>();

            poolObj.key = key;
        }
        else
        {
            obj = pool.Dequeue();
        }

        obj.transform.SetParent(null);
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.SetActive(true);

        return obj;
    }

    public void Return(GameObject obj)
    {
        PoolObject poolObj = obj.GetComponent<PoolObject>();

        if (poolObj == null)
        {
            Debug.LogError("Object không thuộc pool");
            return;
        }

        string key = poolObj.key;

        obj.SetActive(false);
        obj.transform.SetParent(parentDictionary[key]);

        poolDictionary[key].Enqueue(obj);
    }
}