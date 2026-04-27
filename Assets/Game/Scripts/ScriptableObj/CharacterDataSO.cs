using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "Character Data", order = 2)]
public class CharacterDataSO : ScriptableObject
{
    [SerializeField] private ColorDataSO colorData;

    public ColorType GetRandomColorType()
    {
        return (ColorType)Random.Range((int)ColorType.Red, (int)ColorType.Violet + 1);
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
