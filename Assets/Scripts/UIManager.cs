using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Елементи")]
    public GameObject menuPanel;
    public GameObject crosshair;

    [Header("Підказка")]
    public GameObject tooltipPanel;
    public TMP_Text tooltipLabel;

    [Header("Лабораторний попап")]
    public LabPopupController labPopupController;

    [Header("Лаб. Машина Атвуда")]
    public AtwoodLabController atwoodLabController;

    [Header("Скрипти гравця")]
    public MonoBehaviour playerMovement;
    public MonoBehaviour playerInteract;

    public void OpenMenu()
    {
        menuPanel.SetActive(true);
        crosshair.SetActive(false);
        
        playerMovement.enabled = false;
        playerInteract.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenLabMenu(LabData data)
    {
        if (crosshair != null) crosshair.SetActive(false);
        playerMovement.enabled = false;
        playerInteract.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        labPopupController.gameObject.SetActive(true);
        labPopupController.Open(data);
    }

    public void OpenAtwoodLab(AtwoodLabData data)
    {
        if (crosshair != null) crosshair.SetActive(false);
        playerMovement.enabled = false;
        playerInteract.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        atwoodLabController.gameObject.SetActive(true);
        atwoodLabController.Open(data);
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
        crosshair.SetActive(true);

        playerMovement.enabled = true;
        playerInteract.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowTooltip(string text)
    {
        if (tooltipPanel == null) return;
        tooltipLabel.text = text;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipPanel == null) return;
        tooltipPanel.SetActive(false);
    }
}