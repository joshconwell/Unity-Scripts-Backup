using UnityEngine;

public class BossArenaLock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Arena Box Settings")]
    [Tooltip("Width of the boss arena in world units.")]
    [SerializeField] private float arenaWidth = 20f;

    [Tooltip("Height of the boss arena in world units.")]
    [SerializeField] private float arenaHeight = 20f;

    [SerializeField] private float wallThickness = 1f;
    [SerializeField] private float gameplayPlaneZ = 0f;

    [Header("Wall Settings")]
    [SerializeField] private string wallObjectName = "Boss Arena Wall";
    [SerializeField] private bool showDebugWalls = false;

    private GameObject arenaRoot;
    private Vector2 arenaCenter;
    private bool arenaLocked;

    public bool ArenaLocked => arenaLocked;
    public Vector2 ArenaCenter => arenaCenter;

    private void Awake()
    {
        FindPlayerIfNeeded();
    }

    public void LockArenaAtPlayerChunk()
    {
        LockArenaAroundPlayer();
    }

    public void LockArenaAroundPlayer()
    {
        FindPlayerIfNeeded();

        if (player == null)
        {
            Debug.LogWarning("BossArenaLock could not find the player.");
            return;
        }

        arenaCenter = new Vector2(player.position.x, player.position.y);

        BuildArenaWalls();

        arenaLocked = true;

        Debug.Log($"Boss arena locked around player. Center: {arenaCenter}, Size: {arenaWidth} x {arenaHeight}");
    }

    public void UnlockArena()
    {
        if (arenaRoot != null)
        {
            Destroy(arenaRoot);
        }

        arenaRoot = null;
        arenaLocked = false;

        Debug.Log("Boss arena unlocked.");
    }

    public Vector3 GetRandomPointInsideArena(float edgePadding)
    {
        if (!arenaLocked)
        {
            FindPlayerIfNeeded();
            return player != null ? player.position : Vector3.zero;
        }

        float halfWidth = arenaWidth * 0.5f;
        float halfHeight = arenaHeight * 0.5f;

        float safeXPadding = Mathf.Clamp(edgePadding, 0f, halfWidth - 0.5f);
        float safeYPadding = Mathf.Clamp(edgePadding, 0f, halfHeight - 0.5f);

        float randomX = Random.Range(
            arenaCenter.x - halfWidth + safeXPadding,
            arenaCenter.x + halfWidth - safeXPadding
        );

        float randomY = Random.Range(
            arenaCenter.y - halfHeight + safeYPadding,
            arenaCenter.y + halfHeight - safeYPadding
        );

        return new Vector3(randomX, randomY, gameplayPlaneZ);
    }

    public Vector3 GetArenaCenterWorldPosition()
    {
        if (!arenaLocked)
        {
            FindPlayerIfNeeded();
            return player != null ? player.position : Vector3.zero;
        }

        return new Vector3(arenaCenter.x, arenaCenter.y, gameplayPlaneZ);
    }

    private void BuildArenaWalls()
    {
        if (arenaRoot != null)
        {
            Destroy(arenaRoot);
        }

        arenaRoot = new GameObject("Boss Arena Lock");
        arenaRoot.transform.position = new Vector3(arenaCenter.x, arenaCenter.y, gameplayPlaneZ);

        float halfWidth = arenaWidth * 0.5f;
        float halfHeight = arenaHeight * 0.5f;

        CreateWall(
            "Top",
            new Vector2(arenaCenter.x, arenaCenter.y + halfHeight + wallThickness * 0.5f),
            new Vector2(arenaWidth + wallThickness * 2f, wallThickness)
        );

        CreateWall(
            "Bottom",
            new Vector2(arenaCenter.x, arenaCenter.y - halfHeight - wallThickness * 0.5f),
            new Vector2(arenaWidth + wallThickness * 2f, wallThickness)
        );

        CreateWall(
            "Left",
            new Vector2(arenaCenter.x - halfWidth - wallThickness * 0.5f, arenaCenter.y),
            new Vector2(wallThickness, arenaHeight + wallThickness * 2f)
        );

        CreateWall(
            "Right",
            new Vector2(arenaCenter.x + halfWidth + wallThickness * 0.5f, arenaCenter.y),
            new Vector2(wallThickness, arenaHeight + wallThickness * 2f)
        );
    }

    private void CreateWall(string wallName, Vector2 position, Vector2 size)
    {
        GameObject wallObject = new GameObject($"{wallObjectName} {wallName}");
        wallObject.transform.SetParent(arenaRoot.transform);
        wallObject.transform.position = new Vector3(position.x, position.y, gameplayPlaneZ);

        BoxCollider2D wallCollider = wallObject.AddComponent<BoxCollider2D>();
        wallCollider.size = size;
        wallCollider.isTrigger = false;

        Rigidbody2D wallRigidbody = wallObject.AddComponent<Rigidbody2D>();
        wallRigidbody.bodyType = RigidbodyType2D.Static;

        if (showDebugWalls)
        {
            SpriteRenderer spriteRenderer = wallObject.AddComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1f, 0f, 0f, 0.35f);

            Sprite debugSprite = CreateDebugSprite();
            spriteRenderer.sprite = debugSprite;

            wallObject.transform.localScale = new Vector3(size.x, size.y, 1f);
        }
    }

    private Sprite CreateDebugSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
}