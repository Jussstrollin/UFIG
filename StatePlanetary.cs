namespace UFIG;

using static Program;
using static PlanetUI;

using Spectre.Console;

public class StatePlanetary : StateInterface {
    public void GoingIn() {
        GameState.MenuID = Menu.PlanetUiDescSpace;
        // add your very cool Opening Animation or sequence here
        Display();
        return;
    }

    public void GoingOut() {
        AnsiConsole.Clear();
        return;
    }

    public void Display() {
        var PlanetUi = new PlanetUI();

        AnsiConsole.Write(PlanetUi.ShowPlanetUI());
    }

    public void HandleControls(char Key) {
        bool IsInAnyPlanetDesc = ((int)GameState.MenuID >= 200 && (int)GameState.MenuID <= 203);

        if (!IsInAnyPlanetDesc) {
            if (Key == '1') GameState.MenuID = Menu.PlanetUiDescOrigo;
            if (Key == '2') GameState.MenuID = Menu.PlanetUiDescSterelis;
            if (Key == '3') GameState.MenuID = Menu.PlanetUiDescPrimaris;
        }

        if (Key == '\r') {
            if (GameState.MenuID == Menu.PlanetUiDescOrigo) GameState.MenuID = Menu.PlanetTravelConfirmationToOrigo;
            if (GameState.MenuID == Menu.PlanetUiDescSterelis) GameState.MenuID = Menu.PlanetTravelConfirmationToSterelis;
            if (GameState.MenuID == Menu.PlanetUiDescPrimaris) GameState.MenuID = Menu.PlanetTravelConfirmationToPrimaris;
        }

        if ((int)GameState.MenuID >= 205 && (int)GameState.MenuID <= 207) {
            if (Key == 'Y' && GameState.MenuID == Menu.PlanetTravelConfirmationToOrigo) InitTravel(Planet.Origo);
            if (Key == 'Y' && GameState.MenuID == Menu.PlanetTravelConfirmationToSterelis) InitTravel(Planet.Sterelis);
            if (Key == 'Y' && GameState.MenuID == Menu.PlanetTravelConfirmationToPrimaris) InitTravel(Planet.Primaris);

            if (Key == 'N') ReturnPlayerToCurrPlanetUI();
        }
    }

    // ----------------------------------------- //

    private Planet PlanetGoal;
    private int ETAsec;
    private DateTime TravelStartTime;
    private FuelType FuelToUse;

    private bool ErrorHappened = false;

    DateTime FuelTime = DateTime.Now;

    public void Update() {
        if (!GameState.IsTravelling) return;

        var now = DateTime.Now;
        var timeElapsed = (now - TravelStartTime).TotalSeconds;

        // Burn fuel every second
        if ((now - FuelTime).TotalSeconds >= 1.0) {
            BurnFuel(FuelToUse);
            FuelTime = now;  // Reset the fuel tick timer
        }

        // Check if we've arrived
        if (timeElapsed >= ETAsec) {
            TravelLanded();
        }
    }

    private int GetETAinSec() {
        switch (PlanetGoal) { // SHHHHH, dont mind the Values here, just imagine planets is lined up always in a trianle formation
            case Planet.Origo:
                return 5;
            case Planet.Sterelis:
                return 15;
            case Planet.Primaris:
                return 30;
            default:
                ErrorHappened = true;
                AnsiConsole.WriteLine("An Error Occured on ETA Fetch | Unknown Planet Prob got set as Destination");
                Thread.Sleep(2000);
                GameState.MenuID = Menu.PlanetUiDescOrigo;
                return -1;
        }
    }

    private FuelType ChooseFuel() {
        // Replace this with a cool fuel selection Logic
        return FuelType.CrudeFuel; // Default for now
    }

    private void BurnFuel(FuelType FuelToUse) {
        if (FuelToUse == FuelType.NULL) {
            ErrorHappened = true;
            AnsiConsole.WriteLine("An ERROR Occured at Preps, UNKNOWN Fuel Type");
            Thread.Sleep(2000);
            GameState.MenuID = Menu.PlanetUiDescOrigo;
            return;
        }

        if (FuelToUse == FuelType.CrudeFuel) {
            if (PlayerCrudeFuel.Percentage <= 0.0d) GameEnd();
            PlayerCrudeFuel.Percentage -= 6.0d;
        }
        else if (FuelToUse == FuelType.StandardFuel) {
            if (PlayerStandardFuel.Percentage <= 0.0d) GameEnd();
            PlayerStandardFuel.Percentage -= 3.0d;
        }
        else if (FuelToUse == FuelType.RefinedFuel) {
            if (PlayerRefinedFuel.Percentage <= 0.0d) GameEnd();
            PlayerRefinedFuel.Percentage -= 0.5d;
        }
    }

    private void GameEnd() { // private for now, will be moved Later
                             // System.IO.File.Delete("Save.json");
        GameState.Stop = true;
    }

    private void ReturnPlayerToCurrPlanetUI() {
        GameState.MenuID = Menu.PlanetTravelChoice;

        if (GameState.PlanetOn == Planet.Origo) GameState.MenuID = Menu.PlanetUiDescOrigo;
        if (GameState.PlanetOn == Planet.Sterelis) GameState.MenuID = Menu.PlanetUiDescSterelis;
        if (GameState.PlanetOn == Planet.Primaris) GameState.MenuID = Menu.PlanetUiDescPrimaris;
    }

    private void InitTravel(Planet ToGoTo) {
        ErrorHappened = false;

        GameState.IsTravelling = true;
        GameState.IsLanded = false;
        GameState.IsOrbiting = false;

        TravelPrepare(ToGoTo);
    }

    private void TravelPrepare(Planet PlanetTarget) {
        if (ErrorHappened) return;

        GameState.FuelInUse = ChooseFuel();

        PlanetGoal = PlanetTarget;
        ETAsec = GetETAinSec();
        FuelToUse = GameState.FuelInUse;

        TravelStartTime = DateTime.Now;
    }

    private void TravelLanded() {
        GameState.IsTravelling = false;
        GameState.IsLanded = true;
        GameState.IsOrbiting = false;

        GameState.PlanetOn = PlanetGoal;

        PlanetGoal = Planet.NULL;
        ETAsec = 0;
        FuelToUse = FuelType.NULL;
        // TravelStartTime = ; idk how to make this null or something

        ReturnPlayerToCurrPlanetUI();
    }
}
