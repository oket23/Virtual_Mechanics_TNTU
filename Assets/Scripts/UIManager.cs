using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Елементи")]
    public GameObject menuPanel;
    public GameObject crosshair;

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

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
        crosshair.SetActive(true);
        
        playerMovement.enabled = true;
        playerInteract.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}