using UnityEngine;
using UnityEngine.SceneManagement;

public class Switcher : MonoBehaviour
{




    public void Switch(string sceneName)
    {
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void exit()
    {
        Application.Quit();
    }
}
