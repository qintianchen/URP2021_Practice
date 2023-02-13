using UnityEngine;

public class TestIntersectWithSphere : MonoBehaviour
{
    public float radius;

    public Vector3 position;
    public Vector3 ray;

    private void OnDrawGizmos()
    {
        var ray = this.ray.normalized;

        Gizmos.DrawSphere(Vector3.zero, radius);
        
        Gizmos.DrawLine(position, position + 100 * ray);

        var count = GetIntersectPointsWithSphere(position, ray, Vector3.zero, radius, out float distance1, out float distance2);
        if (count >= 1)
        {
            Gizmos.DrawSphere(position + distance1 * ray, 0.1f);
        }
        
        if (count >= 2)
        {
            Gizmos.DrawSphere(position + distance2 * ray, 0.1f);
        }
    }

    private int GetIntersectPointsWithSphere(Vector3 fromPosition, Vector3 ray, Vector3 spherePosition, float sphereRadius, out float distance1, out float distance2)
    {
        float i = ray.x;
        float j = ray.y;
        float k = ray.z;

        float a = spherePosition.x;
        float b = spherePosition.y;
        float c = spherePosition.z;

        float m = fromPosition.x;
        float n = fromPosition.y;
        float l = fromPosition.z;

        float r = sphereRadius;

        float ta = i * i + j * j + k * k;
        float tb = 2 * (i * (m - a) + j * (n - b) + k * (l - c));
        float tc = (m - a) * (m - a) + (n - b) * (n - b) + (l - c) * (l - c) - r * r;

        float rp = tb * tb - 4 * ta * tc;

        distance1 = 0;
        distance2 = 0;
        
        if (rp < 0)
        {
            return 0;
        }

        if (rp == 0)
        {
            distance1 = -tb / (2 * ta);
            distance2 = -tb / (2 * ta);
            return 1;
        }

        float t1 = (-tb + Mathf.Sqrt(rp)) / (2 * ta);
        float t2 = (-tb - Mathf.Sqrt(rp)) / (2 * ta);

        if (t1 < 0 && t2 < 0)
        {
            return 0;
        }

        if (t1 > 0 && t2 > 0)
        {
            distance1 = Mathf.Min(t1, t2);
            distance2 = Mathf.Max(t1, t2);
            return 2;
        }

        distance1 = Mathf.Max(t1, t2);
        return 1;
    }
}
