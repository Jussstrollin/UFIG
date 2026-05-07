namespace UFIG;

using Spectre.Console;

public static class Panels {
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
            1 => new Panel(StringsStuff.ShopAlphaFactoryPanel),
            2 => new Panel(StringsStuff.ShopBetaFactoryPanel),
            3 => new Panel(StringsStuff.ShopGammaFactoryPanel),
            4 => new Panel(StringsStuff.ShopEssenceBaseProductionPanel),
            5 => new Panel(StringsStuff.ShopEssenceMultiplierPanel),
            6 => new Panel(StringsStuff.ShopFactoryInputUpgradePanel),
            7 => new Panel(StringsStuff.ShopFactoryOutputUpgradePanel),
            8 => new Panel(StringsStuff.ShopEssenceMiner),
            _ => new Panel("No entry chosen")
        };

        entry.Width = 71;
        entry.Height = 16;

        switch (panel) {
            case 1:
                entry.Header = new PanelHeader($" Shop : [yellow]Alpha[/] Factory ");
                break;
            case 2:
                entry.Header = new PanelHeader($" Shop : [blue]Beta[/] Factory ");
                break;
            case 3:
                entry.Header = new PanelHeader($" Shop : [green]Gamma[/] Factory  ");
                break;
            case 4:
                entry.Header = new PanelHeader($" Shop : [cyan]Essence Base Production[/] ");
                break;
            case 5:
                entry.Header = new PanelHeader($" Shop : [cyan]Essence Multiplier[/] ");
                break;
            case 6:
                entry.Header = new PanelHeader($" Shop : [purple]Factoruy Input Upgrade[/] ");
                break;
            case 7:
                entry.Header = new PanelHeader($" Shop : [purple]Factory Output Upgrade[/] ");
                break;
            case 8:
                entry.Header = new PanelHeader($" Shop : [cyan]Essence Miner[/] ");
                break;
            default:
                entry.Header = new PanelHeader($" Shop : [red]UNKNOWN MENU! REPORT HOW YOU GOT HERE[/] ");
                break;
        }

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

    public static Panel ShopBuildShopMenu(int CategoryPanel) {
        Panel Entry = CategoryPanel switch {
            0 => new Panel(StringsStuff.ShopMainPanel),
            1 => new Panel(StringsStuff.ShopCategoryFactory),
            2 => new Panel(StringsStuff.ShopCategoryUpgrades),
            3 => new Panel(StringsStuff.ShopCategoryMiners),
            _ => new Panel("[red]ERROR! ShopMainPanel NOT RECOGNIZED, REPORT HOW YOU GOT HERE[/]")
        };

        Entry.Width = 67;
        Entry.Height = 32;

        switch (CategoryPanel) {
            case 0:
                Entry.Header = new PanelHeader($" Shop : Main Panel");
                break;
            case 1:
                Entry.Header = new PanelHeader($" Shop : Factory ");
                break;
            case 2:
                Entry.Header = new PanelHeader($" Shop : Upgrades");
                break;
            case 3:
                Entry.Header = new PanelHeader($" Shop : Miners");
                break;
            default:
                Entry.Header = new PanelHeader($"[red]UNKNOWN MENU[/]");
                break;
        }

        return Entry;
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

    private const int PlanetUIChoiceWidth = 141;
    private const int PlayerUIChoiceHeight = 17;

    public static Panel PlanetUIChoice() {
        var PlanetUIChoice = new Panel(StringsStuff.PlanetChoices);

        PlanetUIChoice.Width = PlanetUIChoiceWidth;
        PlanetUIChoice.Height = PlayerUIChoiceHeight;
        PlanetUIChoice.Header = new PanelHeader(" Navigation Menu ");

        return PlanetUIChoice;
    }

    public static Panel PlanetTravelConfirmationToOrigo() {
        var PlanetUIConfirmationToOrigo = new Panel(StringsStuff.PlanetTravelConfirmationToOrigo);

        PlanetUIConfirmationToOrigo.Width = PlanetUIChoiceWidth;
        PlanetUIConfirmationToOrigo.Height = PlayerUIChoiceHeight;
        PlanetUIConfirmationToOrigo.Header = new PanelHeader(" Navigation Menu ");

        return PlanetUIConfirmationToOrigo;
    }

    public static Panel PlanetTravelConfirmationToSterelis() {
        var PlanetUIConfirmationToSterelis = new Panel(StringsStuff.PlanetTravelConfirmationToSterelis);

        PlanetUIConfirmationToSterelis.Width = PlanetUIChoiceWidth;
        PlanetUIConfirmationToSterelis.Height = PlayerUIChoiceHeight;
        PlanetUIConfirmationToSterelis.Header = new PanelHeader(" Navigation Menu ");

        return PlanetUIConfirmationToSterelis;
    }

    public static Panel PlanetTravelConfirmationToPrimaris() {
        var PlanetUIConfirmationToPrimaris = new Panel(StringsStuff.PlanetTravelConfirmationToPrimaris);

        PlanetUIConfirmationToPrimaris.Width = PlanetUIChoiceWidth;
        PlanetUIConfirmationToPrimaris.Height = PlayerUIChoiceHeight;
        PlanetUIConfirmationToPrimaris.Header = new PanelHeader(" Navigation Menu ");

        return PlanetUIConfirmationToPrimaris;
    }

    public static Panel PlanetUIMap() {
        var PlanetUIMap = new Panel(StringsStuff.SupposedMap);

        PlanetUIMap.Width = 70;
        PlanetUIMap.Height = 16;
        PlanetUIMap.Header = new PanelHeader(" Navigation Menu : Map ");

        return PlanetUIMap;
    }
}
