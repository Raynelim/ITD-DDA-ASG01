using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class EditorTapSimulator : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public Camera arCamera;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public delegate void TapEvent(Vector3 position);
    public static event TapEvent OnTap;   // Subscribe from your pet handler

    void Update()
    {
        #if UNITY_EDITOR
        HandleEditorClick();
        #else
        HandleDeviceTouch();
        #endif
    }

    // -------------------------
    // Simulated Mouse Click Tap
    // -------------------------
    void HandleEditorClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 tapPos = Input.mousePosition;

            // Try AR Raycast
            if (raycastManager != null && 
                raycastManager.Raycast(tapPos, hits, TrackableType.Planes))
            {
                Vector3 worldPos = hits[0].pose.position;
                OnTap?.Invoke(worldPos);
                return;
            }

            // Fallback to a physics raycast for testing
            Ray ray = arCamera.ScreenPointToRay(tapPos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                OnTap?.Invoke(hitInfo.point);
            }
        }
    }

    // -------------------------
    // Real Device Touch
    // -------------------------
    void HandleDeviceTouch()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        if (raycastManager.Raycast(touch.position, hits, TrackableType.Planes))
        {
            Vector3 worldPos = hits[0].pose.position;
            OnTap?.Invoke(worldPos);
        }
    }
}
