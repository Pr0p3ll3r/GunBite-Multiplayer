using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Sprite shieldFull;
    [SerializeField] private Sprite shieldEmpty;
    [SerializeField] private Sprite emptyIcon;
    [SerializeField] private float fadeOutTime = 4f;

    [HideInInspector] public GameObject reloading;

    private List<Image> hearts = new List<Image>();
    private GameObject armorBar;
    private Image[] shields;
    private SlicedFilledImage staminaBar;
    private TextMeshProUGUI ammo;
    private TextMeshProUGUI levelText;
    private SlicedFilledImage expBar;
    private TextMeshProUGUI moneyText;
    private GameObject vignette;
    private TextMeshProUGUI deadText;
    private Transform weaponParent;

    private void Awake()
    {
        InitializeUI();
    }

    private void Start()
    {
        foreach (var heart in hearts)
        {
            heart.gameObject.SetActive(false);
        }
    }

    void InitializeUI()
    {
        //Bottom Left
        moneyText = GameObject.Find("HUD/Game/BottomLeftCorner/Money").GetComponent<TextMeshProUGUI>();
        hearts.AddRange(GameObject.Find("HUD/Game/BottomLeftCorner/HealthBar").GetComponentsInChildren<Image>());
        armorBar = GameObject.Find("HUD/Game/BottomLeftCorner/ArmorBar");
        shields = GameObject.Find("HUD/Game/BottomLeftCorner/ArmorBar").GetComponentsInChildren<Image>();

        //Bottom Right   
        ammo = GameObject.Find("HUD/Game/BottomRightCorner/Ammo/Amount").GetComponent<TextMeshProUGUI>();
        weaponParent = GameObject.Find("HUD/Game/BottomRightCorner/Weapons").transform;

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

    public void RefreshBars(int currentHealth, int maxHealth, int currentArmor)
    {
        //health
        //show heart containers
        for (int i = 0; i < maxHealth / 2; i++)
        {
            hearts[i].gameObject.SetActive(true);
        }

        //set sprite
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHealth / 2)
            {
                hearts[i].sprite = heartFull;
            }
            else
            {
                hearts[i].sprite = heartEmpty;
            }
        }

        if (currentHealth % 2 != 0)
        {
            hearts[currentHealth / 2].sprite = heartHalf;
        }

        //armor
        if (currentArmor > 0)
        {
            armorBar.SetActive(true);
        }
        else
        {
            armorBar.SetActive(false);
        }

        for (int i = 0; i < shields.Length; i++)
        {
            if (i < currentArmor)
            {
                shields[i].sprite = shieldFull;
            }
            else
            {
                shields[i].sprite = shieldEmpty;
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

    public void RefreshWeapon(Weapon[] weapons)
    {
        foreach (Transform weapon in weaponParent)
        {
            Image icon = weapon.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI name = weapon.GetChild(1).GetComponent<TextMeshProUGUI>();
            icon.sprite = emptyIcon;
            name.text = "";
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null) continue;

            Image icon = weaponParent.GetChild(i).GetChild(0).GetComponent<Image>();
            TextMeshProUGUI name = weaponParent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>();
            icon.sprite = weapons[i].icon;
            name.text = weapons[i].itemName;
        }
    }

    public void SelectWeapon(int index)
    {
        foreach (Transform weapon in weaponParent)
        {
            Image background = weapon.GetComponent<Image>();
            background.color = new Color32(0, 0, 0, 102);
        }

        weaponParent.GetChild(index).GetComponent<Image>().color = new Color32(255, 255, 255, 102);
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