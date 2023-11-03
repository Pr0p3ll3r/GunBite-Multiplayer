using UnityEngine;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour, IPointerEnterHandler
{
    public static bool paused = false;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TooglePause();
    }

    public void TooglePause()
    {
        paused = !paused;

        if(paused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
   
        transform.GetChild(0).gameObject.SetActive(paused);
        pauseMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void Options()
    {
        ClickSound();
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void Return()
    {
        transform.GetChild(0).gameObject.SetActive(paused);
        pauseMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void ExitGame()
    {
        ClickSound();
        TooglePause();
        LevelLoader.Instance.LoadScene(0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            SoundManager.Instance.PlayOneShot("Hover");
        }
    }

    public void ClickSound()
    {
        SoundManager.Instance.PlayOneShot("Click");
    }
}