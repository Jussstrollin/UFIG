namespace UFIG;

using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadConsole.Configuration;
using SadRogue.Primitives;

using static Program;

public class GlobalUI {
    public ControlsConsole ControlPanelHost;
    public Button GameButton;
    public Button ShopButton;
    public Button NavigationButton;

    const int ControlPanelWidth = 105; // 70 * 1.5
    const int ControlPanelHeight = 5;

    public GlobalUI() {
        ControlPanelHost = new ControlsConsole(ControlPanelWidth, ControlPanelHeight);
        ControlPanelHost.Position = (6, 33);
        SadConsole.UI.Border.CreateForSurface(ControlPanelHost, "Control Panel");

        BuildControlButtons();
    }

    private void BuildControlButtons() {
        int ButtonWidth = 10;
        int ButtonHeight = 1;
        int Spacing = 4;
        int X_OnPanel = 1;
        int Y_OnPanel = 1;

        GameButton = new Button(ButtonWidth, ButtonHeight);
        GameButton.Position = new Point(X_OnPanel, Y_OnPanel);
        GameButton.Text = " Game [G] ";

        ShopButton = new Button(ButtonWidth, ButtonHeight);
        ShopButton.Position = new Point(X_OnPanel + (ButtonWidth + Spacing), Y_OnPanel);
        ShopButton.Text = " Shop [S] ";

        NavigationButton = new Button(ButtonWidth, ButtonHeight);
        NavigationButton.Position = new Point(X_OnPanel + (ButtonWidth + Spacing) * 2, Y_OnPanel);
        NavigationButton.Text = " Nav [N] ";

        ControlPanelHost.Controls.Add(GameButton);
        ControlPanelHost.Controls.Add(ShopButton);
        ControlPanelHost.Controls.Add(NavigationButton);
    }
}
