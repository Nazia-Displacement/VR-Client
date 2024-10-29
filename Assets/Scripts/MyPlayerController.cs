using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class MyPlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public PlayerControls playerControls;
    public Camera cam;
    public ParticleSystem myParticleSystem;

    private float gravity = -9.8f;
    private float moveSpeed = 5.5f;
    private float lookSpeed = 15f;
    public float maxLookAngle = 80.0f;

    private Vector2 smoothedMouseDelta;
    private float smoothFactor = 0.1f; // Factor for exponential smoothing
    private float maxMouseDelta = 15f;  // Maximum allowed delta to avoid jumps
    private float deadZone = 0.1f; // Adjust as necessary
    private float blendFactor = 0.5f; // Adjust for responsiveness

    private float nextUpdate = 0;
    private float updateEvery = 0.08f;
    private Material lightMat;

    private bool affectingLight = false;

    void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        playerControls = new PlayerControls();
        playerControls.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        lightMat = myParticleSystem.GetComponent<ParticleSystemRenderer>().material;
    }

    void Update()
    {
        if(gameObject.activeInHierarchy) HandleMouseLook();
        nextUpdate += Time.deltaTime;
        if(nextUpdate > updateEvery)
        {
            nextUpdate = 0;
            ConnectionManager.instance.SendPosition(transform.position.x, transform.position.y, transform.position.z, cam.transform.eulerAngles.x, transform.eulerAngles.y, affectingLight);
        }
    }

    public void SetColor(byte r, byte g, byte b)
    {
        Color32 col = new Color32(r, g, b, 255);
        ParticleSystem.MainModule m = myParticleSystem.main;
        m.startColor = new ParticleSystem.MinMaxGradient(col);
        lightMat.SetColor("_EmissionColor", col);
    }

    public void OnTriggerEnter(Collider other)
    {
        affectingLight = true;
    }

    public void OnTriggerExit(Collider other)
    {
        affectingLight = false;
    }

    private void FixedUpdate()
    {
        if (gameObject.activeInHierarchy) HandleMovement();
    }

    private void HandleMouseLook()
    {
        Vector2 look = playerControls.Look.Direction.ReadValue<Vector2>();

        // Clamp input to prevent jumps
        look.x = Mathf.Clamp(look.x, -maxMouseDelta, maxMouseDelta);
        look.y = Mathf.Clamp(look.y, -maxMouseDelta, maxMouseDelta);

        // Implement a dead zone
        if (Mathf.Abs(look.x) < deadZone) look.x = 0;
        if (Mathf.Abs(look.y) < deadZone) look.y = 0;

        // Apply exponential smoothing
        smoothedMouseDelta.x = Mathf.Lerp(smoothedMouseDelta.x, look.x, smoothFactor);
        smoothedMouseDelta.y = Mathf.Lerp(smoothedMouseDelta.y, look.y, smoothFactor);

        // Blend smoothed input with raw input for responsiveness
        smoothedMouseDelta.x = Mathf.Lerp(smoothedMouseDelta.x, look.x, blendFactor);
        smoothedMouseDelta.y = Mathf.Lerp(smoothedMouseDelta.y, look.y, blendFactor);

        // Rotate character on the Y-axis (left/right)
        transform.Rotate(Vector3.up * smoothedMouseDelta.x * lookSpeed * Time.deltaTime);

        // Clamp vertical rotation to avoid flipping the camera
        float currentXRotation = cam.transform.eulerAngles.x;
        currentXRotation -= smoothedMouseDelta.y * lookSpeed * Time.deltaTime;

        // Convert to -180 to 180 degrees for clamping
        if (currentXRotation > 180)
            currentXRotation -= 360;

        currentXRotation = Mathf.Clamp(currentXRotation, -maxLookAngle, maxLookAngle);

        // Apply the clamped rotation to the camera
        cam.transform.eulerAngles = new Vector3(currentXRotation, cam.transform.eulerAngles.y, 0);
    }

    private void HandleMovement()
    {
        Vector2 movement = playerControls.Movement.Direction.ReadValue<Vector2>();
        characterController.Move(
            (movement.y * moveSpeed * Time.fixedDeltaTime * transform.forward) +
            (movement.x * moveSpeed * Time.fixedDeltaTime * transform.right) +
            (gravity * Time.fixedDeltaTime * transform.up)
        );
    }
}
