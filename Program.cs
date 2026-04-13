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
 * ... Later, further here is midgame stuff, goal before? have a fun Gameloop! ...
 *
 * Dont mind :
 *       Add Unlocing of stuff with Research, Alpha, beta, and gamma as 'cost' to unlock new shit (its complex lol)
 *
 *       make essence roduction act as mines, so it takes time to get shit, same goes for Factorues, so its not producing stuff per tick
 *
 * Note : For naming, Im using Pascal Case which just means, for example "Velocity = x;" starts with capital letters, and for multiple words, each "separate" word starts with capital letters like (IsFlying = true;)
 */



using System;
using System.IO;
using System.Text.Json;
using Spectre.Console;
using static Program;
using static StringsStuff;

public static class Program
{
        static DateTime LastGameTick = DateTime.Now;
        static DateTime LastDisplayTick = DateTime.Now;
        static DateTime LastEventTick = DateTime.Now;

        static int EventToShow = 0; // default 0 : show none

        struct ResourceDelta {
                public float Alpha;
                public float Beta;
                public float Gamma;
                public float Essence;
        }

        public struct UpgradeTrackBP {
                // Factory Cost is by default Essence

                public int AlphaFactory;
                public bool AlphaFactoryStatus;
                public int AlphaFactoryCost;
                public int BetaFactory;
                public bool BetaFactoryStatus;
                public int BetaFactoryCost;
                public int GammaFactory;
                public bool GammaFactoryStatus;
                public int GammaFactoryCost;

                // Costs diff Arcospheres, let the wannabuy() how to handle it
                public float EssenceBaseCost;
                public int EssenceBaseBought;
                public float EssenceMultiplierCost;
                public int EssenceMultiplierBought;

                // Costs Gamma
                public float FactoryInputUpgradeBought;
                public float FactoryInputUpgradeCost;
                public float FactoryOutputUpgradeBought;
                public float FactoryOutputUpgradeCost;
        }

        public struct GameStateBP {
                public float MenuID;
                public int Progress;

                public bool Pause;
                public bool Stop;
        }

        // ============================================== //

        static ResourceDelta Pending = new ResourceDelta(); // inits all to Zero

        public static ResourceBP AlphaWallet = new ResourceBP(0.0f);
        public static ResourceBP BetaWallet = new ResourceBP(0.0f);
        public static ResourceBP GammaWallet = new ResourceBP(0.0f);
        public static ResourceBP EssenceWallet = new ResourceBP(1.0f);

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

                        if ((now -LastGameTick).TotalSeconds >= 1.0) {
                                if (!GameState.Pause) {
                                        ProductionTick();
                                }

                                PauseHandler();
                                WipePending();
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

        }

        static void PauseHandler() {
                if (GameState.MenuID >= 1.0f) { // anything greater than 0.9 is in shop
                        GameState.Pause = true;
                } else {
                        GameState.Pause = false;
                }
        }

        static void ColoredWrite(string text, ConsoleColor color) {
                Console.ForegroundColor = color;
                Console.Write(text);
                Console.ResetColor();
        }

        static void FactoryCalc() {
                // get and apply input Upgrades
                float BonusInputReduction = 0.05f * UpgradeTrack.FactoryInputUpgradeBought;

                // AlphaFactory Input
                float AlphaFactoryNeedEssence = 1 - BonusInputReduction;

                // BetaFactory Input
                float BetaFactoryNeedAlpha = 1 - BonusInputReduction;

                // Gamma Factory Input
                float GammaFactoryNeedAlpha = 1 - BonusInputReduction;
                float GammaFactoryNeedBeta = 1 - BonusInputReduction;
                float GammaFactoryNeedEssence = 2 - BonusInputReduction;

                // get and apply Output Upgrades
                float BonusProduction = 1 + (0.10f * UpgradeTrack.FactoryOutputUpgradeBought);
                // if 5 Upgrade 1 + 0.50 = 1.5x production, pretty strong so id reconsider 100 gamma cost to 300 or even 500
                // will be capped at 3x production
                // or make price scaling aggressive

                float AlphaFactoryProduce = (1 * UpgradeTrack.AlphaFactory) * BonusProduction;
                float BetaFactoryProduce = (1 * UpgradeTrack.BetaFactory) * BonusProduction;
                float GammaFactoryProduce = (1 * UpgradeTrack.GammaFactory) * BonusProduction;

                bool HaltAlphaFactory = false;
                bool HaltBetaFactory = false;
                bool HaltGammaFactory = false;

                if (AlphaFactoryNeedEssence > EssenceWallet.Amount || UpgradeTrack.AlphaFactory == 0) HaltAlphaFactory = true;
                if (BetaFactoryNeedAlpha > AlphaWallet.Amount || UpgradeTrack.BetaFactory == 0) HaltBetaFactory = true;
                if (GammaFactoryNeedAlpha > AlphaWallet.Amount || GammaFactoryNeedBeta > BetaWallet.Amount || GammaFactoryNeedEssence > EssenceWallet.Amount || UpgradeTrack.GammaFactory == 0) HaltGammaFactory = true;

                // Alpha Factory block
                if (HaltAlphaFactory) {
                        UpgradeTrack.AlphaFactoryStatus = false;
                        // then skip the adding and subtracting
                } else {
                        // deduction first
                        Pending.Essence -= AlphaFactoryNeedEssence;
                        // then Produce
                        Pending.Alpha += AlphaFactoryProduce;
                        // Update status
                        UpgradeTrack.AlphaFactoryStatus = true;
                }

                if (HaltBetaFactory) {
                        UpgradeTrack.BetaFactoryStatus = false;
                } else {
                        Pending.Alpha -= BetaFactoryNeedAlpha;
                        Pending.Beta += BetaFactoryProduce;
                        UpgradeTrack.BetaFactoryStatus = true;
                }

                if (HaltGammaFactory) {
                        UpgradeTrack.GammaFactoryStatus = false;
                } else {
                        Pending.Essence -= GammaFactoryNeedEssence;
                        Pending.Alpha -= GammaFactoryNeedAlpha;
                        Pending.Beta -= GammaFactoryNeedBeta;

                        Pending.Gamma += GammaFactoryProduce;
                        UpgradeTrack.GammaFactoryStatus = true;
                }

                return;
        }

        static void PushPending() {
                AlphaWallet.Amount += Pending.Alpha;
                BetaWallet.Amount += Pending.Beta;
                GammaWallet.Amount += Pending.Gamma;
                EssenceWallet.Amount += Pending.Essence;
        }

        static void WipePending() {
                Pending.Alpha = 0;
                Pending.Beta = 0;
                Pending.Gamma = 0;
                Pending.Essence = 0;
        }

        static void ProductionTick() { // where Factory, essence and eveery production will be called
                // Source Material always first
                float EssenceBase = 1.0f * UpgradeTrack.EssenceBaseBought;
                float EssenceMultiplier = 1.0f * UpgradeTrack.EssenceMultiplierBought;
                float EssenceGain = EssenceBase * EssenceMultiplier;
                Pending.Essence += EssenceGain;

                // Factories
                FactoryCalc();

                // Production calculation to actualy be added
                PushPending();

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

                AnsiConsole.Clear();

                var GameUi = new GameUI();
                var ShopUi =  new ShopUI();

                if (GameState.MenuID == 0.0f) {
                      GameUi.InitGameLayout();
                } else {
                    ShopUi.ShopMenuLayout();
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
                        FactoryOutputUpgrade = UpgradeTrack.FactoryOutputUpgradeBought
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

                AnsiConsole.Write(GameLayout);
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

                AnsiConsole.Write(ShopLayout);
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
        public float Amount { get; set; } // get; set; tells that its readable and writable
        public float ProductionPerTick { get; set; } // meant for +/- production

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
                $"[green]Gamma : {GammaWallet.Amount}[/]\n"
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
                $"[cyan] Factory Input mechanism [/]\n" +
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
                $"[cyan] Factory Line Performance Optimisation [/]\n" +
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
}
