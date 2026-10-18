using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.2f, -10f);
    public float smooth = 8f;
    public float minX = -8f;
    public float maxX = 34f;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offset;
        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.z = offset.z;
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smooth);
    }
}
