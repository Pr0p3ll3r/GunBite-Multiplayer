using UnityEngine;

public class Pause : MonoBehaviour
{
    public static bool paused = false;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;

    public void TooglePause()
    {
        paused = !paused;
   
        transform.GetChild(0).gameObject.SetActive(paused);
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void Settings()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void ReturnToPause()
    {
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void ReturnToMenu()
    {
        TooglePause();
        LevelLoader.Instance.LoadScene(0);
    }
}