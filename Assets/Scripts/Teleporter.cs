using UnityEngine;
using System.Collections.Generic;

public class Teleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform destination; // Wohin soll teleportiert werden
    public bool canTeleportGhosts = true; // Im Original können Geister auch durch

    [Header("Cooldown")]
    public float teleportCooldown = 0.5f; // Zeit bevor man wieder teleportiert werden kann

    [Header("Offset")]
    public float teleportOffset = 2f; // Abstand vom Ziel-Teleporter

    // Dictionary um für jedes Objekt einen eigenen Cooldown zu tracken
    private Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();

    private void Update()
    {
        // Reduziere alle Cooldowns
        List<GameObject> toRemove = new List<GameObject>();

        foreach (var kvp in cooldowns)
        {
            cooldowns[kvp.Key] -= Time.deltaTime;

            if (cooldowns[kvp.Key] <= 0)
            {
                toRemove.Add(kvp.Key);
            }
        }

        // Entferne abgelaufene Cooldowns
        foreach (var obj in toRemove)
        {
            cooldowns.Remove(obj);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;

        // Prüfe ob Cooldown aktiv ist
        if (cooldowns.ContainsKey(obj) && cooldowns[obj] > 0)
        {
            return; // Noch im Cooldown, nicht teleportieren
        }

        // Prüfe ob es Pac-Man ist
        if (other.CompareTag("Player"))
        {
            TeleportObject(obj);
        }
        // Prüfe ob es ein Geist ist
        else if (other.CompareTag("Ghost") && canTeleportGhosts)
        {
            TeleportObject(obj);
        }
    }

    void TeleportObject(GameObject obj)
    {
        if (destination != null)
        {
            // Berechne Richtung vom Ziel-Teleporter zu diesem Teleporter
            Vector3 direction = (transform.position - destination.position).normalized;

            // Teleportiere MIT Abstand vom Ziel-Teleporter weg
            Vector3 targetPosition = destination.position + direction * teleportOffset;

            obj.transform.position = targetPosition;

            // Setze Cooldown für BEIDE Teleporter
            cooldowns[obj] = teleportCooldown;

            // Setze auch Cooldown für den Ziel-Teleporter
            Teleporter destinationTeleporter = destination.GetComponent<Teleporter>();
            if (destinationTeleporter != null)
            {
                destinationTeleporter.cooldowns[obj] = teleportCooldown;
            }

            Debug.Log($"{obj.name} wurde zu {targetPosition} teleportiert!");
        }
        else
        {
            Debug.LogWarning("Kein Ziel für Teleporter gesetzt!");
        }
    }

    void OnDrawGizmos()
    {
        if (destination != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destination.position);
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // Zeige Zielposition
            Vector3 direction = (transform.position - destination.position).normalized;
            Vector3 targetPos = destination.position + direction * teleportOffset;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPos, 0.3f);
        }
    }
}