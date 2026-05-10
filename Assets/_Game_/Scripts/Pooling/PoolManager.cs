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


    public List<PoolElement> elements;

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Transform> parentDictionary;
    private Dictionary<string, HashSet<GameObject>> activeDictionary;
    private readonly List<GameObject> returnAllBuffer = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        parentDictionary = new Dictionary<string, Transform>();
        activeDictionary = new Dictionary<string, HashSet<GameObject>>();

        InitPool();
    }

    void InitPool()
    {
        foreach (var element in elements)
        {
            GameObject parentObj = new GameObject(element.key + "_Pool");
            parentObj.transform.SetParent(transform);

            parentDictionary.Add(element.key, parentObj.transform);
            activeDictionary.Add(element.key, new HashSet<GameObject>());

            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < element.amount; i++)
            {
                GameObject obj = Instantiate(element.prefab, parentObj.transform);
                
                PoolObject poolObj = obj.GetComponent<PoolObject>();
                if (poolObj == null)
                    poolObj = obj.AddComponent<PoolObject>();

                poolObj.Key = element.key;

                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            poolDictionary.Add(element.key, queue);
        }
    }

    public GameObject Get(string key, Vector3 pos, Quaternion rot)
    {
        if (string.IsNullOrEmpty(key) || poolDictionary == null || !poolDictionary.ContainsKey(key))
        {
            return null;
        }

        Queue<GameObject> pool = poolDictionary[key];
        GameObject obj;

        if (pool.Count == 0)
        {
            PoolElement element = elements.Find(e => e.key == key);
            if (element == null || element.prefab == null)
            {
                return null;
            }

            obj = Instantiate(element.prefab);

            PoolObject poolObj = obj.GetComponent<PoolObject>();
            if (poolObj == null)
                poolObj = obj.AddComponent<PoolObject>();

            poolObj.Key = key;
        }
        else
        {
            obj = pool.Dequeue();
        }

        if (key != "FallBrick")
        {
            obj.transform.SetParent(null);
        }
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.SetActive(true);
        activeDictionary[key].Add(obj);

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null || poolDictionary == null || parentDictionary == null)
        {
            return;
        }

        PoolObject poolObj = obj.GetComponent<PoolObject>();

        if (poolObj == null)
        {
            return;
        }

        string key = poolObj.Key;
        if (string.IsNullOrEmpty(key) || !poolDictionary.ContainsKey(key) || !parentDictionary.ContainsKey(key))
        {
            return;
        }

        if (!activeDictionary[key].Remove(obj))
        {
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(parentDictionary[key]);

        poolDictionary[key].Enqueue(obj);
    }

    public void ReturnAllActive(string key)
    {
        if (string.IsNullOrEmpty(key)
            || !poolDictionary.ContainsKey(key)
            || !parentDictionary.ContainsKey(key)
            || !activeDictionary.ContainsKey(key))
        {
            return;
        }

        returnAllBuffer.Clear();
        foreach (GameObject obj in activeDictionary[key])
        {
            if (obj != null)
            {
                returnAllBuffer.Add(obj);
            }
        }

        for (int i = 0; i < returnAllBuffer.Count; i++)
        {
            Return(returnAllBuffer[i]);
        }

        returnAllBuffer.Clear();
    }
}
