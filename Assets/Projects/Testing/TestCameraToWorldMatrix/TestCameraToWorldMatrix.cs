using UnityEngine;

public class TestCameraToWorldMatrix : MonoBehaviour
{
    public Camera   mainCamera;
    public Material material;

    private void Update()
    {
        if (mainCamera == null || material == null)
        {
            return;
        }
        
        Vector4 toCenter  = new Vector4(0, 0, -mainCamera.nearClipPlane, 0);
        Vector4 viewDirWS = mainCamera.cameraToWorldMatrix * toCenter;
        viewDirWS = viewDirWS.normalized;

        material.color = new Color(viewDirWS.x, viewDirWS.y, viewDirWS.z);
    }
}
