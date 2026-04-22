namespace UFIG;

using Spectre.Console;

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
            8 => new Panel(StringsStuff.ShopEntryPanel8),
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

    public static Panel AlphaForcedEvent3() {
        var panel = new Panel(StringsStuff.ForcedAlphaEvent3);

        panel.Width = 70;
        panel.Height = 16;
        panel.Header = new PanelHeader(" Game : Event Menu ");

        return panel;
    }

    public static Panel AlphaForcedEvent4() {
        var panel = new Panel(StringsStuff.ForcedAlphaEvent4);

        panel.Width = 70;
        panel.Height = 16;
        panel.Header = new PanelHeader(" Game : Event Menu ");

        return panel;
    }

    // Same pattern for BetaForcedEvent3, BetaForcedEvent4, GammaForcedEvent3, GammaForcedEvent4

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

    public static Panel BetaForcedEvent3() {
        var BetaForcedEvent3 = new Panel(StringsStuff.ForcedBetaEvent3);

        BetaForcedEvent3.Width = 70;
        BetaForcedEvent3.Height = 16;
        BetaForcedEvent3.Header = new PanelHeader(" Game : Event Menu ");

        return BetaForcedEvent3;
    }

    public static Panel BetaForcedEvent4() {
        var BetaForcedEvent4 = new Panel(StringsStuff.ForcedBetaEvent4);

        BetaForcedEvent4.Width = 70;
        BetaForcedEvent4.Height = 16;
        BetaForcedEvent4.Header = new PanelHeader(" Game : Event Menu ");

        return BetaForcedEvent4;
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

    public static Panel GammaForcedEvent3() {
        var GammaForcedEvent3 = new Panel(StringsStuff.ForcedGammaEvent3);

        GammaForcedEvent3.Width = 70;
        GammaForcedEvent3.Height = 16;
        GammaForcedEvent3.Header = new PanelHeader(" Game : Event Menu ");

        return GammaForcedEvent3;
    }

    public static Panel GammaForcedEvent4() {
        var GammaForcedEvent4 = new Panel(StringsStuff.ForcedGammaEvent4);

        GammaForcedEvent4.Width = 70;
        GammaForcedEvent4.Height = 16;
        GammaForcedEvent4.Header = new PanelHeader(" Game : Event Menu ");

        return GammaForcedEvent4;
    }

    public static Panel OrigoPlanetPanel() {
        var OrigoPlanetPanel = new Panel(StringsStuff.OrigoPlanetPanelString);

        OrigoPlanetPanel.Width = 71;
        OrigoPlanetPanel.Height = 16;
        OrigoPlanetPanel.Header = new PanelHeader(" Planet : Origo ");

        return OrigoPlanetPanel;
    }

    public static Panel PrimarisPlanetPanel() {
        var PrimarisPlanetPanel = new Panel(StringsStuff.PrimarisPlanetPanelString);

        PrimarisPlanetPanel.Width = 71;
        PrimarisPlanetPanel.Height = 16;
        PrimarisPlanetPanel.Header = new PanelHeader(" Planet : Primaris ");

        return PrimarisPlanetPanel;
    }

    public static Panel SterilisPlanetPanel() {
        var SterilisPlanetPanel = new Panel(StringsStuff.SterilisPlanetPanelString);

        SterilisPlanetPanel.Width = 71;
        SterilisPlanetPanel.Height = 16;
        SterilisPlanetPanel.Header = new PanelHeader(" Planet : Sterilis ");

        return SterilisPlanetPanel;
    }

    public static Panel SpacePanel() {
        var SpacePanel = new Panel(StringsStuff.SpacePanelString);

        SpacePanel.Width = 71;
        SpacePanel.Height = 16;
        SpacePanel.Header = new PanelHeader(" Location : Space ");

        return SpacePanel;
    }

    public static Panel PlanetUIChoice() {
        var PlanetUIChoice = new Panel(StringsStuff.PlanetChoices);

        PlanetUIChoice.Width = 141;
        PlanetUIChoice.Height = 17;
        PlanetUIChoice.Header = new PanelHeader(" Navigation Menu ");

        return PlanetUIChoice;
    }

    public static Panel PlanetUIMap() {
        var PlanetUIMap = new Panel(StringsStuff.SupposedMap);

        PlanetUIMap.Width = 70;
        PlanetUIMap.Height = 16;
        PlanetUIMap.Header = new PanelHeader(" Navigation Menu : Map ");

        return PlanetUIMap;
    }
}
