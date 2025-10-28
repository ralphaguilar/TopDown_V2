using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("Fade + Panel")]
    public CanvasGroup gameOverGroup;
    public float fadeSpeed = 1f;

    [Header("Buttons")]
    public Button retryButton;
    public Button quitButton;
    public Button mainMenuButton;          

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";  

    bool isActive = false;

    void Awake()
    {
        if (gameOverGroup)
        {
            gameOverGroup.alpha = 0f;
            gameOverGroup.interactable = false;
            gameOverGroup.blocksRaycasts = false;
        }

        if (retryButton)    retryButton.onClick.AddListener(OnRetry);
        if (quitButton)     quitButton.onClick.AddListener(OnQuit);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(OnMainMenu);  
    }

    void Update()
    {
        if (isActive && gameOverGroup && gameOverGroup.alpha < 1f)
            gameOverGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
    }

    public void ShowGameOver()
    {
        if (isActive) return;
        isActive = true;
        Time.timeScale = 0f;

        if (gameOverGroup)
        {
            gameOverGroup.alpha = 0f;
            gameOverGroup.interactable = true;
            gameOverGroup.blocksRaycasts = true;
        }
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void OnMainMenu()                              
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[GameOverManager] Main menu scene name not set.");
            return;
        }
        Time.timeScale = 1f;                              
        SceneManager.LoadScene(mainMenuSceneName);
    }
}