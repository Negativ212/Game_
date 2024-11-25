using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelMenuController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite unlockedSprite;
    public Sprite currentLevelSprite;
    public Sprite lockedSprite;

    [Header("Level Settings")]
    public int currentPlayerLevel; // Уровень игрока
    public Button[] levelButtons;

    private void Start()
    {
        // Загрузка текущего уровня при запуске
        currentPlayerLevel = PlayerPrefs.GetInt("CurrentPlayerLevel", 1);
        
    }

    private void Update()
    {
        UpdateLevelButtons();
    }

    public void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1;
            Button button = levelButtons[i];
            Image buttonImage = button.GetComponent<Image>();

            if (levelIndex < currentPlayerLevel)
            {
                buttonImage.sprite = unlockedSprite;
                button.interactable = true;
            }
            else if (levelIndex == currentPlayerLevel)
            {
                buttonImage.sprite = currentLevelSprite;
                button.interactable = true;
            }
            else
            {
                buttonImage.sprite = lockedSprite;
                button.interactable = false;
            }

            int sceneIndex = levelIndex;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => LoadLevel(sceneIndex));
        }
    }

    public void LoadLevel(int levelIndex)
    {
        Debug.Log($"Попытка загрузить уровень {levelIndex}. Текущий уровень игрока: {currentPlayerLevel}");
        if (levelIndex <= currentPlayerLevel)
        {
            SceneManager.LoadScene(levelIndex);
        }
        else
        {
            Debug.LogError($"Уровень {levelIndex} недоступен!");
        }
    }

    public void SaveCurrentLevel(int level)
    {
        currentPlayerLevel = level;
        PlayerPrefs.SetInt("CurrentPlayerLevel", currentPlayerLevel);
        PlayerPrefs.Save();
        Debug.Log($"Сохранён текущий уровень: {currentPlayerLevel}");
    }

    public void ResetLVL()
    {
        currentPlayerLevel = 1;
        PlayerPrefs.SetInt("CurrentPlayerLevel", currentPlayerLevel); // Сохранение в PlayerPrefs
        PlayerPrefs.Save(); // Убедитесь, что изменения сохранены
        Debug.Log("Прогресс сброшен. Текущий уровень: " + currentPlayerLevel);
    }

}
