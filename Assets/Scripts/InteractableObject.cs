using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outline))]
public class InteractableObject : MonoBehaviour
{
    [Header("Події при кліку")]
    public UnityEvent onInteract;
    
    private Outline outline;

    void Start()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    public void Interact()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        onInteract.Invoke();
    }

    public void Highlight()
    {
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public void RemoveHighlight()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}