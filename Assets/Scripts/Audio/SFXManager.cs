using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] private SoundsSO _so;
    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1f)
    {
        if (Instance == null || Instance._so == null) return;

        SoundList soundList = Instance._so.sounds[(int)sound];
        AudioClip[] clips = soundList.sounds;
        if (clips == null || clips.Length == 0) return;

        AudioClip randomClip = clips[Random.Range(0, clips.Length)];

        if (source != null)
        {
            source.outputAudioMixerGroup = soundList.mixer;
            source.clip = randomClip;
            source.volume = volume * soundList.volume;
            source.Play();
        }
        else
        {
            Instance._audioSource.outputAudioMixerGroup = soundList.mixer;
            Instance._audioSource.PlayOneShot(randomClip, volume * soundList.volume);
        }
    }

    public void Pause()
    {
        if (_audioSource.isPlaying)
            _audioSource.Pause();
    }

    public void Unpause()
    {
        if (_audioSource.clip != null && !_audioSource.isPlaying)
            _audioSource.UnPause();
    }
}