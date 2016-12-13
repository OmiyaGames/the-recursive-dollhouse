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
    Vector3 rotateSpeed;
    [SerializeField]
    Light sunlight;
    [SerializeField]
    float changeSpeed = 10f;
    [SerializeField]
    float snapDistance = 0.01f;

    MoodTheme[] allThemes;
    RandomList<MoodTheme> randomTheme;
    MoodTheme currentTheme = null;
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
        transform.Rotate(rotateSpeed * Time.deltaTime);
        if(animateTheme == true)
        {
            // If not, smooth damp
            sunlight.color = Color.Lerp(sunlight.color, CurrentTheme.lightColor, (changeSpeed * Time.deltaTime));
            testColorDiff.x = sunlight.color.r - CurrentTheme.lightColor.r;
            testColorDiff.y = sunlight.color.g - CurrentTheme.lightColor.g;
            testColorDiff.z = sunlight.color.b - CurrentTheme.lightColor.b;

            // Check if we met the target scale yet
            if (testColorDiff.sqrMagnitude < (snapDistance* snapDistance))
            {
                SnapToTheme(CurrentTheme);
            }
        }
    }

    void SnapToTheme(MoodTheme theme)
    {
        sunlight.color = theme.lightColor;
        sunlight.intensity = theme.lightIntensity;
        animateTheme = true;
    }
}
