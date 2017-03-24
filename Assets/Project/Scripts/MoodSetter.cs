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
    float changeSpeed = 10f;

    MoodTheme[] allThemes;
    RandomList<MoodTheme> randomTheme;
    MoodTheme currentTheme = null;
    Color newColor = Color.white;
    bool animateTheme = false;

    public MoodTheme RandomTheme
    {
        get
        {
            return randomTheme.RandomElement;
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
                if (currentTheme == null)
                {
                    SnapToTheme(value);
                }
                else if (value != null)
                {
                    animateTheme = true;
                }
                currentTheme = value;
            }
        }
    }

    void Awake()
    {
        Instance = this;
        allThemes = GetComponentsInChildren<MoodTheme>();
        randomTheme = new RandomList<MoodTheme>(allThemes);
    }

    // Update is called once per frame
    Vector3 testColorDiff = Vector3.zero;
    void Update()
    {
        if(animateTheme == true)
        {
            // Smooth damp Color
            newColor.r = Mathf.SmoothStep(sunlight.color.r, CurrentTheme.LightColor.r, (changeSpeed * Time.deltaTime));
            newColor.g = Mathf.SmoothStep(sunlight.color.g, CurrentTheme.LightColor.g, (changeSpeed * Time.deltaTime));
            newColor.b = Mathf.SmoothStep(sunlight.color.b, CurrentTheme.LightColor.b, (changeSpeed * Time.deltaTime));
            sunlight.color = newColor;

            // Smooth damp Color
            sunlight.intensity = Mathf.SmoothStep(sunlight.intensity, CurrentTheme.LightIntensity, (changeSpeed * Time.deltaTime));

            // Check if we met the target scale yet
            if (Mathf.Approximately(newColor.r, CurrentTheme.LightColor.r) &&
                Mathf.Approximately(newColor.g, CurrentTheme.LightColor.g) &&
                Mathf.Approximately(newColor.b, CurrentTheme.LightColor.b) &&
                Mathf.Approximately(sunlight.intensity, CurrentTheme.LightIntensity))
            {
                SnapToTheme(CurrentTheme);
            }
        }
    }

    void SnapToTheme(MoodTheme theme)
    {
        sunlight.color = theme.LightColor;
        sunlight.intensity = theme.LightIntensity;
        animateTheme = true;
    }
}
