using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;

    public Canvas gameCanvas;
    
    public void CharacterTookDamage(GameObject character, int damageReceived)
    {
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);
        TMP_Text tmpText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity,gameCanvas.transform).GetComponent<TMP_Text>();
        tmpText.text = damageReceived.ToString();
    }

    public void CharacterTookHealth(GameObject character, int healthReceived)
    {
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);
        TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity).GetComponent<TMP_Text>();
        tmpText.text = healthReceived.ToString();
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += CharacterTookDamage;
        CharacterEvents.characterHealed += (CharacterTookHealth);
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= (CharacterTookDamage);
        CharacterEvents.characterHealed -=(CharacterTookHealth);
    }

    void Awake()
    {
        gameCanvas = FindObjectOfType<Canvas>();

    }

}
