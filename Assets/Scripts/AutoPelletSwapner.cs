using UnityEngine;
using UnityEngine.Tilemaps;

public class AutoPelletSpawner : MonoBehaviour
{
    [Header("Required Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap wallTilemap;

    [Header("Pellet Prefabs")]
    public GameObject pelletPrefab;
    public GameObject powerPelletPrefab;

    [Header("Power Pellet Positions")]
    [Tooltip("Definiere hier die Positionen für Power Pellets (z.B. Ecken)")]
    public Vector2Int[] powerPelletPositions = new Vector2Int[]
    {
        new Vector2Int(-7, 9),   // Oben links
        new Vector2Int(6, 9),    // Oben rechts
        new Vector2Int(-7, -4),  // Unten links
        new Vector2Int(6, -4)    // Unten rechts
    };

    [Header("Exclusion Areas")]
    [Tooltip("Bereiche wo KEINE Pellets spawnen sollen (z.B. Geister-Startzone)")]
    public Vector2Int[] exclusionCenters;
    public int exclusionRadius = 5;

    [Header("Settings")]
    public bool spawnOnStart = true;
    public bool clearExistingPellets = true;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnAllPellets();
        }
    }

    [ContextMenu("Spawn All Pellets")]
    public void SpawnAllPellets()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("Ground Tilemap nicht zugewiesen!");
            return;
        }

        if (pelletPrefab == null)
        {
            Debug.LogError("Pellet Prefab nicht zugewiesen!");
            return;
        }

        // Lösche existierende Pellets falls gewünscht
        if (clearExistingPellets)
        {
            ClearAllPellets();
        }

        int pelletCount = 0;
        int powerPelletCount = 0;

        // Durchlaufe alle Tiles in der Ground-Tilemap
        BoundsInt bounds = groundTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // Prüfe ob auf diesem Ground-Tile ein Tile existiert
                if (groundTilemap.HasTile(cellPosition))
                {
                    // Prüfe ob an dieser Position eine Wand ist
                    if (wallTilemap != null && wallTilemap.HasTile(cellPosition))
                    {
                        continue; // Überspringe wenn Wand da ist
                    }

                    // Prüfe ob Position in Exclusion Zone liegt
                    if (IsInExclusionZone(new Vector2Int(x, y)))
                    {
                        continue; // Überspringe Exclusion Zones
                    }

                    // Berechne World-Position (Mitte des Tiles)
                    Vector3 worldPosition = groundTilemap.GetCellCenterWorld(cellPosition);

                    // Prüfe ob hier ein Power Pellet hin soll
                    bool isPowerPelletPosition = IsPowerPelletPosition(new Vector2Int(x, y));

                    if (isPowerPelletPosition && powerPelletPrefab != null)
                    {
                        // Spawne Power Pellet
                        GameObject powerPellet = Instantiate(powerPelletPrefab, worldPosition, Quaternion.identity, transform);
                        powerPellet.name = $"PowerPellet_{x}_{y}";
                        powerPelletCount++;
                    }
                    else
                    {
                        // Spawne normales Pellet
                        GameObject pellet = Instantiate(pelletPrefab, worldPosition, Quaternion.identity, transform);
                        pellet.name = $"Pellet_{x}_{y}";
                        pelletCount++;
                    }
                }
            }
        }

        Debug.Log($"✅ Spawning complete! Created {pelletCount} pellets and {powerPelletCount} power pellets.");
    }

    bool IsPowerPelletPosition(Vector2Int position)
    {
        foreach (Vector2Int powerPos in powerPelletPositions)
        {
            if (powerPos == position)
            {
                return true;
            }
        }
        return false;
    }

    bool IsInExclusionZone(Vector2Int position)
    {
        foreach (Vector2Int center in exclusionCenters)
        {
            float distance = Vector2Int.Distance(position, center);
            if (distance <= exclusionRadius)
            {
                return true;
            }
        }
        return false;
    }

    [ContextMenu("Clear All Pellets")]
    public void ClearAllPellets()
    {
        // Lösche alle Kinder dieses GameObjects
        int count = transform.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        Debug.Log($"🗑️ Cleared {count} pellets.");
    }

    [ContextMenu("Count Ground Tiles")]
    public void CountGroundTiles()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("Ground Tilemap nicht zugewiesen!");
            return;
        }

        int count = 0;
        BoundsInt bounds = groundTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (groundTilemap.HasTile(cellPosition))
                {
                    count++;
                }
            }
        }

        Debug.Log($"📊 Ground Tilemap hat {count} Tiles.");
    }

    // Visualisierung im Editor
    void OnDrawGizmos()
    {
        if (groundTilemap == null) return;

        // Zeige Power Pellet Positionen
        Gizmos.color = Color.cyan;
        foreach (Vector2Int pos in powerPelletPositions)
        {
            Vector3 worldPos = groundTilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
            Gizmos.DrawWireSphere(worldPos, 0.5f);
        }

        // Zeige Exclusion Zones
        Gizmos.color = Color.red;
        foreach (Vector2Int center in exclusionCenters)
        {
            Vector3 worldPos = groundTilemap.GetCellCenterWorld(new Vector3Int(center.x, center.y, 0));
            Gizmos.DrawWireSphere(worldPos, exclusionRadius);
        }
    }
}