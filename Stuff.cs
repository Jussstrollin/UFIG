using System.Collections.Generic;

namespace StellaForge;

public class GlobalVariables {
    public static readonly int MainWindow_W_Cell = 90;
    public static readonly int MainWindow_H_Cell = MainWindow_W_Cell / 3;
}

public class Enums {
    public enum ResourceType {
        Essence,
        Alpha,
        Beta,
        Gamma,
    };

    public enum FactoryTrait {
        TimeDialation,
        PassedQualityAssurance,
        ThoughtfulMakers,
        BrokenOutputHatch,
        Unstable,
        DangerousConstruction,
        Control,
        EnlighteningAura
    }

    public enum MinerTrait {
        Bedrock,
        SoftSoil,
        PristineCondition,
        TooMuchSuck,
        PoorMounting
    }

    public enum FactoryTier {
        Prototype,
        PrototypePlus,
        PrototypePlusPlus,
        Refined,
        Advanced,
        Experimental,
        Apex,
    }

    public enum FactoryTypes {
        AlphaFactory,
        BetaFactory,
        GammaFactory,
    }
}

public class Storage {
    public Dictionary<Enums.ResourceType, double> Resources = new()
    {
        { Enums.ResourceType.Alpha, 0.0d },
        { Enums.ResourceType.Beta, 0.0d },
        { Enums.ResourceType.Gamma, 0.0d },
        { Enums.ResourceType.Essence, 100.0d },
    };

    public enum ReturnType { SUCCESS, FAIL, INVALID }

    public event Action? OnStorageChange;

    public ReturnType TryAppend(Enums.ResourceType Resource, double Val) {
        if (Val < 0) {
            return ReturnType.INVALID;
        }

        var OldVal = Resources[Resource];

        Resources[Resource] += Val;

        var NewVal = Resources[Resource];

        if (OldVal == NewVal) {
            return ReturnType.FAIL;
        }
        else {
            OnStorageChange?.Invoke();
            return ReturnType.SUCCESS;
        }
    }

    public ReturnType TryDeduct(Enums.ResourceType Resource, double Val, bool IsCheck) {
        if (IsCheck == true) {
            if (Resources[Resource] < Val) {
                return ReturnType.FAIL;
            }
            else {
                OnStorageChange?.Invoke();
                return ReturnType.SUCCESS;
            }
        }

        if (Val < 0) {
            return ReturnType.INVALID;
        }

        if (Resources[Resource] < Val) {
            return ReturnType.FAIL;
        }

        var OldVal = Resources[Resource];
        Resources[Resource] -= Val;
        var NewVal = Resources[Resource];

        if (OldVal == NewVal) {
            return ReturnType.FAIL;
        }
        else {
            OnStorageChange?.Invoke();
            return ReturnType.SUCCESS;
        }
    }
}

