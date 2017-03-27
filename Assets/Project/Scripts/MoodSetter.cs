using UnityEngine;
using OmiyaGames;

public class MoodSetter : MonoBehaviour
{
    public static MoodSetter Instance
    {
        get;
        private set;
    }

    [SerializeField]
    Light sunlight;
    [SerializeField]
    Material[] floorMaterials;
    [SerializeField]
    AudioClip[] allMusics;
    [SerializeField]
    float changeSpeed = 10f;

    MoodTheme[] allThemes = null;
    RandomList<MoodTheme> randomTheme = null;
    RandomList<Material> randomFloorMaterial = null;
    RandomList<AudioClip> randomMusic = null;
    MoodTheme currentTheme = null;
    Color newColor = Color.white;
    float newFloat = 0;
    bool animateTheme = false;

    public MoodTheme[] AllThemes
    {
        get
        {
            if(allThemes == null)
            {
                allThemes = GetComponentsInChildren<MoodTheme>();
            }
            return allThemes;
        }
    }

    public MoodTheme RandomTheme
    {
        get
        {
            if(randomTheme == null)
            {
                randomTheme = new RandomList<MoodTheme>(AllThemes);
            }
            return randomTheme.RandomElement;
        }
    }

    public AudioClip RandomMusic
    {
        get
        {
            if(randomMusic == null)
            {
                randomMusic = new RandomList<AudioClip>(allMusics);
            }
            return randomMusic.RandomElement;
        }
    }

    public Material RandomFloorMaterial
    {
        get
        {
            if (randomFloorMaterial == null)
            {
                randomFloorMaterial = new RandomList<Material>(floorMaterials);
            }
            return randomFloorMaterial.RandomElement;
        }
    }

    public MoodTheme CurrentTheme
    {
        get
        {
            return currentTheme;
        }
        set
        {
            if (currentTheme != value)
            {
                // Figure out how to animation the theme
                if (currentTheme == null)
                {
                    SnapToTheme(value);
                }
                else if (value != null)
                {
                    animateTheme = true;
                }

                // Setup variables
                currentTheme = value;

                // Change Music
                //Singleton.Get<BackgroundMusic>().CurrentMusic = currentTheme.BackgroundMusic;
            }
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if(animateTheme == true)
        {
            // Smooth damp sunlight color
            newColor.r = Mathf.SmoothStep(sunlight.color.r, CurrentTheme.LightColor.r, (changeSpeed * Time.deltaTime));
            newColor.g = Mathf.SmoothStep(sunlight.color.g, CurrentTheme.LightColor.g, (changeSpeed * Time.deltaTime));
            newColor.b = Mathf.SmoothStep(sunlight.color.b, CurrentTheme.LightColor.b, (changeSpeed * Time.deltaTime));
            sunlight.color = newColor;

            // Smooth damp light intensity
            sunlight.intensity = Mathf.SmoothStep(sunlight.intensity, CurrentTheme.LightIntensity, (changeSpeed * Time.deltaTime));

            // Smooth damp fog color
            newColor.r = Mathf.SmoothStep(RenderSettings.fogColor.r, CurrentTheme.FogColor.r, (changeSpeed * Time.deltaTime));
            newColor.g = Mathf.SmoothStep(RenderSettings.fogColor.g, CurrentTheme.FogColor.g, (changeSpeed * Time.deltaTime));
            newColor.b = Mathf.SmoothStep(RenderSettings.fogColor.b, CurrentTheme.FogColor.b, (changeSpeed * Time.deltaTime));
            RenderSettings.fogColor = newColor;

            // Smooth damp exposure
            newFloat = Mathf.SmoothStep(RenderSettings.skybox.GetFloat("_Exposure"), CurrentTheme.SkyboxExposure, (changeSpeed * Time.deltaTime));
            RenderSettings.skybox.SetFloat("_Exposure", newFloat);

            // Check if we met the target scale yet
            if (Mathf.Approximately(sunlight.color.r, CurrentTheme.LightColor.r) &&
                Mathf.Approximately(sunlight.color.g, CurrentTheme.LightColor.g) &&
                Mathf.Approximately(sunlight.color.b, CurrentTheme.LightColor.b) &&

                Mathf.Approximately(sunlight.intensity, CurrentTheme.LightIntensity) &&

                Mathf.Approximately(RenderSettings.fogColor.r, CurrentTheme.FogColor.r) &&
                Mathf.Approximately(RenderSettings.fogColor.g, CurrentTheme.FogColor.g) &&
                Mathf.Approximately(RenderSettings.fogColor.b, CurrentTheme.FogColor.b) &&

                Mathf.Approximately(RenderSettings.skybox.GetFloat("_Exposure"), CurrentTheme.SkyboxExposure))
            {
                SnapToTheme(CurrentTheme);
            }
        }
    }

    void SnapToTheme(MoodTheme theme)
    {
        sunlight.color = theme.LightColor;
        sunlight.intensity = theme.LightIntensity;
        RenderSettings.fogColor = theme.FogColor;
        RenderSettings.skybox.SetFloat("_Exposure", theme.SkyboxExposure);

        DynamicGI.UpdateEnvironment();
        animateTheme = false;
    }
}
