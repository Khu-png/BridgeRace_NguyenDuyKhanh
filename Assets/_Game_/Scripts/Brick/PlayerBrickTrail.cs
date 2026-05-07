using UnityEngine;

public class PlayerBrickTrail : MonoBehaviour
{
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float alpha = 0.35f;

    private void Awake()
    {
        ResolveTrailRenderer();
    }

    public void SetColor(Color color)
    {
        TrailRenderer resolvedTrail = ResolveTrailRenderer();
        if (resolvedTrail == null) return;

        color.a = alpha;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(alpha, 0f),
                new GradientAlphaKey(alpha, 1f)
            });

        resolvedTrail.colorGradient = gradient;
    }

    public void SetActive(bool isActive)
    {
        TrailRenderer resolvedTrail = ResolveTrailRenderer();
        if (resolvedTrail == null) return;

        resolvedTrail.emitting = isActive;

        if (!isActive)
        {
            resolvedTrail.Clear();
        }
    }

    private TrailRenderer ResolveTrailRenderer()
    {
        if (trailRenderer != null) return trailRenderer;

        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null) return trailRenderer;

        trailRenderer = GetComponentInChildren<TrailRenderer>(true);
        return trailRenderer;
    }
}
