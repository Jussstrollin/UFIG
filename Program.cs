/*
 * [/] AlphaFactoryCalculations
 * [/] BetaFactoryCalc
 * [/] GammaFactoryCalc
 * [/] Finish Shop basic functionalities
 * [/] Essence Upgrades
 * [/] Start making the early Game loop
 * [ish] Add texts, Start sequences, doialogues and stuff.
 * [ish] Finally add all shop texts
 * [ish] Polish UI
 * [ish] Add EventSystem
 * [ish] Basic Price scaling as Event system
 * ... Later, further here is midgame stuff, goal before? have a fun Gameloop! ...
 *
 * TODO : Add Event queue list
 *      : add random events and more forced events
 *      : get started on essence as being mined, and  time based factories
 *
 * Note : For naming, Im using Pascal Case which just means, for example "Velocity = x;" starts with capital letters, and for multiple words, each "separate" word starts with capital letters like (IsFlying = true;)
 */



using System;
using System.IO;
using System.Text.Json;
using Spectre.Console;
using static Program;
using static StringsStuff;
using static EventSystem;

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

        public static int AlphaFactoryProgressIncrement = 0;
        public static int BetaFactoryProgressIncrement = 0;
        public static int GammaFactoryProgressIncrement = 0;

        public static int EventIncrement = 0;

        public struct ResourceDelta {
                public double Alpha;
                public double Beta;
                public double Gamma;
                public double Essence;
        }

        public struct UpgradeTrackBP {
                // Factory Cost is by default Essence

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
                public float MenuID;
                public int Progress;

                public bool Pause;
                public bool Stop;
        }

        // ============================================== //

        public static ResourceDelta Pending = new ResourceDelta(); // inits all to Zero

        public static ResourceBP AlphaWallet = new ResourceBP(0.0f);
        public static ResourceBP BetaWallet = new ResourceBP(0.0f);
        public static ResourceBP GammaWallet = new ResourceBP(0.0f);
        public static ResourceBP EssenceWallet = new ResourceBP(1.0f);

        public static Factory AlphaFactory = new Factory(Resources.Alpha, 1.0d, 0.0d, 0.0d, 1.0d);
        public static Factory BetaFactory = new Factory(Resources.Beta, 1.0d, 0.0d, 0.0d, 1.0d);
        public static Factory GammaFactory = new Factory(Resources.Gamma, 2.0d, 1.0d, 1.0d, 1.0d);

        public static UpgradeTrackBP UpgradeTrack = new UpgradeTrackBP { // Handles every Upgrades info, but cuurently does too much, will later detach Unrelated stuff
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
                MenuID = 0.0f,
                Progress = 0,
                Pause = false,
                Stop = false
        };

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
                                // if (( now - LastProgressBarsTick).TotalSeconds >= 0.10) {
                                //         // ticks is in .10 steps, its the UI's job to handle Checks and Resets.
                                //         if (UpgradeTrack.AlphaFactory >= 1) {
                                //                 if (AlphaFactoryProgressIncrement <= 15) { // 1.5s
                                //                         AlphaFactoryProgressIncrement++; // it allows 16th yes, i dont care
                                //                 } else if (AlphaFactoryProgressIncrement > 15) {
                                //                         AlphaFactoryProgressIncrement = 0;
                                //                 }
                                //         }
                                //
                                //         if (UpgradeTrack.BetaFactory >= 1) {
                                //                 if (BetaFactoryProgressIncrement <= 20) { // 2.0s
                                //                         BetaFactoryProgressIncrement++;
                                //                 } else if (BetaFactoryProgressIncrement > 20) {
                                //                         BetaFactoryProgressIncrement = 0;
                                //                 }
                                //         }
                                //
                                //         if (UpgradeTrack.GammaFactory >= 1) {
                                //                 if (GammaFactoryProgressIncrement <= 30) { // 3.0s
                                //                         GammaFactoryProgressIncrement++;
                                //                 } else if (GammaFactoryProgressIncrement > 30) {
                                //                         GammaFactoryProgressIncrement = 0;
                                //                 }
                                //         }
                                //
                                //         LastProgressBarsTick = now;
                                // }

                                if ((now - LastAlphaFactoryTick).TotalSeconds >= 1.0) { // debug related alpha shall be 1.5s, beta be 2.0s, and Gamma be 3.0s
                                        AlphaFactory.FactoryCalc();
                                        PushPending();
                                        WipePending();
                                        LastAlphaFactoryTick = now;
                                }

                                if ((now - LastBetaFactoryTick).TotalSeconds >= 1.0) {
                                        BetaFactory.FactoryCalc();
                                        PushPending();
                                        WipePending();
                                        LastBetaFactoryTick = now;
                                }

                                if ((now - LastGammaFactoryTick).TotalSeconds >= 1.0) {
                                        GammaFactory.FactoryCalc();
                                        PushPending();
                                        WipePending();
                                        LastGammaFactoryTick = now;
                                }
                        }

                        if ((now - LastGameTick).TotalSeconds >= 1.0) {
                                if (!GameState.Pause) {
                                        HandleEvents();
                                        EssenceProduction();
                                }
                                PauseHandler();
                                LastGameTick = now;
                        }

                        if ((now - LastDisplayTick).TotalSeconds >= 0.5) {
                                HandleDisplay();
                                LastDisplayTick = now;

                        }

                        if ((now - LastEventTick).TotalSeconds >= 1.0) {

                        }

                        Thread.Sleep(10);
                }
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
                if (GameState.MenuID >= 1.0f) { // anything greater than 0.9 is in shop
                        GameState.Pause = true;
                } else {
                        GameState.Pause = false;
                }
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
                float EssenceGain = EssenceBase * EssenceMultiplier;
                Pending.Essence += EssenceGain;
        }

        enum ToBuy {
                AlphaFactory,
                BetaFactory,
                GammaFactory,

                EssenceBase,
                EssenceMultiplier,

                FactoryInputUpgrade,
                FactoryOutputUpgrade
        }

        static int WannaBuy(ToBuy Upgrade) {
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
                var ShopUi =  new ShopUI();

                if (GameState.MenuID == 0.0f) {
                        AnsiConsole.Write(GameUi.InitGameLayout());
                } else {
                        AnsiConsole.Write(ShopUi.ShopMenuLayout());
                }

                if (GameState.MenuID == 999.999f) {
                        ExitSequence();
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
                bool found;

                if (System.IO.File.Exists("Save.json")) {
                        found = true;
                } else {
                        found = false;
                }

                AnsiConsole.Clear();

                AnsiConsole.Status()
                .Start("Finding Save files...", ctx => {
                        Thread.Sleep(2000);
                });

                if (found) {
                        AnsiConsole.MarkupLine("Found... Loading save");

                        Thread.Sleep(1000);

                        // Read then parse to json
                        string json = System.IO.File.ReadAllText("Save.json");

                        // Deserialize from json to object, the var thingy
                        var SaveData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);

                        // then restore
                        EssenceWallet.Amount = SaveData.GetProperty("Essence").GetSingle();
                        AlphaWallet.Amount = SaveData.GetProperty("Alpha").GetSingle();
                        BetaWallet.Amount = SaveData.GetProperty("Beta").GetSingle();
                        GammaWallet.Amount = SaveData.GetProperty("Gamma").GetSingle();

                        UpgradeTrack.AlphaFactory = SaveData.GetProperty("AlphaFactory").GetInt32();
                        UpgradeTrack.BetaFactory = SaveData.GetProperty("BetaFactory").GetInt32();
                        UpgradeTrack.GammaFactory = SaveData.GetProperty("GammaFactory").GetInt32();

                        UpgradeTrack.EssenceBaseBought = SaveData.GetProperty("EssenceBase").GetInt32();
                        UpgradeTrack.EssenceMultiplierBought = SaveData.GetProperty("EssenceMultiplier").GetInt32();

                        UpgradeTrack.FactoryInputUpgradeBought = SaveData.GetProperty("FactoryInputUpgrade").GetInt32();
                        UpgradeTrack.FactoryOutputUpgradeBought = SaveData.GetProperty("FactoryOutputUpgrade").GetInt32();

                        EventSystem.AlphaForcedEventDone = SaveData.GetProperty("AlphaForcedEventDone").GetInt32();
                        EventSystem.BetaForcedEventDone = SaveData.GetProperty("BetaForcedEventDone").GetInt32();
                        EventSystem.GammaForcedEventDone = SaveData.GetProperty("GammaForcedEventDone").GetInt32();

                        AnsiConsole.MarkupLine("[green]Done![/]");
                        Thread.Sleep(500);
                        return;
                } else {
                        AnsiConsole.MarkupLine("[red]Nothing found[/]... Starting a new Game");

                        Thread.Sleep(1000);

                        return; // just return, base is already set
                }
        }

        static void ExitSequence() {
                AnsiConsole.Clear();

                if (GameState.MenuID == 999.999f) {
                        AnsiConsole.MarkupLine("Are you sure to Quit? [red]Y[/] / [green]N[/] ( will be saved )");
                }

        }

        static void HandleInput(char Key) {
                if (GameState.MenuID == 0.0f) { // on menu
                        if (Key == 'S') GameState.MenuID = 1.0f; // go shop
                } else if (GameState.MenuID == 1.0f || GameState.MenuID == 1.0f || GameState.MenuID == 1.1f || GameState.MenuID == 1.2f || GameState.MenuID == 1.3f || GameState.MenuID == 1.4f || GameState.MenuID == 1.5f || GameState.MenuID == 999.996f || GameState.MenuID == 999.997f || GameState.MenuID == 999.998f) {
                        if (Key == 'S') GameState.MenuID = 0.0f;
                }

                // ==== Shop Functions ==== //

                if (GameState.MenuID == 1.0f || GameState.MenuID == 1.1f || GameState.MenuID == 1.2f || GameState.MenuID == 1.3f || GameState.MenuID == 1.4f || GameState.MenuID == 1.5f || GameState.MenuID == 1.6f || GameState.MenuID == 1.7f || GameState.MenuID == 999.996f || GameState.MenuID == 999.997f || GameState.MenuID == 999.998f ) { // Shop entry choosing
                        if (Key == '1') {
                                GameState.MenuID = 1.1f;
                        } else if (Key == '2') {
                                GameState.MenuID = 1.2f;
                        } else if (Key == '3') {
                                GameState.MenuID = 1.3f;
                        } else if (Key == '4') {
                                GameState.MenuID = 1.4f;
                        } else if (Key == '5') {
                                GameState.MenuID = 1.5f;
                        } else if (Key == '6') {
                                GameState.MenuID = 1.6f;
                        } else if (Key == '7') {
                                GameState.MenuID = 1.7f;
                        }
                }

                // ShopGoBack
                if (GameState.MenuID == 1.1f || GameState.MenuID == 1.2f || GameState.MenuID == 1.3f || GameState.MenuID == 1.4f || GameState.MenuID == 1.5f || GameState.MenuID == 1.6f || GameState.MenuID == 1.7f || GameState.MenuID == 999.996f || GameState.MenuID == 999.997f || GameState.MenuID == 999.998f) {
                        if (Key == 'B') GameState.MenuID = 1.0f;
                }

                // Shop Buy and Feedbacks
                if (GameState.MenuID == 1.1f) { // AlphaFactoryPage
                        if (Key == '\r') { // wanabuy
                                int result = WannaBuy(ToBuy.AlphaFactory);

                                if (result == 1) {
                                        GameState.MenuID = 999.998f; // Successfull
                                } else if (result == 0) {
                                        GameState.MenuID = 999.997f; // fail by cant afford
                                } else if (result == -1) {
                                        GameState.MenuID = 999.996f; // fail by error
                                }
                        }
                }

                if (GameState.MenuID == 1.2f) { // BetaFactoryPage
                        if (Key == '\r') { // wanabuy
                                int result = WannaBuy(ToBuy.BetaFactory);

                                if (result == 1) {
                                        GameState.MenuID = 999.998f; // Successfull
                                } else if (result == 0) {
                                        GameState.MenuID = 999.997f; // fail by cant afford
                                } else if (result == -1) {
                                        GameState.MenuID = 999.996f; // fail by error
                                }
                        }
                }

                if (GameState.MenuID == 1.3f) { // GammaFactoryPage
                        if (Key == '\r') { // wanabuy
                                int result = WannaBuy(ToBuy.GammaFactory);

                                if (result == 1) {
                                        GameState.MenuID = 999.998f; // Successfull
                                } else if (result == 0) {
                                        GameState.MenuID = 999.997f; // fail by cant afford
                                } else if (result == -1) {
                                        GameState.MenuID = 999.996f; // fail by error
                                }
                        }
                }

                if (GameState.MenuID == 1.4f) { // GammaFactoryPage
                        if (Key == '\r') { // wanabuy
                                int result = WannaBuy(ToBuy.EssenceBase);

                                if (result == 1) {
                                        GameState.MenuID = 999.998f; // Successfull
                                } else if (result == 0) {
                                        GameState.MenuID = 999.997f; // fail by cant afford
                                } else if (result == -1) {
                                        GameState.MenuID = 999.996f; // fail by error
                                }
                        }
                }

                if (GameState.MenuID == 1.5f) { // GammaFactoryPage
                        if (Key == '\r') { // wanabuy
                                int result = WannaBuy(ToBuy.EssenceMultiplier);

                                if (result == 1) {
                                        GameState.MenuID = 999.998f; // Successfull
                                } else if (result == 0) {
                                        GameState.MenuID = 999.997f; // fail by cant afford
                                } else if (result == -1) {
                                        GameState.MenuID = 999.996f; // fail by error
                                }
                        }
                }

                if (GameState.MenuID == 1.6f) { // GammaFactoryPage
                        if (Key == '\r') { // wanabuy
                                int result = WannaBuy(ToBuy.FactoryInputUpgrade);

                                if (result == 1) {
                                        GameState.MenuID = 999.998f; // Successfull
                                } else if (result == 0) {
                                        GameState.MenuID = 999.997f; // fail by cant afford
                                } else if (result == -1) {
                                        GameState.MenuID = 999.996f; // fail by error
                                }
                        }
                }

                if (GameState.MenuID == 1.7f) { // GammaFactoryPage
                        if (Key == '\r') { // wanabuy
                                int result = WannaBuy(ToBuy.FactoryOutputUpgrade);

                                if (result == 1) {
                                        GameState.MenuID = 999.998f; // Successfull
                                } else if (result == 0) {
                                        GameState.MenuID = 999.997f; // fail by cant afford
                                } else if (result == -1) {
                                        GameState.MenuID = 999.996f; // fail by error
                                }
                        }
                }

                // Menu Stuff

                if (Key == 'Q' || Key == 'q') {
                        GameState.Pause = true;
                        GameState.MenuID = 999.999f;
                } // Available Everywhere

                if (GameState.MenuID == 999.999f && Key == 'Y') {
                        Save();
                        GameState.Stop = true;
                } else if (GameState.MenuID == 999.999f && Key == 'N') {
                        GameState.Stop = false;
                        GameState.MenuID = 0.0f;
                }
        }
}

public enum Resources {
        Essence,
        Alpha,
        Beta,
        Gamma
}

public class Factory
{
        public Resources Resource { get; set; }
        public double Input1ResourceBase { get; set; } // like 1 for 1 Essence per tick
        public double Input2ResourceBase { get; set; }
        public double Input3ResourceBase { get; set; }
        public double OutputResourceBase { get; set; }

        public Factory(Resources Resource, double Input1ResourceBase, double Input2ResourceBase, double Input3ResourceBase, double OutputResourceBase) {
                this.Resource = Resource;
                this.Input1ResourceBase = Input1ResourceBase;
                this.Input2ResourceBase = Input2ResourceBase;
                this.Input3ResourceBase = Input3ResourceBase;
                this.OutputResourceBase = OutputResourceBase;
        }

        // 5% Input reduction per upgrades boguht
        static double BonusInputReduction => 0.05f * UpgradeTrack.FactoryInputUpgradeBought;
        // 10% Bonus prod per upgrade Bought
        static double BonusProduction => 1 + (0.10f * UpgradeTrack.FactoryOutputUpgradeBought);

        public void FactoryCalc() {
                bool Halt = true;

                // Input check
                if (this.Resource == Resources.Alpha) {
                        if (EssenceWallet.Amount >= ((this.Input1ResourceBase * UpgradeTrack.AlphaFactory) - (BonusInputReduction * UpgradeTrack.AlphaFactory)) && UpgradeTrack.AlphaFactory >= 1) {
                                Halt = false;
                        } else Halt = true;

                        if (!Halt) {
                                // Deduct
                                Pending.Essence -= ((this.Input1ResourceBase * UpgradeTrack.AlphaFactory) - (BonusInputReduction * UpgradeTrack.AlphaFactory));
                                // Apply
                                Pending.Alpha += ((this.OutputResourceBase * UpgradeTrack.AlphaFactory) * BonusProduction);
                        }
                }

                if (this.Resource == Resources.Beta) {
                        if (AlphaWallet.Amount >= ((this.Input1ResourceBase * UpgradeTrack.BetaFactory) - (BonusInputReduction * UpgradeTrack.BetaFactory)) && UpgradeTrack.BetaFactory >= 1) {
                                Halt = false;
                        } else Halt = true;

                        if (!Halt) {
                                // Deduct
                                Pending.Alpha -= ((this.Input1ResourceBase * UpgradeTrack.BetaFactory) - (BonusInputReduction * UpgradeTrack.BetaFactory));
                                // Apply
                                Pending.Beta += ((this.OutputResourceBase * UpgradeTrack.BetaFactory) * BonusProduction);
                        }
                }

                if (this.Resource == Resources.Gamma) {
                        if (EssenceWallet.Amount >= ((this.Input1ResourceBase * UpgradeTrack.GammaFactory) - (BonusInputReduction * UpgradeTrack.GammaFactory)) && AlphaWallet.Amount >= ((this.Input2ResourceBase * UpgradeTrack.GammaFactory) - (BonusInputReduction * UpgradeTrack.GammaFactory)) && BetaWallet.Amount >= ((this.Input3ResourceBase * UpgradeTrack.GammaFactory) - (BonusInputReduction * UpgradeTrack.GammaFactory)) && UpgradeTrack.GammaFactory >= 1) {
                                Halt = false;
                        } else Halt = true;

                        if (!Halt) {
                                // Deduct
                                Pending.Essence -= ((this.Input1ResourceBase * UpgradeTrack.GammaFactory) - (BonusInputReduction * UpgradeTrack.GammaFactory));
                                Pending.Alpha -= ((this.Input2ResourceBase * UpgradeTrack.GammaFactory) - (BonusInputReduction * UpgradeTrack.GammaFactory));
                                Pending.Beta -= ((this.Input3ResourceBase * UpgradeTrack.GammaFactory) - (BonusInputReduction * UpgradeTrack.GammaFactory));
                                // Apply
                                Pending.Gamma += ((this.OutputResourceBase * UpgradeTrack.GammaFactory) * BonusProduction);
                        }
                }

                if (this.Resource == Resources.Alpha && Halt) {
                        UpgradeTrack.AlphaFactoryStatus = false;
                } else if (this.Resource == Resources.Alpha && !Halt) {
                        UpgradeTrack.AlphaFactoryStatus = true;
                } else if (this.Resource == Resources.Beta && Halt) {
                        UpgradeTrack.BetaFactoryStatus = false;
                } else if (this.Resource == Resources.Beta && !Halt) {
                        UpgradeTrack.BetaFactoryStatus = true;
                } else if (this.Resource == Resources.Gamma && Halt) {
                        UpgradeTrack.GammaFactoryStatus = false;
                } else if (this.Resource == Resources.Gamma && !Halt) {
                        UpgradeTrack.GammaFactoryStatus = true;
                }
        }
}

public static class EventSystem
{
        public static int EventToShow = 0; // for panel control, 1x is reserved for Alpha. 2x for beta, 3x for Gamma, and 1-9 for essence, 50 Above is for random events
        public static int EssenceForcedEventDone = 0; // up to 9
        public static int AlphaForcedEventDone = 0;
        public static int BetaForcedEventDone = 0;
        public static int GammaForcedEventDone = 0;
        public static bool CanShowEvent = true; // whether x sec from last event has passed
        public static bool ForcedEventWantToShow = false; // so forced event always take precidence
        public static bool RandomEventCanShow = false;

        private enum Events {
                AlphaEvent1,
                AlphaEvent2,
                BetaEvent1,
                BetaEvent2,
                GammaEvent1,
                GammaEvent2,

                // the rest is W.I.P
                AlphaEventPos,
                AlphaEventNeg,
                BetaEventPos,
                BetaEventNeg,
                GammaEventPos,
                GammaEventNeg,
        }

        private static void ApplyEffect(Events Do) {
                if (Do == Events.AlphaEvent1) {
                        if (AlphaForcedEventDone != 1) return; // only allowed if 1 is current event
                        UpgradeTrack.AlphaFactoryCost = UpgradeTrack.AlphaFactoryBaseCost * 2;
                } else if (Do == Events.AlphaEvent2) {
                        if (AlphaForcedEventDone != 2) return;
                        UpgradeTrack.AlphaFactoryCost = (UpgradeTrack.AlphaFactoryBaseCost * 2.0) * 1.80; // 80%
                } else if (Do == Events.BetaEvent1) {
                        if (BetaForcedEventDone != 1) return;
                        UpgradeTrack.BetaFactoryCost = UpgradeTrack.BetaFactoryBaseCost * 2.50; // 150%
                } else if (Do == Events.BetaEvent2) {
                        if (BetaForcedEventDone != 2) return;
                        UpgradeTrack.BetaFactoryCost = (UpgradeTrack.BetaFactoryBaseCost * 2.50) * 1.80; // 80%
                } else if (Do == Events.GammaEvent1) {
                        if (GammaForcedEventDone != 1) return;
                        UpgradeTrack.GammaFactoryCost = UpgradeTrack.GammaFactoryBaseCost * 2.30; // 130%
                } else if (Do == Events.GammaEvent2) {
                        if (GammaForcedEventDone != 2) return;
                        UpgradeTrack.GammaFactoryCost = (UpgradeTrack.GammaFactoryBaseCost * 2.30) * 1.90; // 90%
                } else {
                        return; // cant really print to console, since we dont have a dedicated log Panel
                }

                // I know, HardCoded and Disgusting
        }

        private static int EventCooldown = 0;

        public static void RefreshEvent() {
                if (ForcedEventWantToShow == true && CanShowEvent == false) {
                        EventCooldown++;
                        // increment cooldown when
                        if (EventCooldown >= 10) {
                                ForcedEventWantToShow = false;
                                EventCooldown = 0;
                                CanShowEvent = true;
                                RandomEventCanShow = true;
                        }
                }
        }

        public static void ForcedAlphaEventHandler() {
                if (UpgradeTrack.AlphaFactory >= 10 && AlphaForcedEventDone == 0 && CanShowEvent && ForcedEventWantToShow == false) { // checks Current factory amount, whether this has been done, and if it can show events, and other ForcedEvents doesnt wanna show
                        EventToShow = 10; // what Panel to use
                        AlphaForcedEventDone = 1; // adds so next AlphaForcedEvent can just skip this

                        // after specifying what event to show, and updating progress, declare that forced event wants to show, and reset even timer to zero
                        ForcedEventWantToShow = true;
                        CanShowEvent = false; // stops other from showing events
                        RandomEventCanShow = false;
                        ApplyEffect(Events.AlphaEvent1);
                        EventCooldown = 0; // set to 0, 10ticks before new event can show
                        return;
                } else if (UpgradeTrack.AlphaFactory >= 30 && AlphaForcedEventDone == 1 && CanShowEvent && ForcedEventWantToShow == false) {
                        EventToShow = 11;
                        AlphaForcedEventDone = 2;

                        ForcedEventWantToShow = true;
                        CanShowEvent = false;
                        RandomEventCanShow = false;
                        ApplyEffect(Events.AlphaEvent2);
                        EventCooldown = 0;
                        return;
                }
        }

        public static void ForcedBetaEventHandler() {
                if (UpgradeTrack.BetaFactory >= 20 && BetaForcedEventDone == 0 && CanShowEvent && ForcedEventWantToShow == false) {
                        EventToShow = 20;
                        BetaForcedEventDone = 1;
                        ForcedEventWantToShow = true;
                        CanShowEvent = false;
                        RandomEventCanShow = false;
                        ApplyEffect(Events.BetaEvent1);
                        EventCooldown = 0;
                        return;
                } else if (UpgradeTrack.BetaFactory >= 50 && BetaForcedEventDone == 1 && CanShowEvent && ForcedEventWantToShow == false) {
                        EventToShow = 21;
                        BetaForcedEventDone = 2;
                        ForcedEventWantToShow = true;
                        CanShowEvent = false;
                        RandomEventCanShow = false;
                        ApplyEffect(Events.BetaEvent2);
                        EventCooldown = 0;
                        return;
                }
        }

        public static void ForcedGammaEventHandler() {
                if (UpgradeTrack.GammaFactory >= 10 && GammaForcedEventDone == 0 && CanShowEvent && ForcedEventWantToShow == false) {
                        EventToShow = 30;
                        GammaForcedEventDone = 1;
                        ForcedEventWantToShow = true;
                        CanShowEvent = false;
                        RandomEventCanShow = false;
                        ApplyEffect(Events.GammaEvent1);
                        EventCooldown = 0;
                        return;
                } else if (UpgradeTrack.GammaFactory >= 30 && GammaForcedEventDone == 1 && CanShowEvent && ForcedEventWantToShow == false) {
                        EventToShow = 31;
                        GammaForcedEventDone = 2;
                        ForcedEventWantToShow = true;
                        CanShowEvent = false;
                        RandomEventCanShow = false;
                        ApplyEffect(Events.GammaEvent2);
                        EventCooldown = 0;
                        return;
                }
        }

        public static void RandomEventHandler() {
                // TODO : make the random chance event thingy here
                return;
        }
        // flow : on Program or wherever this will get called on, call it first to refresh then forced event, then Random
}

public class GameUI
{
        public Layout InitGameLayout() {
                var GameLayout = new Layout("GameRoot")
                .SplitColumns (
                        new Layout("GameLeft").SplitRows(
                                new Layout("GameTopLeft"),
                                                         new Layout("GameBottomLeft")
                        ),
                        new Layout("GameRight").SplitRows(
                                new Layout("GameTopRight"),
                                                          new Layout("GameBottomRight")
                        )
                );

                // GameLayout["LayoutPlace"].Update(Panel);
                GameLayout["GameTopLeft"].Update(Panels.BuildStatPanel());
                GameLayout["GameBottomLeft"].Update(Tables.GameBuildFactoryTable());
                GameLayout["GameBottomRight"].Update(Tables.GameBuildUpgradeTable());

                int EventToShow = EventSystem.EventToShow;
                if (EventToShow == 10) { // no need for Checks, even system already handles the line
                        GameLayout["GameTopRight"].Update(Panels.AlphaForcedEvent1());
                } else if (EventToShow == 11) {
                        GameLayout["GameTopRight"].Update(Panels.AlphaForcedEvent2());
                } else if (EventToShow == 20) {
                        GameLayout["GameTopRight"].Update(Panels.BetaForcedEvent1());
                } else if (EventToShow == 21) {
                        GameLayout["GameTopRight"].Update(Panels.BetaForcedEvent2());
                } else if (EventToShow == 30) {
                        GameLayout["GameTopRight"].Update(Panels.GammaForcedEvent1());
                } else if (EventToShow == 31) {
                        GameLayout["GameTopRight"].Update(Panels.GammaForcedEvent2());
                } else {
                        GameLayout["GameTopRight"].Update(Panels.EmptyEvent());
                }

                return GameLayout;
        }
}

public class ShopUI
{
        public Layout ShopMenuLayout() {
                var ShopLayout = new Layout("ShopRoot")
                .SplitColumns(
                        new Layout("ShopLeft"), // 68W, 32H
                              new Layout("ShopRight").SplitRows(
                                      new Layout("ShopTopRight"), // 69W, 16H
                                                                new Layout("ShopBottomRight")
                              )
                );



                ShopLayout["ShopLeft"].Update(Panels.ShopBuildShopMenu());
                ShopLayout["ShopBottomRight"].Update(Panels.BuildStatPanel());

                if (GameState.MenuID >= 1.0f && GameState.MenuID < 999.888f) {
                        int Entry = GameState.MenuID switch {
                                1.1f => 1,
                                1.2f => 2,
                                1.3f => 3,
                                1.4f => 4,
                                1.5f => 5,
                                1.6f => 6,
                                1.7f => 7,
                                _ => 0
                        };

                        ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(Entry));
                }

                if (GameState.MenuID == 999.998f) {
                        ShopLayout["ShopTopRight"].Update(Panels.ShopBuildBuyFeedback(0)); // success
                } else if (GameState.MenuID == 999.997f) {
                        ShopLayout["ShopTopRight"].Update(Panels.ShopBuildBuyFeedback(1)); // Fail cuz broke
                } else if (GameState.MenuID == 999.996f) {
                        ShopLayout["ShopTopRight"].Update(Panels.ShopBuildBuyFeedback(2)); // Fail cuz broke
                }

                return ShopLayout;

        }


}


public static class Panels
{
        public static Panel BuildStatPanel() {
                // var "PanelName" = new Panel("string to use");
                var GameStatPanel = new Panel(StringsStuff.GamePanelStats);
                // {PanelName}.{Attribute} = {Value};
                GameStatPanel.Width = 70;
                GameStatPanel.Height = 16;
                GameStatPanel.Header = new PanelHeader(" Game : Stat Menu");
                return GameStatPanel;
        }

        public static Panel ShopBuildEntryPanel(int panel) {
                Panel entry = panel switch {
                        1 => new Panel(StringsStuff.ShopEntryPanel1),
                        2 => new Panel(StringsStuff.ShopEntryPanel2),
                        3 => new Panel(StringsStuff.ShopEntryPanel3),
                        4 => new Panel(StringsStuff.ShopEntryPanel4),
                        5 => new Panel(StringsStuff.ShopEntryPanel5),
                        6 => new Panel(StringsStuff.ShopEntryPanel6),
                        7 => new Panel(StringsStuff.ShopEntryPanel7),
                        _ => new Panel("No entry chosen")
                };

                entry.Width = 71;
                entry.Height = 16;
                entry.Header = new PanelHeader($" Shop Menu : Entry {panel}");

                return entry;
        }

        public static Panel ShopBuildBuyFeedback(int result) {
                Panel DaResult = result switch {
                        0 => new Panel($" Successfully Bought! "),
                        1 => new Panel($" Cannot Afford! "),
                        2 => new Panel($" An Error Occured! "),
                        _ => new Panel($" some dumbass used the wrong argument ")
                };

                return DaResult;
        }

        public static Panel ShopBuildShopMenu() {
                var ShopMenu = new Panel(StringsStuff.ShopMainPanel);

                ShopMenu.Header = new PanelHeader(" Shop Menu ");
                ShopMenu.Width = 67;
                ShopMenu.Height = 32;

                return ShopMenu;
        }

        public static Panel EmptyEvent() {
                var EmptyEventPanel = new Panel(StringsStuff.EmptyEvent);

                EmptyEventPanel.Width = 70;
                EmptyEventPanel.Height = 16;
                EmptyEventPanel.Header = new PanelHeader(" Game : Event Menu ");

                return EmptyEventPanel;
        }

        public static Panel AlphaForcedEvent1() {
                var AlphaForcedEvent1 = new Panel(StringsStuff.ForcedAlphaEvent1);

                AlphaForcedEvent1.Width = 70;
                AlphaForcedEvent1.Height = 16;
                AlphaForcedEvent1.Header = new PanelHeader(" Game : Event Menu ");

                return AlphaForcedEvent1;
        }

        public static Panel AlphaForcedEvent2() {
                var AlphaForcedEvent2 = new Panel(StringsStuff.ForcedAlphaEvent2);

                AlphaForcedEvent2.Width = 70;
                AlphaForcedEvent2.Height = 16;
                AlphaForcedEvent2.Header = new PanelHeader(" Game : Event Menu ");

                return AlphaForcedEvent2;
        }

        public static Panel BetaForcedEvent1() {
                var BetaForcedEvent1 = new Panel(StringsStuff.ForcedBetaEvent1);

                BetaForcedEvent1.Width = 70;
                BetaForcedEvent1.Height = 16;
                BetaForcedEvent1.Header = new PanelHeader(" Game : Event Menu ");

                return BetaForcedEvent1;
        }

        public static Panel BetaForcedEvent2() {
                var BetaForcedEvent2 = new Panel(StringsStuff.ForcedBetaEvent2);

                BetaForcedEvent2.Width = 70;
                BetaForcedEvent2.Height = 16;
                BetaForcedEvent2.Header = new PanelHeader(" Game : Event Menu ");

                return BetaForcedEvent2;
        }

        public static Panel GammaForcedEvent1() {
                var GammaForcedEvent1 = new Panel(StringsStuff.ForcedGammaEvent1);

                GammaForcedEvent1.Width = 70;
                GammaForcedEvent1.Height = 16;
                GammaForcedEvent1.Header = new PanelHeader(" Game : Event Menu ");

                return GammaForcedEvent1;
        }

        public static Panel GammaForcedEvent2() {
                var GammaForcedEvent2 = new Panel(StringsStuff.ForcedGammaEvent2);

                GammaForcedEvent2.Width = 70;
                GammaForcedEvent2.Height = 16;
                GammaForcedEvent2.Header = new PanelHeader(" Game : Event Menu ");

                return GammaForcedEvent2;
        }
}

public static class Tables
{
        public static Table GameBuildFactoryTable() {
                var FactoryTable = new Table();

                FactoryTable.AddColumn("[white] Factory [/]"); // Make Columns (the vertical slices)
                FactoryTable.AddColumn("[white] Amount [/]");
                FactoryTable.AddColumn("[white] Status [/]");

                FactoryTable.AddRow(
                        "[yellow] Alpha [/]", // refer to Colums made
                        UpgradeTrack.AlphaFactory.ToString(),
                                    UpgradeTrack.AlphaFactoryStatus ? "[green]▶ Running [/]" : "[red]■ Halted [/]"
                );
                FactoryTable.AddRow(
                        "[blue] Beta [/]",
                        UpgradeTrack.BetaFactory.ToString(),
                                    UpgradeTrack.BetaFactoryStatus ? "[green]▶ Running [/]" : "[red]■ Halted [/]"
                );
                FactoryTable.AddRow(
                        "[green] Gamma [/]",
                        UpgradeTrack.GammaFactory.ToString(),
                                    UpgradeTrack.GammaFactoryStatus ? "[green]▶ Running [/]" : "[red]■ Halted [/]"
                );

                FactoryTable.Border = TableBorder.Rounded;
                FactoryTable.Width = 70;

                return FactoryTable;

        }

        public static Table GameBuildUpgradeTable() {
                var UpgradeTable = new Table();

                // FactoryTable.AddColumn("[white] Amount [/]");

                UpgradeTable.AddColumn("[white] Upgrade [/]");
                UpgradeTable.AddColumn("[white] Bought [/]");
                UpgradeTable.AddColumn("[white] Effects [/]");

                UpgradeTable.AddRow(
                        "[cyan]Essence[/] Base Upgrade", UpgradeTrack.EssenceBaseBought.ToString(), "Temp"
                );
                UpgradeTable.AddRow(
                        "[cyan]Essence[/] Multiplier Upgrade", UpgradeTrack.EssenceMultiplierBought.ToString(), "Temp"
                );
                UpgradeTable.AddRow(
                        "[purple]Factory Input Upgrade[/]", UpgradeTrack.FactoryInputUpgradeBought.ToString(), "Temp"
                );
                UpgradeTable.AddRow(
                        "[purple]Factory Output Upgrade[/]", UpgradeTrack.FactoryOutputUpgradeBought.ToString(), "Temp"
                );

                UpgradeTable.Border = TableBorder.Rounded;
                UpgradeTable.Width = 70;

                return UpgradeTable;
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

public static class StringsStuff
{
        public static string GamePanelStats =>
        $"[white][/]\n" +
        $"[cyan]Essence : {EssenceWallet.Amount}[/]\n" +
        $"[white][/]\n" +
        $"[yellow]Alpha : {AlphaWallet.Amount}[/]\n" +
        $"[blue]Beta : {BetaWallet.Amount}[/]\n" +
        $"[green]Gamma : {GammaWallet.Amount}[/]\n" +
        $"\n" +
        $"\n"
        // $"Debug\n" +
        // $"Event to show : {EventSystem.EventToShow.ToString()}\n" +
        // $"Alpha Forced Event Done : {EventSystem.AlphaForcedEventDone.ToString()}\n" +
        // $"Event Incrementer : {EventIncrement.ToString()}\n" +
        // $"Can Show : {EventSystem.CanShowEvent.ToString()}\n" +
        // $"ForcedEventWantToShow : {EventSystem.ForcedEventWantToShow.ToString()}\n"
        ;

        public static string ShopMainPanel =>
        $"Here, you can Buy more Factories, Essence Upgrades (and Factory upgrades in the future).\n" +
        $"\n" +
        $"\n" +
        $" > Factories < \n" +
        $"  -> [yellow] Alpha Factory [/] Press 1 to see\n" +
        $"  -> [blue] Beta Factory [/] Press 2 to see\n" +
        $"  -> [green] Gamma Factory [/] Press 3 to see\n" +
        $"\n" +
        $"\n" +
        $" > [blue]Essence[/] Upgrades < \n" +
        $"\n" +
        $" -> [cyan]Essence Base Production[/] Press 4 to see\n" +
        $" -> [cyan]Essence Multiplier[/] Press 5 to see\n" +
        $"\n" +
        $"\n" +
        $" > Factory Upgrade < \n" +
        $"\n" +
        $" -> [purple]Make Factory input system safer[/] Press 6 to see \n" +
        $" -> [purple]Make Factory Production line Smarter Press[/] 7 to see \n"
        ;

        public static string ShopEntryPanel1 =>
        $"[yellow] Alpha Factory [/]\n" +
        $"\n" +
        $"Description : \n" +
        $" - A factory that Consumes [cyan]1 Essence[/] to produce [yellow]1 Alpha[/] Per tick.\n" +
        $"\n" +
        $"Cost : [cyan]{UpgradeTrack.AlphaFactoryCost}[/]\n" +
        $"" +
        $"You currently have : [yellow]{UpgradeTrack.AlphaFactory}[/] Factories\n" +
        $"\n" +
        $"Press ENTER to Purchase\n" +
        $"Press B to Go back\n"
        ;

        public static string ShopEntryPanel2 =>
        $"[blue] Beta Factory [/]\n" +
        $"\n" +
        $"Description : \n" +
        $" - A factory that Consumes [yellow]1 Alpha[/] to produce [blue]1 Beta[/] Per tick.\n" +
        $"\n" +
        $"Cost : [cyan]{UpgradeTrack.BetaFactoryCost}[/]\n" +
        $"" +
        $"You currently have : [blue]{UpgradeTrack.BetaFactory}[/] Factories\n" +
        $"\n" +
        $"Press ENTER to Purchase\n" +
        $"Press B to Go back\n"
        ;

        public static string ShopEntryPanel3 =>
        $"[green] Gamma Factory [/]\n" +
        $"\n" +
        $"Description : \n" +
        $" - A factory that Consumes [yellow]1 Alpha[/] and [blue]1 Beta [/]to produce [green]1 Gamma[/] Per tick.\n" +
        $"\n" +
        $"Cost : [cyan]{UpgradeTrack.GammaFactoryCost}[/]\n" +
        $"" +
        $"You currently have : [green]{UpgradeTrack.GammaFactory}[/] Factories\n" +
        $"\n" +
        $"Press ENTER to Purchase\n" +
        $"Press B to Go back\n"
        ;

        public static string ShopEntryPanel4 =>
        $"[cyan] Essence Base Production [/]\n" +
        $"\n" +
        $"Description : \n" +
        $" - [cyan]Essence[/] Is produced at the rate of Base multiplied by a Multiplier ( E = Base*multiplier ), buying this adds +1 Essence per tick times {UpgradeTrack.EssenceMultiplierBought}\n" +
        $"\n" +
        $"Cost : [yellow]{UpgradeTrack.EssenceBaseCost} Alpha[/]\n" +
        $"" +
        $"You currently have : [cyan]{UpgradeTrack.EssenceBaseBought} Base Essence Production[/]\n" +
        $"\n" +
        $"Press ENTER to Purchase\n" +
        $"Press B to Go back\n"
        ;

        public static string ShopEntryPanel5 =>
        $"[cyan] Essence Multiplier [/]\n" +
        $"\n" +
        $"Description : \n" +
        $" - Adds A Multiplier for [cyan]Essence[/] Production\n" +
        $"\n" +
        $"Cost : [blue]{UpgradeTrack.EssenceMultiplierCost} Beta[/]\n" +
        $"" +
        $"You currently have : [cyan]{UpgradeTrack.EssenceMultiplierBought} Essence Multiplier[/]\n" +
        $"\n" +
        $"Press ENTER to Purchase\n" +
        $"Press B to Go back\n"
        ;

        public static string ShopEntryPanel6 =>
        $"[purple] Factory Input mechanism [/]\n" +
        $"\n" +
        $"Description : \n" +
        $" - Improving the Input Mechanism of all Factory, improving and reducing needed Resource input by 5%\n" +
        $"\n" +
        $"Cost : [green]{UpgradeTrack.FactoryInputUpgradeCost} Gamma[/]\n" +
        $"" +
        $"You currently have : [white]{UpgradeTrack.FactoryInputUpgradeBought} Upgrades Bought[/]\n" +
        $"\n" +
        $"Press ENTER to Purchase\n" +
        $"Press B to Go back\n"
        ;

        public static string ShopEntryPanel7 =>
        $"[purple] Factory Line Performance Optimisation [/]\n" +
        $"\n" +
        $"Description : \n" +
        $" - Improving the Factory Line to gain ~10% Output for the same Input some said 'why are we using an inefficient one in the first place?' \n" +
        $"\n" +
        $"Cost : [green]{UpgradeTrack.FactoryOutputUpgradeCost} Gamma[/]\n" +
        $"" +
        $"You currently have : [white]{UpgradeTrack.FactoryOutputUpgradeBought} Upgrades Bought[/]\n" +
        $"\n" +
        $"Press ENTER to Purchase\n" +
        $"Press B to Go back\n"
        ;

        public static string ForcedAlphaEvent1 =>
        $"Alpha Factory Licensing Changes\n" +
        $"\n" +
        $"The [purple]Council[/] has made Changes upon the discovery of Total Alpha Prodcution in the 'PlaceHolder' Sector.. Alpha Factory Prices has been [red]Permanently raised by 100%.[/]\n" +
        $"\n"
        ;

        public static string ForcedAlphaEvent2 =>
        $"Alpha Factory Licensing Changes : A looming threat\n" +
        $"\n" +
        $"The [purple]Council[/] is pushing new Licensing changes on Alpha Factories on the 'PlaceHolder' sector.. Alpha Factory Prices is [red]Permanently raised by 80%[/].\n" +
        $"\n"
        ;

        public static string ForcedBetaEvent1 =>
        $"Beta Factory Licensing Changes\n" +
        $"\n" +
        $"After a board meeting, The [purple]Council[/] has Decided to raise Beta Factory licensing prices to a [red]staggering 150%[/], [bold]'Wouldve been smarter to have bought them in bulk earlier....'[/]\n" +
        $"\n"
        ;

        public static string ForcedBetaEvent2 =>
        $"Beta Factory Cost Raise\n" +
        $"\n" +
        $"Due to some bureaucratic tomfoolery, The Market Cost for Beta factories is [red]raised by 80%[/]....\n" +
        $"\n"
        ;

        public static string ForcedGammaEvent1 =>
        $"Gamma Factory Raised costs\n" +
        $"\n" +
        $"Due to the Rarity and difficulty to Produce Gamma, a Sector spread Panic buying is happening, causing for Gamma Factory cost to Skyrocket up to [red]130% price increase[/]!\n" +
        $"\n"
        ;

        public static string ForcedGammaEvent2 =>
        $"Gamma Factory Construction Material\n" +
        $"\n" +
        $"An Event happened Causing Gamma Factory Construction materials to be rarer, increasing demand causes price to [red]Rise by 90%[/]\n" +
        $"\n"
        ;

        public static string EmptyEvent =>
        $"No event has happened yet."
        ;
}
