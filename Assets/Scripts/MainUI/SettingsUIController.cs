using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("UI 引用")]
    public Slider musicSlider; // 音乐进度条
    public Slider sfxSlider;   // 音效进度条

    private void OnEnable()
    {
        // 1. 初始化进度条数值 (0 - 2 范围)
        if (AudioManager.Instance != null)
        {
            musicSlider.value = AudioManager.Instance.musicVolumeFactor;
            sfxSlider.value = AudioManager.Instance.sfxVolumeFactor;
        }

        // 2. 绑定事件
        musicSlider.onValueChanged.AddListener(HandleMusicChange);
        sfxSlider.onValueChanged.AddListener(HandleSFXChange);
    }

    private void OnDisable()
    {
        // 3. 移除事件并保存
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SaveSettings();
    }

    private void HandleMusicChange(float value)
    {
        AudioManager.Instance.musicVolumeFactor = value;
        AudioManager.Instance.UpdateLiveMusicVolume();
    }

    private void HandleSFXChange(float value)
    {
        AudioManager.Instance.sfxVolumeFactor = value;
        // 音效通常在下次播放时生效，不需要 UpdateLive
    }

    // 给 UI 上的“关闭/返回”按钮绑定
    public void OnBackButtonClick()
    {
        if (GlobalCanvas.Instance != null)
        {
            GlobalCanvas.Instance.CloseSettings(); // 自动恢复 Time.timeScale
        }
    }
}