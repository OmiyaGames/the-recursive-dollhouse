using UnityEngine;

public class MoodTheme : MonoBehaviour
{
    [SerializeField]
    private Material innerWall = null;

    [Header("Directional Light")]
    [SerializeField]
    private Color lightColor = Color.white;
    [SerializeField]
    private float lightIntensity = 1;
    [SerializeField]
    private float lightBounceIntensity = 1;

    [Header("Skybox")]
    [SerializeField]
    private Color fogColor = Color.white;
    [SerializeField]
    private float skyboxExposure = 1f;

    [Header("Music")]
    [SerializeField]
    private AudioClip backgroundMusic = null;

#if UNITY_EDITOR
    [Header("Editor-Only")]
    [SerializeField]
    private Light directionLight = null;
    [SerializeField]
    private MeshRenderer houseMesh = null;
#endif

    public Material WallMaterial
    {
        get
        {
            return innerWall;
        }
    }

    #region Light Properties
    public Color LightColor
    {
        get
        {
            return lightColor;
        }
    }

    public float LightIntensity
    {
        get
        {
            return lightIntensity;
        }
    }

    public float LightBounceIntensity
    {
        get
        {
            return lightBounceIntensity;
        }
    }
    #endregion

    #region Skybox Properties
    public Color FogColor
    {
        get
        {
            return fogColor;
        }
    }

    public float SkyboxExposure
    {
        get
        {
            return skyboxExposure;
        }
    }
    #endregion

    public AudioClip BackgroundMusic
    {
        get
        {
            return backgroundMusic;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Copy Light Information")]
    private void CopyLight()
    {
        if (directionLight != null)
        {
            lightColor = directionLight.color;
            lightIntensity = directionLight.intensity;
            lightBounceIntensity = directionLight.bounceIntensity;
        }
        fogColor = RenderSettings.fogColor;
        skyboxExposure = RenderSettings.skybox.GetFloat("_Exposure");
    }

    [ContextMenu("Set Light Information")]
    private void SetLight()
    {
        if (houseMesh != null)
        {
            Material[] materials = new Material[houseMesh.sharedMaterials.Length];
            for (int index = 0; index < materials.Length; ++index)
            {
                materials[index] = innerWall;
            }
            houseMesh.sharedMaterials = materials;
        }
        if (directionLight != null)
        {
            directionLight.color = lightColor;
            directionLight.intensity = lightIntensity;
            directionLight.bounceIntensity = lightBounceIntensity;
        }
        RenderSettings.fogColor = fogColor;
        RenderSettings.skybox.SetFloat("_Exposure", skyboxExposure);
        DynamicGI.UpdateEnvironment();
    }
#endif
}
