using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPbar : MonoBehaviour
{
    [SerializeField] public List<Sprite> hpSprites; // Список спрайтов для каждого уровня здоровья
    [SerializeField] public UnityEngine.UI.Image image; // Указываем, что это Image из UnityEngine.UI
    public Player player; // Ссылка на игрока

    void Start()
    {
        player = Player.Instance; // Получаем ссылку на игрока
        UpdateHealthDisplay(player.lives); // Обновляем дисплей на старте
    }

    void Update()
    {
        UpdateHealthDisplay(player.lives);
    }

    public void UpdateHealthDisplay(int lives)
    {
        // Проверяем, что индекс здоровья в допустимом диапазоне
        if (lives >= -1)
        {
            image.sprite = hpSprites[lives]; // Устанавливаем соответствующий спрайт
        }
    }
}
