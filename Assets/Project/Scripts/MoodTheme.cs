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

    [Header("Music")]
    [SerializeField]
    private AudioClip backgroundMusic = null;

#if UNITY_EDITOR
    [Header("Editor-Only")]
    [SerializeField]
    private Light directionLight = null;
#endif

    public Material InnerWall
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
        lightColor = directionLight.color;
        lightIntensity = directionLight.intensity;
        lightBounceIntensity = directionLight.bounceIntensity;
        fogColor = RenderSettings.fogColor;
    }
#endif
}
