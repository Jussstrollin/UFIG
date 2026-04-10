/*
 [/] AlphaFactoryCalculations
 [/] BetaFactoryCalc
 [/] GammaFactoryCalc
 [/] Finish Shop basic functionalities
 [/] Essence Upgrades
 [/] Start making the early Game loop
 [] Add texts, Start sequences, doialogues and stuff.
 [/] Finally add all shop texts
 [ish] Polish UI
 ... Later, further here is midgame stuff, goal before? have a fun Gameloop! ...

 Dont mind :
        Add Unlocing of stuff with Research, Alpha, beta, and gamma as 'cost' to unlock new shit (its complex lol)

        make essence roduction act as mines, so it takes time to get shit, same goes for Factorues, so its not producing stuff per tick

 Note : For naming, Im using Pascal Case which just means, for example "Velocity = x;" starts with capital letters, and for multiple words, each "separate" word starts with capital letters like (IsFlying = true;)
*/



using System;
using System.IO;
using System.Text.Json;
using Spectre.Console;

class Program
{
        static float Essence = 0.0f;

        static DateTime LastGameTick = DateTime.Now;
        static DateTime LastDisplayTick = DateTime.Now;

        struct ResourceDelta {
                public float Alpha;
                public float Beta;
                public float Gamma;
                public float Essence;
        }

        struct ResourceBP {
                public float Amount;
                public float TotalProdPerTick;
        }

        struct UpgradeTrackBP {
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

                public float EssenceBaseCost;
                public int EssenceBaseBought;
                public float EssenceMultiplierCost;
                public int EssenceMultiplierBought;
        }

        struct GameStateBP {
                public float MenuID;
                public int Progress;

                public bool Pause;
                public bool Stop;
        }

        // ============================================== //

        static ResourceDelta Pending = new ResourceDelta(); // inits all to Zero

        static ResourceBP AlphaStuff = new ResourceBP {
                Amount = 1.0f,
                TotalProdPerTick = 0.0f
        };

        static ResourceBP BetaStuff = new ResourceBP {
                Amount = 0.0f,
                TotalProdPerTick = 0.0f
        };

        static ResourceBP GammaStuff = new ResourceBP {
                Amount = 0.0f,
                TotalProdPerTick = 0.0f
        };

        static UpgradeTrackBP UpgradeTrack = new UpgradeTrackBP { // Handles every Upgrades info
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
                EssenceMultiplierCost = 50
        };

        static GameStateBP GameState = new GameStateBP {
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

                        Thread.Sleep(10);
                }
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

        static void AlphaFactoryCalc() {
                // All factory is the same structure, this is the documentation for all of em

                // Temporary implementation!!
                // if required essence isnt met to run all Factory, will halt Production
                // later, if im wise enough, i should be able to implement and only see how many Factories we can supply, so essence bottleneck isnt so hard.
                bool HaltFactory = false;
                int InputEssence = 1 * UpgradeTrack.AlphaFactory; // so more alpha fact., more essence needed
                int ProducedAlpha = 1 * UpgradeTrack.AlphaFactory;

                // stop if Input aint enuf
                if (InputEssence > Essence || UpgradeTrack.AlphaFactory == 0) HaltFactory = true;

                if (HaltFactory) {
                        UpgradeTrack.AlphaFactoryStatus = false;
                        return;
                } else {
                        // Structure : Cost first then produce
                        Pending.Essence -= InputEssence;

                        Pending.Alpha += ProducedAlpha;

                        UpgradeTrack.AlphaFactoryStatus = true;
                }
        }

        static void BetaFactoryCalc() {
                bool HaltFactory = false;
                int InputAlpha = 1 * UpgradeTrack.BetaFactory;
                int ProducedBeta = 1 * UpgradeTrack.BetaFactory;

                if (InputAlpha > AlphaStuff.Amount || UpgradeTrack.BetaFactory == 0) HaltFactory = true;

                if (HaltFactory) { // if HaltFactory == true;
                        UpgradeTrack.BetaFactoryStatus = false;
                        return;
                } else {
                        Pending.Alpha -= InputAlpha;

                        Pending.Beta += ProducedBeta;

                        UpgradeTrack.BetaFactoryStatus = true;
                }
        }

        static void GammaFactoryCalc() {
                bool HaltFactory = false;
                int InputAlpha = 1 * UpgradeTrack.GammaFactory;
                int InputBeta = 1 * UpgradeTrack.GammaFactory;
                int InputEssence = 2 * UpgradeTrack.GammaFactory;
                int ProducedGamma = 1 * UpgradeTrack.GammaFactory;

                if (InputAlpha > AlphaStuff.Amount || InputBeta > BetaStuff.Amount || InputEssence > Essence || UpgradeTrack.GammaFactory == 0) {
                        HaltFactory = true;
                }

                if (HaltFactory) {
                        UpgradeTrack.GammaFactoryStatus = false;
                        return;
                } else {
                        Pending.Alpha -= InputAlpha;
                        Pending.Beta -= InputBeta;
                        Pending.Essence -= InputEssence;

                        Pending.Gamma += ProducedGamma;

                        UpgradeTrack.GammaFactoryStatus = true;
                }


        }

        static void PushPending() {
                AlphaStuff.Amount += Pending.Alpha;
                BetaStuff.Amount += Pending.Beta;
                GammaStuff.Amount += Pending.Gamma;
                Essence += Pending.Essence;
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
                float EssenceMultipler = 1.0f * UpgradeTrack.EssenceMultiplierBought;
                float EssenceGain = EssenceBase * EssenceMultipler;
                Pending.Essence += EssenceGain;

                // Factories
                AlphaFactoryCalc();
                BetaFactoryCalc();
                GammaFactoryCalc();

                // Production calculation to actualy be added
                PushPending();

        }

        static int WannaBuy(string Upgrade) {
                if (Upgrade == "BuyAlphaFactory") {
                        if (UpgradeTrack.AlphaFactoryCost <= Essence) { //afford
                                Essence -= UpgradeTrack.AlphaFactoryCost; // deduct
                                UpgradeTrack.AlphaFactory ++; // add
                                return 1; // bought
                        } else { // cant afford
                                return 0; // Nuh uh
                        }
                }

                if (Upgrade == "BuyBetaFactory") {
                        if (UpgradeTrack.BetaFactoryCost <= Essence) { //afford
                                Essence -= UpgradeTrack.BetaFactoryCost;
                                UpgradeTrack.BetaFactory ++;
                                return 1;
                        } else { // cant afford
                                return 0;
                        }
                }

                if (Upgrade == "BuyGammaFactory") {
                        if (UpgradeTrack.GammaFactoryCost <= Essence) { //afford
                                Essence -= UpgradeTrack.GammaFactoryCost;
                                UpgradeTrack.GammaFactory ++;
                                return 1;
                        } else { // cant afford
                                return 0;
                        }
                }

                if (Upgrade == "BuyEssenceBase") {
                        if (UpgradeTrack.EssenceBaseCost <= AlphaStuff.Amount) { // can afford
                                AlphaStuff.Amount -= UpgradeTrack.EssenceBaseCost;
                                UpgradeTrack.EssenceBaseBought++;
                                return 1;
                        } else {
                                return 0; // broke ass
                        }
                }

                if (Upgrade == "BuyEssenceMultiplier") {
                        if (UpgradeTrack.EssenceMultiplierCost <= BetaStuff.Amount) { // can afford
                                BetaStuff.Amount -= UpgradeTrack.EssenceMultiplierCost;
                                UpgradeTrack.EssenceMultiplierCost++;
                                return 1;
                        } else {
                                return 0; // broke ass
                        }
                }

                return -1; // some wierd happened
        }

        static void HandleDisplay() {
                int TerminalWidth = Console.WindowWidth;
                int TerminalHeight = Console.WindowHeight;

                AnsiConsole.Clear();

                // ============== Game Panel Stuff ==================== //

                var GameStatPanel = new Panel(
                        $"[white][/]\n" +
                        $"[cyan]Essence : {Essence}[/]\n" +
                        $"[white][/]\n" +
                        $"[yellow]Alpha : {AlphaStuff.Amount}[/]\n" +
                        $"[blue]Beta : {BetaStuff.Amount}[/]\n" +
                        $"[green]Gamma : {GammaStuff.Amount}[/]\n"
                );
                GameStatPanel.Width = 67;
                GameStatPanel.Height = 16;
                GameStatPanel.Header = new PanelHeader(" Game : Stat Menu");

                // --- //

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

                // ================= Shop Panel Stuff ================== //
                // Da Checks the goal is for HandleDisplay to just print shop and shop should handle itself
                int ChosenEntry = 0;

                if (GameState.MenuID == 1.1f) { // has Chosen the first one
                        ChosenEntry = 1;
                } else if (GameState.MenuID == 1.2f) { // has Chosen the first one
                        ChosenEntry = 2;
                } else if (GameState.MenuID == 1.3f) { // has Chosen the first one
                        ChosenEntry = 3;
                } else if (GameState.MenuID == 1.4f) { // has Chosen the first one
                        ChosenEntry = 4;
                } else if (GameState.MenuID == 1.5f) { // has Chosen the first one
                        ChosenEntry = 5;
                } else {
                        ChosenEntry = 0;
                }

                // they dont need to care if any entry is chosen
                if (GameState.MenuID == 999.998f) {
                        ChosenEntry = -1; // success to buy
                        GameState.MenuID = 1.0f;
                } else if (GameState.MenuID == 999.997f) {
                        ChosenEntry = -2; // fail to buy cuz cant afford
                        GameState.MenuID = 1.0f;
                } else if (GameState.MenuID == 999.996f) {
                        ChosenEntry = -3; // fail to buy cuz error
                        GameState.MenuID = 1.0f;
                }

                // ---- The UI ---- //
                var ShopMenu = new Panel(
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
                        $" -> [cyan]Essence Multiplier[/] Press 5 to see\n"
                );

                var ShopEntryPanel0 = new Panel(
                        $"No entry Chosen"
                );

                var ShopErrorBuying = new Panel(
                        $" An Error Occured! "
                );

                var ShopCanAfford = new Panel(
                        $" Successfully Bought! "
                );

                var ShopCannotAfford = new Panel(
                        $" Cannot Afford Factory! "
                );

                var ShopEntryPanel1 = new Panel(
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
                );

                var ShopEntryPanel2 = new Panel(
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
                );

                var ShopEntryPanel3 = new Panel(
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
                );

                var ShopEntryPanel4 = new Panel(
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
                );

                var ShopEntryPanel5 = new Panel(
                        $"[cyan] Essence Multiplier [/]\n" +
                        $"\n" +
                        $"Description : \n" +
                        $" - Adds A Multiplier for [cyan]Essence[/] Production\n" +
                        $"\n" +
                        $"Cost : [blue]{UpgradeTrack.EssenceMultiplierCost} Beta[/]\n" +
                        $"" +
                        $"You currently have : [cyan]{UpgradeTrack.EssenceMultiplierBought} Essence Multipler[/]\n" +
                        $"\n" +
                        $"Press ENTER to Purchase\n" +
                        $"Press B to Go back\n"
                );

                ShopMenu.Header = new PanelHeader(" Shop Menu ");
                ShopMenu.Width = 67;
                ShopMenu.Height = 32;
                ShopEntryPanel1.Header = new PanelHeader(" Shop Menu : Entry 1");
                ShopEntryPanel1.Width = 71;
                ShopEntryPanel1.Height = 16;
                ShopEntryPanel2.Header = new PanelHeader(" Shop Menu : Entry 2");
                ShopEntryPanel2.Width = 71;
                ShopEntryPanel2.Height = 16;
                ShopEntryPanel3.Header = new PanelHeader(" Shop Menu : Entry 3");
                ShopEntryPanel3.Width = 71;
                ShopEntryPanel3.Height = 16;
                ShopEntryPanel4.Header = new PanelHeader(" Shop Menu : Entry 4");
                ShopEntryPanel4.Width = 71;
                ShopEntryPanel4.Height = 16;
                ShopEntryPanel5.Header = new PanelHeader(" Shop Menu : Entry 5");
                ShopEntryPanel5.Width = 71;
                ShopEntryPanel5.Height = 16;

                // ============== Layout shenanegans =========== //

                var GameLayout = new Layout("GameRoot")
                        .SplitColumns(
                                new Layout("GameLeft"), // 70W, 33H
                                new Layout("GameRight").SplitRows(
                                        new Layout("GameTopRight"), // 71W, 16H
                                        new Layout("GameBottomRight") // 71W, 17H
                                )
                        );

                var ShopLayout = new Layout("ShopRoot")
                        .SplitColumns(
                                new Layout("ShopLeft"), // 68W, 32H
                                new Layout("ShopRight").SplitRows(
                                        new Layout("ShopTopRight"), // 69W, 16H
                                        new Layout("ShopBottomRight")
                                )
                        );

                GameLayout["GameTopRight"].Update(GameStatPanel);
                GameLayout["GameBottomRight"].Update(FactoryTable);
                ShopLayout["ShopBottomRight"].Update(GameStatPanel);
                ShopLayout["ShopLeft"].Update(ShopMenu);

                // Shop entry handles (choosing)
                if (ChosenEntry == 1) {
                        ShopLayout["ShopTopRight"].Update(ShopEntryPanel1);
                } else if (ChosenEntry == 2) {
                        ShopLayout["ShopTopRight"].Update(ShopEntryPanel2);
                } else if (ChosenEntry == 3) {
                        ShopLayout["ShopTopRight"].Update(ShopEntryPanel3);
                } else if (ChosenEntry == 4) {
                        ShopLayout["ShopTopRight"].Update(ShopEntryPanel4);
                } else if (ChosenEntry == 5) {
                        ShopLayout["ShopTopRight"].Update(ShopEntryPanel5);
                }else if (ChosenEntry == 0) { // Default
                        ShopLayout["ShopTopRight"].Update(ShopEntryPanel0);
                }

                // Buy feedback
                if (ChosenEntry == -1) { // success
                        ShopLayout["ShopTopRight"].Update(ShopCanAfford);
                } else if (ChosenEntry == -2) {
                        ShopLayout["ShopTopRight"].Update(ShopCannotAfford);
                } else if (ChosenEntry == -3) {
                        ShopLayout["ShopTopRight"].Update(ShopErrorBuying);
                }

                // ================ Actually print it ============ //

                if (GameState.MenuID == 0.0f) {
                        AnsiConsole.Write(GameLayout);
                }

                if (GameState.MenuID >= 1.0f) {
                        AnsiConsole.Write(ShopLayout);
                }

                if (GameState.MenuID == 999.999f) {
                        ExitSequence();
                }
        }

        static void Save() {
                var ToBeSaved = new {
                        Alpha = AlphaStuff.Amount,
                        Beta = BetaStuff.Amount,
                        Gamma = GammaStuff.Amount,
                        Essence = Essence,

                        AlphaFactory = UpgradeTrack.AlphaFactory,
                        BetaFactory = UpgradeTrack.BetaFactory,
                        GammaFactory = UpgradeTrack.GammaFactory,

                        EssenceBase = UpgradeTrack.EssenceBaseBought,
                        EssenceMultiplier = UpgradeTrack.EssenceMultiplierBought
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
                        Essence = SaveData.GetProperty("Essence").GetSingle();
                        AlphaStuff.Amount = SaveData.GetProperty("Alpha").GetSingle();
                        BetaStuff.Amount = SaveData.GetProperty("Beta").GetSingle();
                        GammaStuff.Amount = SaveData.GetProperty("Gamma").GetSingle();

                        UpgradeTrack.AlphaFactory = SaveData.GetProperty("AlphaFactory").GetInt32();
                        UpgradeTrack.BetaFactory = SaveData.GetProperty("BetaFactory").GetInt32();
                        UpgradeTrack.GammaFactory = SaveData.GetProperty("GammaFactory").GetInt32();

                        UpgradeTrack.EssenceBaseBought = SaveData.GetProperty("EssenceBase").GetInt32();
                        UpgradeTrack.EssenceMultiplierBought = SaveData.GetProperty("EssenceMultiplier").GetInt32();

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
                } else if (GameState.MenuID == 1.0f || GameState.MenuID == 1.0f || GameState.MenuID == 1.1f || GameState.MenuID == 1.2f || GameState.MenuID == 1.3f || GameState.MenuID == 1.4f || GameState.MenuID == 1.5f) {
                        if (Key == 'S') GameState.MenuID = 0.0f;
                }

                // ==== Shop Functions ==== //

                if (GameState.MenuID == 1.0f || GameState.MenuID == 1.1f || GameState.MenuID == 1.2f || GameState.MenuID == 1.3f || GameState.MenuID == 1.4f || GameState.MenuID == 1.5f) { // Shop entry choosing
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
                        }
                }

                // ShopGoBack
                if (GameState.MenuID == 1.1f || GameState.MenuID == 1.2f || GameState.MenuID == 1.3f || GameState.MenuID == 1.4f || GameState.MenuID == 1.5f) {
                        if (Key == 'B') GameState.MenuID = 1.0f;
                }

                // Shop Buy and Feedbacks
                if (GameState.MenuID == 1.1f) { // AlphaFactoryPage
                        if (Key == '\r') { // wanabuy
                                int result = WannaBuy("BuyAlphaFactory");

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
                                int result = WannaBuy("BuyBetaFactory");

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
                                int result = WannaBuy("BuyGammaFactory");

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
                                int result = WannaBuy("BuyEssenceBase");

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
                                int result = WannaBuy("BuyEssenceMultiplier");

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
