using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    [SerializeField] private int requireExp = 60;
    [SerializeField] private int level;
    [SerializeField] private int exp;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button healthUpgrade;
    [SerializeField] private Button speedUpgrade;
    [SerializeField] private Button dashPowerUpgrade;
    [SerializeField] private Button dashCooldownUpgrade;

    private PlayerHUD hud;
    private int upgrade = 0;
    private bool chosen = false;
    private Player player;
    private PlayerController controller;

    private void Start()
    {
        hud = GetComponent<PlayerHUD>();
        player = Player.Instance;
        controller = GetComponent<PlayerController>();
        hud.UpdateLevel(level, exp, requireExp);
        upgradeUI.SetActive(false);
        healthUpgrade.onClick.AddListener(delegate { UpgradeHealth(); });
        speedUpgrade.onClick.AddListener(delegate { UpgradeSpeed(); });
        dashPowerUpgrade.onClick.AddListener(delegate { UpgradeDashPower(); });
        dashCooldownUpgrade.onClick.AddListener(delegate { UpgradeDashCooldown(); });
    }

    public void GetExp(int amount)
    {
        exp += amount;

        CheckLevelUp();
    }

    void CheckLevelUp()
    {
        if (exp >= requireExp)
        {
            exp = exp - requireExp;
            requireExp += 30;
            level++;
            if(level < 26)
                upgrade++;
            CheckLevelUp();
        }

        hud.UpdateLevel(level, exp, requireExp);
    }

    IEnumerator ChooseUpgrade()
    {
        upgradeUI.SetActive(true);
        yield return new WaitUntil (() => chosen);
        chosen = false;
        upgradeUI.SetActive(false);
        if (TimeToUpgrade() == true)
            StartCoroutine(ChooseUpgrade());
        else
        {
            Player.Instance.Control(true);
            GameManager.Instance.TimerStatus(true);
            GameManager.Instance.isShopTime = true;
            Shop.Instance.shopText.text = "Press F to open the Shop";
        }         
    }

    public bool TimeToUpgrade()
    {
        if(upgrade > 0)
        {
            upgrade--;
            Player.Instance.Control(false);
            GameManager.Instance.TimerStatus(false);
            StartCoroutine(ChooseUpgrade());
            return true;
        }
        return false;    
    }

    void UpgradeHealth()
    {       
        chosen = true;
        player.maxHealth += 1;
        hud.RefreshBars(player.currentHealth, player.maxHealth, player.currentArmor);
        if (player.maxHealth == 10)
            healthUpgrade.gameObject.SetActive(false);
    }

    void UpgradeSpeed()
    {
        chosen = true;
        controller.moveSpeed += 1;
        if (controller.moveSpeed == 10)
            speedUpgrade.gameObject.SetActive(false);
    }

    void UpgradeDashPower()
    {
        chosen = true;
        controller.dashDistance += 10;
        if (controller.dashDistance == 100)
            dashPowerUpgrade.gameObject.SetActive(false);
    }

    void UpgradeDashCooldown()
    {
        chosen = true;
        controller.dashCooldown -= 1;
        if (controller.dashCooldown == 1)
            dashCooldownUpgrade.gameObject.SetActive(false);
    }
}
