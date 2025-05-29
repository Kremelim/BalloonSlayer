using UnityEngine;
using UnityEngine.InputSystem; // REQUIRED

public class CrosshairController : MonoBehaviour
{
    private PlayerControls playerControls;
    private Vector2 mouseLookPosition;
    // public Camera mainCamera; // Assign in Inspector if not Camera.main

    void Awake()
    {
        playerControls = new PlayerControls();
        // if (mainCamera == null) mainCamera = Camera.main; // Fallback
    }

    private void OnEnable()
    {
        playerControls.Gameplay.Enable();
        playerControls.Gameplay.Shoot.performed += ctx => HandleShoot(); // Renamed for clarity
    }

    private void OnDisable()
    {
        playerControls.Gameplay.Shoot.performed -= ctx => HandleShoot();
        playerControls.Gameplay.Disable();
    }

    void Update()
    {
        mouseLookPosition = playerControls.Gameplay.Look.ReadValue<Vector2>();
        Vector3 screenMousePosition = new Vector3(mouseLookPosition.x, mouseLookPosition.y, Camera.main.nearClipPlane + 1);
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(screenMousePosition);
        transform.position = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, transform.position.z);
    }

    void HandleShoot() // Renamed from Shoot to avoid confusion if you had a Shoot variable
    {
        // Debug.Log("Shoot action performed via Input System!");

        Ray ray = Camera.main.ScreenPointToRay(mouseLookPosition); // Use current mouseLookPosition
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            // Debug.Log("Raycast Hit: " + hit.collider.name);
            Balloon balloon = hit.collider.GetComponent<Balloon>();
            if (balloon != null)
            {
                balloon.TakeDamage(1); // This calls the method on the Balloon.cs script
            }
            // Potentially check for other clickable things here later
        }
    }
}