using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnCollision : MonoBehaviour
{
    // Список объектов, которые нужно отслеживать
    public List<GameObject> objectsToTrack;

    // Объект, который будет активирован при столкновении
    public GameObject objectToActivate;

    // Слой, с которым должен взаимодействовать игрок
    public int playerLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, совпадает ли слой объекта с заданным слоем для игрока
        if (other.gameObject.layer == playerLayer)
        {
            // Активируем объект
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
        }
    }

    private void Update()
    {
        // Проверяем, активны ли все объекты в списке
        bool allInactive = true;

        foreach (GameObject obj in objectsToTrack)
        {
            // Если хотя бы один объект активен, выходим из цикла
            if (obj != null && obj.activeInHierarchy)
            {
                allInactive = false;
                break;
            }
        }

        // Если все объекты неактивны, деактивируем objectToActivate
        if (allInactive && objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }
}
