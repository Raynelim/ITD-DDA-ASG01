// ============================================================================
// File: Assets/Scripts/PetInputController.cs
// Handles screen touch input: tap to move, grab and drag the pet
// ============================================================================

using UnityEngine;

public class PetInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera arCamera;

    [Header("Layers")]
    [SerializeField] private LayerMask petLayerMask;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Tap Settings")]
    [Tooltip("Max drag distance (in screen pixels) to still count as a tap.")]
    [SerializeField] private float tapMaxMovement = 20f;

    private PetController grabbedPet;
    private float grabDistance;
    private bool isTap;
    private Vector2 tapStartPos;

    private void Reset()
    {
        // Try to auto-assign AR camera if not set
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.touchCount == 0)
        {
            return;
        }

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                HandleTouchBegan(touch);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                HandleTouchMovedOrStationary(touch);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                HandleTouchEnded(touch);
                break;
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        if (arCamera == null)
        {
            return;
        }

        Ray ray = arCamera.ScreenPointToRay(touch.position);

        // First: try to grab the pet if we touch it
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, petLayerMask))
        {
            grabbedPet = hit.collider.GetComponentInParent<PetController>();
            if (grabbedPet != null)
            {
                grabDistance = Vector3.Distance(arCamera.transform.position, hit.point);
                grabbedPet.BeginGrab();
                isTap = false;
                return;
            }
        }

        // Else, consider it a potential tap for movement
        isTap = true;
        tapStartPos = touch.position;
    }

    private void HandleTouchMovedOrStationary(Touch touch)
    {
        if (arCamera == null)
        {
            return;
        }

        if (grabbedPet != null)
        {
            // Dragging the pet in 3D
            Ray ray = arCamera.ScreenPointToRay(touch.position);
            Vector3 newPos = ray.origin + ray.direction.normalized * grabDistance;
            grabbedPet.UpdateGrab(newPos);
            return;
        }

        // If hand moved too much, it's no longer a tap
        if (isTap)
        {
            float distance = Vector2.Distance(tapStartPos, touch.position);
            if (distance > tapMaxMovement)
            {
                isTap = false;
            }
        }
    }

    private void HandleTouchEnded(Touch touch)
    {
        if (arCamera == null)
        {
            return;
        }

        if (grabbedPet != null)
        {
            grabbedPet.EndGrab();
            grabbedPet = null;
            return;
        }

        if (!isTap)
        {
            return;
        }

        // This was a tap -> move the active pet to tap position
        Ray ray = arCamera.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            PetController activePet = PetController.ActivePet;
            if (activePet != null)
            {
                activePet.MoveTo(hit.point);
            }
        }

        isTap = false;
    }
}