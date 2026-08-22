using UnityEngine;
using UnityEngine.SceneManagement;

public class Switcher : MonoBehaviour
{




    public void Switch(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void exit()
    {
        Application.Quit();
    }
}
