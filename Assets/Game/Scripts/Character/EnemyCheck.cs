using UnityEngine;

public class EnemyCheck : MonoBehaviour
{
    private Character owner;

    private void Awake()
    {
        owner = GetComponentInParent<Character>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner == null) return;

        Character otherCharacter = other.GetComponentInParent<Character>();
        if (otherCharacter == null || otherCharacter == owner) return;

        if (!otherCharacter.CompareTag("Player") && !otherCharacter.CompareTag("Enemy")) return;
        if (!owner.CompareTag("Player") && !owner.CompareTag("Enemy")) return;

        if (owner is Enemy ownerEnemy && ownerEnemy.IsTransformDrivenMovement) return;
        if (otherCharacter is Enemy otherEnemy && otherEnemy.IsTransformDrivenMovement) return;
        if (owner.IsStunned || otherCharacter.IsStunned) return;
        if (owner.BrickCount == otherCharacter.BrickCount) return;

        Character loser = owner.BrickCount < otherCharacter.BrickCount ? owner : otherCharacter;
        loser.TryKnockDown();
    }
}
