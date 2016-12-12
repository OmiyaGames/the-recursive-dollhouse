using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorRandomizer : MonoBehaviour
{
    [SerializeField]
    Vector2 saturationRange = new Vector2(0.5f, 1f);
    [SerializeField]
    Vector2 valueRange = new Vector2(0.5f, 1f);

    // Use this for initialization
    void Start()
    {
        Renderer changeColor = GetComponent<Renderer>();
        HSBColor randomColor = new HSBColor(Random.value, Random.Range(saturationRange.x, saturationRange.y), Random.Range(valueRange.x, valueRange.y));
        changeColor.material.color = randomColor.ToColor();
    }
}
