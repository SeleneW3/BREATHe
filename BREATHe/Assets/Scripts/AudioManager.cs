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

    // 添加音效名称常量
    public static class SoundNames
    {
        // 玩家相关音效
        public const string JUMP_1 = "Jump1";
        public const string JUMP_2 = "Jump2";
        public const string JUMP_3 = "Jump3";
        public const string JUMP_4 = "Jump4";
        public const string JUMP_5 = "Jump5";

        // 碰撞音效
        public const string BOUNCE_1 = "bounce1";
        public const string BOUNCE_2 = "bounce2";
        public const string BOUNCE_3 = "bounce3";
        public const string BOUNCE_4 = "bounce4";

        // 收集物品音效
        public const string FOOD_COLLECT = "Pickup";     // Food收集音效
        public const string DIAMOND_COLLECT = "Collect"; // Diamond收集音效
        public const string LINE_HIT = "LineHit";       // 碰到Line的音效

        // 其他音效
        public const string DEATH = "Death";
        public const string WIND = "Wind";
    }

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
        if (s == null)
        {
            Debug.LogWarning($"背景音乐: {name} 未找到!");
            return;
        }

        StopAllCoroutines();  // 停止所有正在进行的渐变
        StartCoroutine(FadeIn(musicSource, s, duration));
    }

    // 淡出背景音乐
    public void FadeOutMusic(float duration)
    {
        if (musicSource.isPlaying)
        {
            StopAllCoroutines();  // 停止所有正在进行的渐变
            StartCoroutine(FadeOut(musicSource, duration));
        }
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
        // 确保音乐源存在且未在播放
        if (musicSource != null && !musicSource.isPlaying)
        {
            // 淡入播放背景音乐
            FadeInMusic("BGM1", 2f);
        }
    }

    // 在死亡时调用此方法
    public void HandlePlayerDeath()
    {
        // 淡出背景音乐
        FadeOutMusic(1f);
        // 播放死亡音效
        PlaySound(SoundNames.DEATH);

        // 停止所有可能正在播放的循环音效
        StopSound(SoundNames.WIND);  // 停止风声
        
        // 停止所有跳跃音效
        for (int i = 1; i <= 5; i++)
        {
            StopSound($"Jump{i}");
        }
        
        // 停止所有弹跳音效
        for (int i = 1; i <= 4; i++)
        {
            StopSound($"bounce{i}");
        }
    }

    // 在重新开始时调用此方法
    public void HandleGameRestart()
    {
        // 停止所有当前音效
        foreach (var source in effectSources.Values)
        {
            source.Stop();
        }

        // 重置音源状态
        foreach (Sound s in soundEffects)
        {
            if (effectSources.TryGetValue(s.name, out AudioSource source))
            {
                source.volume = s.volume;
                source.pitch = s.pitch;
            }
        }

        // 重新开始播放背景音乐
        if (musicSource != null)
        {
            musicSource.Stop();  // 确保先停止当前音乐
            PlayBackgroundMusic();
        }
    }

    // 播放收集音效
    public void PlayCollectSound(string soundName)
    {
        PlaySound(soundName);
    }

    // 播放碰撞音效
    public void PlayCollisionSound(string soundName)
    {
        PlaySound(soundName);
    }

    // 循环播放跳跃音效
    private int currentJumpIndex = 0;
    private float lastJumpTime = 0f;
    private const float JUMP_SOUND_INTERVAL = 1f;

    public void PlayJumpSound()
    {
        float currentTime = Time.time;
        if (currentTime - lastJumpTime > JUMP_SOUND_INTERVAL)
        {
            currentJumpIndex = 0;
        }

        string[] jumpSounds = new string[] 
        { 
            SoundNames.JUMP_1, 
            SoundNames.JUMP_2, 
            SoundNames.JUMP_3, 
            SoundNames.JUMP_4, 
            SoundNames.JUMP_5 
        };

        PlaySound(jumpSounds[currentJumpIndex]);
        currentJumpIndex = (currentJumpIndex + 1) % jumpSounds.Length;
        lastJumpTime = currentTime;
    }

    // 循环播放弹跳音效
    private int currentBounceIndex = 0;
    public void PlayBounceSound()
    {
        string[] bounceSounds = new string[] 
        { 
            SoundNames.BOUNCE_1, 
            SoundNames.BOUNCE_2, 
            SoundNames.BOUNCE_3, 
            SoundNames.BOUNCE_4 
        };

        PlaySound(bounceSounds[currentBounceIndex]);
        currentBounceIndex = (currentBounceIndex + 1) % bounceSounds.Length;
    }
}
