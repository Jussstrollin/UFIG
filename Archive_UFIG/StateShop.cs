namespace UFIG;

using static Program;

using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadConsole.Configuration;
using SadRogue.Primitives;

public class StateShop : ScreenSurface {
    // ==============================================
    // DECLARATIONS — Panels & Layout Constants
    // ==============================================

    private ControlsConsole ShopEntryListHost;
    private ControlsConsole ShopEntryList;

    private ScreenSurface FactoryInventory;

    private ControlsConsole ShopEntryDescriptionHost;
    private ControlsConsole ShopEntryDescription;

    private const int CanvasWidth = 115;
    private const int CanvasHeight = 40;

    private const int ShopX = 2;
    private const int ShopY = 2;
    private const int ShopWidth = 50;
    private const int ShopHeight = 28;

    private const int FactoryInventoryX = 69;
    private const int FactoryInventoryY = 2;
    private const int FactoryInventoryWidth = 37;
    private const int FactoryInventoryHeight = 8;

    private const int DescX = 76;
    private const int DescY = 18;
    private const int DescWidth = 60;
    private const int DescHeight = 18;

    // ==============================================
    // DECLARATIONS — Menu State
    // ==============================================

    private ShopEntry MenuOn = ShopEntry.Main;
    private ShopEntry LastMenu;

    private enum ShopEntry {
        Main = 0,

        // Factory Category
        CategoryFactory = 101,
        EntryAlphaFactory = 102,
        EntryBetaFactory = 103,
        EntryGammaFactory = 104,

        // Upgrade Category
        CategoryUgrade = 201,
        EntryEssenceBaseUpgrade = 202,
        EntryEssenceMultiplierUpgrade = 203,
        EntryFactoryInputUpgrade = 204,
        EntryFactoryOutputUpgrade = 205,

        // Miner Category
        CategoryMiner = 301,
        EntryEssenceMiner = 302
    }

    // ==============================================
    // DECLARATIONS — Shop Catalog
    // ==============================================

    public struct ShopItem {
        public string Name;
        public string Description;
        public Resources ResourceCostToBuy;
        public double CostAmount;
        public Action OnBuy;
        public Func<double> CurrentOwned;
    }

    private Dictionary<ShopEntry, ShopItem> ShopItems;

    // ==============================================
    // INITIALIZATION
    // ==============================================

    public StateShop() : base(CanvasWidth, CanvasHeight) {
        Children.Add(new GlobalUI().ControlPanelHost);

        BuildCatalog();

        AssembleShopEntryList();
        AssembleFactoryInventory();
        AssembleDescriptionPanel();

        CreateBorders();
        HandleUI();

        this.IsFocused = true;
        LastMenu = MenuOn;
    }

    private void AssembleShopEntryList() {
        ShopEntryListHost = new ControlsConsole(ShopWidth, ShopHeight);
        ShopEntryListHost.Position = (ShopX, ShopY);
        Children.Add(ShopEntryListHost);

        ShopEntryList = new ControlsConsole(ShopWidth, ShopHeight);
        ShopEntryList.Position = (0, 0);
        ShopEntryListHost.Children.Add(ShopEntryList);
    }

    private void AssembleFactoryInventory() {
        FactoryInventory = new ScreenSurface(FactoryInventoryWidth, FactoryInventoryHeight);
        FactoryInventory.Position = (FactoryInventoryX, FactoryInventoryY);
        Children.Add(FactoryInventory);
    }

    private void AssembleDescriptionPanel() {
        ShopEntryDescriptionHost = new ControlsConsole(DescWidth, DescHeight);
        ShopEntryDescriptionHost.Position = (56, 12);
        Children.Add(ShopEntryDescriptionHost);

        ShopEntryDescription = new ControlsConsole(DescWidth, DescHeight);
        ShopEntryDescription.Position = (0, 0);
        ShopEntryDescriptionHost.Children.Add(ShopEntryDescription);
    }

    private void CreateBorders() {
        SadConsole.UI.Border.CreateForSurface(ShopEntryListHost, "Shop");
        SadConsole.UI.Border.CreateForSurface(FactoryInventory, "Factory Inventory");
        SadConsole.UI.Border.CreateForSurface(ShopEntryDescriptionHost, "Description");
    }

    // ==============================================
    // GAME LOOP
    // ==============================================

    public override void Update(TimeSpan delta) {
        base.Update(delta);

        if (MenuOn == LastMenu) HandleUI();
        LastMenu = MenuOn;
    }

    // ==============================================
    // INPUT HANDLING
    // ==============================================

    public override bool ProcessKeyboard(SadConsole.Input.Keyboard KEY) {
        if (HandleGlobalBinds(KEY)) return true;
        if (HandleShopBinds(KEY)) return true;

        return base.ProcessKeyboard(KEY);
    }

    private bool HandleShopBinds(SadConsole.Input.Keyboard ListOfKey) {
        foreach (var KEY in ListOfKey.KeysPressed) {
            // Back navigation
            if (KEY.Key == Keys.B) {
                if ((int)MenuOn >= 101) { MenuOn = ShopEntry.Main; return true; }
            }

            // Buy item
            if (KEY.Key == Keys.E && (int)MenuOn >= 102) {
                WannaBuy(MenuOn);
                return true;
            }

            // Factory category entries
            if ((int)MenuOn >= 101 && (int)MenuOn <= 200) {
                if (KEY.Key == Keys.D1) { MenuOn = ShopEntry.EntryAlphaFactory; return true; }
                if (KEY.Key == Keys.D2) { MenuOn = ShopEntry.EntryBetaFactory; return true; }
                if (KEY.Key == Keys.D3) { MenuOn = ShopEntry.EntryGammaFactory; return true; }
            }

            // Upgrade category entries
            if ((int)MenuOn >= 201 && (int)MenuOn <= 300) {
                if (KEY.Key == Keys.D1) { MenuOn = ShopEntry.EntryEssenceBaseUpgrade; return true; }
                if (KEY.Key == Keys.D2) { MenuOn = ShopEntry.EntryEssenceMultiplierUpgrade; return true; }
                if (KEY.Key == Keys.D3) { MenuOn = ShopEntry.EntryFactoryInputUpgrade; return true; }
                if (KEY.Key == Keys.D4) { MenuOn = ShopEntry.EntryFactoryOutputUpgrade; return true; }
            }

            // Miner category entries
            if ((int)MenuOn >= 301 && (int)MenuOn <= 400) {
                if (KEY.Key == Keys.D1) { MenuOn = ShopEntry.EntryEssenceMiner; return true; }
            }

            // Main menu categories
            if ((int)MenuOn >= 0 && (int)MenuOn <= 100) {
                if (KEY.Key == Keys.D1) { MenuOn = ShopEntry.CategoryFactory; return true; }
                if (KEY.Key == Keys.D2) { MenuOn = ShopEntry.CategoryUgrade; return true; }
                if (KEY.Key == Keys.D3) { MenuOn = ShopEntry.CategoryMiner; return true; }
            }
        }

        return false;
    }

    // ==============================================
    // SHOP LOGIC
    // ==============================================

    private double GetWallet(Resources ToGet) {
        if (ToGet == Resources.Alpha) return AlphaWallet.Amount;
        if (ToGet == Resources.Beta) return BetaWallet.Amount;
        if (ToGet == Resources.Gamma) return GammaWallet.Amount;
        if (ToGet == Resources.Essence) return EssenceWallet.Amount;

        return -1;
    }

    private void WannaBuy(ShopEntry ToBuy) {
        if (!ShopItems.TryGetValue(ToBuy, out var item)) return;

        if (GetWallet(item.ResourceCostToBuy) >= item.CostAmount) {
            item.OnBuy();
        }
        // TODO: Show feedback popup for success/failure
    }

    // ==============================================
    // SHOP CATALOG
    // ==============================================

    private void BuildCatalog() {
        ShopItems = new Dictionary<ShopEntry, ShopItem> {
            [ShopEntry.EntryAlphaFactory] = new ShopItem {
                Name = "Alpha Factory",
                Description = "A Factory used to Produce 1 Alpha Arcosphere in expense of 5 Essence",
                ResourceCostToBuy = Resources.Essence,
                CostAmount = Structure.AlphaFactoryCost,
                OnBuy = () => { EssenceWallet.Amount -= Structure.AlphaFactoryCost; Structure.AlphaFactory++; },
                CurrentOwned = () => Structure.AlphaFactory
            },

            [ShopEntry.EntryBetaFactory] = new ShopItem {
                Name = "Beta Factory",
                Description = "Produces Beta from Essence + Alpha",
                ResourceCostToBuy = Resources.Alpha,
                CostAmount = Structure.BetaFactoryCost,
                OnBuy = () => { EssenceWallet.Amount -= Structure.BetaFactoryCost; Structure.BetaFactory++; },
                CurrentOwned = () => Structure.BetaFactory
            },

            [ShopEntry.EntryGammaFactory] = new ShopItem {
                Name = "Gamma Factory",
                Description = "Produces Gamma from Essence + Alpha + Beta",
                ResourceCostToBuy = Resources.Beta,
                CostAmount = Structure.GammaFactoryCost,
                OnBuy = () => { EssenceWallet.Amount -= Structure.GammaFactoryCost; Structure.GammaFactory++; },
                CurrentOwned = () => Structure.GammaFactory
            },

            [ShopEntry.EntryEssenceBaseUpgrade] = new ShopItem {
                Name = "Essence Base Upgrade",
                Description = "Increases base Essence production per tick",
                ResourceCostToBuy = Resources.Essence,
                CostAmount = 5,
                OnBuy = () => { AlphaWallet.Amount -= UpgradeTrack.EssenceBaseCost; UpgradeTrack.EssenceBaseBought++; },
                CurrentOwned = () => UpgradeTrack.EssenceBaseBought
            },

            [ShopEntry.EntryEssenceMultiplierUpgrade] = new ShopItem {
                Name = "Essence Multiplier Upgrade",
                Description = "Multiplies your Essence production",
                ResourceCostToBuy = Resources.Essence,
                CostAmount = 50,
                OnBuy = () => { BetaWallet.Amount -= UpgradeTrack.EssenceMultiplierCost; UpgradeTrack.EssenceMultiplierBought++; },
                CurrentOwned = () => UpgradeTrack.EssenceMultiplierBought
            },

            [ShopEntry.EntryFactoryInputUpgrade] = new ShopItem {
                Name = "Factory Input Upgrade",
                Description = "Reduces resource input costs",
                ResourceCostToBuy = Resources.Gamma,
                CostAmount = 50,
                OnBuy = () => { GammaWallet.Amount -= UpgradeTrack.FactoryInputUpgradeCost; UpgradeTrack.FactoryInputUpgradeBought++; },
                CurrentOwned = () => UpgradeTrack.FactoryInputUpgradeBought
            },

            [ShopEntry.EntryFactoryOutputUpgrade] = new ShopItem {
                Name = "Factory Output Upgrade",
                Description = "Increases factory production output",
                ResourceCostToBuy = Resources.Gamma,
                CostAmount = 100,
                OnBuy = () => { GammaWallet.Amount -= UpgradeTrack.FactoryOutputUpgradeCost; UpgradeTrack.FactoryOutputUpgradeBought++; },
                CurrentOwned = () => UpgradeTrack.FactoryOutputUpgradeBought
            },

            [ShopEntry.EntryEssenceMiner] = new ShopItem {
                Name = "Essence Miner",
                Description = "The foundation of your empire",
                ResourceCostToBuy = Resources.Essence,
                CostAmount = 10,
                OnBuy = () => { AlphaWallet.Amount -= Structure.EssenceMinerCost; Structure.EssenceMiner++; },
                CurrentOwned = () => Structure.EssenceMiner
            }
        };
    }

    // ==============================================
    // UI RENDERING
    // ==============================================

    private void HandleUI() {
        ShopEntryList.Clear();
        ShopEntryDescription.Clear();

        bool IsOnMain = true;
        bool IsOnFactoryCategory = false;
        bool IsOnUpgradeCategory = false;
        bool IsOnMinersCategory = false;

        if ((int)MenuOn >= 0 && (int)MenuOn <= 100) {
            ShowMain();
            IsOnMain = true;
            IsOnFactoryCategory = false;
            IsOnUpgradeCategory = false;
            IsOnMinersCategory = false;
        }
        else if ((int)MenuOn >= 101 && (int)MenuOn <= 200) {
            ShowFactoryCategory();
            IsOnMain = false;
            IsOnFactoryCategory = true;
            IsOnUpgradeCategory = false;
            IsOnMinersCategory = false;
        }
        else if ((int)MenuOn >= 201 && (int)MenuOn <= 300) {
            ShowUpgradeCategory();
            IsOnMain = false;
            IsOnFactoryCategory = false;
            IsOnUpgradeCategory = true;
            IsOnMinersCategory = false;
        }
        else if ((int)MenuOn >= 301 && (int)MenuOn <= 400) {
            ShowMinerCategory();
            IsOnMain = false;
            IsOnFactoryCategory = false;
            IsOnUpgradeCategory = false;
            IsOnMinersCategory = true;
        }

        if (IsOnFactoryCategory) ShowItemDescription(MenuOn);
        else if (IsOnUpgradeCategory) ShowItemDescription(MenuOn);
        else if (IsOnMinersCategory) ShowItemDescription(MenuOn);
        else ShowNoEntryChosen();

        UpdateInventoryDisplay();
    }

    private void ShowItemDescription(ShopEntry Entry) {
        if (!ShopItems.TryGetValue(Entry, out var item)) {
            ShowNoEntryChosen();
            return;
        }

        ShopEntryDescription.Clear();
        ShopEntryDescription.Print(0, 0, $" > {item.Name} < ");
        ShopEntryDescription.Print(0, 2, $"Cost : {item.CostAmount} {item.ResourceCostToBuy} ");
        ShopEntryDescription.Print(0, 3, $"Owned : {item.CurrentOwned()}");
        ShopEntryDescription.Print(0, 5, $"{item.Description}");
        ShopEntryDescription.Print(0, 7, $"Press E To Buy");
    }

    private void ShowNoEntryChosen() {
        ShopEntryDescription.Print(28, 6, "No Entry Chosen");
    }

    private void ShowMain() {
        ShopEntryList.Print(0, 0, "Shop Categories : ");
        ShopEntryList.Print(0, 2, "Factory Category [1]");
        ShopEntryList.Print(0, 4, "Upgrades Category [2]");
        ShopEntryList.Print(0, 6, "Miners Category [3]");
    }

    private void ShowFactoryCategory() {
        ShopEntryList.Print(0, 0, "Factory Category");
        ShopEntryList.Print(0, 1, "Here, you can spend resources to make Factories that Produces other resources");
        ShopEntryList.Print(0, 4, " 𜰙  Alpha Factory [1]");
        ShopEntryList.Print(0, 5, " 𜰙  Beta Factory [2]");
        ShopEntryList.Print(0, 6, " 𜰙  Gamma Factory [3]");
    }

    private void ShowUpgradeCategory() {
        ShopEntryList.Print(0, 0, "Upgrades Category");
        ShopEntryList.Print(0, 1, "Enhance your production with permanent upgrades");
        ShopEntryList.Print(0, 3, " 𜰙  Essence Base [1]");
        ShopEntryList.Print(0, 4, " 𜰙  Essence Multiplier [2]");
        ShopEntryList.Print(0, 5, " 𜰙  Factory Input [3]");
        ShopEntryList.Print(0, 6, " 𜰙  Factory Output [4]");
    }

    private void ShowMinerCategory() {
        ShopEntryList.Print(0, 0, "Miner Category");
        ShopEntryList.Print(0, 1, "Essence is the foundation. Invest in mining.");
        ShopEntryList.Print(0, 3, " 𜰙  Essence Miner [1]");
    }

    private void UpdateInventoryDisplay() {
        FactoryInventory.Print(0, 0, $"Alpha  : {AlphaWallet.Amount.ToString("F1").PadLeft(12)}");
        FactoryInventory.Print(0, 1, $"Beta   : {BetaWallet.Amount.ToString("F1").PadLeft(12)}");
        FactoryInventory.Print(0, 2, $"Gamma  : {GammaWallet.Amount.ToString("F1").PadLeft(12)}");
        FactoryInventory.Print(0, 3, $"Essence: {EssenceWallet.Amount.ToString("F1").PadLeft(12)}");
    }
}
