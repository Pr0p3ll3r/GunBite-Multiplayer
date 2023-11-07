using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Sprite emptyIcon;
    [SerializeField] private float fadeOutTime = 4f;

    [HideInInspector] public GameObject reloading;

    private List<Image> hearts = new List<Image>();
    private SlicedFilledImage staminaBar;
    private TextMeshProUGUI ammo;
    private TextMeshProUGUI levelText;
    private SlicedFilledImage expBar;
    private TextMeshProUGUI moneyText;
    private GameObject vignette;
    private TextMeshProUGUI deadText;

    private void Awake()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        //Bottom Left
        moneyText = GameObject.Find("HUD/Game/BottomLeftCorner/Money").GetComponent<TextMeshProUGUI>();
        hearts.AddRange(GameObject.Find("HUD/Game/BottomLeftCorner/HealthBar").GetComponentsInChildren<Image>());

        //Bottom Right   
        ammo = GameObject.Find("HUD/Game/BottomRightCorner/Ammo/Amount").GetComponent<TextMeshProUGUI>();

        //Exp Bar
        levelText = GameObject.Find("HUD/Game/ExpBar/LevelText").GetComponent<TextMeshProUGUI>();
        expBar = GameObject.Find("HUD/Game/ExpBar/Bar").GetComponent<SlicedFilledImage>();

        //Center
        vignette = GameObject.Find("HUD/Game/Vignette").gameObject;
        deadText = GameObject.Find("HUD/Game/DeadText").GetComponent<TextMeshProUGUI>();

        //Others
        reloading = gameObject.transform.Find("UI/Reloading").gameObject;
        staminaBar = GameObject.Find("HUD/Game/StaminaBar/Bar").GetComponent<SlicedFilledImage>();
        reloading.SetActive(false);
    }

    private IEnumerator FadeToZeroAlpha()
    {
        vignette.GetComponent<CanvasGroup>().alpha = 0.5f;

        while (vignette.GetComponent<CanvasGroup>().alpha > 0.0f)
        {
            vignette.GetComponent<CanvasGroup>().alpha -= (Time.deltaTime / fadeOutTime);
            yield return null;
        }
    }

    public void RefreshBars(int currentHealth, int maxHealth)
    {
        foreach (var heart in hearts)
        {
            heart.gameObject.SetActive(false);
        }

        for (int i = 0; i < maxHealth; i++)
        {
            hearts[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = heartFull;
            }
            else
            {
                hearts[i].sprite = heartEmpty;
            }
        }
    }

    public void RefreshAmmo(int currentAmmo)
    {;
        ammo.text = currentAmmo.ToString();
    }

    public void UpdateLevel(int level, int exp, int requireExp)
    {
        levelText.text = $"Level: {level} ({exp}/{requireExp})";
        float percentage = (float)exp / requireExp;
        expBar.fillAmount = percentage;
    }

    public void UpdateMoney(int money)
    {
        moneyText.text = $"${money}";
    }

    public void ShowVignette()
    {
        StartCoroutine(FadeToZeroAlpha());
    }

    public void ShowDeadText()
    {
        vignette.GetComponent<CanvasGroup>().alpha = 1f;
        deadText.text = "YOU ARE DEAD!";
    }

    public IEnumerator StaminaRestore(float cooldown)
    {
        staminaBar.fillAmount = 0;
        while (staminaBar.fillAmount != 1)
        {
            staminaBar.fillAmount += (Time.deltaTime / cooldown);
            yield return null;
        }
    }
}