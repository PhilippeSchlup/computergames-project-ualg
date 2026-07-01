using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Tracking")]
    public Transform target; 
    
    [Header("Camera Settings")]
    public float distanceBehind = 6f; // How far back the camera sits
    public float heightAbove = 3f;    // How high up the camera sits
    public float smoothSpeed = 5f; 

    [Header("Occlusion Settings")]
    public LayerMask obstructionLayers = ~0; // Layers that block the camera's view (e.g. Default, Obstacles, Environment)
    public float cameraRadius = 0.25f;       // Spherecast radius to prevent near clipping into walls

    void LateUpdate()
    {
        if (target != null)
        {
            // 1. Calculate default desired position behind and above the target based on target rotation
            Vector3 desiredPosition = target.position - (target.forward * distanceBehind) + (Vector3.up * heightAbove);
            
            // 2. Perform a SphereCast from target center to desired camera position to check for obstacles
            Vector3 targetCenter = target.position + Vector3.up * 1f; // Target chest/head level
            Vector3 rayDirection = desiredPosition - targetCenter;
            float rayLength = rayDirection.magnitude;

            // We perform a spherecast to give the camera collision volume (so it doesn't clip through wall edges)
            if (Physics.SphereCast(targetCenter, cameraRadius, rayDirection.normalized, out RaycastHit hit, rayLength, obstructionLayers))
            {
                // Move the desired camera position to the contact point (leaving a small 0.1m gap)
                desiredPosition = targetCenter + rayDirection.normalized * (hit.distance - 0.1f);
            }
            
            // 3. Smoothly move the camera
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // 4. Always look directly at the target's chest level (rather than feet)
            transform.LookAt(target.position + Vector3.up * 1f);
        }
    }
    
    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position - (target.forward * distanceBehind) + (Vector3.up * heightAbove);
            transform.LookAt(target);
        }
    }
}