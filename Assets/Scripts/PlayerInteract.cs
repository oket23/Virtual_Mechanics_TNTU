using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public Transform playerCamera;
    public UIManager uiManager;

    private InteractableObject currentTarget;

    void Update()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            
            if (interactable != null)
            {
                if (interactable != currentTarget)
                {
                    ClearTarget();
                    currentTarget = interactable;
                    currentTarget.Highlight();
                    if (uiManager != null)
                        uiManager.ShowTooltip(currentTarget.tooltipText);
                }

                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                {
                    interactable.Interact();
                }
            }
            else
            {
                ClearTarget();
            }
        }
        else
        {
            ClearTarget();
        }
    }

    private void ClearTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.RemoveHighlight();
            currentTarget = null;
            if (uiManager != null)
                uiManager.HideTooltip();
        }
    }
}