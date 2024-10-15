using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public int lives; // Текущие жизни сущности
    public int maxLives; // Максимальные жизни сущности
    private bool canTakeDamage = true;

    public virtual void GetDamage(int damage)
    {
        if (canTakeDamage && lives > 0)
        {
            // Логика получения урона
            lives -= damage; // Уменьшаем жизни
            lives = Mathf.Max(lives, 0); // Не допускаем отрицательных жизней
            Debug.Log($"Получено {damage} урона! Осталось жизней: {lives}");

            if (lives <= 0)
            {
                Die();
            }

            // Устанавливаем флаг, что урон уже был получен
            canTakeDamage = false;

            // Восстанавливаем возможность получать урон через заданное время
            StartCoroutine(ResetDamageAbility());
        }
    }

    private IEnumerator ResetDamageAbility()
    {
        yield return new WaitForSeconds(0.5f); // Задержка перед восстановлением
        canTakeDamage = true;
    }

    public bool IsAttackable()
    {
        return canTakeDamage;
    }

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} погиб!");
        // Здесь вы можете добавить логику для уничтожения объекта или его деактивации
        gameObject.SetActive(false); // Пример уничтожения объекта
    }
}
