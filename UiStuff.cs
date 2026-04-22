namespace UFIG;

using Spectre.Console;

using static Program;

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
        } else if (EventToShow == 12) {
            GameLayout["GameTopRight"].Update(Panels.AlphaForcedEvent3());
        } else if (EventToShow == 13) {
            GameLayout["GameTopRight"].Update(Panels.AlphaForcedEvent4());
        } else if (EventToShow == 20) {
            GameLayout["GameTopRight"].Update(Panels.BetaForcedEvent1());
        } else if (EventToShow == 21) {
            GameLayout["GameTopRight"].Update(Panels.BetaForcedEvent2());
        } else if (EventToShow == 22) {
            GameLayout["GameTopRight"].Update(Panels.BetaForcedEvent3());
        } else if (EventToShow == 23) {
            GameLayout["GameTopRight"].Update(Panels.BetaForcedEvent4());
        } else if (EventToShow == 30) {
            GameLayout["GameTopRight"].Update(Panels.GammaForcedEvent1());
        } else if (EventToShow == 31) {
            GameLayout["GameTopRight"].Update(Panels.GammaForcedEvent2());
        } else if (EventToShow == 32) {
            GameLayout["GameTopRight"].Update(Panels.GammaForcedEvent3());
        } else if (EventToShow == 33) {
            GameLayout["GameTopRight"].Update(Panels.GammaForcedEvent4());
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

        if (GameState.MenuID != Menu.Game && GameState.MenuID != Menu.ExitMenu) {
            int Entry = GameState.MenuID switch {
                Menu.ShopEntry1 => 1,
                Menu.ShopEntry2 => 2,
                Menu.ShopEntry3 => 3,
                Menu.ShopEntry4 => 4,
                Menu.ShopEntry5 => 5,
                Menu.ShopEntry6 => 6,
                Menu.ShopEntry7 => 7,
                Menu.ShopEntry8 => 8,
                _ => 0
            };

            ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(Entry));
        }

        if (GameState.MenuID == Menu.ShopFeedBackSuccess) {
            ShopLayout["ShopTopRight"].Update(Panels.ShopBuildBuyFeedback(0)); // success
        } else if (GameState.MenuID == Menu.ShopFeedBackRejected) {
            ShopLayout["ShopTopRight"].Update(Panels.ShopBuildBuyFeedback(1)); // Fail cuz broke
        } else if (GameState.MenuID == Menu.ShopFeedBackFailByError) {
            ShopLayout["ShopTopRight"].Update(Panels.ShopBuildBuyFeedback(2)); // Fail cuz broke
        }

        return ShopLayout;

    }
}

public class PlanetUI
{
    public Layout ShowPlanetUI() {
        var PlanetLayout = new Layout("PlanetRoot").SplitRows(
            new Layout("PlanetTop").SplitColumns(
                new Layout("PlanetTopLeft"), // Something like a map
                                                 new Layout("PlanetTopRight") // Chosen Planet Description
            ),
            new Layout("PlanetBottom") // Planet you can move to, and keybinds
        );

        PlanetLayout["PlanetTopLeft"].Update(Panels.PlanetUIMap());
        PlanetLayout["PlanetBottom"].Update(Panels.PlanetUIChoice());

        if (GameState.MenuID == Menu.PlanetUiOrigo) {
            PlanetLayout["PlanetTopRight"].Update(Panels.OrigoPlanetPanel());
        } else if (GameState.MenuID == Menu.PlanetUiSterelis) {
            PlanetLayout["PlanetTopRight"].Update(Panels.SterilisPlanetPanel());
        } else if (GameState.MenuID == Menu.PlanetUiPrimaris) {
            PlanetLayout["PlanetTopRight"].Update(Panels.PrimarisPlanetPanel());
        } else if (GameState.MenuID == Menu.PlanetUiSpace) {
            PlanetLayout["PlanetTopRight"].Update(Panels.SpacePanel());
        }

        return PlanetLayout;
    }
}
