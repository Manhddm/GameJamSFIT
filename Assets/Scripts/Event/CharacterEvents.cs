
using UnityEngine;
using UnityEngine.Events;

public class CharacterEvents
{
    public static UnityAction<GameObject, int> characterDamaged;
    public static UnityAction<GameObject, int> characterHealed;
}
