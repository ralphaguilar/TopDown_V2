using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start button
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); 
    }

    //  Quit button
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit(); 
    }

    //  Options button 
    public void OpenOptions()
    {
        Debug.Log("Options button clicked (no functionality yet)");
    }
}
