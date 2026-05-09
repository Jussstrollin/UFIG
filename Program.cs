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
using SadConsole;
using SadConsole.Configuration;


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
    public static StateInterface ShopStateInstance;
    public static StateInterface PlanetaryStateInstance;

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
            set => Math.Clamp(value, 0.0, 100.0);
        } // Pls be kind and make all things that add to cap it to 1.0, or not, I dont care [AS]

        public FuelType FuelType;
    };

    // ============================================== //

    public static ResourceDelta NetProd = new ResourceDelta();
    public static ResourceDelta Pending = new ResourceDelta(); // inits all to Zero

    public static ResourceBP AlphaWallet = new ResourceBP(9999999.9f);
    public static ResourceBP BetaWallet = new ResourceBP(9999999.9f);
    public static ResourceBP GammaWallet = new ResourceBP(9999999.9f);
    public static ResourceBP EssenceWallet = new ResourceBP(9999999.9f);
    // magic nums : Alpha : 1 essence input
    public static FactoryStuff AlphaFactory = new FactoryStuff(Resources.Alpha);
    // Beta : 1 Alpha Input
    public static FactoryStuff BetaFactory = new FactoryStuff(Resources.Beta);
    // Gamma : 2 Essence, 1 Alpha, 1 Beta Input
    public static FactoryStuff GammaFactory = new FactoryStuff(Resources.Gamma);

    public static StructuresBP Structure = new StructuresBP {
        EssenceMiner = 1,
        EssenceMinerCost = 10,

        AlphaFactory = 1,
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
        PlanetUiDescSpace = 200, // not on any Planet
        PlanetUiDescOrigo = 201,
        PlanetUiDescSterelis = 202,
        PlanetUiDescPrimaris = 203,
        PlanetTravelChoice = 204,
        PlanetTravelConfirmationToOrigo = 205,
        PlanetTravelConfirmationToSterelis = 206,
        PlanetTravelConfirmationToPrimaris = 207,
        PlanetTravellingUI = 208
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
        RefinedFuel,
        NULL
    }

    public enum Resources {
        Essence,
        Alpha,
        Beta,
        Gamma
    }

    static void Main() {
        Settings.WindowTitle = "UFIG";

        Game.Create(120, 40);

        Game.Instance.Started += (sender, e) => {
            Game.Instance.Screen = new StatePlaying();
        };

        Game.Instance.Run();
    }

    static void Init() {

    }

    public static void GoToState(States ToGoTo) {
        CurrentState.GoingOut();

        CurrentState = ToGoTo switch {
            States.StatePlaying => PlayingStateInstance,
            States.StateShop => ShopStateInstance,
            States.StatePlanetary => PlanetaryStateInstance,
            _ => CurrentState
        };

        CurrentState.GoingIn();
        return;
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

    static bool CheckExit(char Key) {
        if (Key == 'Q') {
            AnsiConsole.Clear();

            AnsiConsole.MarkupLine("Are you sure to Quit? [red]Y[/] / [green]N[/] ( a save will be made )");
            GameState.MenuID = Menu.ExitMenu;
            return true;
        }

        if (GameState.MenuID == Menu.ExitMenu) {
            if (Key == 'Y') {
                Save();
                GameState.Stop = true;
            }
            else if (Key == 'N') {
                CurrentState.GoingIn();
                GameState.Stop = false;
            }

            return true;
        }
        return false;
    }

    static bool StateHub(char Key) {
        if (Key == 'G') {
            GoToState(States.StatePlaying);
            return true;
        }
        else if (Key == 'S') {
            GoToState(States.StateShop);
            return true;
        }
        else if (Key == 'N') {
            GoToState(States.StatePlanetary);
            return true;
        }

        return false;
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
