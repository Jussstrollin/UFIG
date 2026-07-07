using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StellaForge;

public static class Main {
    private static double _tickTimer { get; set; } = 0;
    private const double TickInterval = 0.5;

    public static event Action? OnUpdate;

    public static void Setup() {
        if (GlobalState.OutpostPlayerOn != null) {
            return;
        }

        GlobalState.OutpostPlayerOn = new MainFactory();
        Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, Enums.FactoryTypes.AlphaFactory, GlobalState.OutpostPlayerOn.FactoryStorage, Enums.FactoryTier.Prototype);
        Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, Enums.FactoryTypes.AlphaFactory, GlobalState.OutpostPlayerOn.FactoryStorage, Enums.FactoryTier.PrototypePlus);
        Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, Enums.FactoryTypes.AlphaFactory, GlobalState.OutpostPlayerOn.FactoryStorage, Enums.FactoryTier.PrototypePlusPlus);
        Miners.AddMiner(Miners.MinerType.EssenceMiner, GlobalState.OutpostPlayerOn);
        OnUpdate += GlobalState.OutpostPlayerOn.FactoryTick;
        OnUpdate += GlobalState.OutpostPlayerOn.MinersTick;
    }

    public static void Loop(GameTime gameTime) {
        Keybinds.KeyHandle(gameTime);
        _tickTimer += gameTime.ElapsedGameTime.TotalSeconds;

        if (_tickTimer >= TickInterval) {
            _tickTimer -= TickInterval;
            OnUpdate?.Invoke();
        }
    }
}

public static class Keybinds { // done without regard for quality, god pls refactor when you feel like doing it.
    public static readonly Dictionary<Keys, Action> Binds = new() {
        { Keys.A, () => DebugAddFactoryBind(Enums.FactoryTypes.AlphaFactory, Enums.FactoryTier.PrototypePlusPlus)},
        { Keys.B, () => DebugAddFactoryBind(Enums.FactoryTypes.BetaFactory, Enums.FactoryTier.PrototypePlusPlus)},
        { Keys.G, () => DebugAddFactoryBind(Enums.FactoryTypes.GammaFactory, Enums.FactoryTier.Prototype)},
        { Keys.R, () => DebugRerollFactoryTrait()}
    };

    static double LastTime;

    public static void KeyHandle(GameTime gameTime) {
        double currentTime = gameTime.TotalGameTime.TotalSeconds;
        KeyboardState state = Keyboard.GetState();
        Keys[] pressedKeys = state.GetPressedKeys();

        if ((currentTime - LastTime) <= 0.5) {
            return; // Still on cooldown
        }

        foreach (Keys key in pressedKeys) {
            if (Binds.TryGetValue(key, out Action action)) {
                action();
                LastTime = currentTime;
                break; // Only one key per press
            }
        }
    }

    public static void DebugAddFactoryBind(Enums.FactoryTypes Type, Enums.FactoryTier Tier) {
        Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, Type, GlobalState.OutpostPlayerOn.FactoryStorage, Tier);
    }

    public static void DebugRerollFactoryTrait() {
        foreach (var Factory in GlobalState.OutpostPlayerOn.FactoryList)
            Factories.FactoryCreationRelated.RerollTraits(Factory);
    }
}

public static class GlobalState {
    public static MainFactory OutpostPlayerOn;
    public static List<MainFactory> Outposts = new List<MainFactory>();
}

public class MainFactory {
    public event Action OnFactoryTick;
    public event Action OnMinerTick;
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

