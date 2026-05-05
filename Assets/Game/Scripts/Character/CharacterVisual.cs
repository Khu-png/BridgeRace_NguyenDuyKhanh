using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    [SerializeField] private CharacterDataSO characterData;
    [SerializeField] private Transform brickHolder;
    [SerializeField] private Renderer[] renderers;
    [SerializeField, HideInInspector] private ColorType colorType = ColorType.None;

    public Color CharacterColor { get; private set; } = Color.white;

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
        CharacterColor = characterData.GetColor(colorType);

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer.transform.IsChildOf(brickHolder)) continue;

            if (Application.isPlaying)
            {
                targetRenderer.material = selectedMaterial;
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
