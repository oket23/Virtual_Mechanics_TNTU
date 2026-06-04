using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outline))]
public class InteractableObject : MonoBehaviour
{
    [Header("Підказка при наведенні")]
    public string tooltipText = "Натисніть E для взаємодії";

    [Header("Лабораторна робота")]
    public LabData labData;
    public AtwoodLabData atwoodLabData;

    [Header("Події при кліку (якщо немає labData)")]
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

        UIManager uim = FindFirstObjectByType<UIManager>();
        if (uim == null) { Debug.LogError("[Interact] UIManager не знайдено!"); return; }

        if (atwoodLabData != null)
            uim.OpenAtwoodLab(atwoodLabData);
        else if (labData != null)
            uim.OpenLabMenu(labData);
        else
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