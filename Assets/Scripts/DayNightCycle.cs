using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayLengthSeconds = 120f;
    public float axisTiltDegrees = 45f;
    public float startAngleDegrees = 0f;
    public Vector3 axisBase = Vector3.up;

    float angle;

    void Start()
    {
        angle = startAngleDegrees;
        Apply();
    }

    void Update()
    {
        float degPerSec = 360f / Mathf.Max(0.01f, dayLengthSeconds);
        angle += degPerSec * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        Apply();
    }

    void Apply()
    {
        Vector3 axis = Quaternion.AngleAxis(axisTiltDegrees, Vector3.right) * axisBase;
        transform.rotation = Quaternion.AngleAxis(angle, axis);
    }
}