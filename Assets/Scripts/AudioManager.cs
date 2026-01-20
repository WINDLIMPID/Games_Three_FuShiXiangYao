using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundData
{
    public string name;      // 音效名字 (例如 "Attack", "Click")
    public AudioClip clip;   // 音频文件
    [Range(0f, 1f)]
    public float volume = 1f;// 策划配置的基础音量
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("=== 播放器组件 ===")]
    public AudioSource musicSource; // 专门播 BGM
    public AudioSource sfxSource;   // 专门播 音效

    [Header("=== 音频库配置 ===")]
    public List<SoundData> soundLibrary;
    private Dictionary<string, SoundData> _soundDict;

    // 🔥 两个独立的全局倍率 (0 - 2.0)，默认值为 1.0
    [HideInInspector] public float musicVolumeFactor = 1f;
    [HideInInspector] public float sfxVolumeFactor = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 💾 1. 从本地加载音量设置
            musicVolumeFactor = PlayerPrefs.GetFloat("MusicVolumeFactor", 1f);
            sfxVolumeFactor = PlayerPrefs.GetFloat("SFXVolumeFactor", 1f);

            InitializeLibrary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeLibrary()
    {
        _soundDict = new Dictionary<string, SoundData>();
        foreach (var sound in soundLibrary)
        {
            if (!string.IsNullOrEmpty(sound.name) && sound.clip != null)
            {
                if (!_soundDict.ContainsKey(sound.name))
                    _soundDict.Add(sound.name, sound);
            }
        }
    }

    // ==========================================
    // 🎵 播放 BGM (应用独立音乐倍率)
    // ==========================================
    public void PlayMusic(string name)
    {
        if (_soundDict.TryGetValue(name, out SoundData sound))
        {
            if (musicSource.clip == sound.clip && musicSource.isPlaying) return;

            musicSource.clip = sound.clip;
            // 核心公式：基础音量 * 全局音乐倍率
            musicSource.volume = Mathf.Clamp(sound.volume * musicVolumeFactor, 0f, 1f);
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // ⚙️ 实时更新正在播放的 BGM 音量 (给滑动条实时反馈用)
    public void UpdateLiveMusicVolume()
    {
        if (musicSource.clip != null)
        {
            SoundData sound = soundLibrary.Find(s => s.clip == musicSource.clip);
            float baseVol = (sound != null) ? sound.volume : 1f;
            musicSource.volume = Mathf.Clamp(baseVol * musicVolumeFactor, 0f, 1f);
        }
    }

    // ==========================================
    // 🔊 播放音效 (应用独立音效倍率)
    // ==========================================
    public void PlaySFX(string name)
    {
        if (_soundDict.TryGetValue(name, out SoundData sound))
        {
            // 核心公式：基础音量 * 全局音效倍率
            float finalVol = Mathf.Clamp(sound.volume * sfxVolumeFactor, 0f, 1f);
            sfxSource.PlayOneShot(sound.clip, finalVol);
        }
    }

    // 💾 持久化保存
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolumeFactor", musicVolumeFactor);
        PlayerPrefs.SetFloat("SFXVolumeFactor", sfxVolumeFactor);
        PlayerPrefs.Save();
    }
}