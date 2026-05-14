namespace UFIG;

using static Program;
using static EventSystem;

using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadConsole.Configuration;
using SadRogue.Primitives;

public class StatePlaying : ScreenSurface {
    private ControlsConsole StatPanel;
    private SadConsole.Console EventPanel;

    private ControlsConsole FactoryTableHost;
    private Table FactoryTable;

    private TimeSpan ProgressTimer = TimeSpan.Zero;
    private const double TickRate = 0.10; // Seconds Per Progress Tick

    private int AlphaFactoryProgressIncrement = 0;
    private int BetaFactoryProgressIncrement = 0;
    private int GammaFactoryProgressIncrement = 0;
    private int EssenceMinerProgressIncrement = 0;

    private ProgressBar AlphaProgressBar;
    private ProgressBar BetaProgressBar;
    private ProgressBar GammaProgressBar;
    private ProgressBar EssenceProgressBar;

    private const int PanelsWidth = 70;
    private const int PanelsHeight = 15;
    private const int TablesWidth = (int)(PanelsWidth * 1.5); // dont Say "why not use X", because I will not Listen
    private const int TablesHeight = 10;


    public StatePlaying() : base(115, 40) {
        Children.Add(new GlobalUI().ControlPanelHost);

        AssembleStatPanel();
        AssembleEventPanel();
        AssembleFactoryTable();

        CreateSkin();

        // UpdateAllSurfaces();
    }

    private void UpdateAllSurfaces() { // dis the dynamic Update Content, if i care to do Optimized Updates like changing only what changed
        UpdateStatPanel();
        UpdateProgressBars();
    }

    public override void Update(TimeSpan Delta) {
        base.Update(Delta); // <-- Needed cuz Controls doesnt get auto Updated/Gets cleared, Idunno why and dont ask me why
        ApplyPlanetBuffs();

        if (GameState.Pause) return;

        ProgressTimer += Delta;

        while (ProgressTimer >= TimeSpan.FromSeconds(TickRate)) { // a TickRate Happens (set as 0.10 before a tick happen so 10ticks per sec)
            TickFactories();
            EssenceTick();
            ProgressTimer -= TimeSpan.FromSeconds(TickRate); // reset
        }

        UpdateAllSurfaces();
    }

    public override bool ProcessKeyboard(SadConsole.Input.Keyboard KEY) {
        if (HandleGlobalBinds(KEY)) return true;

        // all it needs to do really

        return base.ProcessKeyboard(KEY);
    }


    private void TickFactories() {
        if (AlphaFactory.InputCheck()) {
            AlphaFactoryProgressIncrement++;
            if (AlphaFactoryProgressIncrement > 20) {
                AlphaFactory.RunFactory();
                AlphaFactoryProgressIncrement = 0;
            }
        }

        if (BetaFactory.InputCheck()) {
            BetaFactoryProgressIncrement++;
            if (BetaFactoryProgressIncrement > 50) {
                BetaFactory.RunFactory();
                BetaFactoryProgressIncrement = 0;
            }
        }

        if (GammaFactory.InputCheck()) {
            GammaFactoryProgressIncrement++;
            if (GammaFactoryProgressIncrement > 80) {
                GammaFactory.RunFactory();
                GammaFactoryProgressIncrement = 0;
            }
        }
    }

    private void ApplyPlanetBuffs() {
        switch (GameState.PlanetOn) {
            case Planet.Origo:
                PlanetFactoryBonus = 0.0d;
                PlanetMiningBonus = 0.0d;
                break;
            case Planet.Sterelis:
                PlanetFactoryBonus = 0.8d;
                PlanetMiningBonus = -0.9d;
                break;
            case Planet.Primaris:
                PlanetFactoryBonus = -0.80d;
                PlanetMiningBonus = 1.0d;
                break;
            case Planet.Space:
                PlanetFactoryBonus = -1.0d;
                PlanetMiningBonus = -1.0d;
                break;
            default:
                break;
        }
    }

    private void EssenceTick() {
        EssenceMinerProgressIncrement++;
        if (EssenceMinerProgressIncrement > 20) {
            EssenceProduction();
            EssenceMinerProgressIncrement = 0;
        }
    }

    private static void EssenceProduction() { // where Factory, essence and eveery production will be called                              // Source Material always first
        float EssenceBase = 1.0f * UpgradeTrack.EssenceBaseBought;
        float EssenceMultiplier = 1.0f * UpgradeTrack.EssenceMultiplierBought;
        float EssenceGain = (EssenceBase * EssenceMultiplier) * Structure.EssenceMiner;
        Pending.Essence += EssenceGain;
        NetProd.Essence += EssenceGain;

        PushPending();
        WipePending();
    }

    private void CreateSkin() {
        int Glyph = '-';

        // --- //

        StatConsoleHelper(0, 0, "Alpha", AlphaWallet);
        StatConsoleHelper(0, 2, "Beta", BetaWallet);
        StatConsoleHelper(0, 4, "Gamma", GammaWallet);
        StatConsoleHelper(0, 8, "Essence", EssenceWallet);

        StatPanel.Print(23, 6, $"Mined Resources");

        // StatPanel.Surface.DrawLine(new Point(0, 1), new Point(68, 1), Glyph);
        // StatPanel.Surface.DrawLine(new Point(0, 3), new Point(68, 3), Glyph);
        StatPanel.Surface.DrawLine(new Point(0, 6), new Point(21, 6), Glyph);
        StatPanel.Surface.DrawLine(new Point(39, 6), new Point(68, 6), Glyph);

        // --- Event Panel --- //
        if (EventToShow == 0) EventPanel.Print(0, 0, $"No Event To show :,");

        // --- Factory Status --- //
        // Weirdly Cells[y, x], Prob using [row, column]?
        FactoryTable.Cells[0, 0].Value = "Factory";
        FactoryTable.Cells[0, 1].Value = "Progress";
        FactoryTable.Cells[0, 2].Value = "Status";

        FactoryTable.Cells[2, 0].Value = $"Alpha Factory : {Structure.AlphaFactory}";
        FactoryTable.Cells[2, 1].Value = $"ProgressBarHere"; // Probably also Production/Min
        FactoryTable.Cells[2, 2].Value = $"Status : IsWorking? : {Structure.AlphaFactoryStatus}";

        FactoryTable.Cells[3, 0].Value = $"Beta Factory : {Structure.BetaFactory}";
        FactoryTable.Cells[3, 1].Value = $"ProgressBarHere";
        FactoryTable.Cells[3, 2].Value = $"Status : IsWorking? : {Structure.BetaFactoryStatus}";

        FactoryTable.Cells[4, 0].Value = $"Gamma Factory : {Structure.GammaFactory}";
        FactoryTable.Cells[4, 1].Value = $"ProgressBarHere";
        FactoryTable.Cells[4, 2].Value = $"Status : IsWorking? : {Structure.GammaFactoryStatus}";

        // --- Progress Bars --- //
        BuildProgressBars();
    }

    // Made mostly by AI, i Could not for the love of god wrap my head about dynamic strings
    // Makes a fixed width String for the StatPanel so Both ProgressBar and the print doesnt fight eachother
    private void StatConsoleHelper(int Start_X, int Start_Y, string Name, ResourceBP WalletToSee) {
        const int MaxNameUnit = 8;
        const int MaxAmountUnit = 10; // max 999,999,999.9

        // Pad/truncate the name to exactly MaxNameUnit characters
        string paddedName = Name.Length > MaxNameUnit
            ? Name.Substring(0, MaxNameUnit)
            : Name.PadRight(MaxNameUnit);

        // Format the amount to F1 (1 decimal) and pad/truncate to MaxAmountUnit
        string amountStr = WalletToSee.Amount.ToString("F1");
        string paddedAmount = amountStr.Length > MaxAmountUnit
            ? amountStr.Substring(0, MaxAmountUnit)
            : amountStr.PadLeft(MaxAmountUnit);

        // Print the formatted cell
        StatPanel.Print(Start_X, Start_Y, $"{paddedName}: {paddedAmount} |");
    }

    private void BuildProgressBars() {
        const int MaxNameUnit = 8;
        const int MaxAmountUnit = 10;

        const int StartingX = (MaxNameUnit + 2) + (MaxAmountUnit + 2); // 24 cells, 70-24 = 46 free Cells
        const int BarWidth = 25;
        const int BarHeight = 1;

        AlphaProgressBar = new ProgressBar(BarWidth, BarHeight, HorizontalAlignment.Left);
        AlphaProgressBar.Position = new Point(StartingX, 0);
        AlphaProgressBar.BarColor = Color.AnsiYellow;
        AlphaProgressBar.BarGlyph = 219;
        AlphaProgressBar.DisplayText = "%";
        StatPanel.Controls.Add(AlphaProgressBar);

        BetaProgressBar = new ProgressBar(BarWidth, BarHeight, HorizontalAlignment.Left);
        BetaProgressBar.Position = new Point(StartingX, 2);
        BetaProgressBar.BarColor = Color.AnsiBlue;
        BetaProgressBar.BarGlyph = 219;
        BetaProgressBar.DisplayText = "%";
        StatPanel.Controls.Add(BetaProgressBar);

        GammaProgressBar = new ProgressBar(BarWidth, BarHeight, HorizontalAlignment.Left);
        GammaProgressBar.Position = new Point(StartingX, 4);
        GammaProgressBar.BarColor = Color.AnsiGreen;
        GammaProgressBar.BarGlyph = 219;
        GammaProgressBar.DisplayText = "%";
        StatPanel.Controls.Add(GammaProgressBar);

        EssenceProgressBar = new ProgressBar(BarWidth, BarHeight, HorizontalAlignment.Left);
        EssenceProgressBar.Position = new Point(StartingX, 8);
        EssenceProgressBar.BarColor = Color.AnsiCyan;
        EssenceProgressBar.BarGlyph = 219;
        EssenceProgressBar.DisplayText = "%";
        StatPanel.Controls.Add(EssenceProgressBar);
    }

    private void UpdateProgressBars() { // Add to the Proper dedicated UpdateAll
        AlphaProgressBar.Progress = AlphaFactoryProgressIncrement / 20f;
        BetaProgressBar.Progress = BetaFactoryProgressIncrement / 50f;
        GammaProgressBar.Progress = GammaFactoryProgressIncrement / 80f;
        EssenceProgressBar.Progress = EssenceMinerProgressIncrement / 20f;
    }

    private void UpdateStatPanel() {
        // Clear and redraw the stat lines
        StatPanel.Surface.Clear();
        StatConsoleHelper(0, 0, "Alpha", AlphaWallet);
        StatConsoleHelper(0, 2, "Beta", BetaWallet);
        StatConsoleHelper(0, 4, "Gamma", GammaWallet);
        StatConsoleHelper(0, 8, "Essence", EssenceWallet);

        // Redraw the static elements
        int Glyph = '-';
        StatPanel.Print(23, 6, $"Mined Resources");
        StatPanel.Surface.DrawLine(new Point(0, 6), new Point(21, 6), Glyph);
        StatPanel.Surface.DrawLine(new Point(39, 6), new Point(68, 6), Glyph);
    }

    private void AssembleStatPanel() {
        StatPanel = new ControlsConsole(PanelsWidth, PanelsHeight);
        StatPanel.Position = (2, 2);
        Children.Add(StatPanel);

        SadConsole.UI.Border.CreateForSurface(StatPanel, "Factory Statistics");
    }

    private void AssembleEventPanel() {
        EventPanel = new SadConsole.Console(PanelsWidth / 2, PanelsHeight);
        EventPanel.Position = (78, 2);
        Children.Add(EventPanel);

        SadConsole.UI.Border.CreateForSurface(EventPanel, "Current Event");
    }

    private void AssembleFactoryTable() {
        FactoryTableHost = new ControlsConsole(TablesWidth, TablesHeight);
        FactoryTableHost.Position = (6, 20);
        Children.Add(FactoryTableHost);

        FactoryTable = new Table(TablesWidth, TablesHeight, TablesWidth / 3, 1);
        FactoryTable.Position = (0, 0);
        FactoryTableHost.Controls.Add(FactoryTable);

        SadConsole.UI.Border.CreateForSurface(FactoryTableHost, "Factories");
    }
}



