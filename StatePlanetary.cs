namespace UFIG;

using static Program;
using static PlanetUI;

using Spectre.Console;

public class StatePlanetary : StateInterface {
    public void GoingIn() {
        GameState.MenuID = Menu.PlanetUiSpace;
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
        if (Key == '1') GameState.MenuID = Menu.PlanetUiOrigo;
        if (Key == '2') GameState.MenuID = Menu.PlanetUiSterelis;
        if (Key == '3') GameState.MenuID = Menu.PlanetUiPrimaris;
    }

    public void Update() {
        return;
    }
}
