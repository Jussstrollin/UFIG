namespace UFIG;

using static Program;

public static class StringsStuff {
    public static string GetAlphaBar() {
        // This uses a fixed-width, double-precision bar to prevent terminal layout flickering.
        // Instead of changing the number of characters, it uses "▌" (half-block) to represent
        // a subtler gradation inside a cell. The total width of the bar never changes,
        // so the terminal doesn't need to recalculate the layout of the entire panel mid-frame.
        if (Structure.AlphaFactory == 0) return "";

        const int barLength = 10;
        double progress = (double)AlphaFactoryProgressIncrement / 20.0;
        progress = Math.Clamp(progress, 0.0, 1.0);

        int totalHalfBlocks = (int)(progress * barLength * 2);
        System.Text.StringBuilder bar = new System.Text.StringBuilder();

        for (int i = 0; i < barLength; i++) {
            int remainingHalfBlocks = totalHalfBlocks - (i * 2);
            if (remainingHalfBlocks >= 2) bar.Append('█');
            else if (remainingHalfBlocks == 1) bar.Append('▌');
            else bar.Append('▒');
        }

        return $"{{{bar}}}";
    }

    public static string GetBetaBar() {
        if (Structure.BetaFactory == 0) return "";

        const int barLength = 10;
        double progress = (double)BetaFactoryProgressIncrement / 50.0;
        progress = Math.Clamp(progress, 0.0, 1.0); // every .10 is 10%

        int totalHalfBlocks = (int)(progress * barLength * 2);
        System.Text.StringBuilder bar = new System.Text.StringBuilder();

        for (int i = 0; i < barLength; i++) {
            int remainingHalfBlocks = totalHalfBlocks - (i * 2);
            if (remainingHalfBlocks >= 2) bar.Append('█');
            else if (remainingHalfBlocks == 1) bar.Append('▌');
            else bar.Append('▒');
        }

        return $"{{{bar}}}";
    }

    public static string GetGammaBar() {
        if (Structure.GammaFactory == 0) return "";

        const int barLength = 10;
        double progress = (double)GammaFactoryProgressIncrement / 80.0;
        progress = Math.Clamp(progress, 0.0, 1.0);

        int totalHalfBlocks = (int)(progress * barLength * 2);
        System.Text.StringBuilder bar = new System.Text.StringBuilder();

        for (int i = 0; i < barLength; i++) {
            int remainingHalfBlocks = totalHalfBlocks - (i * 2);
            if (remainingHalfBlocks >= 2) bar.Append('█');
            else if (remainingHalfBlocks == 1) bar.Append('▌');
            else bar.Append('▒');
        }

        return $"{{{bar}}}";
    }

    public static string GetEssenceBar() {
        const int barLength = 10;
        double progress = (double)EssenceMinerProgressIncrement / 30.0;
        progress = Math.Clamp(progress, 0.0, 1.0);

        int totalHalfBlocks = (int)(progress * barLength * 2);
        System.Text.StringBuilder bar = new System.Text.StringBuilder();

        for (int i = 0; i < barLength; i++) {
            int remainingHalfBlocks = totalHalfBlocks - (i * 2);
            if (remainingHalfBlocks >= 2) bar.Append('█');
            else if (remainingHalfBlocks == 1) bar.Append('▌');
            else bar.Append('▒');
        }

        return $"{{{bar}}}";
    }


    public static string GamePanelStats =>
    $"\n" +
    $"Essence : {EssenceWallet.Amount:F1} | {NetProd.Essence.ToString("F2")} / sec | {GetEssenceBar()}\n" +
    $"\n" +
    $"Alpha : {AlphaWallet.Amount:F1} | {NetProd.Alpha.ToString("F2")} / sec |  {GetAlphaBar()}\n" +
    $"Beta : {BetaWallet.Amount:F1} | {NetProd.Beta.ToString("F2")} / sec | {GetBetaBar()}\n" +
    $"Gamma : {GammaWallet.Amount:F1} | {NetProd.Gamma.ToString("F2")} / sec | {GetGammaBar()}\n" +
    $"\n" +
    $"\n"
    // $"Debug\n" +
    // $"AlphaFactory : {AlphaFactoryProgressIncrement}\n" +
    // $"BetaFactory : {BetaFactoryProgressIncrement}\n" +
    // $"GammaFactory : {GammaFactoryProgressIncrement}\n"
    // $"Event to show : {EventSystem.EventToShow.ToString()}\n" +
    // $"Alpha Forced Event Done : {EventSystem.AlphaForcedEventDone.ToString()}\n" +
    // $"Event Incrementer : {EventIncrement.ToString()}\n" +
    // $"Can Show : {EventSystem.CanShowEvent.ToString()}\n" +
    // $"ForcedEventWantToShow : {EventSystem.ForcedEventWantToShow.ToString()}\n"
    ;

    public static string ShopMainPanel =>
    $"Random String here\n" +
    $"\n" +
    $"\n" +
    $" > Factories | Press 1 to Open\n " +
    $" > Upgrades | Press 2 to Open\n " +
    $" > Miners | Press 3 to Open\n "
    ;

    public static string ShopCategoryFactory =>
    $" Here, you can buy Factories to expand the production \n" +
    $"\n" +
    $"\n" +
    $" > Factories < \n" +
    $" \n " +
    $" Alpha Factory Press 1 to see\n" +
    $"  Beta Factory  Press 2 to see \n" +
    $"  Gamma Factory  Press 3 to see \n"
    ;

    public static string ShopCategoryUpgrades =>
    $" Here, you can buy Upgrades to Make you Factory more efficient \n" +
    $"\n" +
    $"\n" +
    $" > Upgrades < \n" +
    $" \n " +
    $" Factory Input Upgrades Press 1 to see\n" +
    $" Factory Output Upgrades Press 2 to see \n" +
    $" Essence Miner Base Production Upgrade Press 3 to see \n" +
    $" essence Miner Multiplier Upgrade Press 4 to see"
    ;

    public static string ShopCategoryMiners =>
    $" Here, you can buy Miners that extract the planets Resource \n" +
    $"\n" +
    $"\n" +
    $" > Miners < \n" +
    $" \n " +
    $" Essence Miner Press 1 to see\n"
    ;

    public static string ShopAlphaFactoryPanel =>
    $" Alpha Factory \n" +
    $"\n" +
    $"Description : \n" +
    $" - A factory that Consumes 1 Essence to produce 1 Alpha Per tick.\n" +
    $"\n" +
    $"Cost : {Structure.AlphaFactoryCost}\n" +
    $"" +
    $"You currently have : {Structure.AlphaFactory} Factories\n" +
    $"\n" +
    $"Press ENTER to Purchase\n" +
    $"Press B to Go back\n"
    ;

    public static string ShopBetaFactoryPanel =>
    $" Beta Factory \n" +
    $"\n" +
    $"Description : \n" +
    $" - A factory that Consumes 1 Alpha to produce 1 Beta Per tick.\n" +
    $"\n" +
    $"Cost : {Structure.BetaFactoryCost}\n" +
    $"" +
    $"You currently have : {Structure.BetaFactory} Factories\n" +
    $"\n" +
    $"Press ENTER to Purchase\n" +
    $"Press B to Go back\n"
    ;

    public static string ShopGammaFactoryPanel =>
    $" Gamma Factory \n" +
    $"\n" +
    $"Description : \n" +
    $" - A factory that Consumes 1 Alpha and 1 Beta to produce 1 Gamma Per tick.\n" +
    $"\n" +
    $"Cost : {Structure.GammaFactoryCost}\n" +
    $"" +
    $"You currently have : {Structure.GammaFactory} Factories\n" +
    $"\n" +
    $"Press ENTER to Purchase\n" +
    $"Press B to Go back\n"
    ;

    public static string ShopEssenceBaseProductionPanel =>
    $" Essence Base Production \n" +
    $"\n" +
    $"Description : \n" +
    $" - Essence Is produced at the rate of Base multiplied by a Multiplier ( E = Base*multiplier ), buying this adds +1 Essence per tick times {UpgradeTrack.EssenceMultiplierBought}\n" +
    $"\n" +
    $"Cost : {UpgradeTrack.EssenceBaseCost} Alpha\n" +
    $"" +
    $"You currently have : {UpgradeTrack.EssenceBaseBought} Base Essence Production\n" +
    $"\n" +
    $"Press ENTER to Purchase\n" +
    $"Press B to Go back\n"
    ;

    public static string ShopEssenceMultiplierPanel =>
    $" Essence Multiplier \n" +
    $"\n" +
    $"Description : \n" +
    $" - Adds A Multiplier for Essence Production\n" +
    $"\n" +
    $"Cost : {UpgradeTrack.EssenceMultiplierCost} Beta\n" +
    $"" +
    $"You currently have : {UpgradeTrack.EssenceMultiplierBought} Essence Multiplier\n" +
    $"\n" +
    $"Press ENTER to Purchase\n" +
    $"Press B to Go back\n"
    ;

    public static string ShopFactoryInputUpgradePanel =>
    $" Factory Input mechanism \n" +
    $"\n" +
    $"Description : \n" +
    $" - Improving the Input Mechanism of all Factory, improving and reducing needed Resource input by 5%\n" +
    $"\n" +
    $"Cost : {UpgradeTrack.FactoryInputUpgradeCost} Gamma\n" +
    $"" +
    $"You currently have : {UpgradeTrack.FactoryInputUpgradeBought} Upgrades Bought\n" +
    $"\n" +
    $"Press ENTER to Purchase\n" +
    $"Press B to Go back\n"
    ;

    public static string ShopFactoryOutputUpgradePanel =>
    $" Factory Line Performance Optimisation \n" +
    $"\n" +
    $"Description : \n" +
    $" - Improving the Factory Line to gain ~10% Output for the same Input some said 'why are we using an inefficient one in the first place?' \n" +
    $"\n" +
    $"Cost : {UpgradeTrack.FactoryOutputUpgradeCost} Gamma\n" +
    $"" +
    $"You currently have : {UpgradeTrack.FactoryOutputUpgradeBought} Upgrades Bought\n" +
    $"\n" +
    $"Press ENTER to Purchase\n" +
    $"Press B to Go back\n"
    ;

    public static string ShopEssenceMiner =>
    $" Essence Miner \n" +
    $"\n" +
    $"Description : \n" +
    $" - An Essence Miner, To mine the Mysterious Material 'Essence', said to have an unknown origin, but is the Base Material in Synthesizing Alpha.\n" +
    $"\n" +
    $"Cost : {Structure.EssenceMinerCost} Alpha\n" +
    $"" +
    $"You currently have : {Structure.EssenceMiner} Miners Bought\n" +
    $"\n" +
    $"Press ENTER to Purchase\n" +
    $"Press B to Go back\n"
    ;

    public static string ForcedAlphaEvent1 =>
    $"Alpha Factory Licensing Changes\n" +
    $"\n" +
    $"The Council has made Changes upon the discovery of Total Alpha Prodcution in the 'PlaceHolder' Sector.. Alpha Factory Prices has been Permanently raised by 100%.\n" +
    $"\n"
    ;

    public static string ForcedAlphaEvent2 =>
    $"Alpha Factory Licensing Changes : A looming threat\n" +
    $"\n" +
    $"The Council is pushing new Licensing changes on Alpha Factories on the 'PlaceHolder' sector.. Alpha Factory Prices is Permanently raised by 80%.\n" +
    $"\n"
    ;

    public static string ForcedAlphaEvent3 =>
    $"Alpha Factory Tariff Adjustment\n" +
    $"\n" +
    $"The Council has reviewed updated Alpha production figures across the sector. A revised tariff schedule has been filed. Alpha Factory costs increased by 130%.\n" +
    $"\n" +
    $"\"Market stabilization protocol. Nothing personal.\"\n"
    ;

    public static string ForcedAlphaEvent4 =>
    $"Alpha Factory Quota Update\n" +
    $"\n" +
    $"With Alpha production reaching unprecedented levels, the Council has enacted a supplementary licensing adjustment. Alpha Factory costs increased by 50%.\n" +
    $"\n" +
    $"\"You've been busy. We noticed. This is just procedure.\"\n"
    ;

    public static string ForcedBetaEvent1 =>
    $"Beta Factory Licensing Changes\n" +
    $"\n" +
    $"After a board meeting, The Council has Decided to raise Beta Factory licensing prices to a staggering 150%, 'Wouldve been smarter to have bought them in bulk earlier....'\n" +
    $"\n"
    ;

    public static string ForcedBetaEvent2 =>
    $"Beta Factory Cost Raise\n" +
    $"\n" +
    $"Due to some bureaucratic tomfoolery, The Market Cost for Beta factories is raised by 80%....\n" +
    $"\n"
    ;

    public static string ForcedBetaEvent3 =>
    $"Beta Factory Compliance Review\n" +
    $"\n" +
    $"The Council's quarterly compliance review has concluded. Beta Factory licensing has been recategorized under a new administrative schedule. Beta Factory costs increased by 40%.\n" +
    $"\n" +
    $"\"The forms were filed weeks ago. You were notified.\"\n"
    ;

    public static string ForcedBetaEvent4 =>
    $"Beta Factory Licensing Restructure\n" +
    $"\n" +
    $"Significant Beta production output has triggered an automatic licensing restructure per Council mandate. Beta Factory costs increased by 150%.\n" +
    $"\n" +
    $"\"We don't make the rules. Well, we do. But we filed them properly.\"\n"
    ;

    public static string ForcedGammaEvent1 =>
    $"Gamma Factory Raised costs\n" +
    $"\n" +
    $"Due to the Rarity and difficulty to Produce Gamma, a Sector spread Panic buying is happening, causing for Gamma Factory cost to Skyrocket up to 130% price increase!\n" +
    $"\n"
    ;

    public static string ForcedGammaEvent2 =>
    $"Gamma Factory Construction Material\n" +
    $"\n" +
    $"An Event happened Causing Gamma Factory Construction materials to be rarer, increasing demand causes price to Rise by 90%\n" +
    $"\n"
    ;

    public static string ForcedGammaEvent3 =>
    $"Gamma Factory Administrative Review\n" +
    $"\n" +
    $"The Council has completed its review of Gamma production permits. A new fee structure has been approved and applied retroactively. Gamma Factory costs increased by 60%.\n" +
    $"\n" +
    $"\"You can appeal. Form 47-B. Processing time: 18 months.\"\n"
    ;

    public static string ForcedGammaEvent4 =>
    $"Gamma Factory Sector-Wide Adjustment\n" +
    $"\n" +
    $"Gamma production has exceeded projected thresholds. The Council has issued a mandatory licensing adjustment to maintain market equilibrium. Gamma Factory costs increased by 100%.\n" +
    $"\n" +
    $"\"Congratulations on the milestone. Here's the bill.\"\n"
    ;

    public static string EmptyEvent =>
    $"No event has happened yet."
    ;

    public static string PrimarisPlanetPanelString =>
    $"Primāris\n" +
    $"\n" +
    $"A Planet Rich in Essence, Scriptures Dates back to 2099, when the first humans First discovered this Planet,\n" +
    $"it is documented to have Uneven and rough terrain making factory production almost impossible, only Miners is unaffected\n" +
    $"Scriptures document Structures like &%#%## %#$#@ and ##%**% Containing #%##@@!, Last Expidition Sent has not returned\n" +
    $"Messages sent by the last team reports : @$@$!U@%*@ -HEL #@$@% IT'S GO*%#% TOW#%# US " +
    $"Expedition to structures is not recommended\n" +
    $"\n" +
    $"\n" +
    $"Bonus Mining Productivity : 100% \n" +
    $"Bonus Factory Productivity : -80% \n"
    ;

    public static string SterilisPlanetPanelString =>
    $"Sterelis\n" +
    $"\n" +
    $"This Planet is not rich on any minable resources hence the name, however, it is known to have a Large open area and friendly climate that Greatly enhances Factory Production.\n This is a great Planet to setup a big Factory line!" +
    $"\n" +
    $"\n" +
    $"Bonus Mining Productivity : -90% \n" +
    $"Bonus Factory Productivity : +80% \n"
    ;

    public static string OrigoPlanetPanelString =>
    $"Origo\n" +
    $"\n" +
    $"The only known data: Scripture from the old world (2006)\n" +
    $"A rocky, terrestrial planet. A radius of around @%##*, 70% of its\n" +
    $"surface is covered with #&%#@*, and enveloped by *#%@) protecting it\n" +
    $"from the harshness of space. It contains the material known as\n" +
    $"'@$@$(%)'...\n" +
    $"\"It is the only place to date, known to have life in the Universe.\"\n" +
    $"The rest is unreadable.\n" +
    $"\n" +
    $"Bonus Mining Productivity: 0%\n" +
    $"Bonus Factory Productivity: 0%\n"
    ;

    public static string SupposedMap =>
    $"Hi! Im map, I may now look like it right now, But Im trying my best!"
    ;

    public static string PlanetChoices =>
    $"Press ENTER to Travel to that Planet \n" +
    $"Known Planets : \n" +
    $"\n" +
    $" > Planets < \n" +
    $"\n" +
    $" > Origo (Landed) (1)\n" +
    $" > Sterelis (Landed) (2)\n" +
    $" > Primaris (Landed) (3)\n"
    ;

    public static string PlanetTravelConfirmationToOrigo =>
    $"Are you Sure to Travel to Origo?\n" +
    $" Yes (Y)  |  No (N) "
    ;

    public static string PlanetTravelConfirmationToSterelis =>
    $"Are you Sure to Travel to Sterelis?\n" +
    $" Yes (Y)  |  No (N) "
    ;

    public static string PlanetTravelConfirmationToPrimaris =>
    $"Are you Sure to Travel to Primaris?\n" +
    $" Yes (Y)  |  No (N) "
    ;

    public static string SpacePanelString =>
    $"Space\n" +
    $"\n" +
    $"Its cold here.." +
    $"No Life, Other than..Me..\n" +
    $"\n" +
    $"Bonus Mining Productivity: -100%\n" +
    $"Bonus Factory Productivity: -100%\n"
    ;
}
