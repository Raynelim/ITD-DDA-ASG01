#if UNITY_EDITOR
using UnityEngine;

public class EditorGrabSimulator : MonoBehaviour
{
    [SerializeField] private Camera arCamera;

    private PetController grabbedPet;
    private float grabHeightOffset = 0.05f;

    void Update()
    {
        if (arCamera == null)
            return;

        // --- Begin Grab ---
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PetController pet = hit.collider.GetComponentInParent<PetController>();
                if (pet != null)
                {
                    grabbedPet = pet;
                    grabbedPet.BeginGrab();
                }
            }
        }

        // --- Drag / Hold ---
        if (grabbedPet != null && Input.GetMouseButton(0))
        {
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Keep pet slightly above ground
                Vector3 newPos = hit.point;
                newPos.y += grabHeightOffset;

                grabbedPet.UpdateGrab(newPos);
            }
        }

        // --- Release Grab ---
        if (grabbedPet != null && Input.GetMouseButtonUp(0))
        {
            grabbedPet.EndGrab();
            grabbedPet = null;
        }
    }
}
#endif
