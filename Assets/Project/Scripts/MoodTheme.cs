using UnityEngine;
using System.Collections;

public class MoodTheme : MonoBehaviour
{
    [SerializeField]
    private Color lightColor = Color.white;
    [SerializeField]
    private float lightIntensity = 1;
    //[SerializeField]
    //private Color houseColor = Color.white;
    //[SerializeField]
    //private Texture houseTexture;

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

    //public Color HouseColor
    //{
    //    get
    //    {
    //        return houseColor;
    //    }
    //}

    //public Texture HouseTexture
    //{
    //    get
    //    {
    //        return houseTexture;
    //    }
    //}
}
