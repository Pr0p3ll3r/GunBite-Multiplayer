using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    private int requireExp = 60;
    public int level;
    public int exp;
    public GameObject upgradeUI;
    [SerializeField] private Button healthUpgrade;
    [SerializeField] private Button speedUpgrade;
    [SerializeField] private Button dashPowerUpgrade;
    [SerializeField] private Button dashCooldownUpgrade;

    private PlayerHUD hud;
    private int upgrade = 0;
    private bool chosen = false;
    Player player;

    private void Start()
    {
        hud = GetComponent<PlayerHUD>();
        player = Player.Instance;
        hud.UpdateLevel(level, exp, requireExp);
        //upgradeUI.SetActive(false);
        //healthUpgrade.onClick.AddListener(delegate { UpgradeHealth(); });
        //speedUpgrade.onClick.AddListener(delegate { UpgradeSpeed(); });
        //dashPowerUpgrade.onClick.AddListener(delegate { UpgradeDashPower(); });
        //dashCooldownUpgrade.onClick.AddListener(delegate { UpgradeDashCooldown(); });
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
            exp -= requireExp;
            requireExp += 30;
            level++;
            upgrade++;
            CheckLevelUp();
        }

        hud.UpdateLevel(level, exp, requireExp);
    }

    IEnumerator ChooseUpgrade()
    {
        upgradeUI.SetActive(true);
        yield return new WaitUntil(() => chosen);
        chosen = false;
        upgradeUI.SetActive(false);
        if (TimeToUpgrade() == true)
            StartCoroutine(ChooseUpgrade());
    }

    public bool TimeToUpgrade()
    {
        if (upgrade > 0)
        {
            upgrade--;
            Player.Instance.Control(false);
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
        player.RefillHealth();
        if (player.maxHealth == 20)
            healthUpgrade.gameObject.SetActive(false);
    }

    void UpgradeSpeed()
    {
        chosen = true;
        player.GetComponent<PlayerController>().moveSpeed += 1;
    }

    void UpgradeDashPower()
    {
        chosen = true;
        player.GetComponent<PlayerController>().dashDistance += 10;
        if (player.GetComponent<PlayerController>().dashDistance == 100)
            dashPowerUpgrade.gameObject.SetActive(false);
    }

    void UpgradeDashCooldown()
    {
        chosen = true;
        player.GetComponent<PlayerController>().dashDistance -= 1;
        if (player.GetComponent<PlayerController>().dashCooldown == 1)
            dashCooldownUpgrade.gameObject.SetActive(false);
    }
}
