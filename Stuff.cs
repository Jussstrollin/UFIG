using System.Collections.Generic;

namespace StellaForge;

public class Enums {
    public enum ResourceType {
        Essence,
        Alpha,
        Beta,
        Gamma,
    };

    public enum FactoryTrait {
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
            return ReturnType.SUCCESS;
        }
    }

    public ReturnType TryDeduct(Enums.ResourceType Resource, double Val, bool IsCheck) {
        if (IsCheck == true) {
            if (Resources[Resource] < Val) {
                return ReturnType.FAIL;
            }
            else {
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
            return ReturnType.SUCCESS;
        }
    }
}

