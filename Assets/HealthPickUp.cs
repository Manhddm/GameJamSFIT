using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickUp : MonoBehaviour
{
    public int healRestore = 20;
    public Vector3 spinRotationSpeed = new Vector3(0, 180, 0);
    private void OnTriggerEnter2D(Collider2D other)
    {
        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null)
        {
            bool wasHeal = damageable.Heal(healRestore);
            if (wasHeal)
                Destroy(gameObject);
        }
    }

    private void Update()
    {
        transform.eulerAngles += spinRotationSpeed * Time.deltaTime;
    }
}
