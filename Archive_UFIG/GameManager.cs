namespace UFIG;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#region Enum and Data
public enum Resources { Alpha, Beta, Gamma, Essence };
public enum Structures { AlphaFactory, BetaFactory, GammaFactory, EssenceMiner }
public enum WorkshopCategories { Structures, Upgrades, Miners }
public enum Upgrade { EssenceMinerBase, EssenceMinerMultiplier, FactoryInput, FactoryOutput }
public enum States { StatePlaying, StateShop }
public enum Locations { Space, Origo, Sterelis, Primaris }
public enum Recipes { AlphaRecipe, BetaRecipe, GammaRecipe }
public enum MinerOutput { Essence }

public class UpgradesData {
    public Resources ResourceCost { get; set; }
    public double Cost { get; set; }
}
public class StructureData {
    public Resources ResourceCost { get; set; }
    public double CostPerStructure { get; set; }
}
public class FactoryData {
    public int Amount { get; set; }
    public bool IsRunning { get; set; }
}
public class WalletData {
    public double Amount { get; set; }
}
public class RecipeData {
    public Dictionary<Resources, double> Inputs { get; set; } = new();
    public Dictionary<Resources, double> Outputs { get; set; } = new();
}
#endregion

public static class GameManager {
    //Global Canvas
    public const int CellX = 120;
    public const int CellY = 40;

    public static States CurrentState { get; set; } = States.StatePlaying;
    public static bool IsPaused { get; set; } = false;
    public static bool IsStopped { get; set; } = false;

    public static Locations LocationOn { get; set; } = Locations.Origo;
    public static bool IsLanded { get; set; } = true;
    public static bool IsOrbiting { get; set; } = false;

    static GameManager() {
	    
    }
}

/// <summary>
/// Dedicated system handling resource wallets, structures, and upgrades tracking.
/// </summary>
public static class FactoryEcon {
    // inner
    public static readonly Dictionary<Resources, WalletData> Wallets = new();
    public static readonly Dictionary<Upgrade, UpgradesData> Upgrades = new();
    public static readonly Dictionary<Structures, StructureData> Structure = new();

    #region Workshop Related | setting values and prices
    // outer
    public static readonly Dictionary<WorkshopCategories, Dictionary<Structures, StructureData>> StructuresCat = new();
    public static readonly Dictionary<WorkshopCategories, Dictionary<Upgrade, UpgradesData>> UpgradesCat = new();

    static FactoryEcon() {
        StructuresCat[WorkshopCategories.Structures] = new Dictionary<Structures, StructureData>();
        UpgradesCat[WorkshopCategories.Upgrades] = new Dictionary<Upgrade, UpgradesData>();

        StructuresCat[WorkshopCategories.Structures][Structures.AlphaFactory] = new StructureData { ResourceCost = Resources.Essence, CostPerStructure = 5.0 };
        StructuresCat[WorkshopCategories.Structures][Structures.BetaFactory] = new StructureData { ResourceCost = Resources.Essence, CostPerStructure = 30.0 };
        StructuresCat[WorkshopCategories.Structures][Structures.GammaFactory] = new StructureData { ResourceCost = Resources.Essence, CostPerStructure = 50.0 };
        StructuresCat[WorkshopCategories.Structures][Structures.EssenceMiner] = new StructureData { ResourceCost = Resources.Alpha, CostPerStructure = 10.0 };

        UpgradesCat[WorkshopCategories.Upgrades][Upgrade.EssenceMinerBase] = new UpgradesData { ResourceCost = Resources.Alpha, Cost = 10.0 };
        UpgradesCat[WorkshopCategories.Upgrades][Upgrade.EssenceMinerMultiplier] = new UpgradesData { ResourceCost = Resources.Beta, Cost = 50.0 };
        UpgradesCat[WorkshopCategories.Upgrades][Upgrade.FactoryInput] = new UpgradesData { ResourceCost = Resources.Gamma, Cost = 50.0 };
        UpgradesCat[WorkshopCategories.Upgrades][Upgrade.FactoryOutput] = new UpgradesData { ResourceCost = Resources.Gamma, Cost = 50.0 };
    }
    #endregion
}

public static class FactoryVariables {

    private const double Base = 1.0d;

    public static double AlphaBaseProd = 1;
    public static double BetaBaseProd = 1;
    public static double GammaBaseProd = 1;

    #region Planet Related buffs
    private static double PlanetMiningBonusRaw = 0.0;
    private static double PlanetFactoryBonusRaw = 0.0;

    public static double PlanetMiningBonus {
        get => PlanetMiningBonusRaw;
        set => PlanetMiningBonusRaw = Math.Max(0.0, value);
    }
    public static double PlanetFactoryBonus {
        get => PlanetFactoryBonusRaw;
        set => PlanetFactoryBonusRaw = Math.Max(0.0, value);
    }

    public static double PlanetMiningMultiplier => 1 + PlanetMiningBonus;
    public static double PlanetFactoryMultiplier => 1 + PlanetFactoryBonus;
    #endregion

}

public static class RecipesStuff {

    static RecipesStuff() {
        BuildRecipes();

        RecipeBook = new ReadOnlyDictionary<Recipes, RecipeData>(GodOfAllRecipe);
    }

    private static readonly Dictionary<Recipes, RecipeData> GodOfAllRecipe = new();

    public static ReadOnlyDictionary<Recipes, RecipeData> RecipeBook { get; }

    private static void BuildRecipes() {
        RecipeData AlphaRecipe = new RecipeData();
        AlphaRecipe.Inputs.Add(Resources.Essence, 5);
        AlphaRecipe.Outputs.Add(Resources.Alpha, 1);

        RecipeData BetaRecipe = new RecipeData();
        BetaRecipe.Inputs.Add(Resources.Alpha, 5);
        BetaRecipe.Inputs.Add(Resources.Essence, 10);
        BetaRecipe.Outputs.Add(Resources.Beta, 1);

        RecipeData GammaRecipe = new RecipeData();
        GammaRecipe.Inputs.Add(Resources.Alpha, 5);
        GammaRecipe.Inputs.Add(Resources.Beta, 10);
        GammaRecipe.Inputs.Add(Resources.Essence, 20);
        GammaRecipe.Outputs.Add(Resources.Gamma, 1);

        GodOfAllRecipe[Recipes.AlphaRecipe] = AlphaRecipe;
        GodOfAllRecipe[Recipes.BetaRecipe] = BetaRecipe;
        GodOfAllRecipe[Recipes.GammaRecipe] = GammaRecipe;
    }
}








