using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private int damageAmount = 12; // Урон от шипа
    [SerializeField] private float checkRadius = 0.5f; // Радиус проверки для обнаружения игрока
    [SerializeField] private LayerMask playerMask; // Маска слоя для игрока
    public Transform checkPoint; // Точка проверки; если не указана, будет использоваться позиция Spike

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckForPlayerAndDealDamage();
    }

    private void CheckForPlayerAndDealDamage()
    {
        // Используем позицию `checkPoint` или позицию самого объекта Spike, если `checkPoint` не назначен
        Vector2 checkPosition = checkPoint != null ? checkPoint.position : transform.position;

        // Находим все объекты в радиусе, принадлежащие слою игрока
        Collider2D[] playersInRange = Physics2D.OverlapCircleAll(checkPosition, checkRadius, playerMask);

        foreach (var playerCollider in playersInRange)
        {
            Player playerComponent = playerCollider.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.TakeDamage(damageAmount);
                Debug.Log("Игрок получил урон от шипа!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Визуализируем радиус проверки, используя либо `checkPoint`, либо позицию объекта Spike
        Vector3 gizmoPosition = checkPoint != null ? checkPoint.position : transform.position;
        Gizmos.DrawWireSphere(gizmoPosition, checkRadius);
    }
}
