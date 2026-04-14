using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    [Header("Ramp Settings")]
    [SerializeField] private GameObject rampPrefab;
    [SerializeField] private float rampWidth = 0.4f;
    [SerializeField] private float rampThickness = 0.2f;
    [SerializeField] private float offsetUp;
    [SerializeField] private float offsetBack;

    [Header("Brick Settings")]
    public GameObject brickPrefab;
    public int brickCount = 20;
    public float stepLength = 1f;
    public float stepHeight = 0.2f;

    [Header("Start Point")]
    public Transform startPoint;

    [Header("Build brick")]
    public List<GameObject> bricks = new List<GameObject>();
    public int currentIndex = 0;
    
    
    void Awake()
    {
        GenerateBridge();
        GenerateRamp();
    }
    
    void GenerateBridge()
    {
        Vector3 localPos = Vector3.zero;
        
        Vector3 step = startPoint.up * stepHeight + startPoint.forward * stepLength;

        for (int i = 0; i < brickCount; i++)
        {
            GameObject brick = Instantiate(brickPrefab, startPoint);

            brick.transform.localPosition = localPos;
            brick.transform.localRotation = Quaternion.identity;

            brick.SetActive(false); 

            bricks.Add(brick);

            localPos += step;
        }
    }
    
    void GenerateRamp()
    {
        Vector3 step = startPoint.up * stepHeight + startPoint.forward * stepLength;

        Vector3 start = startPoint.position;
        Vector3 end = start + step * brickCount;

        Vector3 direction = end - start;
        float length = direction.magnitude;

        GameObject ramp = Instantiate(rampPrefab, transform);
        
        ramp.transform.rotation = Quaternion.LookRotation(direction, startPoint.up);
        
        ramp.transform.localScale = new Vector3(rampWidth, rampThickness, length);
        
        Vector3 center = start + direction / 2f;
        
        Vector3 offset =
            (-startPoint.up * (rampThickness / 2f)) + 
            (startPoint.up * offsetUp) +              
            (-startPoint.forward * offsetBack);       

        ramp.transform.position = center + offset;
        
        MeshRenderer mr = ramp.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
    }
    
    public bool CanBuild() => currentIndex < bricks.Count;
    
    public bool BuildStep()
    {
        if (currentIndex >= bricks.Count) return false;

        bricks[currentIndex].SetActive(true);
        currentIndex++;
        return true;
    }

    public void NextStep()
    {
        currentIndex++;
    }
}
