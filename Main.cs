using System;
using SadConsole;
using System.IO;
using System.Collections.Generic;
using SadConsole.Configuration;
using SadConsole.Input;

namespace StellaForge;

public static class Main {
    private static double _tickTimer { get; set; } = 0;
    private const double TickInterval = 0.5;


    public static event Action? OnUpdate;
    // public static event Action? OnRender;

    public static void Setup() {
        Logger.Start();
        Logger.Log("Game Started..");

        Keybind.KeyBoardState = Game.Instance.GetKeyboardState();
        Logger.Log("Started KeyBoard..");

        if (GlobalState.OutpostPlayerOn != null) {
            Logger.Log("Existing Outpost Found.");
            return;
        }

        GlobalState.OutpostPlayerOn = new MainFactory();
        Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, Enums.FactoryTypes.AlphaFactory, GlobalState.OutpostPlayerOn.FactoryStorage, Enums.FactoryTier.Prototype);
        Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, Enums.FactoryTypes.AlphaFactory, GlobalState.OutpostPlayerOn.FactoryStorage, Enums.FactoryTier.PrototypePlus);
        Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, Enums.FactoryTypes.AlphaFactory, GlobalState.OutpostPlayerOn.FactoryStorage, Enums.FactoryTier.PrototypePlusPlus);
        Miners.AddMiner(Miners.MinerType.EssenceMiner, GlobalState.OutpostPlayerOn);
        OnUpdate += GlobalState.OutpostPlayerOn.FactoryTick;
        OnUpdate += GlobalState.OutpostPlayerOn.MinersTick;
        Logger.Log("Game Initialized..");

        UI.UI_Manager.InitUI();
        Logger.Log("Ui Started...");
    }

    public static void Loop(GameHost host) {
        // Keybinds.KeyHandle(gameTime);
        _tickTimer += 0.016; // Approximate 60fps for now
        Keybind.currTime += 0.016;
        Keybind.HandleKeyPress();

        if (_tickTimer >= TickInterval) {
            _tickTimer -= TickInterval;
            OnUpdate?.Invoke();
            // OnRender?.Invoke();
        }
    }
}

public static class Keybind {
    public static IKeyboardState? KeyBoardState;
    static double LastTime;
    public static double currTime;

    public static readonly Dictionary<Keys, Action> KeyMap = new() {
        { Keys.Escape, () => Exit() },
        { Keys.A, () => DebugAlphaFactorySpawn() }
    };

    private static void Exit() {
        Logger.Log("Game Ended.");
        Game.Instance.Stop();
    }

    private static void DebugAlphaFactorySpawn() {
        if (GlobalState.OutpostPlayerOn != null) {
            Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, Enums.FactoryTypes.AlphaFactory, new Storage(), Enums.FactoryTier.Prototype);
        }
    }

    public static void HandleKeyPress() {
        if (KeyBoardState == null) { return; }

        if ((currTime - LastTime) <= 0.5) {
            return;
        }

        Keys[] CurrPressedkey = KeyBoardState.GetPressedKeys();
        if (CurrPressedkey.Length == 0) { return; }

        for (int i = 0; i <= (CurrPressedkey.Length - 1); i++) {
            if (KeyMap.TryGetValue(CurrPressedkey[i], out var Function)) {
                Logger.Log("FoundKey!");
                Function.Invoke();
                LastTime = currTime;
            }
        }
    }
}

public static class GlobalState {
    public static MainFactory? OutpostPlayerOn;
    public static List<MainFactory> Outposts = new List<MainFactory>();
}

public static class Logger {
    private static readonly string LogPath = "LatestGame.log";
    private static string TimeStamp => DateTime.Now.ToString("[HH : mm : ss]");

    public static void Start() {
        File.Delete(LogPath);
    }

    public static void Log(string Msg) {
        string Log = $"{TimeStamp} > {Msg}";
        File.AppendAllText(LogPath, Log + '\n');
    }

    public static void ErrorLog(string Msg) {
        string Log = $"{TimeStamp} > [ERROR] {Msg}";
        File.AppendAllText(LogPath, Log + '\n');
    }
}

public class MainFactory {
    public event Action? OnFactoryTick;
    public event Action? OnMinerTick;
    public Storage FactoryStorage = new Storage();

    public List<Factories.GenericFactory> FactoryList = new();
    public List<Miners.GenericMiner> MinersList = new();

    public event Action? OnOutpostChange;

    public MainFactory() {
        // whatever, its here when needed
    }

    public void InvokeChangeHasHappened() {
        OnOutpostChange?.Invoke();
    }

    public void FactoryTick() {
        OnFactoryTick?.Invoke();
    }

    public void MinersTick() {
        OnMinerTick?.Invoke();
    }
}

