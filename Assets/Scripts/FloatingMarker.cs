using UnityEngine;

public class FloatingMarker : MonoBehaviour
{
    [Header("Bobbing")]
    [SerializeField] private float amplitude = 0.08f;
    [SerializeField] private float frequency = 1.2f;

    [Header("Rotation")]
    [SerializeField] private float yawDegreesPerSecond = 35f;

    private Vector3 startLocalPos;

    void Awake()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * Mathf.PI * 2f * frequency) * amplitude;
        transform.localPosition = startLocalPos + new Vector3(0f, yOffset, 0f);
        transform.Rotate(0f, yawDegreesPerSecond * Time.deltaTime, 0f, Space.World);
    }
}
