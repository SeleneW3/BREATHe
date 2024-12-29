using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }

    public Sound[] backgroundMusic;  // 背景音乐数组
    public Sound[] soundEffects;     // 音效数组

    private AudioSource musicSource;  // 背景音乐播放器
    private Dictionary<string, AudioSource> effectSources = new Dictionary<string, AudioSource>();  // 音效播放器字典

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 创建背景音乐播放器
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        // 初始化所有音效
        foreach (Sound s in soundEffects)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            s.source = source;
            source.clip = s.clip;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.loop = s.loop;
            effectSources[s.name] = source;
        }
    }

    private void Start()
    {
        // 游戏开始时播放背景音乐
        PlayBackgroundMusic();
    }

    // 播放背景音乐
    public void PlayMusic(string name)
    {
        Sound s = System.Array.Find(backgroundMusic, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"背景音乐: {name} 未找到!");
            return;
        }

        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.pitch = s.pitch;
        musicSource.Play();
    }

    // 播放音效
    public void PlaySound(string name)
    {
        if (effectSources.TryGetValue(name, out AudioSource source))
        {
            source.Play();
        }
        else
        {
            Debug.LogWarning($"音效: {name} 未找到!");
        }
    }

    // 停止背景音乐
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // 停止特定音效
    public void StopSound(string name)
    {
        if (effectSources.TryGetValue(name, out AudioSource source))
        {
            source.Stop();
        }
    }

    // 设置背景音乐音量
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    // 设置音效音量
    public void SetSoundVolume(float volume)
    {
        foreach (var source in effectSources.Values)
        {
            source.volume = Mathf.Clamp01(volume);
        }
    }

    // 淡入背景音乐
    public void FadeInMusic(string name, float duration)
    {
        Sound s = System.Array.Find(backgroundMusic, sound => sound.name == name);
        if (s == null) return;

        StartCoroutine(FadeIn(musicSource, s, duration));
    }

    // 淡出背景音乐
    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeOut(musicSource, duration));
    }

    private System.Collections.IEnumerator FadeIn(AudioSource source, Sound sound, float duration)
    {
        source.clip = sound.clip;
        source.volume = 0f;
        source.Play();

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            source.volume = Mathf.Lerp(0, sound.volume, (Time.time - startTime) / duration);
            yield return null;
        }
        source.volume = sound.volume;
    }

    private System.Collections.IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0, (Time.time - startTime) / duration);
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
    }

    // 播放背景音乐
    private void PlayBackgroundMusic()
    {
        // 淡入播放背景音乐
        FadeInMusic("BGM1", 2f);
    }

    // 在死亡时调用此方法
    public void HandlePlayerDeath()
    {
        // 淡出背景音乐
        FadeOutMusic(1f);
    }

    // 在重新开始时调用此方法
    public void HandleGameRestart()
    {
        // 重新开始播放背景音乐
        PlayBackgroundMusic();
    }
}
