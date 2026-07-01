using UnityEngine;

public class TitleScreenCameraPan : MonoBehaviour
{
    [Header("Orbit around this point (place it at the center of the scenery you want to show)")]
    public Transform lookTarget;

    [Header("Orbit Settings")]
    public float orbitRadius = 15f;
    public float orbitHeight = 6f;
    public float orbitSpeedDegreesPerSecond = 2f;

    private float angle;

    void OnEnable()
    {
        angle = 0f;
    }

    void Update()
    {
        if (lookTarget == null) return;

        angle += orbitSpeedDegreesPerSecond * Time.deltaTime;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * orbitRadius;

        transform.position = lookTarget.position + offset + Vector3.up * orbitHeight;
        transform.LookAt(lookTarget.position + Vector3.up * 1.5f);
    }
}
