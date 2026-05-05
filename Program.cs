namespace UFIG;

/*
 * THE WORLD IS YOUR CANVAS
 * SO TAKE UP THE BRUSH
 * AND PAINT
 * THE WORLD
 * RED
 *
 * Note : For naming, Im using Pascal Case which just means, for example "Velocity = x;" starts with capital letters, and for multiple words, each "separate" word starts with capital letters like (IsFlying = true;)
 */



using System;
using System.Text.Json;
using Spectre.Console;


public static class Program {
    static DateTime LastGameTick = DateTime.Now;
    static DateTime LastDisplayTick = DateTime.Now;
    static DateTime LastEventTick = DateTime.Now;

    static DateTime LastEssenceTick = DateTime.Now;
    static DateTime LastAlphaFactoryTick = DateTime.Now;
    static DateTime LastBetaFactoryTick = DateTime.Now;
    static DateTime LastGammaFactoryTick = DateTime.Now;
    public static DateTime LastProgressBarsTick = DateTime.Now;

    public static StateInterface CurrentState;
    public static StateInterface PlayingStateInstance;

    // lowest -0.99, highest 1.0
    public static double PlanetMiningBonus = 0.0d;
    // lowest -0.99, highest 1.0
    public static double PlanetFactoryBonus = 0.0d;

    public static int AlphaFactoryProgressIncrement = 0;
    public static int BetaFactoryProgressIncrement = 0;
    public static int GammaFactoryProgressIncrement = 0;

    public static int EssenceMinerProgressIncrement = 0;

    public static int EventIncrement = 0;

    public struct ResourceDelta {
        public double Alpha;
        public double Beta;
        public double Gamma;
        public double Essence;
    }

    public struct StructuresBP { // includes their Status and cost
        public int AlphaFactory;
        public bool AlphaFactoryStatus;
        public double AlphaFactoryCost;
        public double AlphaFactoryBaseCost;
        public int BetaFactory;
        public bool BetaFactoryStatus;
        public double BetaFactoryCost;
        public double BetaFactoryBaseCost;
        public int GammaFactory;
        public bool GammaFactoryStatus;
        public double GammaFactoryCost;
        public double GammaFactoryBaseCost;

        public int EssenceMiner;
        public int EssenceMinerCost;
    }

    public struct UpgradeTrackBP {
        // Costs diff Arcospheres, let the wannabuy() how to handle it
        public float EssenceBaseCost;
        public int EssenceBaseBought;
        public float EssenceMultiplierCost;
        public int EssenceMultiplierBought;

        // Costs Gamma
        public int FactoryInputUpgradeBought;
        public float FactoryInputUpgradeCost;
        public int FactoryOutputUpgradeBought;
        public float FactoryOutputUpgradeCost;
    }

    public struct GameStateBP { // Unofficial Official Player Struct
        public Menu MenuID;
        public int Progress;

        public bool Pause;
        public bool Stop;

        public Planet PlanetOn;
        public bool IsOrbiting;
        public bool IsLanded;

        public bool IsTravelling;
        public Planet PlanetFrom;
        public Planet PlanetTo;

        public FuelType FuelInUse;
    }

    public struct FuelsBP {
        private double _Percentage;

        public double Percentage {
            get => _Percentage;
            set => Math.Clamp(value, 0.0, 1.0);
        } // Pls be kind and make all things that add to cap it to 1.0, or not, I dont care [AS]

        public FuelType FuelType;
    };

    // ============================================== //

    public static ResourceDelta NetProd = new ResourceDelta();
    public static ResourceDelta Pending = new ResourceDelta(); // inits all to Zero

    public static ResourceBP AlphaWallet = new ResourceBP(0.0f);
    public static ResourceBP BetaWallet = new ResourceBP(0.0f);
    public static ResourceBP GammaWallet = new ResourceBP(0.0f);
    public static ResourceBP EssenceWallet = new ResourceBP(1.0f);
    // magic nums : Alpha : 1 essence input
    public static FactoryStuff AlphaFactory = new FactoryStuff(Resources.Alpha);
    // Beta : 1 Alpha Input
    public static FactoryStuff BetaFactory = new FactoryStuff(Resources.Beta);
    // Gamma : 2 Essence, 1 Alpha, 1 Beta Input
    public static FactoryStuff GammaFactory = new FactoryStuff(Resources.Gamma);

    public static StructuresBP Structure = new StructuresBP {
        EssenceMiner = 1,
        EssenceMinerCost = 10,

        AlphaFactory = 0,
        BetaFactory = 0,
        GammaFactory = 0,

        AlphaFactoryStatus = true,
        BetaFactoryStatus = true,
        GammaFactoryStatus = true,

        AlphaFactoryCost = 10,
        BetaFactoryCost = 50,
        GammaFactoryCost = 100,
    };

    public static UpgradeTrackBP UpgradeTrack = new UpgradeTrackBP { // Handles every Upgrades info, but cuurently does too much, will later detach Unrelated stuff
                                                                     // will be in Alpha, i wont bother specifying it here, buy function have to handle this
                                                                     // also no Price scaling.. for now
        EssenceBaseBought = 1,
        EssenceBaseCost = 5,
        EssenceMultiplierBought = 1,
        EssenceMultiplierCost = 50,

        FactoryInputUpgradeBought = 0,
        FactoryInputUpgradeCost = 50, // gamma
        FactoryOutputUpgradeBought = 0,
        FactoryOutputUpgradeCost = 100 // gamma
    };

    public static GameStateBP GameState = new GameStateBP {
        MenuID = Menu.Game,
        Progress = 0,
        Pause = false,
        Stop = false,

        PlanetOn = Planet.Origo, // Default starting Planet and states
        IsOrbiting = false,
        IsLanded = true,

        IsTravelling = false,
        PlanetFrom = Planet.NULL,
        PlanetTo = Planet.NULL,

        FuelInUse = FuelType.CrudeFuel
    };

    public static FuelsBP PlayerCrudeFuel = new FuelsBP {
        Percentage = 0.0,
        FuelType = FuelType.CrudeFuel
    };

    public static FuelsBP PlayerStandardFuel = new FuelsBP {
        Percentage = 0.0,
        FuelType = FuelType.StandardFuel
    };

    public static FuelsBP PlayerRefinedFuel = new FuelsBP {
        Percentage = 0.0,
        FuelType = FuelType.RefinedFuel
    };

    public enum States {
        StatePlaying,
        StateShop,
        StatePlanetary // Includes Planetary map to choose, thenTravel Logic
    }

    public enum Menu { // NOTE : ALWAYS BE **EXPLICIT** TO SET THE INT VALUE FOR EACH
                       // its Job is mostly Specifying what menu or submeny to show in States
                       // any submenu from Game will be from 0-98
        Game = 0,

        // Submenu for Shop is 99-199
        ShopNoEntry = 99,

        ShopCategoryFactories = 100, // 100-109, 9 per thingy
        ShopAlphaFactoryPage = 102,
        ShopBetaFactoryPage = 103,
        ShopGammaFactoryPage = 104,
        ShopCategoryMine = 110, // 110-119
        ShopEssenceMinerPage = 111,
        ShopCategoryUpgrades = 120, // 120-129
        ShopFactoryInputUpgradePage = 121,
        ShopFactoryOutputUpgradePage = 122,
        ShopEssenceBaseUpgradePage = 123,
        ShopEssenceMultiplierUpgradePage = 124,
        ShopFeedBackSuccess = 199,
        ShopFeedBackRejected = 198,
        ShopFeedBackFailByError = 197,

        // Special stuff is reserved for 900-1000
        ExitMenu = 900,

        // Planet related is 200-299
        PlanetUiSpace = 200, // not on any Planet
        PlanetUiOrigo = 201,
        PlanetUiSterelis = 202,
        PlanetUiPrimaris = 203,
        PlanetTravelUI = 204
    }

    public enum Planet {
        Space,
        Origo,
        Primaris,
        Sterelis,
        NULL
    }

    public enum FuelType {
        CrudeFuel,
        StandardFuel,
        RefinedFuel
    }

    public enum Resources {
        Essence,
        Alpha,
        Beta,
        Gamma
    }

    static void Main() {
        Console.Clear();
        AnsiConsole.Clear();
        DateTime now = DateTime.Now;

        Load();

        // Assign them
        PlayingStateInstance = new StatePlaying();

        // Set Default StartingState
        CurrentState = PlayingStateInstance;
        CurrentState.GoingIn(); // Start Enter Sequence

        while (!GameState.Stop) { // while stop == false, loop
            if (Console.KeyAvailable) {
                var Key = Console.ReadKey(true).KeyChar;
                HandleInput(Key);
            }

            if ((now - LastGameTick).TotalSeconds >= 1.0) {
                if (!GameState.Pause) {
                    HandleEvents();
                    PushPending();
                    WipeNetProd();
                }
                PauseHandler();
                LastGameTick = now;
            }
        }

        if ((now - LastDisplayTick).TotalSeconds >= 0.1) {
            HandleDisplay();
            LastDisplayTick = now;
        }

        CurrentState.Update();
        CurrentState.Display();

        Thread.Sleep(10);
    }

    public static void GoToState(States ToGoTo) {
        CurrentState.GoingOut();

        CurrentState = ToGoTo switch {
            States.StatePlaying => PlayingStateInstance,
            _ => CurrentState
        };

        CurrentState.GoingIn();
        return;
    }

    public static string GetAlphaBar() {
        // This uses a fixed-width, double-precision bar to prevent terminal layout flickering.
        // Instead of changing the number of characters, it uses "▌" (half-block) to represent
        // a subtler gradation inside a cell. The total width of the bar never changes,
        // so the terminal doesn't need to recalculate the layout of the entire panel mid-frame.
        if (Structure.AlphaFactory == 0) return "";

        const int barLength = 10;
        double progress = (double)AlphaFactoryProgressIncrement / 20.0;
        progress = Math.Clamp(progress, 0.0, 1.0);

        int totalHalfBlocks = (int)(progress * barLength * 2);
        System.Text.StringBuilder bar = new System.Text.StringBuilder();

        for (int i = 0; i < barLength; i++) {
            int remainingHalfBlocks = totalHalfBlocks - (i * 2);
            if (remainingHalfBlocks >= 2) bar.Append('█');
            else if (remainingHalfBlocks == 1) bar.Append('▌');
            else bar.Append('▒');
        }

        return $"{{{bar}}}";
    }

    public static string GetBetaBar() {
        if (Structure.BetaFactory == 0) return "";

        const int barLength = 10;
        double progress = (double)BetaFactoryProgressIncrement / 50.0;
        progress = Math.Clamp(progress, 0.0, 1.0); // every .10 is 10%

        int totalHalfBlocks = (int)(progress * barLength * 2);
        System.Text.StringBuilder bar = new System.Text.StringBuilder();

        for (int i = 0; i < barLength; i++) {
            int remainingHalfBlocks = totalHalfBlocks - (i * 2);
            if (remainingHalfBlocks >= 2) bar.Append('█');
            else if (remainingHalfBlocks == 1) bar.Append('▌');
            else bar.Append('▒');
        }

        return $"{{{bar}}}";
    }

    public static string GetGammaBar() {
        if (Structure.GammaFactory == 0) return "";

        const int barLength = 10;
        double progress = (double)GammaFactoryProgressIncrement / 80.0;
        progress = Math.Clamp(progress, 0.0, 1.0);

        int totalHalfBlocks = (int)(progress * barLength * 2);
        System.Text.StringBuilder bar = new System.Text.StringBuilder();

        for (int i = 0; i < barLength; i++) {
            int remainingHalfBlocks = totalHalfBlocks - (i * 2);
            if (remainingHalfBlocks >= 2) bar.Append('█');
            else if (remainingHalfBlocks == 1) bar.Append('▌');
            else bar.Append('▒');
        }

        return $"{{{bar}}}";
    }

    public static string GetEssenceBar() {
        const int barLength = 10;
        double progress = (double)EssenceMinerProgressIncrement / 30.0;
        progress = Math.Clamp(progress, 0.0, 1.0);

        int totalHalfBlocks = (int)(progress * barLength * 2);
        System.Text.StringBuilder bar = new System.Text.StringBuilder();

        for (int i = 0; i < barLength; i++) {
            int remainingHalfBlocks = totalHalfBlocks - (i * 2);
            if (remainingHalfBlocks >= 2) bar.Append('█');
            else if (remainingHalfBlocks == 1) bar.Append('▌');
            else bar.Append('▒');
        }

        return $"{{{bar}}}";
    }

    static void HandleEvents() {
        EventSystem.RefreshEvent();
        EventSystem.ForcedAlphaEventHandler();
        EventSystem.ForcedBetaEventHandler();
        EventSystem.ForcedGammaEventHandler();
        EventSystem.RandomEventHandler();

        EventIncrement++;
    }

    static void PauseHandler() {
        if (GameState.MenuID != Menu.Game) { // anywhere not in GameMenu, means to pause
            GameState.Pause = true;
        }
        else {
            GameState.Pause = false;
        }
    }

    public static void WipeNetProd() {
        NetProd.Alpha = 0;
        NetProd.Beta = 0;
        NetProd.Gamma = 0;
        NetProd.Essence = 0;
    }

    public static void PushPending() {
        AlphaWallet.Amount += Pending.Alpha;
        BetaWallet.Amount += Pending.Beta;
        GammaWallet.Amount += Pending.Gamma;
        EssenceWallet.Amount += Pending.Essence;
    }

    public static void WipePending() {
        Pending.Alpha = 0;
        Pending.Beta = 0;
        Pending.Gamma = 0;
        Pending.Essence = 0;
    }

    enum ToBuy {
        EssenceMiner,

        AlphaFactory,
        BetaFactory,
        GammaFactory,

        EssenceBase,
        EssenceMultiplier,

        FactoryInputUpgrade,
        FactoryOutputUpgrade,

        CrudeFuel,
        StandardFuel,
        RefinedFuel
    }

    static int WannaBuy(ToBuy Upgrade) {
        if (Upgrade == ToBuy.EssenceMiner) {
            if (AlphaWallet.Amount >= Structure.EssenceMinerCost) {
                AlphaWallet.Amount -= Structure.EssenceMinerCost;
                Structure.EssenceMiner++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.AlphaFactory) {
            if (EssenceWallet.Amount >= Structure.AlphaFactoryCost) { // afford
                EssenceWallet.Amount -= Structure.AlphaFactoryCost;
                Structure.AlphaFactory++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.BetaFactory) {
            if (EssenceWallet.Amount >= Structure.BetaFactoryCost) { // afford
                EssenceWallet.Amount -= Structure.BetaFactoryCost;
                Structure.BetaFactory++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.GammaFactory) {
            if (EssenceWallet.Amount >= Structure.GammaFactoryCost) { // afford
                EssenceWallet.Amount -= Structure.GammaFactoryCost;
                Structure.GammaFactory++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.EssenceBase) {
            if (AlphaWallet.Amount >= UpgradeTrack.EssenceBaseCost) {
                AlphaWallet.Amount -= UpgradeTrack.EssenceBaseCost;
                UpgradeTrack.EssenceBaseBought++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.EssenceMultiplier) {
            if (BetaWallet.Amount >= UpgradeTrack.EssenceMultiplierCost) {
                BetaWallet.Amount -= UpgradeTrack.EssenceMultiplierCost;
                UpgradeTrack.EssenceMultiplierBought++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.FactoryInputUpgrade) {
            if (GammaWallet.Amount >= UpgradeTrack.FactoryInputUpgradeCost) {
                GammaWallet.Amount -= UpgradeTrack.FactoryInputUpgradeCost;
                UpgradeTrack.FactoryInputUpgradeBought++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.FactoryOutputUpgrade) {
            if (GammaWallet.Amount >= UpgradeTrack.FactoryOutputUpgradeCost) {
                GammaWallet.Amount -= UpgradeTrack.FactoryOutputUpgradeCost;
                UpgradeTrack.FactoryOutputUpgradeBought++;
                return 1;
            }
            else {
                return 0;
            }
        }

        return -1; // some wierd happened
    }

    static void HandleDisplay() {
        int TerminalWidth = Console.WindowWidth;
        int TerminalHeight = Console.WindowHeight;

        var GameUi = new GameUI();
        var ShopUi = new ShopUI();
        var PlanetUi = new PlanetUI();

        if (GameState.MenuID == Menu.ExitMenu) {
            ExitSequence();
        }
        else if (GameState.MenuID == Menu.PlanetUiSpace ||
                GameState.MenuID == Menu.PlanetUiOrigo ||
                GameState.MenuID == Menu.PlanetUiPrimaris ||
                GameState.MenuID == Menu.PlanetUiSterelis
        ) {
            AnsiConsole.Write(PlanetUi.ShowPlanetUI());
        }
        else if (GameState.MenuID != Menu.Game &&
                GameState.MenuID != Menu.ExitMenu &&
                GameState.MenuID != Menu.PlanetUiSpace &&
                GameState.MenuID != Menu.PlanetUiOrigo &&
                GameState.MenuID != Menu.PlanetUiPrimaris &&
                GameState.MenuID != Menu.PlanetUiSterelis) {
            AnsiConsole.Write(ShopUi.ShopMenuLayout());
        }
    }

    static void Save() {
        var ToBeSaved = new {
            Alpha = AlphaWallet.Amount,
            Beta = BetaWallet.Amount,
            Gamma = GammaWallet.Amount,
            Essence = EssenceWallet.Amount,

            AlphaFactory = Structure.AlphaFactory,
            BetaFactory = Structure.BetaFactory,
            GammaFactory = Structure.GammaFactory,

            EssenceBase = UpgradeTrack.EssenceBaseBought,
            EssenceMultiplier = UpgradeTrack.EssenceMultiplierBought,

            FactoryInputUpgrade = UpgradeTrack.FactoryInputUpgradeBought,
            FactoryOutputUpgrade = UpgradeTrack.FactoryOutputUpgradeBought,

            AlphaForcedEventDone = EventSystem.AlphaForcedEventDone,
            BetaForcedEventDone = EventSystem.BetaForcedEventDone,
            GammaForcedEventDone = EventSystem.GammaForcedEventDone
        };

        string json = System.Text.Json.JsonSerializer.Serialize(ToBeSaved);

        System.IO.File.WriteAllText("Save.json", json);
    }

    static void Load() {
        if (!System.IO.File.Exists("Save.json")) {
            AnsiConsole.Clear();
            AnsiConsole.Status()
            .Start("Finding Save files...", ctx => {
                Thread.Sleep(2000);
            });
            AnsiConsole.MarkupLine("[red]Nothing found[/]... Starting a new Game");
            Thread.Sleep(1000);
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Status()
        .Start("Finding Save files...", ctx => {
            Thread.Sleep(2000);
        });

        AnsiConsole.MarkupLine("Found... Loading save");
        Thread.Sleep(1000);

        string json = System.IO.File.ReadAllText("Save.json");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        EssenceWallet.Amount = root.GetProperty("Essence").GetDouble();
        AlphaWallet.Amount = root.GetProperty("Alpha").GetDouble();
        BetaWallet.Amount = root.GetProperty("Beta").GetDouble();
        GammaWallet.Amount = root.GetProperty("Gamma").GetDouble();

        Structure.AlphaFactory = root.GetProperty("AlphaFactory").GetInt32();
        Structure.BetaFactory = root.GetProperty("BetaFactory").GetInt32();
        Structure.GammaFactory = root.GetProperty("GammaFactory").GetInt32();

        UpgradeTrack.EssenceBaseBought = root.GetProperty("EssenceBase").GetInt32();
        UpgradeTrack.EssenceMultiplierBought = root.GetProperty("EssenceMultiplier").GetInt32();

        UpgradeTrack.FactoryInputUpgradeBought = root.GetProperty("FactoryInputUpgrade").GetInt32();
        UpgradeTrack.FactoryOutputUpgradeBought = root.GetProperty("FactoryOutputUpgrade").GetInt32();

        EventSystem.AlphaForcedEventDone = root.GetProperty("AlphaForcedEventDone").GetInt32();
        EventSystem.BetaForcedEventDone = root.GetProperty("BetaForcedEventDone").GetInt32();
        EventSystem.GammaForcedEventDone = root.GetProperty("GammaForcedEventDone").GetInt32();

        AnsiConsole.MarkupLine("[green]Done![/]");
        Thread.Sleep(500);
    }

    static void ExitSequence() {
        AnsiConsole.Clear();

        if (GameState.MenuID == Menu.ExitMenu) {
            AnsiConsole.MarkupLine("Are you sure to Quit? [red]Y[/] / [green]N[/] ( a save will be made )");
        }

    }

    static void HandleInput(char Key) {
        bool IsInGame = true; // default values
        bool IsInShop = false;
        bool IsInExit = false;
        bool IsInPlanetaryMap = false;

        if (GameState.MenuID == Menu.Game) {
            IsInGame = true;
            IsInShop = false;
            IsInExit = false;
            IsInPlanetaryMap = false;
        }
        else if ((int)GameState.MenuID >= 99 && (int)GameState.MenuID <= 199) {
            IsInGame = false;
            IsInShop = true;
            IsInExit = false;
            IsInPlanetaryMap = false;
        }
        else if (GameState.MenuID == Menu.ExitMenu) {
            IsInGame = false;
            IsInShop = false;
            IsInExit = true;
            IsInPlanetaryMap = false;
        }
        else if ((int)GameState.MenuID >= 200 && (int)GameState.MenuID <= 299) {
            IsInGame = false;
            IsInShop = false;
            IsInExit = false;
            IsInPlanetaryMap = true;
        }
        else {
            AnsiConsole.MarkupLine($"[red]UNKNOWN MENU![/] Report to as Bug and Explain how you got here");
        }

        if (IsInGame || IsInShop || IsInPlanetaryMap) {
            if (Key == 'S') GameState.MenuID = Menu.ShopNoEntry;
            if (Key == 'G') GameState.MenuID = Menu.Game;
            if (Key == 'N') {
                GameState.MenuID = GameState.PlanetOn switch {
                    Planet.Space => Menu.PlanetUiSpace,
                    Planet.Origo => Menu.PlanetUiOrigo,
                    Planet.Sterelis => Menu.PlanetUiSterelis,
                    Planet.Primaris => Menu.PlanetUiPrimaris,
                    _ => Menu.PlanetUiSpace
                };
            }
        }

        // ==== Shop Functions ==== //
        if (IsInShop) {
            int result = -1; // default on error

            bool IsInFactories = ((int)GameState.MenuID >= 100 && (int)GameState.MenuID <= 109);
            bool IsInUpgrades = ((int)GameState.MenuID >= 120 && (int)GameState.MenuID <= 129); // was 110-119
            bool IsInMine = ((int)GameState.MenuID >= 110 && (int)GameState.MenuID <= 119); // was 120-129
            bool IsInFeedback = ((int)GameState.MenuID >= 197 && (int)GameState.MenuID <= 199);
            bool IsInShopMain = (GameState.MenuID == Menu.ShopNoEntry);

            if (IsInShopMain) {
                if (Key == '1') GameState.MenuID = Menu.ShopCategoryFactories;
                if (Key == '2') GameState.MenuID = Menu.ShopCategoryUpgrades;
                if (Key == '3') GameState.MenuID = Menu.ShopCategoryMine;
            }

            // ShopGoBack from entry
            if (IsInFactories) {
                if (Key == 'B') GameState.MenuID = Menu.ShopCategoryFactories;
            }
            else if (IsInUpgrades) {
                if (Key == 'B') GameState.MenuID = Menu.ShopCategoryUpgrades;
            }
            else if (IsInMine) {
                if (Key == 'B') GameState.MenuID = Menu.ShopCategoryMine;
            }

            // Shop Go back from Category
            if ((GameState.MenuID == Menu.ShopCategoryFactories ||
                    GameState.MenuID == Menu.ShopCategoryUpgrades ||
                    GameState.MenuID == Menu.ShopCategoryMine) && Key == 'B') {
                GameState.MenuID = Menu.ShopNoEntry;
            }

            if (IsInFactories) {
                if (Key == '1') GameState.MenuID = Menu.ShopAlphaFactoryPage;
                if (Key == '2') GameState.MenuID = Menu.ShopBetaFactoryPage;
                if (Key == '3') GameState.MenuID = Menu.ShopGammaFactoryPage;

                if (Key == '\r') {
                    if (GameState.MenuID == Menu.ShopAlphaFactoryPage) result = WannaBuy(ToBuy.AlphaFactory);
                    if (GameState.MenuID == Menu.ShopBetaFactoryPage) result = WannaBuy(ToBuy.BetaFactory);
                    if (GameState.MenuID == Menu.ShopGammaFactoryPage) result = WannaBuy(ToBuy.GammaFactory);
                }
            }

            if (IsInUpgrades) {
                if (Key == '1') GameState.MenuID = Menu.ShopFactoryInputUpgradePage;
                if (Key == '2') GameState.MenuID = Menu.ShopFactoryOutputUpgradePage;
                if (Key == '3') GameState.MenuID = Menu.ShopEssenceBaseUpgradePage;
                if (Key == '4') GameState.MenuID = Menu.ShopEssenceMultiplierUpgradePage;

                if (Key == '\r') {
                    if (GameState.MenuID == Menu.ShopFactoryInputUpgradePage) result = WannaBuy(ToBuy.FactoryInputUpgrade);
                    if (GameState.MenuID == Menu.ShopFactoryOutputUpgradePage) result = WannaBuy(ToBuy.FactoryOutputUpgrade);
                    if (GameState.MenuID == Menu.ShopEssenceBaseUpgradePage) result = WannaBuy(ToBuy.EssenceBase);
                    if (GameState.MenuID == Menu.ShopEssenceMultiplierUpgradePage) result = WannaBuy(ToBuy.EssenceMultiplier);
                }
            }

            if (IsInMine) {
                if (Key == '1') GameState.MenuID = Menu.ShopEssenceMinerPage;

                if (Key == '\r') {
                    if (GameState.MenuID == Menu.ShopEssenceMinerPage) result = WannaBuy(ToBuy.EssenceMiner);
                }
            }
        }



        // PlanetaryMap

        if (IsInPlanetaryMap) { // Switching Between Planet Descriptions
            if (Key == '1') GameState.MenuID = Menu.PlanetUiOrigo;
            if (Key == '2') GameState.MenuID = Menu.PlanetUiSterelis;
            if (Key == '3') GameState.MenuID = Menu.PlanetUiPrimaris;
        }


        // Menu Stuff

        if (Key == 'Q' || Key == 'q') {
            GameState.Pause = true;
            GameState.MenuID = Menu.ExitMenu;
        } // Available Everywhere

        if (GameState.MenuID == Menu.ExitMenu && Key == 'Y') {
            Save();
            GameState.Stop = true;
        }
        else if (GameState.MenuID == Menu.ExitMenu && Key == 'N') {
            GameState.Stop = false;
            GameState.MenuID = Menu.Game;
        }
    }
}

public class ResourceBP {
    public double Amount { get; set; } // get; set; tells that its readable and writable
    public double ProductionPerTick { get; set; } // meant for +/- production

    public ResourceBP(float StartAmount) { // you can call this to Make a new Resource with this characteristics/Data
        Amount = StartAmount;
    }
}

public interface StateInterface {
    public void GoingIn();
    public void GoingOut(); // TODO: Add a argument to wherever State to goto
    public void Display();
    public void HandleControls(char Key);
    public void Update();
}
