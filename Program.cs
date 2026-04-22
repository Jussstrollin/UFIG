namespace UFIG;

/*
 * [/] AlphaFactoryCalculations
 * [/] BetaFactoryCalc
 * [/] GammaFactoryCalc
 * [/] Finish Shop basic functionalities
 * [/] Essence Upgrades
 * [/] Start making the early Game loop
 * [/] Add texts, Start sequences, doialogues and stuff.
 * [/] Finally add all shop texts
 * [ish] Polish UI
 * [ish] Add EventSystem
 * [/] Basic Price scaling as Event system
 * [] Better game pacing
 * [/] get started on essence as being mined, and time based factories
 * ... Later, further here is midgame stuff, goal before? have a fun Gameloop! ...
 *
 * TODO : Add Event queue list
 *      : add random events and more forced events
 *
 * Note : For naming, Im using Pascal Case which just means, for example "Velocity = x;" starts with capital letters, and for multiple words, each "separate" word starts with capital letters like (IsFlying = true;)
 */



using System;
using System.Text.Json;
using Spectre.Console;

using static Program;
using static FactoryStuff;


public static class Program
{
        static DateTime LastGameTick = DateTime.Now;
        static DateTime LastDisplayTick = DateTime.Now;
        static DateTime LastEventTick = DateTime.Now;

        static DateTime LastEssenceTick = DateTime.Now;
        static DateTime LastAlphaFactoryTick = DateTime.Now;
        static DateTime LastBetaFactoryTick = DateTime.Now;
        static DateTime LastGammaFactoryTick = DateTime.Now;
        static DateTime LastProgressBarsTick = DateTime.Now;

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

        public struct UpgradeTrackBP {
                // Factory Cost is by default Essence
                public int EssenceMiner;
                public int EssenceMinerCost;

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

        public struct GameStateBP {
                public Menu MenuID;
                public int Progress;

                public bool Pause;
                public bool Stop;

                public Planet PlanetOn;
                public bool IsOrbiting;
                public bool IsLanded;
        }

        public struct FuelsBP {
                public  double Percentage; // Pls be kind and make all things that add to cap it to 1.0, or not, I dont care [AS]
                public  FuelType FuelType;
                public  double CostToMake; // in essence, think of essence as fluid and its accompanying arcosphere in the recipe is container material
        };

        public struct RecipeBP {
                public string Name;
                public Resources[] ResourceNeeded;
                public double[] NeededPerResource;
                public FuelType[] OutputFuel;
                public double OutputAmount;
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

        public static UpgradeTrackBP UpgradeTrack = new UpgradeTrackBP { // Handles every Upgrades info, but cuurently does too much, will later detach Unrelated stuff
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
                AlphaFactoryBaseCost = 10,
                BetaFactoryBaseCost = 50,
                GammaFactoryBaseCost = 100,

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
                IsLanded = true
        };

        public static FuelsBP PlayerLowTierFuel = new FuelsBP {
                Percentage = 0.0,
                FuelType = FuelType.LowTierFuel,
                CostToMake = 100
        };

        public static RecipeBP LowTierFuelRecipe = new RecipeBP {
                Name = "LowTierFuel",
                ResourceNeeded = new Resources[] { Resources.Essence, Resources.Alpha },
                OutputFuel = new FuelType[] {FuelType.LowTierFuel},
                OutputAmount = 0.01
        };

        public enum Menu { // NOTE : ALWAYS BE **EXPLICIT** TO SET THE INT VALUE FOR EACH
                // any submenu from Game will be from 0-99
                Game = 0,

                // Submenu for Shop is 100-199
                ShopNoEntry = 100,
                ShopEntry1 = 101,
                ShopEntry2 = 102,
                ShopEntry3 = 103,
                ShopEntry4 = 104,
                ShopEntry5 = 105,
                ShopEntry6 = 106,
                ShopEntry7 = 107,
                ShopEntry8 = 108,
                ShopFeedBackSuccess = 109,
                ShopFeedBackRejected = 110,
                ShopFeedBackFailByError = 111,

                // Special stuff is reserved for 900-1000
                ExitMenu = 900,

                // Planet map related is 200-299
                PlanetUiSpace = 200, // not on any Planet
                PlanetUiOrigo = 201,
                PlanetUiSterelis = 202,
                PlanetUiPrimaris = 203
        }

        public enum Planet {
                Space,
                Origo,
                Primaris,
                Sterelis
        }

        public enum FuelType {
                LowTierFuel,
                MediumTierFuel,
                HighTierFuel
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

                Load();

                while (!GameState.Stop) { // while stop == false, loop
                        if (Console.KeyAvailable) {
                                var Key = Console.ReadKey(true).KeyChar;
                                HandleInput(Key);
                        }

                        DateTime now = DateTime.Now;

                        if (!GameState.Pause) {
                                if (( now - LastProgressBarsTick).TotalSeconds >= 0.10) {
                                        // ticks is in .10 steps, its the UI's job to handle Checks and Resets.
                                        if (AlphaFactory.InputCheck() == true) {
                                                if (UpgradeTrack.AlphaFactory >= 1) {
                                                        if (AlphaFactoryProgressIncrement <= 20) { // 2.0s
                                                                AlphaFactoryProgressIncrement++; // it allows 16th yes, i dont care
                                                        } else if (AlphaFactoryProgressIncrement > 20) {
                                                                AlphaFactory.RunFactory();
                                                                AlphaFactoryProgressIncrement = 0;
                                                        }
                                                }
                                        }

                                        if (BetaFactory.InputCheck() == true) {
                                                if (UpgradeTrack.BetaFactory >= 1) {
                                                        if (BetaFactoryProgressIncrement <= 50) { // 5.0s
                                                                BetaFactoryProgressIncrement++;
                                                        } else if (BetaFactoryProgressIncrement > 50) {
                                                                BetaFactory.RunFactory();
                                                                BetaFactoryProgressIncrement = 0;
                                                        }
                                                }
                                        }

                                        if (GammaFactory.InputCheck() == true) {
                                                if (UpgradeTrack.GammaFactory >= 1) {
                                                        if (GammaFactoryProgressIncrement <= 80) { // 8.0s
                                                                GammaFactoryProgressIncrement++;
                                                        } else if (GammaFactoryProgressIncrement > 80) {
                                                                GammaFactory.RunFactory();
                                                                GammaFactoryProgressIncrement = 0;
                                                        }
                                                }

                                        }

                                        if (EssenceMinerProgressIncrement <= 30) {
                                                EssenceMinerProgressIncrement++;
                                        } else if (EssenceMinerProgressIncrement > 30) {
                                                EssenceProduction();
                                                EssenceMinerProgressIncrement = 0;
                                        }

                                        LastProgressBarsTick = now;
                                }
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

                        if ((now - LastDisplayTick).TotalSeconds >= 0.1) {
                                HandleDisplay();
                                LastDisplayTick = now;

                        }

                        if ((now - LastEventTick).TotalSeconds >= 1.0) {

                        }

                        Thread.Sleep(10);
                }
        }

        public static string GetAlphaBar() {
                // This uses a fixed-width, double-precision bar to prevent terminal layout flickering.
                // Instead of changing the number of characters, it uses "▌" (half-block) to represent
                // a subtler gradation inside a cell. The total width of the bar never changes,
                // so the terminal doesn't need to recalculate the layout of the entire panel mid-frame.
                if (UpgradeTrack.AlphaFactory == 0) return "";

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
                if (UpgradeTrack.BetaFactory == 0) return "";

                const int barLength = 10;
                double progress = (double)BetaFactoryProgressIncrement / 50.0;
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

        public static string GetGammaBar() {
                if (UpgradeTrack.GammaFactory == 0) return "";

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
                } else {
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

        static void EssenceProduction() { // where Factory, essence and eveery production will be called
                // Source Material always first
                float EssenceBase = 1.0f * UpgradeTrack.EssenceBaseBought;
                float EssenceMultiplier = 1.0f * UpgradeTrack.EssenceMultiplierBought;
                float EssenceGain = (EssenceBase * EssenceMultiplier) * UpgradeTrack.EssenceMiner;
                Pending.Essence += EssenceGain;
                NetProd.Essence += EssenceGain;

                PushPending();
                WipePending();
        }

        enum ToBuy {
                EssenceMiner,

                AlphaFactory,
                BetaFactory,
                GammaFactory,

                EssenceBase,
                EssenceMultiplier,

                FactoryInputUpgrade,
                FactoryOutputUpgrade
        }

        static int WannaBuy(ToBuy Upgrade) {
                if (Upgrade == ToBuy.EssenceMiner) {
                        if (AlphaWallet.Amount >= UpgradeTrack.EssenceMinerCost) {
                                AlphaWallet.Amount -= UpgradeTrack.EssenceMinerCost;
                                UpgradeTrack.EssenceMiner++;
                                return 1;
                        } else {
                                return 0;
                        }
                }

                if (Upgrade == ToBuy.AlphaFactory) {
                        if (EssenceWallet.Amount >= UpgradeTrack.AlphaFactoryCost) { // afford
                                EssenceWallet.Amount -= UpgradeTrack.AlphaFactoryCost;
                                UpgradeTrack.AlphaFactory++;
                                return 1;
                        } else {
                                return 0;
                        }
                }

                if (Upgrade == ToBuy.BetaFactory) {
                        if (EssenceWallet.Amount >= UpgradeTrack.BetaFactoryCost) { // afford
                                EssenceWallet.Amount -= UpgradeTrack.BetaFactoryCost;
                                UpgradeTrack.BetaFactory++;
                                return 1;
                        } else {
                                return 0;
                        }
                }

                if (Upgrade == ToBuy.GammaFactory) {
                        if (EssenceWallet.Amount >= UpgradeTrack.GammaFactoryCost) { // afford
                                EssenceWallet.Amount -= UpgradeTrack.GammaFactoryCost;
                                UpgradeTrack.GammaFactory++;
                                return 1;
                        } else {
                                return 0;
                        }
                }

                if (Upgrade == ToBuy.EssenceBase) {
                        if (AlphaWallet.Amount >= UpgradeTrack.EssenceBaseCost) {
                                AlphaWallet.Amount -= UpgradeTrack.EssenceBaseCost;
                                UpgradeTrack.EssenceBaseBought++;
                                return 1;
                        } else {
                                return 0;
                        }
                }

                if (Upgrade == ToBuy.EssenceMultiplier) {
                        if (BetaWallet.Amount >= UpgradeTrack.EssenceMultiplierCost) {
                                BetaWallet.Amount -= UpgradeTrack.EssenceMultiplierCost;
                                UpgradeTrack.EssenceMultiplierBought++;
                                return 1;
                        } else {
                                return 0;
                        }
                }

                if (Upgrade == ToBuy.FactoryInputUpgrade) {
                        if (GammaWallet.Amount >= UpgradeTrack.FactoryInputUpgradeCost) {
                                GammaWallet.Amount -= UpgradeTrack.FactoryInputUpgradeCost;
                                UpgradeTrack.FactoryInputUpgradeBought++;
                                return 1;
                        } else {
                                return 0;
                        }
                }

                if (Upgrade == ToBuy.FactoryOutputUpgrade) {
                        if (GammaWallet.Amount >= UpgradeTrack.FactoryOutputUpgradeCost) {
                                GammaWallet.Amount -= UpgradeTrack.FactoryOutputUpgradeCost;
                                UpgradeTrack.FactoryOutputUpgradeBought++;
                                return 1;
                        } else {
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

                if (GameState.MenuID == Menu.Game) {
                        AnsiConsole.Write(GameUi.InitGameLayout());
                } else if (GameState.MenuID == Menu.ExitMenu) {
                        ExitSequence();
                } else if (GameState.MenuID == Menu.PlanetUiSpace ||
                        GameState.MenuID == Menu.PlanetUiOrigo ||
                        GameState.MenuID == Menu.PlanetUiPrimaris ||
                        GameState.MenuID == Menu.PlanetUiSterelis
                ) {
                        AnsiConsole.Write(PlanetUi.ShowPlanetUI());
                } else if (GameState.MenuID != Menu.Game &&
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

                        AlphaFactory = UpgradeTrack.AlphaFactory,
                        BetaFactory = UpgradeTrack.BetaFactory,
                        GammaFactory = UpgradeTrack.GammaFactory,

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

                UpgradeTrack.AlphaFactory = root.GetProperty("AlphaFactory").GetInt32();
                UpgradeTrack.BetaFactory = root.GetProperty("BetaFactory").GetInt32();
                UpgradeTrack.GammaFactory = root.GetProperty("GammaFactory").GetInt32();

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
                } else if ((int)GameState.MenuID >= 100 && (int)GameState.MenuID <= 199) {
                        IsInGame = false;
                        IsInShop = true;
                        IsInExit = false;
                        IsInPlanetaryMap = false;
                } else if (GameState.MenuID == Menu.ExitMenu) {
                        IsInGame = false;
                        IsInShop = false;
                        IsInExit = true;
                        IsInPlanetaryMap = false;
                } else if ((int)GameState.MenuID >= 200 && (int)GameState.MenuID <= 299) {
                        IsInGame = false;
                        IsInShop = false;
                        IsInExit = false;
                        IsInPlanetaryMap = true;
                } else {
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

                if (IsInShop) { // Shop entry choosing
                        if (Key == '1') {
                                GameState.MenuID = Menu.ShopEntry1;
                        } else if (Key == '2') {
                                GameState.MenuID = Menu.ShopEntry2;
                        } else if (Key == '3') {
                                GameState.MenuID = Menu.ShopEntry3;
                        } else if (Key == '4') {
                                GameState.MenuID = Menu.ShopEntry4;
                        } else if (Key == '5') {
                                GameState.MenuID = Menu.ShopEntry5;
                        } else if (Key == '6') {
                                GameState.MenuID = Menu.ShopEntry6;
                        } else if (Key == '7') {
                                GameState.MenuID = Menu.ShopEntry7;
                        } else if (Key == '8') {
                                GameState.MenuID = Menu.ShopEntry8;
                        }
                }

                // ShopGoBack
                if (IsInShop) {
                        if (Key == 'B') GameState.MenuID = Menu.ShopNoEntry;
                }

                // Shop Buy and Feedbacks
                if (Key == '\r' && !IsInExit && !IsInGame && !IsInPlanetaryMap) {

                        int result = -1; // default on error

                        if (GameState.MenuID == Menu.ShopEntry1) {
                                result = WannaBuy(ToBuy.AlphaFactory);
                        } else if (GameState.MenuID == Menu.ShopEntry2) {
                                result = WannaBuy(ToBuy.BetaFactory);
                        } else if (GameState.MenuID == Menu.ShopEntry3) {
                                result = WannaBuy(ToBuy.GammaFactory);
                        } else if (GameState.MenuID == Menu.ShopEntry4) {
                                result = WannaBuy(ToBuy.EssenceBase);
                        } else if (GameState.MenuID == Menu.ShopEntry5) {
                                result = WannaBuy(ToBuy.EssenceMultiplier);
                        } else if (GameState.MenuID == Menu.ShopEntry6) {
                                result = WannaBuy(ToBuy.FactoryInputUpgrade);
                        } else if (GameState.MenuID == Menu.ShopEntry7) {
                                result = WannaBuy(ToBuy.FactoryOutputUpgrade);
                        } else if (GameState.MenuID == Menu.ShopEntry8) {
                                result = WannaBuy(ToBuy.EssenceMiner);
                        }

                        if (result == 1) {
                                GameState.MenuID = Menu.ShopFeedBackSuccess;
                        } else if (result == 0) {
                                GameState.MenuID = Menu.ShopFeedBackRejected;
                        } else if (result == -1) {
                                GameState.MenuID = Menu.ShopFeedBackFailByError;
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
                } else if (GameState.MenuID == Menu.ExitMenu && Key == 'N') {
                        GameState.Stop = false;
                        GameState.MenuID = Menu.Game;
                }
        }
}

public class ResourceBP
{
        public double Amount { get; set; } // get; set; tells that its readable and writable
        public double ProductionPerTick { get; set; } // meant for +/- production

        public ResourceBP(float StartAmount) { // you can call this to Make a new Resource with this characteristics/Data
                Amount = StartAmount;
        }
}


public class SpecialFactory // ones that could change Recipes
{

}
