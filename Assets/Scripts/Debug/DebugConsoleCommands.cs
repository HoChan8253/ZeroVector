using UnityEngine;
using UnityEngine.SceneManagement;

public static class DebugConsoleCommands
{
    private static bool _godMode = false;
    private static bool _fpsActive = false;

    public static void Execute(string input)
    {
        string[] parts = input.Trim().Split(' ');
        string cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "help": CmdHelp(); break;
            case "gold": CmdGold(parts); break;
            case "boss": CmdBoss(); break;
            case "kill": CmdKill(); break;
            case "wave": CmdWave(parts); break;
            case "god": CmdGod(); break;
            case "time": CmdTime(parts); break;
            case "fps": CmdFps(); break;
            case "reload": CmdReload(); break;
            case "clear": ClearConsole(); break;
            default:
                Log($"알 수 없는 명령어: '{cmd}'  —  help 로 목록 확인", LogLevel.Error);
                break;
        }
    }

    private static void CmdHelp()
    {
        Log("──────────────────────────────");
        Log("  help              명령어 목록");
        Log("  gold [n]          골드 추가 (기본 100000)");
        Log("  boss              보스전 즉시 진입");
        Log("  kill              모든 적 제거");
        Log("  wave [n]          n 웨이브로 이동");
        Log("  god               무적 모드 토글");
        Log("  time [n]          TimeScale 변경 (예: time 0.5)");
        Log("  fps               FPS 표시 토글");
        Log("  reload            현재 씬 리로드");
        Log("  clear             콘솔 초기화");
        Log("──────────────────────────────");
    }

    private static void CmdGold(string[] parts)
    {
        int amount = 100000;
        if (parts.Length > 1 && int.TryParse(parts[1], out int parsed))
            amount = parsed;

        if (GoldManager.Instance == null)
        { Log("GoldManager 없음", LogLevel.Error); return; }

        GoldManager.Instance.Add(amount, Vector3.zero);
        Log($"[Cheat] 골드 +{amount:N0} 지급");
    }

    private static void CmdBoss()
    {
        var waveManager = Object.FindFirstObjectByType<WaveManager>();
        if (waveManager == null)
        { Log("WaveManager 없음", LogLevel.Error); return; }

        waveManager.CheatToLastWave();
        Log("[Cheat] 보스전 진입!");
    }

    private static void CmdKill()
    {
        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        if (enemies.Length == 0)
        { Log("처치할 적 없음", LogLevel.Warning); return; }

        var list = new System.Collections.Generic.List<EnemyHealth>(enemies);
        int count = 0;
        foreach (var e in list)
        {
            if (e != null && e.Hp > 0)
            {
                e.TakeDamage(999999, false, e.transform.position, Vector3.up);
                count++;
            }
        }
        Log($"[Cheat] 적 {count}마리 제거");
    }

    private static void CmdWave(string[] parts)
    {
        if (parts.Length < 2 || !int.TryParse(parts[1], out int n))
        { Log("사용법: wave [웨이브 번호]  예) wave 5", LogLevel.Warning); return; }

        var waveManager = Object.FindFirstObjectByType<WaveManager>();
        if (waveManager == null)
        { Log("WaveManager 없음", LogLevel.Error); return; }

        waveManager.CheatToWave(n);
        Log($"[Cheat] 웨이브 {n} 로 이동");
    }

    private static void CmdGod()
    {
        _godMode = !_godMode;
        var player = Object.FindFirstObjectByType<PlayerStats>();
        if (player == null)
        { Log("PlayerStats 없음", LogLevel.Error); return; }

        player.SetInvincible(_godMode);
        Log($"[Cheat] 무적 모드 {(_godMode ? "ON" : "OFF")}");
    }

    private static void CmdTime(string[] parts)
    {
        if (parts.Length < 2 || !float.TryParse(parts[1], out float scale))
        { Log("사용법: time [배율]  예) time 0.5", LogLevel.Warning); return; }

        scale = Mathf.Clamp(scale, 0f, 10f);
        Time.timeScale = scale;
        Log($"[Cheat] Time.timeScale = {scale}");
    }

    private static void CmdFps()
    {
        _fpsActive = !_fpsActive;
        var results = Object.FindObjectsByType<FPSDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var fps = results.Length > 0 ? results[0] : null;
        if (fps != null) fps.gameObject.SetActive(_fpsActive);
        Log($"[Cheat] FPS 표시 {(_fpsActive ? "ON" : "OFF")}");
    }

    private static void CmdReload()
    {
        Log("[Cheat] 씬 리로드...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static void ClearConsole()
    {
        if (DebugConsoleUI.Instance == null) return;
        var container = DebugConsoleUI.Instance.GetLogContainer();
        if (container == null) return;
        foreach (Transform child in container)
            Object.Destroy(child.gameObject);
    }

    private enum LogLevel { Normal, Warning, Error }

    private static void Log(string msg, LogLevel level = LogLevel.Normal)
    {
        if (DebugConsoleUI.Instance == null) return;
        Color color = level switch
        {
            LogLevel.Warning => new Color(1f, 0.85f, 0.3f),
            LogLevel.Error => new Color(1f, 0.35f, 0.35f),
            _ => new Color(0.85f, 0.85f, 0.85f),
        };
        DebugConsoleUI.Instance.AddLine(msg, color);
    }
}