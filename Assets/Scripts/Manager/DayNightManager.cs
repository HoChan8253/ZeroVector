using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    public bool IsNight { get; private set; }

    [SerializeField] private PlayerInputHub _input;

    private void Awake()
    {
        Instance = this;
        if (_input == null) _input = FindFirstObjectByType<PlayerInputHub>();
    }

    private void Update()
    {
        if (_input != null && _input.ToggleDayNightPressedThisFrame)
        {
            IsNight = !IsNight;
            Debug.Log(IsNight ? "Night" : "Day");
        }
    }
}