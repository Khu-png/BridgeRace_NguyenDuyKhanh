using UnityEngine;

public enum  ColorType
{
    None = 0,
    Red = 1,
    Blue = 2,
    Green = 3,
    Orange = 4,
    Black = 5,
    Violet = 6,
}

[CreateAssetMenu(fileName = "ColorDataSO", menuName = "Color Data", order = 1)]
public class ColorDataSO : ScriptableObject
{
    [SerializeField] Material[] materials;

    public Material GetMat(ColorType type)
    {
        return materials[(int)type];   
    }
}
