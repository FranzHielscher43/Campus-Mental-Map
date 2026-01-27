using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManagement : MonoBehaviour
{
    public AudioMixer mixer;
    public string volumeParameter = "MasterVolume";

    public Slider volumeSlider;          
    public float defaultVolume = 1f;

    public float minSliderValue = 0.25f;
    public float curve = 1.6f;
    public bool debugLogs = true;

    float _lastValue = -1f;

    void Start()
    {
        float saved = PlayerPrefs.GetFloat("master_volume", defaultVolume);

        if (volumeSlider)
        {
            volumeSlider.minValue = minSliderValue;
            volumeSlider.maxValue = 1f;
            volumeSlider.SetValueWithoutNotify(saved);

            volumeSlider.onValueChanged.RemoveListener(SetVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        ApplyVolume(saved, save: false);

        if (mixer && mixer.GetFloat(volumeParameter, out float currentDb))
            Debug.Log($"[Audio] Mixer '{volumeParameter}' dB = {currentDb:0.00}");
    }

    public void SetVolume(float value)
    {
        ApplyVolume(value, save: true);
    }

    void ApplyVolume(float value, bool save)
    {
        if (!mixer) return;

        value = Mathf.Clamp(value, minSliderValue, 1f);

        if (_lastValue >= 0f && Mathf.Abs(value - _lastValue) < 0.002f)
            return;

        _lastValue = value;

        float v = Mathf.Pow(value, curve);
        float db = Mathf.Log10(Mathf.Max(0.0001f, v)) * 20f;
        mixer.SetFloat(volumeParameter, db);

        if (debugLogs)
            Debug.Log($"[Audio] value={value:0.000} -> db={db:0.0}");

        if (save)
        {
            PlayerPrefs.SetFloat("master_volume", value);
            PlayerPrefs.Save();
        }
    }
}