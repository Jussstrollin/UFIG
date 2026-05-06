namespace UFIG;

using static Program;
using static ShopUI;

using Spectre.Console;

public class StateShop : StateInterface {
    public void GoingIn() {
        GameState.MenuID = Menu.ShopNoEntry;
        // add entering Sequence, Animation, and whatnot here
        Display();
        return;
    }

    public void GoingOut() {
        // clean up or whatever
        AnsiConsole.Clear();
        return;
    }

    public void Display() {
        var ShopUi = new ShopUI();

        AnsiConsole.Write(ShopUi.ShopMenuLayout());
        return;
    }

    public void HandleControls(char Key) {
        int result = -1; // default on error

        bool IsInFactories = ((int)GameState.MenuID >= 100 && (int)GameState.MenuID <= 109);
        bool IsInUpgrades = ((int)GameState.MenuID >= 120 && (int)GameState.MenuID <= 129); // was 110-119
        bool IsInMine = ((int)GameState.MenuID >= 110 && (int)GameState.MenuID <= 119); // was 120-129
        bool IsInFeedback = ((int)GameState.MenuID >= 197 && (int)GameState.MenuID <= 199);
        bool IsInShopMain = (GameState.MenuID == Menu.ShopNoEntry);

        if (IsInShopMain) {
            if (Key == '1') GameState.MenuID = Menu.ShopCategoryFactories;
            if (Key == '2') GameState.MenuID = Menu.ShopCategoryUpgrades;
            if (Key == '3') GameState.MenuID = Menu.ShopCategoryMine;
        }

        // ShopGoBack from entry
        if (IsInFactories) {
            if (Key == 'B') GameState.MenuID = Menu.ShopCategoryFactories;
        }
        else if (IsInUpgrades) {
            if (Key == 'B') GameState.MenuID = Menu.ShopCategoryUpgrades;
        }
        else if (IsInMine) {
            if (Key == 'B') GameState.MenuID = Menu.ShopCategoryMine;
        }

        // Shop Go back from Category
        if ((GameState.MenuID == Menu.ShopCategoryFactories ||
                GameState.MenuID == Menu.ShopCategoryUpgrades ||
                GameState.MenuID == Menu.ShopCategoryMine) && Key == 'B') {
            GameState.MenuID = Menu.ShopNoEntry;
        }

        if (IsInFactories) {
            if (Key == '1') GameState.MenuID = Menu.ShopAlphaFactoryPage;
            if (Key == '2') GameState.MenuID = Menu.ShopBetaFactoryPage;
            if (Key == '3') GameState.MenuID = Menu.ShopGammaFactoryPage;

            if (Key == '\r') {
                if (GameState.MenuID == Menu.ShopAlphaFactoryPage) result = WannaBuy(ToBuy.AlphaFactory);
                if (GameState.MenuID == Menu.ShopBetaFactoryPage) result = WannaBuy(ToBuy.BetaFactory);
                if (GameState.MenuID == Menu.ShopGammaFactoryPage) result = WannaBuy(ToBuy.GammaFactory);
            }
        }

        if (IsInUpgrades) {
            if (Key == '1') GameState.MenuID = Menu.ShopFactoryInputUpgradePage;
            if (Key == '2') GameState.MenuID = Menu.ShopFactoryOutputUpgradePage;
            if (Key == '3') GameState.MenuID = Menu.ShopEssenceBaseUpgradePage;
            if (Key == '4') GameState.MenuID = Menu.ShopEssenceMultiplierUpgradePage;

            if (Key == '\r') {
                if (GameState.MenuID == Menu.ShopFactoryInputUpgradePage) result = WannaBuy(ToBuy.FactoryInputUpgrade);
                if (GameState.MenuID == Menu.ShopFactoryOutputUpgradePage) result = WannaBuy(ToBuy.FactoryOutputUpgrade);
                if (GameState.MenuID == Menu.ShopEssenceBaseUpgradePage) result = WannaBuy(ToBuy.EssenceBase);
                if (GameState.MenuID == Menu.ShopEssenceMultiplierUpgradePage) result = WannaBuy(ToBuy.EssenceMultiplier);
            }
        }

        if (IsInMine) {
            if (Key == '1') GameState.MenuID = Menu.ShopEssenceMinerPage;

            if (Key == '\r') {
                if (GameState.MenuID == Menu.ShopEssenceMinerPage) result = WannaBuy(ToBuy.EssenceMiner);
            }
        }

        return;
    }

    public void Update() {
        return;
    }

    public enum ToBuy {
        EssenceMiner,

        AlphaFactory,
        BetaFactory,
        GammaFactory,

        EssenceBase,
        EssenceMultiplier,

        FactoryInputUpgrade,
        FactoryOutputUpgrade,

        CrudeFuel,
        StandardFuel,
        RefinedFuel
    }


    private static int WannaBuy(ToBuy Upgrade) {
        if (Upgrade == ToBuy.EssenceMiner) {
            if (AlphaWallet.Amount >= Structure.EssenceMinerCost) {
                AlphaWallet.Amount -= Structure.EssenceMinerCost;
                Structure.EssenceMiner++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.AlphaFactory) {
            if (EssenceWallet.Amount >= Structure.AlphaFactoryCost) { // afford
                EssenceWallet.Amount -= Structure.AlphaFactoryCost;
                Structure.AlphaFactory++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.BetaFactory) {
            if (EssenceWallet.Amount >= Structure.BetaFactoryCost) { // afford
                EssenceWallet.Amount -= Structure.BetaFactoryCost;
                Structure.BetaFactory++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.GammaFactory) {
            if (EssenceWallet.Amount >= Structure.GammaFactoryCost) { // afford
                EssenceWallet.Amount -= Structure.GammaFactoryCost;
                Structure.GammaFactory++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.EssenceBase) {
            if (AlphaWallet.Amount >= UpgradeTrack.EssenceBaseCost) {
                AlphaWallet.Amount -= UpgradeTrack.EssenceBaseCost;
                UpgradeTrack.EssenceBaseBought++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.EssenceMultiplier) {
            if (BetaWallet.Amount >= UpgradeTrack.EssenceMultiplierCost) {
                BetaWallet.Amount -= UpgradeTrack.EssenceMultiplierCost;
                UpgradeTrack.EssenceMultiplierBought++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.FactoryInputUpgrade) {
            if (GammaWallet.Amount >= UpgradeTrack.FactoryInputUpgradeCost) {
                GammaWallet.Amount -= UpgradeTrack.FactoryInputUpgradeCost;
                UpgradeTrack.FactoryInputUpgradeBought++;
                return 1;
            }
            else {
                return 0;
            }
        }

        if (Upgrade == ToBuy.FactoryOutputUpgrade) {
            if (GammaWallet.Amount >= UpgradeTrack.FactoryOutputUpgradeCost) {
                GammaWallet.Amount -= UpgradeTrack.FactoryOutputUpgradeCost;
                UpgradeTrack.FactoryOutputUpgradeBought++;
                return 1;
            }
            else {
                return 0;
            }
        }
        return -1; // some wierd happened
    }
}
