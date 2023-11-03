using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;

public class OptionsMenu : MonoBehaviour
{
	[Header("MIXER")]
	[SerializeField] private AudioMixer masterMixer;

	[Header("Panels")]
	[SerializeField] private GameObject panelVideo;
	[SerializeField] private GameObject panelGame;
	[SerializeField] private GameObject panelAudio;

	[Header("GAME SETTINGS")]
	[SerializeField] private Toggle showFpsToggle;

	[Header("VIDEO SETTINGS")]
	[SerializeField] private Toggle fullscreenToggle;
	[SerializeField] private Toggle vsyncToggle;
	[SerializeField] private TMP_Dropdown resolutionDropdown;

	private Resolution[] resolutions;
	private List<string> Options = new List<string>();

	[Header("AUDIO SETTINGS")]
	[SerializeField] private GameObject musicSlider;
	[SerializeField] private GameObject sfxSlider;
	[SerializeField] private GameObject uiSlider;

    private void Awake()
    {
        if (PlayerPrefs.GetInt("FirstRun") == 0)
        {
			PlayerPrefs.SetInt("FirstRun", 1);
			PlayerPrefs.SetInt("FPS", 0);
			PlayerPrefs.SetInt("Extra", 0);
			PlayerPrefs.SetFloat("Music", 0.5f);
			PlayerPrefs.SetFloat("SFX", 0.5f);
			PlayerPrefs.SetFloat("UI", 0.5f);
		}
    }

    public void Start()
	{
		resolutions = Screen.resolutions;

		int currentResolutionIndex = 0;
		int x = 0;

		int lw = -1;
		int lh = -1;

		foreach (var res in resolutions)
		{
			if (lw != res.width || lh != res.height)
			{
				string option = res.width + " x " + res.height;
				Options.Add(option);
				lw = res.width;
				lh = res.height;

				if (lw == Screen.currentResolution.width &&
				lh == Screen.currentResolution.height)
				{
					currentResolutionIndex = x;
				}

				x++;
			}
		}

		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(Options);
		resolutionDropdown.value = currentResolutionIndex;

        #region PlayerPrefs

        //check resolution
        if (PlayerPrefs.HasKey("Resolution"))
		{
			int resolutionIndex = PlayerPrefs.GetInt("Resolution");
			string resolution = Options[resolutionIndex];
			int ind = resolution.IndexOf('x');
			int width = int.Parse(resolution.Substring(0, ind - 1));
			int height = int.Parse(resolution.Substring(ind + 1));
			Screen.SetResolution(width, height, Screen.fullScreen);
			resolutionDropdown.value = resolutionIndex;
		}

		// check full screen
		if (Screen.fullScreen == true)
		{
			fullscreenToggle.isOn = true;
		}
		else if (Screen.fullScreen == false)
		{
			fullscreenToggle.isOn = false;
		}

		if (PlayerPrefs.HasKey("Music"))
		{
			float volume;
			volume = PlayerPrefs.GetFloat("Music");
			masterMixer.SetFloat("musicVolume", volume);
			musicSlider.GetComponent<Slider>().value = volume;
		}

		if (PlayerPrefs.HasKey("SFX"))
		{
			float volume;
			volume = PlayerPrefs.GetFloat("SFX");
			masterMixer.SetFloat("sfxVolume", volume);
			sfxSlider.GetComponent<Slider>().value = volume;
		}

		if (PlayerPrefs.HasKey("UI"))
		{
			float volume;
			volume = PlayerPrefs.GetFloat("UI");
			masterMixer.SetFloat("uiVolume", volume);
			uiSlider.GetComponent<Slider>().value = volume;
		}

		//check fps value
		if (PlayerPrefs.GetInt("FPS") == 0)
		{
			showFpsToggle.SetIsOnWithoutNotify(false);
		}
		else
		{
			showFpsToggle.SetIsOnWithoutNotify(true);
		}

		// check vsync
		if (QualitySettings.vSyncCount == 0)
		{
			fullscreenToggle.SetIsOnWithoutNotify(false);
		}
		else if (QualitySettings.vSyncCount == 1)
		{
			fullscreenToggle.SetIsOnWithoutNotify(true);
		}
        #endregion
    }

    #region Panels
    public void GamePanel()
	{
		panelVideo.SetActive(false);
		panelGame.SetActive(true);
		panelAudio.SetActive(false);
	}

	public void VideoPanel()
	{
		panelVideo.SetActive(true);
		panelGame.SetActive(false);
		panelAudio.SetActive(false);
	}

	public void AudioPanel()
	{
		panelVideo.SetActive(false);
		panelGame.SetActive(false);
		panelAudio.SetActive(true);
	}
	#endregion

	#region PanelGame

	public void ShowFPS()
	{
		if (PlayerPrefs.GetInt("FPS") == 0)
		{
			PlayerPrefs.SetInt("FPS", 1);
			showFpsToggle.SetIsOnWithoutNotify(true);
		}
		else if (PlayerPrefs.GetInt("FPS") == 1)
		{
			PlayerPrefs.SetInt("FPS", 0);
			showFpsToggle.SetIsOnWithoutNotify(false);
		}
	}

	#endregion

	#region PanelVideo

	public void FullScreen()
	{
		Screen.fullScreen = !Screen.fullScreen;

		fullscreenToggle.SetIsOnWithoutNotify(!Screen.fullScreen);
	}

	public void SetResolution(int resolutionIndex)
	{
		string resolution = Options[resolutionIndex];
		int ind = resolution.IndexOf('x');
		int width = int.Parse(resolution.Substring(0, ind - 1));
		int height = int.Parse(resolution.Substring(ind + 1));
		Screen.SetResolution(width, height, Screen.fullScreen);
		PlayerPrefs.SetInt("Resolution", resolutionIndex);
	}

	public void SetQuality(int qualityIndex)
	{
		QualitySettings.SetQualityLevel(qualityIndex);
		PlayerPrefs.SetInt("Quality", qualityIndex);
	}

	public void Vsync()
	{
		if (QualitySettings.vSyncCount == 0)
		{
			QualitySettings.vSyncCount = 1;
			vsyncToggle.SetIsOnWithoutNotify(true);
		}
		else if (QualitySettings.vSyncCount == 1)
		{
			QualitySettings.vSyncCount = 0;
			vsyncToggle.SetIsOnWithoutNotify(false);
		}
	}

	#endregion

	#region PanelAudio

	public void SetMusicVolume()
    {
		float volume = musicSlider.GetComponent<Slider>().value;
        masterMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
		PlayerPrefs.SetFloat("Music", volume);
    }

    public void SetSFXVolume()
    {
		float volume = sfxSlider.GetComponent<Slider>().value;
		masterMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
		PlayerPrefs.SetFloat("SFX", volume);
    }

    public void SetUIVolume()
    {
		float volume = uiSlider.GetComponent<Slider>().value;
		masterMixer.SetFloat("uiVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("UI", volume);
    }

    #endregion

	public void PlayClick()
	{
		SoundManager.Instance.PlayOneShot("Click");
	}

	public void PlayHover()
	{
		SoundManager.Instance.Play("Hover");
	}
}
