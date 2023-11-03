using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    public GameObject tabConnect;
    public GameObject tabMain;
    public GameObject tabLobbies;
    public GameObject tabCreateLobby;
    public GameObject tabLobby;
    public GameObject tabSettings;
    public GameObject tabAbout;
    public GameObject tabLoading;

    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button quitGameMainButton;
    [SerializeField] private Button quitGameConnectButton;

    private void Awake()
    {
        Instance = this;

        playButton.onClick.AddListener(() =>
        {
            OpenTab(tabLobbies);
        });
        optionsButton.onClick.AddListener(() => {
            OpenTab(tabSettings);
        });
        aboutButton.onClick.AddListener(() => {
            OpenTab(tabAbout);
        });
        quitGameMainButton.onClick.AddListener(QuitGame);
        quitGameConnectButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        OpenTab(tabConnect);
    }

    private void TabCloseAll()
    {
        tabConnect.SetActive(false);
        tabMain.SetActive(false);
        tabLobbies.SetActive(false);
        tabCreateLobby.SetActive(false);
        tabLobby.SetActive(false);
        tabLoading.SetActive(false);
        tabSettings.SetActive(false);
        tabAbout.SetActive(false);
    }

    public void OpenTab(GameObject tab)
    {
        TabCloseAll();
        tab.SetActive(true);
        tab.GetComponent<Animator>().enabled = true;
    }

    public void LoadingBox()
    {
        TabCloseAll();
        tabLoading.SetActive(true);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
