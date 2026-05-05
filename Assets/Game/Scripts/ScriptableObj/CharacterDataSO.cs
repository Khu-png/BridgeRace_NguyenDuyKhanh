using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "Character Data", order = 2)]
public class CharacterDataSO : ScriptableObject
{
    [SerializeField] private ColorDataSO colorData;

    public ColorType GetRandomColorType()
    {
        return (ColorType)Random.Range((int)ColorType.Red, (int)ColorType.Violet + 1);
    }

    public ColorType GetRandomColorTypeExcept(ICollection<ColorType> excludedTypes)
    {
        List<ColorType> availableTypes = new List<ColorType>();

        for (int i = (int)ColorType.Red; i <= (int)ColorType.Violet; i++)
        {
            ColorType colorType = (ColorType)i;
            if (excludedTypes == null || !excludedTypes.Contains(colorType))
            {
                availableTypes.Add(colorType);
            }
        }

        if (availableTypes.Count == 0)
        {
            return GetRandomColorType();
        }

        return availableTypes[Random.Range(0, availableTypes.Count)];
    }

    public Material GetMaterial(ColorType colorType)
    {
        return colorData.GetMat(colorType);
    }

    public Color GetColor(ColorType colorType)
    {
        Material material = GetMaterial(colorType);
        return material.color;
    }
}
