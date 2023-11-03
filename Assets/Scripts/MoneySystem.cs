using UnityEngine;

public class MoneySystem : MonoBehaviour
{
	public int money = 0;

	private PlayerHUD hud;

	void Start()
	{
		hud = GetComponent<PlayerHUD>();
		hud.UpdateMoney(money);
	}

	public void GetMoney(int amount)
	{
		money += amount;
		hud.UpdateMoney(money);
	}

	public void TakeMoney(int amount)
	{
		money -= amount;
		hud.UpdateMoney(money);
	}
}
