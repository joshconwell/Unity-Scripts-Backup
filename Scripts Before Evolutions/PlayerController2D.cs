using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerStats playerStats;

    [Header("Fallback Movement")]
    [SerializeField] private float fallbackMoveSpeed = 6f;

    [Header("Aim Settings")]
    [SerializeField] private bool useMouseAimByDefault = true;

    private Rigidbody2D rb;

    private Vector2 moveInput;
    private Vector2 mouseWorldPosition;
    private Vector2 externalAimDirection = Vector2.right;

    private bool usingExternalAim;
    private float targetRotationAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        usingExternalAim = !useMouseAimByDefault;
    }

    private void Update()
    {
        ReadMovementInput();

        if (usingExternalAim)
        {
            CalculateExternalAimRotation();
        }
        else
        {
            ReadMouseAim();
            CalculateMouseAimRotation();
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void ReadMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(moveX, moveY).normalized;
    }

    private void ReadMouseAim()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        mouseWorldPosition = new Vector2(worldPosition.x, worldPosition.y);
    }

    private void CalculateMouseAimRotation()
    {
        Vector2 aimDirection = mouseWorldPosition - rb.position;

        if (aimDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        targetRotationAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
    }

    private void CalculateExternalAimRotation()
    {
        if (externalAimDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        targetRotationAngle = Mathf.Atan2(externalAimDirection.y, externalAimDirection.x) * Mathf.Rad2Deg;
    }

    private void MovePlayer()
    {
        float moveSpeed = fallbackMoveSpeed;

        if (playerStats != null)
        {
            moveSpeed = playerStats.MoveSpeed;
        }

        Vector2 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void RotatePlayer()
    {
        rb.MoveRotation(targetRotationAngle);
    }

    public void UseMouseAim()
    {
        usingExternalAim = false;
    }

    public void UseExternalAimDirection(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        externalAimDirection = aimDirection.normalized;
        usingExternalAim = true;
    }
}