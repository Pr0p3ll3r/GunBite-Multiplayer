using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Enemy
{
	public string name;
	public int amount;
	public GameObject prefab;
}

[System.Serializable]
public class Wave
{
	public string name = "Wave";
	public Enemy[] enemies;
	public float spawnDelay;
	public bool boss;
}

public class WaveManager : MonoBehaviour
{
	public enum SpawnState 
	{
		SPAWNING, 
		WAITING, 
		COUNTING,
		SHOPTIME
	};

	[Header("Spawn Management")]
	[SerializeField] private Wave[] waves;
	private int currentWave = 1;
	[SerializeField] private Transform[] spawnPoints;
	[SerializeField] private Transform[] spawnPointsBossPlant;
	public GameObject bigHeadPrefab;

	[Header("Start Values")]
	[SerializeField] private int startHealth = 100;
	[SerializeField] private float startMoveSpeed = 1f;
	[SerializeField] private int startEXP = 3;
	[SerializeField] private int startMoney = 4;

	[Header("Current Wave")]
	[SerializeField] private int currentHealth;
	[SerializeField] private float currentMoveSpeed;
	[SerializeField] private int currentEXP;
	[SerializeField] private int currentMoney;

	[SerializeField] private TextMeshProUGUI wave;
	private GameManager gm;

	private int currentZombies;
	private float makeSound;
	private int currentWaveUI = 1;

	private SpawnState state = SpawnState.COUNTING;

	void Start()
	{
		gm = GetComponent<GameManager>();
		currentHealth = startHealth;
		currentMoveSpeed = startMoveSpeed;
		currentEXP = startEXP;
		currentMoney = startMoney;
		wave.text = $"{currentWaveUI}/?";
	}

	void Update()
	{
		if (state == SpawnState.SHOPTIME) return;

        if (state == SpawnState.WAITING)
        {
			if (currentZombies == 0)
			{
				StartCoroutine(WaveCompleted()); 
			}
			else
			{
				//Zombie Sound
				if (makeSound <= 0)
				{
					SoundManager.Instance.Play("Zombie");
					makeSound = 5f;
				}

				makeSound -= Time.deltaTime;
			}
			return;
		}

		if (state != SpawnState.SPAWNING)
		{
			StartCoroutine(NewWave(waves[currentWave-1]));
		}
	}

	IEnumerator NewWave(Wave _wave)
	{
		state = SpawnState.SPAWNING;
		
		if(_wave.boss)
        {
			SoundManager.Instance.Play("BossApear");
			switch (_wave.name)
            {
				case "BossPlant":
					foreach (Transform spawnPoint in spawnPointsBossPlant)
					{
						Instantiate(_wave.enemies[0].prefab, spawnPoint.position, spawnPoint.rotation);

						currentZombies++;
					}
					break;
				case "BossHead":
					Transform s = spawnPoints[Random.Range(0, spawnPoints.Length)];
					Instantiate(_wave.enemies[0].prefab, s.position, s.rotation);
					currentZombies++;
					break;
            }
		}
		else
        {
			foreach (Enemy enemy in _wave.enemies)
			{
				for (int i = 0; i < enemy.amount; i++)
				{
					if (Player.Instance == null)
						break;
					SpawnEnemy(enemy.prefab);
					yield return new WaitForSeconds(_wave.spawnDelay);
				}
			}
		}

		state = SpawnState.WAITING;

		yield break;
	}

	IEnumerator WaveCompleted()
    {
		Debug.Log("Wave Completed!");

		state = SpawnState.SHOPTIME;

		if (Player.Instance.GetComponent<LevelSystem>().TimeToUpgrade())
		{
			yield return new WaitUntil(() => GameManager.Instance.isShopTime = true);
		}
		else
		{
			GameManager.Instance.isShopTime = true;
			Shop.Instance.shopText.text = "Press F to open the Shop";
		}

		while (gm.isShopTime)
		{
			yield return null;
		} 
		
		if(!gm.isShopTime)
        {
			if (currentWave != waves.Length)
			{
				currentWave++;
			}

			currentWaveUI++;
			state = SpawnState.COUNTING;
			Shop.Instance.shopText.text = "";
			Shop.Instance.CloseShop();
			wave.text = $"{currentWaveUI}/?";
			gm.shopTime = 15;
			UpgradeEnemy();
			Player.Instance.RefillHealth();
			SoundManager.Instance.Play("StartGame");
		}
		yield break;
	}

	void SpawnEnemy(GameObject enemy)
    {
		Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

		GameObject zombieGO = Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);

		Zombie zombie = zombieGO.GetComponent<Zombie>();
		zombie.maxHealth = currentHealth;
		zombie.exp = currentEXP;
		zombie.money = currentMoney;
		zombie.speed = currentMoveSpeed;

		currentZombies++;
	}

	void UpgradeEnemy()
	{
		Debug.Log("ENEMY UPGRADED");

		currentHealth += 5;

		if (currentMoveSpeed < 6.0f)
		{
			currentMoveSpeed += 0.4f;
		}

		currentEXP++;
		currentMoney += 6;
	}

	public void ZombieKilled(int amount)
    {
		currentZombies += amount;
    }
}
