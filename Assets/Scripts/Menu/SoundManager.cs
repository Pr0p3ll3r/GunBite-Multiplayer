using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public AudioMixerGroup mixer;
    [Range(0f, 1f)] public float volume;
    [Range(0.1f, 3f)] public float pitch;

    public bool loop;

    [HideInInspector] public AudioSource audioSource;
}

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;
    public static SoundManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
 
        DontDestroyOnLoad(gameObject);

        foreach(Sound s in sounds)
        {
            s.audioSource = gameObject.AddComponent<AudioSource>();
            s.audioSource.clip = s.clip;

            s.audioSource.volume = s.volume;
            s.audioSource.pitch = s.pitch;
            s.audioSource.loop = s.loop;
            s.audioSource.outputAudioMixerGroup = s.mixer;
        }
    }

    private Sound FindSound(string name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == name)
                return sounds[i];
        }
        return null;
    }

    public void Play(string name)
    {
        Sound s = FindSound(name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        s.audioSource.Play();
    }

    public void PlayOneShot(string name)
    {
        Sound s = FindSound(name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        s.audioSource.PlayOneShot(s.clip);
    }
}