using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameLauncher : MonoBehaviour
{
    private MenuManager menuManager;

    [Header("Connect")]
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private TextMeshProUGUI progressStatus;
    [SerializeField] private Button connectButton;

    [Header("Profile")]
    private string nickname;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Lobby List")]
    [SerializeField] private GameObject lobbyListItemPrefab;
    [SerializeField] private Transform lobbyList;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button openCreateLobbyButton;

    [Header("Lobby Settings")]
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private TMP_Dropdown maxPlayersDropdown;
    [SerializeField] private TextMeshProUGUI warningTextLobby;
    [SerializeField] private Button createLobbyButton;

    private string lobbyName;
    private int maxPlayers;

    [Header("In Lobby")]
    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private Transform playerList;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;
    private int playersReady = 0;
    private bool started = false;

    public string GetPlayerName()
    {
        return nickname;
    }

    private void Awake()
    {
        menuManager = GetComponent<MenuManager>();
        menuManager.OpenTab(menuManager.tabConnect);

        connectButton.onClick.AddListener(Authenticate);

        openCreateLobbyButton.onClick.AddListener(() => {
            menuManager.OpenTab(menuManager.tabCreateLobby);
            DefaultSettings();
        });
        refreshButton.onClick.AddListener(() => { 
            LobbyManager.Instance.RefreshLobbyList();
            StartCoroutine(DisableButton(refreshButton));
        });

        maxPlayersDropdown.onValueChanged.AddListener(delegate {
            ChangeMaxPlayers(maxPlayersDropdown);
        });
        createLobbyButton.onClick.AddListener(CreateLobby);
        readyButton.onClick.AddListener(ReadyOnClick);
        leaveButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
            StartCoroutine(DisableButton(leaveButton));
        });
        startButton.onClick.AddListener(() => {
            started = true;
            LobbyManager.Instance.StartGame();
            StartCoroutine(DisableButton(startButton, 3f));
        });
    }

    private void Authenticate()
    {
        if (string.IsNullOrEmpty(nicknameInputField.text))
        {
            progressStatus.color = new Color32(255, 23, 23, 255);
            progressStatus.text = "Set username!";
            return;
        }
        StartCoroutine(DisableButton(connectButton));
        progressStatus.color = new Color32(255, 255, 55, 255);
        progressStatus.text = "Connecting";
        nickname = nicknameInputField.text;
        nicknameText.text = nickname;
        LobbyManager.Instance.Authenticate(GetPlayerName());
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnPlayerLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnPlayerLeftLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobbies);
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobbies);
    }

    private void LobbyManager_OnPlayerLeftLobby(object sender, EventArgs e)
    {
        ClearLobby();
        ShowStartButton();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        menuManager.OpenTab(menuManager.tabLobby);
        ShowStartButton();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in this.lobbyList)
        {
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            GameObject lobbyListGO = Instantiate(lobbyListItemPrefab, this.lobbyList);
            LobbyListItem lobbyListItem = lobbyListGO.GetComponent<LobbyListItem>();
            lobbyListItem.UpdateLobby(lobby);
        }
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby)
    {
        if (started) return;

        ClearLobby();
        playersReady = 0;
        foreach (Unity.Services.Lobbies.Models.Player player in lobby.Players)
        {
            GameObject playerListItem = Instantiate(lobbyPlayerPrefab, playerList);
            LobbyPlayer lobbyPlayer = playerListItem.GetComponent<LobbyPlayer>();

            lobbyPlayer.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() && player.Id != AuthenticationService.Instance.PlayerId
            );
            if(player.Data[LobbyManager.KEY_READY].Value == "true")
                playersReady++;
            lobbyPlayer.UpdatePlayer(player);
        }
        if (playersReady == lobby.Players.Count)
            startButton.interactable = true;
        else
            startButton.interactable = false;    
    }

    private void ClearLobby()
    {
        foreach (Transform child in playerList)
        {
            Destroy(child.gameObject);
        }
    }

    private void StartTimer()
    {
        startButton.GetComponent<Button>().interactable = false;
        readyButton.interactable = false;
        //chatManager.StartCounting();
    }

    private void DefaultSettings()
    {
        lobbyNameInputField.text = "";
        warningTextLobby.text = "";
        maxPlayers = 2;
    }

    private void ChangeMaxPlayers(TMP_Dropdown change)
    {
        maxPlayers = byte.Parse(change.options[change.value].text);
    }

    private void LogOut()
    {      
        menuManager.OpenTab(menuManager.tabConnect);
        nicknameText.text = "Username";
        levelText.text = "Level 1";
        expText.text = "0/0";
    }

    private void CreateLobby()
    {
        if (string.IsNullOrEmpty(lobbyNameInputField.text))
        {
            warningTextLobby.text = "Set lobby name!";
            return;
        }

        Debug.Log("Creating lobby");
        warningTextLobby.text = "Creating lobby";
        started = false;
        lobbyName = lobbyNameInputField.text;
        lobbyNameText.text = lobbyName;
        maxPlayersText.text = maxPlayers.ToString();
        StartCoroutine(DisableButton(createLobbyButton));
        LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers);
    }

    private IEnumerator DisableButton(Button button, float time = 1f)
    {
        button.interactable = false;
        yield return new WaitForSeconds(time);
        button.interactable = true;
    }    

    private void ReadyOnClick()
    {
        if (readyButton.GetComponent<Image>().color == Color.red)
        {
            LobbyManager.Instance.UpdatePlayerReady("true");
            readyButton.GetComponent<Image>().color = Color.green;
        }
        else if (readyButton.GetComponent<Image>().color == Color.green)
        {
            LobbyManager.Instance.UpdatePlayerReady("false");
            readyButton.GetComponent<Image>().color = Color.red;
        }
        StartCoroutine(DisableButton(readyButton));
    }

    private void ShowStartButton()
    {
        if (LobbyManager.Instance.IsLobbyHost())
            startButton.gameObject.SetActive(true);
        else
            startButton.gameObject.SetActive(false);
    }
}