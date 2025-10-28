using UnityEngine;
using System;

public class NightClock : MonoBehaviour
{
    [Header("Night Window (24h clock)")]
    [Range(0,23)] public int startHour = 22;    
    [Range(0,23)] public int endHour   = 6;     

    [Header("Duration")]
    [Tooltip("How long the whole night lasts in real time (seconds).")]
    public float nightDurationSeconds = 480f;   

    [Header("Difficulty Curves (0..1 over the night)")]
    public AnimationCurve healthCurve       = AnimationCurve.Linear(0,1,1,2);   
    public AnimationCurve speedCurve        = AnimationCurve.Linear(0,1,1,1.3f); 
    public AnimationCurve spawnIntervalCurve= AnimationCurve.Linear(0,1,1,0.5f); 

    [Header("Control")]
    public bool autoStart = true;
    public bool paused    = false;

    public event Action<int> OnHourChanged;     
    public event Action OnNightEnded;

    float _elapsed;          
    float _progress01;       
    int   _lastShownHour = -999;

    public float Progress01 => _progress01;
    public bool NightRunning { get; private set; } = false;

    void Start()
    {
        if (autoStart) StartNight();
    }

    public void StartNight()
    {
        _elapsed = 0f;
        _progress01 = 0f;
        NightRunning = true;
        _lastShownHour = -999;
        FireHourIfChanged();
    }

    public void Pause(bool pause) => paused = pause;

    void Update()
    {
        if (!NightRunning || paused) return;

        _elapsed += Time.deltaTime;
        _progress01 = Mathf.Clamp01(_elapsed / Mathf.Max(0.0001f, nightDurationSeconds));
        FireHourIfChanged();

        if (_progress01 >= 1f)
        {
            NightRunning = false;
            OnNightEnded?.Invoke();
        }
    }

    // Time display
    public void GetDisplayTime(out int hour12, out int minute, out bool isAM)
    {
        // 8 in game hours
        float nightHours = 8f;
        float totalHoursSinceStart = _progress01 * nightHours;

        // Starts at 10PM
        float hour24 = (startHour + totalHoursSinceStart) % 24f;
        int hourInt  = Mathf.FloorToInt(hour24);
        float hourFrac = hour24 - hourInt;
        minute = Mathf.FloorToInt(hourFrac * 60f);

        // 24 to 12 conversion
        isAM   = hour24 < 12f;
        int h12 = hourInt % 12;
        if (h12 == 0) h12 = 12;
        hour12 = h12;
    }

    void FireHourIfChanged()
    {
        GetDisplayTime(out int hour12, out int minute, out bool isAM);
        
        int key = (isAM ? 0 : 100) + hour12;
        if (key != _lastShownHour)
        {
            _lastShownHour = key;
            OnHourChanged?.Invoke(hour12);
        }
    }

    //  Difficulty getters 
    public float GetHealthMultiplier()        => Mathf.Max(0.01f, healthCurve.Evaluate(_progress01));
    public float GetSpeedMultiplier()         => Mathf.Max(0.01f, speedCurve.Evaluate(_progress01));
    public float GetSpawnIntervalMultiplier() => Mathf.Clamp(spawnIntervalCurve.Evaluate(_progress01), 0.05f, 10f);
}
