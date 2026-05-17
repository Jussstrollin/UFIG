namespace UFIG;

using System;
using System.Collections.Generic;

public static class GameManager {
    //Global Canvas
    public const int CellX = 120;
    public const int CellY = 40;

    // Consts / Variables
    private const double Base = 1.0d;

    public static double AlphaBaseProd = 1;
    public static double BetaBaseProd = 1;
    public static double GammaBaseProd = 1;

    private static double PlanetMiningBonusRaw = 0.0;
    public static double PlanetMiningBonus {
        get => PlanetMiningBonusRaw;
        set => PlanetMiningBonusRaw = Math.Max(0.0, value);
    }
    public static double PlanetMiningMultiplier => 1 + PlanetMiningBonus;

    private static double PlanetFactoryBonusRaw = 0.0;
    public static double PlanetFactoryBonus {
        get => PlanetFactoryBonusRaw;
        set => PlanetFactoryBonusRaw = Math.Max(0.0, value);
    }
    public static double PlanetFactoryMultiplier => 1 + PlanetFactoryBonus;

    private static double PercentShopDiscountRaw = 0.0;
    public static double PercentShopDiscount {
        get => PercentShopDiscountRaw;
        set => PercentShopDiscountRaw = Math.Max(0.0, value);
    }
    public static double PercentShopDiscountMultiplier => 1 - (PercentShopDiscount / 100);

    // Enums
    public enum Resources {
        Alpha,
        Beta,
        Gamma,
        Essence
    }
    public enum Structures {
        AlphaFactory,
        BetaFactory,
        GammaFactory,
        EssenceMiner
    }
    public enum Upgrade {
        EssenceMinerBase,
        EssenceMinerMultiplier,

        FactoryInput,
        FactoryOutput
    }
    public enum States {
        StatePlaying,
        StateShop,
        StatePlanetary
    }
    public enum Planets {
        Space,
        Origo,
        Sterelis,
        Primaris
    }
    public enum Recipes {
        Alpha,
        Beta,
        Gamma
    }

    // Data
    public class UpgradesData {
        public int Amount;
        public double Cost;
    }
    public class FactoryData {
        public int Amount;
        public double Cost;
        public bool IsRunning;
    }
    public class WalletData {
        public double Amount;
    }
    public class RecipeData {
        public Dictionary<Resources, double> Inputs { get; set; };
        public Dictionary<Resources, double> Outputs { get; set; }:
    }

    // DICS
    public static Dictionary<Resources, WalletData> Wallets = new();

    public static Dictionary<Structures, FactoryData> Factory = new();

    public static Dictionary<Upgrade, UpgradesData> Upgrades = new();

    // Init
    static void InitDefaults() {
        Wallets[Resources.Alpha] = new WalletData { Amount = 0.0d };
        Wallets[Resources.Beta] = new WalletData { Amount = 0.0d };
        Wallets[Resources.Gamma] = new WalletData { Amount = 0.0d };
        Wallets[Resources.Essence] = new WalletData { Amount = 0.0d };

        Factory[Structures.AlphaFactory] = new FactoryData { Amount = 0, Cost = 10, IsRunning = false };
        Factory[Structures.BetaFactory] = new FactoryData { Amount = 0, Cost = 30, IsRunning = false };
        Factory[Structures.GammaFactory] = new FactoryData { Amount = 0, Cost = 60, IsRunning = false };
        Factory[Structures.EssenceMiner] = new FactoryData { Amount = 0, Cost = 20, IsRunning = false };

        Upgrades[Upgrade.EssenceMinerBase] = new UpgradesData { Amount = 0, Cost = 20 }; // Alpha
        Upgrades[Upgrade.EssenceMinerMultiplier] = new UpgradesData { Amount = 0, Cost = 50 }; // Alpha
        Upgrades[Upgrade.FactoryInput] = new UpgradesData { Amount = 0, Cost = 50 }; // cost Beta
        Upgrades[Upgrade.FactoryOutput] = new UpgradesData { Amount = 0, Cost = 50 }; // Cost Gamma
    }

    // classes

    public static class PlayerState {
        public static States StateOn = States.StatePlaying;

        public static bool IsPaused = false;
        public static bool IsStopped = false;

        public static Planets PlanetOn = Planets.Origo;
        public static bool IsLanded = true;
        public static bool IsOrbiting = false;
    }

    public static class Manipulate {
        public static bool WalletManipulate(Resources ToTarget, double AmountTo, bool IsDeduct) {
            if (IsDeduct) {
                if (AmountTo <= Wallets[ToTarget].Amount) {
                    Wallets[ToTarget].Amount -= AmountTo;
                    return true;
                }
                return false;
            }

            if (!IsDeduct) {
                Wallets[ToTarget].Amount += AmountTo;
                return true;
            }

            return false;
        }

        public static bool FactoryManipulate(Structures ToTarget, int ToAdd) {
            Factory[ToTarget].Amount += ToAdd;
            return true;
        }
        public static bool FactoryStatusManipulate(Structures ToTarget, bool ISRunning) {
            Factory[ToTarget].IsRunning = ISRunning;
            return true;
        }

        public static bool UpgradesManipulate(Upgrade ToTarget, int ToAdd) {
            Upgrades[ToTarget].Amount += ToAdd;
            return true;
        }
    }

    public static class Fetch {
    }

    public static class Recipes {
        public static Dictionary<
    }
































}
