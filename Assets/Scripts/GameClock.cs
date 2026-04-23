using UnityEngine;
using UnityEngine.Events;

public class GameClock : MonoBehaviour
{
    public static GameClock Instance { get; private set; }

    // 24 игровых часа = 5 реальных минут → 4.8 игровых минут/сек
    private const float GameMinutesPerRealSecond = 4.8f;

    [Range(1f, 100f)]
    public float TimeScale = 1f;

    public float TotalMinutes { get; private set; }
    public int Hour  => (int)(TotalMinutes / 60f) % 24;
    public int Day   => (int)(TotalMinutes / 1440f);

    public UnityEvent<int> OnHourChanged = new UnityEvent<int>();
    public UnityEvent       OnMidnight   = new UnityEvent();

    private int lastHour = -1;
    private bool paused;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (paused) return;

        TotalMinutes += GameMinutesPerRealSecond * TimeScale * Time.deltaTime;

        int hour = Hour;
        if (hour != lastHour)
        {
            if (lastHour != -1)
            {
                OnHourChanged?.Invoke(hour);
                if (hour == 0)
                    OnMidnight?.Invoke();
            }
            lastHour = hour;
        }
    }

    public void Pause()  => paused = true;
    public void Resume() => paused = false;

#if UNITY_EDITOR
    private int minutes => (int)(TotalMinutes % 60f);
    void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 24, 200, 24),
            $"Day {Day + 1}  {Hour:00}:{minutes:00}");
    }
#endif
}
