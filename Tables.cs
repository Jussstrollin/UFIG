namespace UFIG;

using Spectre.Console;
using static Program;

public static class Tables
{
    public static Table GameBuildFactoryTable() {
        var FactoryTable = new Table();

        FactoryTable.AddColumn("[white] Factory [/]"); // Make Columns (the vertical slices)
        FactoryTable.AddColumn("[white] Amount [/]");
        FactoryTable.AddColumn("[white] Status [/]");

        FactoryTable.AddRow(
            "[yellow] Alpha [/]", // refer to Colums made
            Structure.AlphaFactory.ToString(),
                            AlphaFactory.InputCheck() ? "[green]▶ Running [/]" : $"{AlphaFactory.HaltReason}"
        );
        FactoryTable.AddRow(
            "[blue] Beta [/]",
            Structure.BetaFactory.ToString(),
                            BetaFactory.InputCheck() ? "[green]▶ Running [/]" : $"{BetaFactory.HaltReason}"
        );
        FactoryTable.AddRow(
            "[green] Gamma [/]",
            Structure.GammaFactory.ToString(),
                            GammaFactory.InputCheck() ? "[green]▶ Running [/]" : $"{GammaFactory.HaltReason}"
        );

        FactoryTable.Border = TableBorder.Rounded;
        FactoryTable.Width = 70;

        return FactoryTable;

    }

    public static Table GameBuildUpgradeTable() {
        var UpgradeTable = new Table();

        // FactoryTable.AddColumn("[white] Amount [/]");

        UpgradeTable.AddColumn("[white] Upgrade [/]");
        UpgradeTable.AddColumn("[white] Bought [/]");
        UpgradeTable.AddColumn("[white] Effects [/]");

        UpgradeTable.AddRow(
            "[cyan]Essence[/] Base Upgrade", UpgradeTrack.EssenceBaseBought.ToString(), "Temp"
        );
        UpgradeTable.AddRow(
            "[cyan]Essence[/] Multiplier Upgrade", UpgradeTrack.EssenceMultiplierBought.ToString(), "Temp"
        );
        UpgradeTable.AddRow(
            "[purple]Factory Input Upgrade[/]", UpgradeTrack.FactoryInputUpgradeBought.ToString(), "Temp"
        );
        UpgradeTable.AddRow(
            "[purple]Factory Output Upgrade[/]", UpgradeTrack.FactoryOutputUpgradeBought.ToString(), "Temp"
        );

        UpgradeTable.Border = TableBorder.Rounded;
        UpgradeTable.Width = 70;

        return UpgradeTable;
    }
}
