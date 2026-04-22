namespace UFIG;

using static Program;

public class PlanetStuff
{
    public static void ApplyPlanetBuff() {
        if (GameState.PlanetOn == Planet.Origo && GameState.IsLanded) { // default starting planet
            PlanetFactoryBonus = 0.0d; // in percent form its applied as (baseproduction) * (1 + this) , so if 1.0 (Max) you get a 100% bonus on production, if -0.99 (max lowest) you get -99% production, going past -0.99 you go * 0 which is just zero thats a nono, unless you want to disable this function on this planet
            PlanetMiningBonus = 0.0d;
            // no buffs
        } else if (GameState.PlanetOn == Planet.Primaris && GameState.IsLanded) {
            PlanetFactoryBonus = -0.80d; // -80%
            PlanetMiningBonus = 1.0d; // 100%
        } else if (GameState.PlanetOn == Planet.Sterelis && GameState.IsLanded) {
            PlanetFactoryBonus = 0.80d; // 80%
            PlanetMiningBonus = -0.90d; // -90%
        } else if (GameState.PlanetOn == Planet.Space || GameState.IsOrbiting) {
            PlanetFactoryBonus = -1.0d; // -100%
            PlanetMiningBonus = -1.0d; // -100%
            // theres no gravity, your factories wont work, and you cant mine in nothing
        }
    }
}
