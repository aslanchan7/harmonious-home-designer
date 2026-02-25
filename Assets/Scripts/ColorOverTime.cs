using UnityEngine;
using UnityEngine.UI;

public class ColorOverTime : MonoBehaviour
{
    public Image image;
    public float duration = 60f;
    public bool loop = true;

    [Range(0f,1f)] public float opacity0 = 0f;
    [Range(0f,1f)] public float opacity1 = 0.25f;
    [Range(0f,1f)] public float opacity2 = 0.5f;
    [Range(0f,1f)] public float opacity3 = 0.25f;
    [Range(0f,1f)] public float opacity4 = 0f;

    float t;

    void Update()
    {
        if (image == null || duration <= 0f) return;

        t += Time.deltaTime / duration;

        if (t >= 1f)
        {
            if (loop) t -= 1f;
            else t = 1f;
        }

        float alpha;

        if (t < 0.25f)
            alpha = Mathf.Lerp(opacity0, opacity1, t / 0.25f);
        else if (t < 0.5f)
            alpha = Mathf.Lerp(opacity1, opacity2, (t - 0.25f) / 0.25f);
        else if (t < 0.75f)
            alpha = Mathf.Lerp(opacity2, opacity3, (t - 0.5f) / 0.25f);
        else
            alpha = Mathf.Lerp(opacity3, opacity4, (t - 0.75f) / 0.25f);

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}