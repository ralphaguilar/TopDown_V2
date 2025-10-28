using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called by Start button
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // <-- replace with your scene
    }

    // Called by Quit button
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit(); // works in build, won’t close editor
    }

    // Called by Options button (empty for now)
    public void OpenOptions()
    {
        Debug.Log("Options button clicked (no functionality yet)");
    }
}
