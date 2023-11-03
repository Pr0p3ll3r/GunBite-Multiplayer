using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [HideInInspector] public WaveManager waveManager;

    public bool isShopTime = false;
    public int shopTime = 30;

    [SerializeField] private float startTimestamp = 0;
    [SerializeField] private TextMeshProUGUI zombieKilledText;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI shopTimer;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private TextMeshProUGUI timeSurvivedText;
    [SerializeField] private TextMeshProUGUI timeSurvivedScoreText;
    [SerializeField] private TextMeshProUGUI zombieKilledEndText;
    [SerializeField] private TextMeshProUGUI zombieKilledScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private int currentGameTime;
    private Coroutine timerCoroutine;
    private Coroutine countingCo;
    private bool started = false;
    private bool shopOpened = false;
    private int zombieKilled = 0;

    public ObjectPooler bulletPooler;
    public ObjectPooler acidPooler;
    public ObjectPooler plantAcidPooler;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        waveManager = GetComponent<WaveManager>();
    }

    private void Update()
    {
        if (started == false)
        {
            if (countingCo == null) countingCo = StartCoroutine(CountingSound());
            timer.text = "Prepare to fight..." + (1 + (int)(startTimestamp - Time.timeSinceLevelLoad));
            if (Time.timeSinceLevelLoad >= startTimestamp) StartGame();
            return;
        }       

        if(isShopTime && Player.Instance.GetComponent<WeaponManager>().isReloading == false)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if(shopOpened)
                    Shop.Instance.CloseShop();
                else
                    Shop.Instance.OpenShop();

                shopOpened = !shopOpened;
            }
        }
    }

    IEnumerator CountingSound()
    {
        for (int i = (int)startTimestamp; i > 0; i--)
        {
            SoundManager.Instance.Play("CountingSound");
            yield return new WaitForSeconds(1f);
        }
    }

    void StartGame()
    {
        Debug.Log("Game Started");
        started = true;
        InitializeTimer();
        waveManager.enabled = true;
        SoundManager.Instance.Play("StartGame");
    }

    public void Gameover()
    {
        waveManager.enabled = false;

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        SetGameOverScreen(zombieKilled);

        StartCoroutine(Wait(3f));
    }

    private void InitializeTimer()
    {
        currentGameTime = 0;
        RefreshTimerUI();

        StartCoroutine(Timer());
    }

    private void RefreshTimerUI()
    {
        string hours = (currentGameTime / 3600).ToString("00");
        int m = currentGameTime % 3600;
        string minutes = (m / 60).ToString("00");
        string seconds = (m % 60).ToString("00");
        timer.text = $"{hours}:{minutes}:{seconds}";

        string minutesShop = (shopTime / 60).ToString("00");
        string secondsShop = (shopTime % 60).ToString("00");
        shopTimer.text = $"Shop closes in {minutesShop}:{secondsShop}";
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);

        gameOver.SetActive(true);
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currentGameTime += 1;
        if (isShopTime)
            shopTime -= 1;
        if (shopTime == 0)
            isShopTime = false;

        RefreshTimerUI();
        timerCoroutine = StartCoroutine(Timer());
    }

    public void TimerStatus(bool status)
    {
        if (status) timerCoroutine = StartCoroutine(Timer());
        else StopCoroutine(timerCoroutine);
    }

    public void ZombieKilled()
    {
        zombieKilled++;
        zombieKilledText.text = zombieKilled.ToString();
    }

    public void SetGameOverScreen(int zombieKilled)
    {
        string hours = (currentGameTime / 3600).ToString("00");
        int m = currentGameTime % 3600;
        string minutes = (m / 60).ToString("00");
        string seconds = (m % 60).ToString("00");
        timeSurvivedText.text = $"{hours}:{minutes}:{seconds}";
        timeSurvivedScoreText.text = $"+{currentGameTime}";

        zombieKilledEndText.text = zombieKilled.ToString();
        int zombieScore = 10 * zombieKilled;
        zombieKilledScoreText.text = $"+{zombieScore}";

        int totalScore = zombieScore + currentGameTime;
        currentScoreText.text = totalScore.ToString();

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = highScore.ToString();

        if (totalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", totalScore);
            highScoreText.text = totalScore.ToString();
        }
    }
}