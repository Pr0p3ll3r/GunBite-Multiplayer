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
    [SerializeField] private Image[] hearts;
    [SerializeField] private GameObject armorBar;
    [SerializeField] private Image[] shields;
    [SerializeField] private SlicedFilledImage staminaBar;
    [SerializeField] private TextMeshProUGUI clipSize;
    [SerializeField] private TextMeshProUGUI ammo;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private SlicedFilledImage expBar;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject vignette;
    [SerializeField] private TextMeshProUGUI deadText;
    [SerializeField] private Transform weaponParent;

    private void Awake()
    {
        foreach (Image heart in hearts)
        {
            heart.gameObject.SetActive(false);
        }
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
        int amount = maxHealth % 2 == 0 ? maxHealth/2 : maxHealth/2 + 1;
        for (int i = 0; i < amount; i++)
        {
            hearts[i].gameObject.SetActive(true);
        }

        //set sprite
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth/2)
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
            hearts[currentHealth/2].sprite = heartHalf;
        }

        //armor
        if(currentArmor > 0)
        {
            armorBar.SetActive(true);
        }
        else
        {
            armorBar.SetActive(false);
        }

        for (int i = 0; i < shields.Length; i++)
        {
            if(i < currentArmor)
            {
                shields[i].sprite = shieldFull;
            }
            else
            {
                shields[i].sprite = shieldEmpty;
            }
        }
    }

    public void RefreshAmmo(int clip, int currentAmmo)
    {
        clipSize.text = clip.ToString();
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