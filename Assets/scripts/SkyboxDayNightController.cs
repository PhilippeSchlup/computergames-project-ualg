using UnityEngine;

public class SkyboxDayNightController : MonoBehaviour
{
    [Header("Optional: dims/reddens the sun alongside the skybox")]
    public Light sunLight;

    [Header("Evening")]
    public Color eveningSkyTint = new Color(1f, 0.55f, 0.3f);
    public Color eveningGroundColor = new Color(0.5f, 0.3f, 0.2f);
    public float eveningExposure = 1.3f;
    public float eveningAtmosphereThickness = 1f;
    public float eveningSunIntensity = 1f;
    public Color eveningSunColor = new Color(1f, 0.7f, 0.5f);

    [Header("Night")]
    public Color nightSkyTint = new Color(0.05f, 0.05f, 0.15f);
    public Color nightGroundColor = new Color(0.02f, 0.02f, 0.03f);
    public float nightExposure = 0.15f;
    public float nightAtmosphereThickness = 0.4f;
    public float nightSunIntensity = 0.05f;
    public Color nightSunColor = new Color(0.3f, 0.35f, 0.55f);

    private static readonly int SkyTintId = Shader.PropertyToID("_SkyTint");
    private static readonly int GroundColorId = Shader.PropertyToID("_GroundColor");
    private static readonly int ExposureId = Shader.PropertyToID("_Exposure");
    private static readonly int AtmosphereThicknessId = Shader.PropertyToID("_AtmosphereThickness");

    private Material skyboxInstance;
    private Coroutine activeTransition;

    void Awake()
    {
        // Clone the skybox material so we never overwrite the shared asset.
        skyboxInstance = new Material(RenderSettings.skybox);
        RenderSettings.skybox = skyboxInstance;
    }

    public void PlayEveningToNight(float duration)
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(TransitionRoutine(duration));
    }

    private System.Collections.IEnumerator TransitionRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Apply(t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Apply(1f);
    }

    private void Apply(float t)
    {
        skyboxInstance.SetColor(SkyTintId, Color.Lerp(eveningSkyTint, nightSkyTint, t));
        skyboxInstance.SetColor(GroundColorId, Color.Lerp(eveningGroundColor, nightGroundColor, t));
        skyboxInstance.SetFloat(ExposureId, Mathf.Lerp(eveningExposure, nightExposure, t));
        skyboxInstance.SetFloat(AtmosphereThicknessId, Mathf.Lerp(eveningAtmosphereThickness, nightAtmosphereThickness, t));

        if (sunLight != null)
        {
            sunLight.intensity = Mathf.Lerp(eveningSunIntensity, nightSunIntensity, t);
            sunLight.color = Color.Lerp(eveningSunColor, nightSunColor, t);
        }
    }
}
