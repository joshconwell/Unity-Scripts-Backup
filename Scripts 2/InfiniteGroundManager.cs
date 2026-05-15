using System.Collections.Generic;
using UnityEngine;

public class InfiniteGroundManager : MonoBehaviour
{
    [System.Serializable]
    private class GroundChunk
    {
        public GameObject chunkObject;
        public Vector2Int chunkCoordinate;
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject groundChunkPrefab;

    [Header("Chunk Settings")]
    [SerializeField] private float chunkSize = 40f;
    [SerializeField] private int gridRadius = 2;
    [SerializeField] private float groundZPosition = 1f;

    [Header("Hierarchy")]
    [SerializeField] private bool parentChunksToManager = true;

    private readonly List<GroundChunk> chunks = new List<GroundChunk>();
    private Vector2Int currentPlayerChunk;
    private bool hasInitialized;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("InfiniteGroundManager could not find the player.");
            return;
        }

        if (groundChunkPrefab == null)
        {
            Debug.LogWarning("InfiniteGroundManager is missing a ground chunk prefab.");
            return;
        }

        BuildChunkGrid();

        currentPlayerChunk = GetChunkCoordinate(player.position);
        RefreshChunkPositions();

        hasInitialized = true;
    }

    private void Update()
    {
        if (!hasInitialized)
        {
            return;
        }

        Vector2Int newPlayerChunk = GetChunkCoordinate(player.position);

        if (newPlayerChunk != currentPlayerChunk)
        {
            currentPlayerChunk = newPlayerChunk;
            RefreshChunkPositions();
        }
    }

    private void BuildChunkGrid()
    {
        chunks.Clear();

        int chunkCountPerAxis = gridRadius * 2 + 1;
        int totalChunkCount = chunkCountPerAxis * chunkCountPerAxis;

        for (int i = 0; i < totalChunkCount; i++)
        {
            GameObject chunkObject = Instantiate(
                groundChunkPrefab,
                Vector3.zero,
                Quaternion.identity
            );

            if (parentChunksToManager)
            {
                chunkObject.transform.SetParent(transform);
            }

            GroundChunk chunk = new GroundChunk
            {
                chunkObject = chunkObject,
                chunkCoordinate = Vector2Int.zero
            };

            chunks.Add(chunk);
        }
    }

    private void RefreshChunkPositions()
    {
        int chunkIndex = 0;

        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int y = -gridRadius; y <= gridRadius; y++)
            {
                if (chunkIndex >= chunks.Count)
                {
                    return;
                }

                Vector2Int chunkCoordinate = new Vector2Int(
                    currentPlayerChunk.x + x,
                    currentPlayerChunk.y + y
                );

                PositionChunk(chunks[chunkIndex], chunkCoordinate);

                chunkIndex++;
            }
        }
    }

    private void PositionChunk(GroundChunk chunk, Vector2Int chunkCoordinate)
    {
        if (chunk == null || chunk.chunkObject == null)
        {
            return;
        }

        chunk.chunkCoordinate = chunkCoordinate;

        Vector3 chunkWorldPosition = GetChunkWorldPosition(chunkCoordinate);

        chunk.chunkObject.transform.position = chunkWorldPosition;
        chunk.chunkObject.transform.localScale = new Vector3(chunkSize, chunkSize, 1f);
    }

    private Vector2Int GetChunkCoordinate(Vector3 worldPosition)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int chunkY = Mathf.FloorToInt(worldPosition.y / chunkSize);

        return new Vector2Int(chunkX, chunkY);
    }

    private Vector3 GetChunkWorldPosition(Vector2Int chunkCoordinate)
    {
        float worldX = chunkCoordinate.x * chunkSize + chunkSize * 0.5f;
        float worldY = chunkCoordinate.y * chunkSize + chunkSize * 0.5f;

        return new Vector3(worldX, worldY, groundZPosition);
    }
}