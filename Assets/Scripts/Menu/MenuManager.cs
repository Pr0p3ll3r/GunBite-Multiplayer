using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Tabs")]
    public GameObject tabConnect;
    public GameObject tabMain;
    public GameObject tabLobbies;
    public GameObject tabCreateLobby;
    public GameObject tabLobby;
    public GameObject tabSettings;
    public GameObject tabAbout;

    [Header("Loading")]
    public GameObject tabLoading;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Buttons")]
    [SerializeField] private Button findLobbyButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button mainQuitGameButton;
    [SerializeField] private Button connectQuitGameButton;

    private void Awake()
    {
        Instance = this;

        findLobbyButton.onClick.AddListener(() => {
            OpenTab(tabLobbies);
        });
        settingsButton.onClick.AddListener(() => {
            OpenTab(tabSettings);
        });
        aboutButton.onClick.AddListener(() => {
            OpenTab(tabAbout);
        });
        mainQuitGameButton.onClick.AddListener(QuitGame);
        connectQuitGameButton.onClick.AddListener(QuitGame);
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

    public void LoadingBox(string text)
    {
        TabCloseAll();
        loadingText.text = text;
        tabLoading.SetActive(true);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
