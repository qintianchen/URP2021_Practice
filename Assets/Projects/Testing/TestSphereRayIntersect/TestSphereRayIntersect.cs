using UnityEngine;

public class TestSphereRayIntersect : MonoBehaviour
{
    public Vector3 position;
    public Vector3 ray;
    
    private bool Run(Vector3 spherePosition, float radius, Vector3 rayPosition, Vector3 rayDir)
    {
        rayDir = rayDir.normalized;
        
        Vector3 rayToSphere = spherePosition - rayPosition;
        float   a2          = Vector3.Dot(rayToSphere, rayToSphere);

        float r2 = radius * radius;
        if (a2 <= r2)
        {
            return true;
        }

        float l = Vector3.Dot(rayToSphere, rayDir);
        if (l < 0)
        {
            return false;
        }

        if (a2 - l * l <= r2)
        {
            return true;
        }
        
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(0, 0, 0), 10f);
        Gizmos.DrawLine(position, position + ray);
    }

    private void OnGUI()
    {
        var isIntersect = Run(new Vector3(0, 0, 0), 10f, position, ray);
        GUI.Label(new Rect(100, 100, 300, 100), $"是否相交: {isIntersect}");
    }
}
