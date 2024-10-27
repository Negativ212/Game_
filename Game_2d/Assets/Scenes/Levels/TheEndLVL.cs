using UnityEngine;

public class TheEndLVL : MonoBehaviour
{
    public GameObject objectToActivate; // Объект, который будет активирован при отсутствии врагов

    void Update()
    {
        // Ищем все объекты с тегом "enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");

        // Проверяем, остались ли враги
        if (enemies.Length == 1)
        {
            // Активируем объект
            objectToActivate.SetActive(true);
        }
        else
        {
            // Если враги есть, деактивируем объект (опционально)
            objectToActivate.SetActive(false);
        }
    }
}
