namespace UFIG;

using static Program;

public class FactoryStuff
{
    public Resources Resource { get; set; }
    public string HaltReason { get; private set; } = ""; // a defualt value so C# wont cry bout it

    public FactoryStuff(Resources Resource) {
        this.Resource = Resource;
    }

    // 5% Input reduction per upgrades boguht
    static double BonusInputReduction => 0.05f * UpgradeTrack.FactoryInputUpgradeBought;
    // 10% Bonus prod per upgrade Bought
    static double BonusProduction => 1 + (0.10f * UpgradeTrack.FactoryOutputUpgradeBought);

    bool FactoryHalted = true;

    private (Resources[] InputTypes, double[] InputAmount) GetInputRequirements() { // takes Base Resources
        return this.Resource switch {
            Resources.Alpha => (
                new Resources[] { Resources.Essence },
                new double[] { 5.0d }
            ),
            Resources.Beta => (
                new Resources[] { Resources.Essence, Resources.Alpha },
                new double[] { 15.0d, 10.0d }
            ),
            Resources.Gamma => (
                new Resources[] { Resources.Essence, Resources.Alpha, Resources.Beta },
                new double[] { 30.0d, 20.0d, 15.0d}
            ),
            _ => (new Resources[] {}, new double[] {})
        };
    }

    // LookUp what this Factory Produces
    private (Resources OutputResource, double OutputAmount) GetOutputs() {
        return this.Resource switch { // what factor this is and Output then how many
            Resources.Alpha => (Resources.Alpha, 1.0d),
            Resources.Beta => (Resources.Beta, 1.0d),
            Resources.Gamma => (Resources.Gamma, 1.0d),
            _ => (Resources.Essence, 0.0d)
        };
    }

    // LookUp what Resource we have or rather, the user have. Based on the crap we need
    private double GetResourceAmount(Resources Resource) {
        return Resource switch {
            Resources.Essence => EssenceWallet.Amount,
            Resources.Alpha => AlphaWallet.Amount,
            Resources.Beta => BetaWallet.Amount,
            Resources.Gamma => GammaWallet.Amount,
            _ => 0.0d
        };
    }

    private int GetFactoryCount(Resources Resource) {
        return Resource switch {
            Resources.Alpha => Structure.AlphaFactory,
            Resources.Beta => Structure.BetaFactory,
            Resources.Gamma => Structure.GammaFactory,
            _ => 0
        };
    }

    // Return False if Failed, and true if otherwise
    public bool InputCheck() {
        int FactoryAmount = GetFactoryCount(this.Resource);
        if (FactoryAmount <= 0) {
            this.HaltReason = $"[gray]No Factory have been Purchased[/]";

            return false;
        }

        var (InputTypes, InputAmount) = GetInputRequirements();

        for (int i = 0; i < InputTypes.Length; i++) {
            double Needed = (InputAmount[i] * FactoryAmount) - (BonusInputReduction * FactoryAmount);
            double Available = GetResourceAmount(InputTypes[i]);

            if (Needed > Available) {
                this.HaltReason = $"[red] Waiting for {InputTypes[i]}[/]";
                return false;
            }
        }

        return true;
    }

    public bool RunFactory() {
        if (InputCheck()) {
            int FactoryAmount = GetFactoryCount(this.Resource);
            var (InputTypes, InputAmount) = GetInputRequirements();
            var (OutputResource, OutputAmount) = GetOutputs();

            // Deduct inputs (no planet bonus here, input cost is fixed)
            for (int i = 0; i < InputTypes.Length; i++) {
                double Needed = (InputAmount[i] * FactoryAmount) - (BonusInputReduction * FactoryAmount);

                // Deduct from pending based on resource type
                switch (InputTypes[i]) {
                    case Resources.Essence:
                        Pending.Essence -= Needed;
                        NetProd.Essence -= Needed;
                        break;
                    case Resources.Alpha:
                        Pending.Alpha -= Needed;
                        NetProd.Alpha -= Needed;
                        break;
                    case Resources.Beta:
                        Pending.Beta -= Needed;
                        NetProd.Beta -= Needed;
                        break;
                    case Resources.Gamma:
                        Pending.Gamma -= Needed;
                        NetProd.Gamma -= Needed;
                        break;
                }
            }

            // Calculate output with planet bonus AND production bonus
            double ToAdd = (OutputAmount * FactoryAmount) * BonusProduction * (1.0d + PlanetFactoryBonus);

            // Add output to pending
            switch (OutputResource) {
                case Resources.Alpha:
                    Pending.Alpha += ToAdd;
                    NetProd.Alpha += ToAdd;
                    break;
                case Resources.Beta:
                    Pending.Beta += ToAdd;
                    NetProd.Beta += ToAdd;
                    break;
                case Resources.Gamma:
                    Pending.Gamma += ToAdd;
                    NetProd.Gamma += ToAdd;
                    break;
            }

            // Push and wipe in same function
            PushPending();
            WipePending();
            return true;
        }
        return false;
    }
}
