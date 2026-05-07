using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    [SerializeField] private CharacterDataSO characterData;
    [SerializeField] private Transform brickHolder;
    [SerializeField] private Renderer[] renderers;
    [SerializeField, HideInInspector] private ColorType colorType = ColorType.None;

    public Color CharacterColor { get; private set; } = Color.white;
    public ColorType CharacterColorType => colorType;
    public Material CharacterMaterial { get; private set; }

    private void Awake()
    {
        CacheRenderersIfNeeded();
    }

    private void OnValidate()
    {
        CacheRenderersIfNeeded();
        if (Application.isPlaying || colorType == ColorType.None)
        {
            return;
        }

        ApplyColor();
    }

    public void RandomizeColor()
    {
        colorType = LevelManager.Instance != null
            ? LevelManager.Instance.GetUniqueCharacterColorType(characterData)
            : characterData.GetRandomColorType();

        ApplyColor();
    }

    private void ApplyColor()
    {
        Material selectedMaterial = characterData.GetMaterial(colorType);
        CharacterMaterial = selectedMaterial;
        CharacterColor = characterData.GetColor(colorType);

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer.transform.IsChildOf(brickHolder)) continue;

            if (Application.isPlaying)
            {
                RendererColorUtility.ApplyMaterial(targetRenderer, selectedMaterial);
            }
            else
            {
                targetRenderer.sharedMaterial = selectedMaterial;
            }
        }
    }

    private void CacheRenderersIfNeeded()
    {
        if (renderers != null && renderers.Length > 0)
        {
            return;
        }

        renderers = GetComponentsInChildren<Renderer>(true);
    }
}

public static class RendererColorUtility
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static MaterialPropertyBlock propertyBlock;

    public static void ApplyColor(Renderer renderer, Color color)
    {
        if (renderer == null) return;

        MaterialPropertyBlock block = GetPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetColor(BaseColorId, color);
        block.SetColor(ColorId, color);
        renderer.SetPropertyBlock(block);
    }

    public static void ApplyMaterial(Renderer renderer, Material material)
    {
        if (renderer == null || material == null) return;

        renderer.sharedMaterial = material;
        renderer.SetPropertyBlock(null);
    }

    private static MaterialPropertyBlock GetPropertyBlock()
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        return propertyBlock;
    }
}
