using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop Instance;

    [SerializeField] private GameObject shopUI;
    public TextMeshProUGUI shopText;

    private GameObject playerGO;
    private MoneySystem playerMoney;
    private WeaponManager wm;
    private PlayerHUD hud;
    private Player player;

    [SerializeField] private GameObject shopItem;
    [SerializeField] private Transform shopList;
    [SerializeField] private TextMeshProUGUI moneyText;

    [SerializeField] private TextMeshProUGUI warningText;
    IEnumerator warningTextCo = null;

    [SerializeField] private Transform inventoryList;
    [SerializeField] private GameObject inventoryItem;
    [SerializeField] private GameObject armor;
    [SerializeField] private Button exitButton;

    [SerializeField] private Transform itemInfoList;
    [SerializeField] private GameObject itemInfoCard;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerGO = GameObject.FindGameObjectWithTag("Player");
        playerMoney = playerGO.GetComponent<MoneySystem>();
        wm = playerGO.GetComponent<WeaponManager>();
        hud = playerGO.GetComponent<PlayerHUD>();
        player = playerGO.GetComponent<Player>();

        warningText.text = "";
        shopUI.SetActive(false);

        exitButton.onClick.AddListener(delegate { CloseShop(); });

        Refresh();
    }

    void BuyMag(Weapon weapon)
    {
        if(playerMoney.money >= weapon.clipPrice)
        {
            weapon.AddMag();
            playerMoney.TakeMoney(weapon.clipPrice);
            ShopSound();
            Refresh();
            hud.RefreshAmmo(wm.loadout[wm.selectedWeapon].GetClip(), wm.loadout[wm.selectedWeapon].GetAmmo());
        }
        else
        {
            PrintWarning("Not enough money!");
        }
    }

    void BuyFull(Weapon weapon)
    {
        if (playerMoney.money >= weapon.GetRefillPrice())
        {
            playerMoney.TakeMoney(weapon.GetRefillPrice());
            weapon.Refill();
            ShopSound();
            Refresh();
            hud.RefreshAmmo(wm.loadout[wm.selectedWeapon].GetClip(), wm.loadout[wm.selectedWeapon].GetAmmo());
        }
        else
        {
            PrintWarning("Not enough money!");
        }
    }

    void BuyWeapon(Weapon weapon)
    {
        if(wm.loadout[weapon.type] != null)
        {
            PrintWarning("You can have only one primary weapon");
            return;
        }

        if (playerMoney.money >= weapon.startPrice)
        {
            playerMoney.TakeMoney(weapon.startPrice);
            weapon.Initialize();
            wm.loadout[weapon.type] = weapon;
            ShopSound();
            hud.RefreshWeapon(wm.loadout);
            Refresh();
        }
        else
        {
            PrintWarning("Not enough money!");
        }
    }

    void SellWeapon(Weapon weapon, GameObject itemInfo)
    {
        playerMoney.money += weapon.GetSellPrice();
        wm.loadout[weapon.type] = null;
        wm.StartCoroutine("Equip", 1);
        ShopSound();
        Refresh();
        hud.RefreshWeapon(wm.loadout);
        Destroy(itemInfo);
        weapon.Initialize();
    }

    void UpgradeWeapon(Weapon weapon, GameObject infoCard)
    {
        if (playerMoney.money >= weapon.upgradePrices[weapon.GetTier()])
        {
            playerMoney.TakeMoney(weapon.upgradePrices[weapon.GetTier()]);
            weapon.Upgrade();
            ShopSound();
            RefreshCurrentCard(weapon, infoCard);
        }
        else
        {
            PrintWarning("Not enough money!");
        }
    }

    void BuyOrRepairArmor()
    {
        int remainToFullArmor = 5 - player.currentArmor;
        int price = remainToFullArmor * 100;
        if (playerMoney.money >= price)
        {
            player.currentArmor = 5;
            playerMoney.TakeMoney(price);
            hud.RefreshBars(player.currentHealth, player.maxHealth, player.currentArmor);
            ShopSound();
            Refresh();
        }
        else
        {
            PrintWarning("Not enough money!");
        }         
    }

    void Refresh()
    {
        ClearLists();

        moneyText.text = $"Money: {playerMoney.money}$";

        //Armor
        armor.transform.Find("Percent").GetComponent<TextMeshProUGUI>().text = $"{player.currentArmor * 20}%";
        int price = (5 - player.currentArmor) * 100;
        armor.transform.Find("ButtonBuy").GetComponent<Button>().onClick.RemoveAllListeners();
        if (player.currentArmor == 5)
        {
            armor.transform.Find("ButtonBuy").GetComponent<Button>().interactable = false;
            armor.transform.Find("ButtonBuy/Price").GetComponent<TextMeshProUGUI>().text = "0$";
        }
        else if (player.currentArmor <= 0)
        {
            armor.transform.Find("ButtonBuy").GetComponent<Button>().interactable = true;
            armor.transform.Find("ButtonBuy").GetComponent<Button>().onClick.AddListener(delegate { BuyOrRepairArmor(); });
            armor.transform.Find("ButtonBuy/Price").GetComponent<TextMeshProUGUI>().text = $"Buy: {price}$";
        }
        else
        {
            armor.transform.Find("ButtonBuy").GetComponent<Button>().interactable = true;
            armor.transform.Find("ButtonBuy").GetComponent<Button>().onClick.AddListener(delegate { BuyOrRepairArmor(); });
            armor.transform.Find("ButtonBuy/Price").GetComponent<TextMeshProUGUI>().text = $"Repair: {price}$";
        }

        //Inventory List
        foreach (Weapon weapon in wm.loadout)
        {
            if (weapon != null)
            {
                GameObject item = Instantiate(inventoryItem, inventoryList) as GameObject;

                item.transform.Find("Item/Icon").GetComponent<Image>().sprite = weapon.icon;
                item.transform.Find("Item/Name").GetComponent<TextMeshProUGUI>().text = weapon.itemName;
                item.transform.Find("Item/Ammo").GetComponent<TextMeshProUGUI>().text = $"{weapon.GetAmmo()}/{weapon.ammo}";

                if (weapon.FullAmmo())
                {
                    item.transform.Find("Item/ButtonMag").GetComponent<Button>().interactable = false;
                    item.transform.Find("Item/ButtonMag/Price").GetComponent<TextMeshProUGUI>().text = "0$";

                    item.transform.Find("Item/ButtonFill").GetComponent<Button>().interactable = false;
                    item.transform.Find("Item/ButtonFill/Price").GetComponent<TextMeshProUGUI>().text = "0$";
                }
                else
                {
                    item.transform.Find("Item/ButtonMag").GetComponent<Button>().onClick.AddListener(delegate { BuyMag(weapon); });
                    item.transform.Find("Item/ButtonMag/Price").GetComponent<TextMeshProUGUI>().text = weapon.clipPrice.ToString() + "$";

                    item.transform.Find("Item/ButtonFill").GetComponent<Button>().onClick.AddListener(delegate { BuyFull(weapon); });
                    item.transform.Find("Item/ButtonFill/Price").GetComponent<TextMeshProUGUI>().text = weapon.GetRefillPrice().ToString() + "$";
                }

                //item info card
                GameObject infoCard = Instantiate(itemInfoCard, itemInfoList) as GameObject;

                infoCard.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = weapon.itemName;
                infoCard.transform.Find("Image/Icon").GetComponent<Image>().sprite = weapon.icon;
                infoCard.transform.Find("Damage").GetComponent<TextMeshProUGUI>().text = $"Damage: {weapon.GetDamage()}";

                if (weapon.CanStillBeUpgraded())
                {
                    infoCard.transform.Find("ButtonUpgrade").GetComponent<Button>().onClick.AddListener(delegate { UpgradeWeapon(weapon, infoCard); });
                    infoCard.transform.Find("ButtonUpgrade/Price").GetComponent<TextMeshProUGUI>().text = $"Upgrade: {weapon.upgradePrices[weapon.GetTier()]}$";
                }
                else
                {
                    infoCard.transform.Find("ButtonUpgrade/Price").GetComponent<TextMeshProUGUI>().text = "Cannot be upgraded";
                    infoCard.transform.Find("ButtonUpgrade").GetComponent<Button>().interactable = false;              
                }

                if(weapon.canBeSold)
                {
                    infoCard.transform.Find("ButtonSell").GetComponent<Button>().onClick.AddListener(delegate { SellWeapon(weapon, infoCard); });
                    infoCard.transform.Find("ButtonSell/Price").GetComponent<TextMeshProUGUI>().text = $"Sell: {weapon.GetSellPrice()}$";
                }
                else
                {
                    infoCard.transform.Find("ButtonSell/Price").GetComponent<TextMeshProUGUI>().text = "Cannot be sold";
                    infoCard.transform.Find("ButtonSell").GetComponent<Button>().interactable = false;
                }

                infoCard.SetActive(false);
                item.transform.Find("Item").GetComponent<Button>().onClick.AddListener(delegate { ShowItemInfo(infoCard); });
            }
        }

        //Shop List
        foreach (Weapon weapon in WeaponLibrary.weapons)
        {
            bool add = true;

            for (int i = 0; i < wm.loadout.Length; i++)
            {
                if (wm.loadout[i] != null && weapon.itemName == wm.loadout[i].itemName)
                {
                    add = false;
                    break;
                }
            }

            if(add==true)
            {
                GameObject newItem = Instantiate(shopItem, shopList) as GameObject;

                newItem.transform.Find("Icon").GetComponent<Image>().sprite = weapon.icon;
                newItem.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = weapon.itemName;
                newItem.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = weapon.startPrice.ToString() + "$";

                newItem.GetComponent<Button>().onClick.AddListener(delegate { BuyWeapon(weapon); });
            }    
        }
    }

    void ClearLists()
    {
        for(int i = 0; i<shopList.childCount;i++)
        {
            Destroy(shopList.GetChild(i).gameObject);
        }

        for (int i = 1; i < inventoryList.childCount; i++)
        {
            Destroy(inventoryList.GetChild(i).gameObject);
        }

        for(int i = 0; i < itemInfoList.childCount; i++)
        {
            Destroy(itemInfoList.GetChild(i).gameObject);
        }
    }

    void RefreshCurrentCard(Weapon weapon, GameObject infoCard)
    {
        moneyText.text = $"Money: {playerMoney.money}$";

        infoCard.transform.Find("Damage").GetComponent<TextMeshProUGUI>().text = $"Damage: {weapon.GetDamage()}";

        if (weapon.CanStillBeUpgraded())
        {
            infoCard.transform.Find("ButtonUpgrade/Price").GetComponent<TextMeshProUGUI>().text = $"Upgrade: {weapon.upgradePrices[weapon.GetTier()]}$";
        }
        else
        {
            infoCard.transform.Find("ButtonUpgrade/Price").GetComponent<TextMeshProUGUI>().text = "Cannot be upgraded";
            infoCard.transform.Find("ButtonUpgrade").GetComponent<Button>().interactable = false;
        }

        if (weapon.canBeSold)
        {
            infoCard.transform.Find("ButtonSell/Price").GetComponent<TextMeshProUGUI>().text = $"Sell: {weapon.GetSellPrice()}$";
        }
        else
        {
            infoCard.transform.Find("ButtonSell/Price").GetComponent<TextMeshProUGUI>().text = "Cannot be sold";
            infoCard.transform.Find("ButtonSell").GetComponent<Button>().interactable = false;
        }
    }

    void ShowItemInfo(GameObject itemInfo)
    {
        for(int i = 0; i<itemInfoList.childCount; i++)
        {
            itemInfoList.GetChild(i).gameObject.SetActive(false);
        }

        itemInfo.SetActive(true);
    }
    
    void PrintWarning(string text)
    {
        if (warningTextCo != null) StopCoroutine(warningTextCo);

        warningTextCo = HideWarningText();
        warningText.text = text;

        StartCoroutine(warningTextCo);
    }

    IEnumerator HideWarningText()
    {
        yield return new WaitForSeconds(3f);

        warningText.text = "";
        yield break;
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);

        player.Control(true);
    }

    public void OpenShop()
    {
        Refresh();

        shopUI.SetActive(true);

        player.Control(false);
    }

    void ShopSound()
    {
        SoundManager.Instance.PlayOneShot("Purchase");
    }
}
