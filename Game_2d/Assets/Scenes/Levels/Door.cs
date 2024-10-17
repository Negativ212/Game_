using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Список ключей, необходимых для открытия двери
    public List<Key> keys;

    void Update()
    {
        // Проверяем, остались ли ключи в списке
        if (keys.Count == 0)
        {
            // Если ключей не осталось, уничтожаем дверь
            Destroy(gameObject);
        }
    }

    // Метод для удаления ключа из списка
    public void RemoveKey(Key key)
    {
        if (keys.Contains(key))
        {
            keys.Remove(key);
        }
    }
}
