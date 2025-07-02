using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class DetectionZone : MonoBehaviour
{
    public List<Collider2D> detectedColliders = new List<Collider2D>();
    public Collider2D col;
    #region MonoBehaviour

    private void Awake()
    {
        if (col == null) col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TRIGGER ENTERED: " + other.name);
        Debug.Log("This object: " + gameObject.name);
        Debug.Log("Other layer: " + other.gameObject.layer);
        detectedColliders.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        detectedColliders.Remove(other);
    }

    #endregion
}
