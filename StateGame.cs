namespace UFIG;

using static Program;
using static GameUI;

using Spectre.Console;

public class StatePlaying : StateInterface {
    public void GoingIn() {
        AnsiConsole.Clear();
        GameState.MenuID = Menu.Game;
        ApplyPlanetBuffs();
        // Feel Free to add an Opening Seequence Here 
        Display();
    }

    public void GoingOut() {
        AnsiConsole.Clear();
        return;
    }

    public void Display() {
        var GameUi = new GameUI();

        if (GameState.MenuID == Menu.Game) {
            AnsiConsole.Write(GameUi.InitGameLayout());
        }
    }

    public void HandleControls(char key) {
        return; // No function to really do here
    }

    public void Update() {
        DateTime now = DateTime.Now;

        if (!GameState.Pause) {
            if ((now - LastProgressBarsTick).TotalSeconds >= 0.10) {
                // ticks is in .10 steps, its the UI's job to handle Checks and Resets.
                if (AlphaFactory.InputCheck() == true) {
                    if (Structure.AlphaFactory >= 1) {
                        if (AlphaFactoryProgressIncrement <= 20) { // 2.0s
                            AlphaFactoryProgressIncrement++; // it allows 16th yes, i dont care
                        }
                        else if (AlphaFactoryProgressIncrement > 20) {
                            AlphaFactory.RunFactory();
                            AlphaFactoryProgressIncrement = 0;
                        }
                    }
                }

                if (BetaFactory.InputCheck() == true) {
                    if (Structure.BetaFactory >= 1) {
                        if (BetaFactoryProgressIncrement <= 50) { // 5.0s
                            BetaFactoryProgressIncrement++;
                        }
                        else if (BetaFactoryProgressIncrement > 50) {
                            BetaFactory.RunFactory();
                            BetaFactoryProgressIncrement = 0;
                        }
                    }
                }

                if (GammaFactory.InputCheck() == true) {
                    if (Structure.GammaFactory >= 1) {
                        if (GammaFactoryProgressIncrement <= 80) { // 8.0s
                            GammaFactoryProgressIncrement++;
                        }
                        else if (GammaFactoryProgressIncrement > 80) {
                            GammaFactory.RunFactory();
                            GammaFactoryProgressIncrement = 0;
                        }
                    }

                }

                if (EssenceMinerProgressIncrement <= 30) {
                    EssenceMinerProgressIncrement++;
                }
                else if (EssenceMinerProgressIncrement > 30) {
                    EssenceProduction();
                    EssenceMinerProgressIncrement = 0;
                }

                LastProgressBarsTick = now;
            }
        }
    }

    private void ApplyPlanetBuffs() {
        switch (GameState.PlanetOn) {
            case Planet.Origo:
                PlanetFactoryBonus = 0.0d;
                PlanetMiningBonus = 0.0d;
                break;
            case Planet.Sterelis:
                PlanetFactoryBonus = 0.8d;
                PlanetMiningBonus = -0.9d;
                break;
            case Planet.Primaris:
                PlanetFactoryBonus = -0.80d;
                PlanetMiningBonus = 1.0d;
                break;
            case Planet.Space:
                PlanetFactoryBonus = -1.0d;
                PlanetMiningBonus = -1.0d;
                break;
            default:
                break;
        }
    }

    private static void EssenceProduction() { // where Factory, essence and eveery production will be called                              // Source Material always first
        float EssenceBase = 1.0f * UpgradeTrack.EssenceBaseBought;
        float EssenceMultiplier = 1.0f * UpgradeTrack.EssenceMultiplierBought;
        float EssenceGain = (EssenceBase * EssenceMultiplier) * Structure.EssenceMiner;
        Pending.Essence += EssenceGain;
        NetProd.Essence += EssenceGain;

        PushPending();
        WipePending();
    }
}



