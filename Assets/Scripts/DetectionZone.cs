using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class DetectionZone : MonoBehaviour
{
    public List<Collider2D> detectedColliders = new List<Collider2D>();
    public Collider2D col;
    private LayerMask playerLayer;
    private String playerTag = "Player";
    public System.Action<Transform> OnPlayerEnter;
    public System.Action<Transform> OnPlayerExit;
    #region MonoBehaviour

    private void Awake()
    {
        if (col == null) col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        detectedColliders.Add(other);
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player enter");
            OnPlayerEnter?.Invoke(other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        detectedColliders.Remove(other);
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player left detection zone!");
            OnPlayerExit?.Invoke(other.transform);
        }
    }

    public bool HasPlayer()
    {
        foreach (Collider2D col in detectedColliders)
        {
            if (col != null &&col.gameObject.layer == playerLayer) return true;
        }
        return false;
    }
    
    #endregion
}
