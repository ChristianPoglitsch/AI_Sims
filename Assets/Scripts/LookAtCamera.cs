using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField]
    private Transform targetCamera;

    void LateUpdate()
    {
        var camera = targetCamera 
            ? targetCamera 
            : Camera.main?.transform;

        if (camera == null)
        {
            return;
        }

        Vector3 dir = camera.position - transform.position;

        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(-dir.normalized, Vector3.up);
    }
}
