using SadConsole;
using SadConsole.Configuration;

Settings.WindowTitle = "StellaForge";

Builder _Builder = new Builder();

_Builder
    .SetWindowSizeInCells(StellaForge.GlobalVariables.MainWindow_W_Cell, StellaForge.GlobalVariables.MainWindow_H_Cell)
    .ConfigureFonts(true)
    .UseDefaultConsole()
    .OnStart(Startup)
    .Run();

static void Startup(object? sender, GameHost host) {
    StellaForge.Main.Setup();


    var console = Game.Instance.StartingConsole;
    if (console != null) {
        console.Print(1, 1, "StellaForge with SadConsole");
        console.Print(1, 2, "Game initialized successfully!");
        console.Print(1, 3, "Press ESC to exit");
    }

    Game.Instance.FrameUpdate += GameLoop;
}

static void GameLoop(object? sender, GameHost host) {
    StellaForge.Main.Loop(host);
}
