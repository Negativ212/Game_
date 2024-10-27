using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBossBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer; // Ссылка на SpriteRenderer для отображения здоровья
    [SerializeField] private SlimeBoss _slime; // Ссылка на объект слайма
    [SerializeField] private List<Sprite> hpSprites; // Список спрайтов для отображения здоровья

    private void Start()
    {
        // Убедимся, что у нас есть ссылки на все необходимые компоненты
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // Получаем SpriteRenderer, если не указан
        }

        UpdateHealthDisplay(_slime.lives); // Обновляем отображение здоровья на старте
    }

    private void Update()
    {
        if (_slime.lives <= 0)
        {
            //RemoveHPBar(); // Удаляем HP-бар, если у слайма 0 жизней
            //return;
        }

        UpdateHPBar(); // Обновляем отображение спрайта
    }

    private void UpdateHPBar()
    {
        // Обновляем спрайт в зависимости от оставшихся жизней
        UpdateHealthDisplay(_slime.lives);
    }

    private void UpdateHealthDisplay(int lives)
    {
        // Убедитесь, что жизни находятся в пределах 0-4, так как у нас 5 спрайтов
        int index = Mathf.Clamp(lives, 0, hpSprites.Count - 1);
        spriteRenderer.sprite = hpSprites[index]; // Устанавливаем соответствующий спрайт
    }

    private void RemoveHPBar()
    {
        gameObject.SetActive(false); // Деактивируем объект, если слайм мертв
    }
}
