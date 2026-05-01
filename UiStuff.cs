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

        bool InFactoryCategory = ( (int)GameState.MenuID >=  100 && (int)GameState.MenuID <=  109);
        bool InMinersCategory = ( (int)GameState.MenuID >=  110 && (int)GameState.MenuID <=  119);
        bool InUgradesCategory = ( (int)GameState.MenuID >=  120 && (int)GameState.MenuID <=  129);
        bool InShopMain = (GameState.MenuID == Menu.ShopNoEntry);

        if (InShopMain) {
            ShopLayout["ShopLeft"].Update(Panels.ShopBuildShopMenu(0));
            ShopLayout["ShopTopRight"].Update(new Panel($"No Entry has been chosen yet"));
        }

        if (InFactoryCategory) {

            ShopLayout["ShopLeft"].Update(Panels.ShopBuildShopMenu(1));

            if (GameState.MenuID == Menu.ShopAlphaFactoryPage) {
                ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(1));
            } else if (GameState.MenuID == Menu.ShopBetaFactoryPage) {
                ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(2));
            } else if (GameState.MenuID == Menu.ShopGammaFactoryPage) {
                ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(3));
            } else {
                ShopLayout["ShopTopRight"].Update(new Panel($"No Entry has been chosen yet"));
            }
        }

        if (InUgradesCategory) {
            ShopLayout["ShopLeft"].Update(Panels.ShopBuildShopMenu(2));

            if (GameState.MenuID == Menu.ShopFactoryInputUpgradePage) {
                ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(6)); // Input = 6
            } else if (GameState.MenuID == Menu.ShopFactoryOutputUpgradePage) {
                ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(7)); // Output = 7
            } else if (GameState.MenuID == Menu.ShopEssenceBaseUpgradePage) {
                ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(4));
            } else if (GameState.MenuID == Menu.ShopEssenceMultiplierUpgradePage) {
                ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(5));
            }  else {
                ShopLayout["ShopTopRight"].Update(new Panel($"No Entry has been chosen yet"));
            }
        }

        if (InMinersCategory) {
            ShopLayout["ShopLeft"].Update(Panels.ShopBuildShopMenu(3));

            if (GameState.MenuID == Menu.ShopEssenceMinerPage) {
                ShopLayout["ShopTopRight"].Update(Panels.ShopBuildEntryPanel(8));
            } else {
                ShopLayout["ShopTopRight"].Update(new Panel($"No Entry has been chosen yet"));
            }
        }

        // if ((int)GameState.MenuID >= 99 && (int)GameState.MenuID <= 199) {
        //     int CategoryEntry = GameState.MenuID switch {
        //         Menu.ShopNoEntry => 0,
        //         Menu.ShopCategoryFactories => 1,
        //         Menu.ShopCategoryUpgrades => 2,
        //         Menu.ShopCategoryMine => 3,
        //         _ => -1
        //     };
        //
        //     int Entry = GameState.MenuID switch {
        //         Menu.ShopAlphaFactoryPage => 1,
        //         Menu.ShopBetaFactoryPage => 2,
        //         Menu.ShopGammaFactoryPage => 3,
        //         Menu.ShopEssenceBaseUpgradePage => 4,
        //         Menu.ShopEssenceMultiplierUpgradePage => 5,
        //         Menu.ShopFactoryInputUpgradePage => 6,
        //         Menu.ShopFactoryOutputUpgradePage => 7,
        //         Menu.ShopEssenceMinerPage => 8,
        //         _ => -1
        //     };

        ShopLayout["ShopBottomRight"].Update(Panels.BuildStatPanel());

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
