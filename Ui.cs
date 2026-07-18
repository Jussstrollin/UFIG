using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadConsole.Configuration;
using SadRogue.Primitives;

namespace StellaForge;

public class UI {
    public static class UI_Manager {
        public static BaseRoot? CurrentView;
        private static UIMain MainPanel = new UIMain();

        public static void InitUI() {
            Game.Instance.Screen = MainPanel;
        }

        public static void ShowUI(BaseRoot ToShow) {
            CurrentView = ToShow;
            Game.Instance.Screen = ToShow;
        }
    }

    public class BaseRoot : ScreenSurface {
        protected double _updateTimer = 0;
        protected const double UpdateInterval = 0.5;

        public BaseRoot(int Width, int Height) : base(Width, Height) {
        }

        public override void Update(TimeSpan delta) {
            base.Update(delta);

            _updateTimer += delta.TotalSeconds;

            if (_updateTimer >= UpdateInterval) {
                _updateTimer = 0;
                UpdateSurfaces();
            }
        }

        public virtual void UpdateSurfaces() { }
    }

    public class UIMain : BaseRoot {
        public UIMain() : base(GlobalVariables.MainWindow_W_Cell, GlobalVariables.MainWindow_H_Cell) {
            AssembleResourcePanel();
            AssembleFactoryList();
            AssembleFactoryPanel();
            AssembleControlPanel();
        }


        const int Padding = 3;

        ControlsConsole? ResourcePanel;
        const int ResourcePanel_W = 30;
        const int ResourcePanel_H = 10;
        const int ResourcePanel_X = 1;
        const int ResourcePanel_Y = 1;

        ControlsConsole? FactoryList;
        List<Console>? FactoryCardList;
        Factories.GenericFactory? CurrentFactoryRaised;
        private event Action? NewFactoryRaised;
        const int FactoryList_W = 30;
        const int FactoryList_H = 13;
        const int FactoryList_X = 1;
        const int FactoryList_Y = 1 + ResourcePanel_H + Padding;
        private void SelectFactory(Factories.GenericFactory ToSel) {
            CurrentFactoryRaised = ToSel;
            NewFactoryRaised?.Invoke();
        }

        ControlsConsole? FactoryPanel;
        const int FactoryPanel_W = 52;
        const int FactoryPanel_H = 20;
        const int FactoryPanel_X = 2 + ResourcePanel_W + Padding;
        const int FactoryPanel_Y = 1;

        ControlsConsole? ControlPanel;
        const int ControlPanel_W = 52;
        const int ControlPanel_H = 4;
        const int ControlPanel_X = FactoryPanel_X;
        const int ControlPanel_Y = 1 + FactoryPanel_H + Padding;

        private void AssembleResourcePanel() {
            Logger.Log("[Main] : Loading Resource Panel.");
            ResourcePanel = new ControlsConsole(ResourcePanel_W, ResourcePanel_H);
            ResourcePanel.Position = new Point(ResourcePanel_X, ResourcePanel_Y);
            Border.CreateForSurface(ResourcePanel, "Resource Panel");

            ResourcePanel.Surface.Print(1, 1, "Essence : ", Color.AnsiCyan);
            ResourcePanel.Surface.Print(1, 3, "Alpha : ", Color.AnsiYellowBright);
            ResourcePanel.Surface.Print(1, 5, "Beta : ", Color.AnsiBlue);
            ResourcePanel.Surface.Print(1, 7, "Gamma : ", Color.AnsiGreen);

            Children.Add(ResourcePanel);
        }

        private void AssembleFactoryList() {
            Logger.Log("[Main] : Loading FactoryList.");

            FactoryList = new ControlsConsole(FactoryList_W, FactoryList_H);
            FactoryList.Position = new Point(FactoryList_X, FactoryList_Y);

            Border.CreateForSurface(FactoryList, "Factory List");

            FactoryCardList = new();

            if (GlobalState.OutpostPlayerOn != null) {
                GlobalState.OutpostPlayerOn.OnOutpostChange += UpdateFactoryList;
            }
            else {
                Logger.ErrorLog("AssembleFactoryList : Cannot Hook To Outpost, Outpost does not exist!");
            }

            Children.Add(FactoryList);
            UpdateFactoryList();
        }

        private void AssembleFactoryPanel() {
            Logger.Log("[Main] : Loading AssembleFactoryList.");

            FactoryPanel = new ControlsConsole(FactoryPanel_W, FactoryPanel_H);
            FactoryPanel.Position = new Point(FactoryPanel_X, FactoryPanel_Y);
            Border.CreateForSurface(FactoryPanel, "Factory");

            NewFactoryRaised += UpdateFactoryPanel;

            if (GlobalState.OutpostPlayerOn != null) {
                GlobalState.OutpostPlayerOn.OnOutpostChange += UpdateFactoryPanel;
            }
            else {
                Logger.ErrorLog("AssembleFactoryPanel : Cannot Hook To Outpost, Outpost does not exist!");
            }

            Children.Add(FactoryPanel);
            UpdateFactoryPanel();
        }

        private void AssembleControlPanel() {
            Logger.Log("[Main] : Loading ControlPanel..");
            ControlPanel = new ControlsConsole(ControlPanel_W, ControlPanel_H);
            ControlPanel.Position = new Point(ControlPanel_X, ControlPanel_Y);
            Border.CreateForSurface(ControlPanel, "Controls");

            Children.Add(ControlPanel);
        }

        private void UpdateResourcePanel() {
            if (ResourcePanel != null && GlobalState.OutpostPlayerOn != null) {
                ResourcePanel.Surface.Print(15, 1, $"{GlobalState.OutpostPlayerOn.FactoryStorage.Resources[Enums.ResourceType.Essence]:F1}", Color.AnsiCyan);
                ResourcePanel.Surface.Print(15, 3, $"{GlobalState.OutpostPlayerOn.FactoryStorage.Resources[Enums.ResourceType.Alpha]:F1}", Color.AnsiYellowBright);
                ResourcePanel.Surface.Print(15, 5, $"{GlobalState.OutpostPlayerOn.FactoryStorage.Resources[Enums.ResourceType.Beta]:F1}", Color.AnsiBlue);
                ResourcePanel.Surface.Print(15, 7, $"{GlobalState.OutpostPlayerOn.FactoryStorage.Resources[Enums.ResourceType.Gamma]:F1}", Color.AnsiGreen);
                ResourcePanel.Surface.Print(1, 8, $"{GlobalState.OutpostPlayerOn.FactoryList.Count.ToString()}");
            }
        }

        private void UpdateFactoryList() {
            if (GlobalState.OutpostPlayerOn == null) { return; }
            if (FactoryList == null) { return; }
            if (FactoryCardList == null) { return; }
            foreach (var Card in FactoryCardList) {
                FactoryList.Children.Remove(Card);
            }
            FactoryCardList.Clear();

            int Offset_Y = 1;

            foreach (var Factory in GlobalState.OutpostPlayerOn.FactoryList) {
                Console Factorycard = new(FactoryList_W - Padding, 1);
                Factorycard.Position = new Point(1, Offset_Y);
                Offset_Y++;

                Color Default = Color.Black;
                Color Hover = Color.DimGray;
                Color Clicked = Color.AliceBlue;
                Color RaisedCol = Color.AnsiBlue;
                bool Raised = false;

                if (CurrentFactoryRaised == Factory) {
                    Factorycard.Surface.DefaultBackground = RaisedCol;
                    Raised = true;
                }
                else {
                    Raised = false;
                    Factorycard.Surface.DefaultBackground = Default;
                }

                // Hover effect
                Factorycard.MouseEnter += (sender, args) => {
                    if (!Raised) {
                        Factorycard.Surface.DefaultBackground = Hover;
                        Factorycard.Surface.Clear();
                        Factorycard.Surface.Print(0, 0, $"[{Factory.FactoryType.ToString()}] : {Factory.FactoryTier.ToString()}");
                    }
                };

                Factorycard.MouseExit += (sender, args) => {
                    if (Raised) { Factorycard.Surface.DefaultBackground = RaisedCol; }
                    else { Factorycard.Surface.DefaultBackground = Default; }

                    Factorycard.Surface.Clear();
                    Factorycard.Surface.Print(0, 0, $"[{Factory.FactoryType.ToString()}] : {Factory.FactoryTier.ToString()}");
                };

                // Click effect
                Factorycard.MouseButtonClicked += (sender, args) => {
                    SelectFactory(Factory);
                    Raised = false;
                    Factorycard.Surface.DefaultBackground = RaisedCol;
                    Factorycard.Surface.Clear();
                    Factorycard.Surface.Print(0, 0, $"[{Factory.FactoryType.ToString()}] : {Factory.FactoryTier.ToString()}");
                    UpdateFactoryList();
                };

                Factorycard.Surface.Print(0, 0, $"[{Factory.FactoryType.ToString()}] : {Factory.FactoryTier.ToString()}");

                FactoryCardList.Add(Factorycard);
                FactoryList.Children.Add(Factorycard);
            }
        }

        private void UpdateFactoryPanel() {
            if (FactoryPanel == null) {
                Logger.Log("[Main.FactoryPanel] : UpdateFactoryPanel Was Called whilst FactoryPanel is Null. FactoryPanel was Possibly nulled Unknowingly.");
                return;
            }

            FactoryPanel.Surface.Clear();
            Factories.GenericFactory? FactoryRaised = CurrentFactoryRaised;

            if (FactoryRaised == null) {
                FactoryPanel.Surface.Print(0, 0, " Please Select a Factory ");
                return;
            }

            Color FactoryTypeColor = FactoryRaised.FactoryType switch {
                Enums.FactoryTypes.AlphaFactory => Color.AnsiYellowBright,
                Enums.FactoryTypes.BetaFactory => Color.AnsiBlue,
                Enums.FactoryTypes.GammaFactory => Color.AnsiGreen,
                _ => Color.White
            };

            int TraitListOffset = 0;
            int Padding = 2;

            string Spacer = $" -------------------------------------------------- ";

            FactoryPanel.Surface.Print(1, 0, $"{FactoryRaised.FactoryType.ToString()}", FactoryTypeColor); // Will Be replaced when Factories can be names / have Generated Name

            FactoryPanel.Surface.Print(0, 2, Spacer);

            FactoryPanel.Surface.Print(1, 4, $"Type : {FactoryRaised.FactoryType.ToString()}");
            FactoryPanel.Surface.Print(1, 5, $"Tier : {FactoryRaised.FactoryTier.ToString()}");

            FactoryPanel.Surface.Print(1, 7, $"Traits : {FactoryRaised.TraitList.Count.ToString()}");
            FactoryPanel.Surface.Print(1, 8, "[");

            if (FactoryRaised.TraitList.Count != 0) {
                for (int i = 0; i < FactoryRaised.TraitList.Count; i++) {
                    string Content = $"{FactoryRaised.TraitList[i].TraitIdentifier.ToString()} [?]";
                    FactoryPanel.Surface.Print(1 + TraitListOffset + Padding, 8, Content);
                    TraitListOffset += Content.Length + 1;
                }
            }

            FactoryPanel.Surface.Print(2 + TraitListOffset + Padding, 8, "]");
        }

        public override void UpdateSurfaces() {
            UpdateResourcePanel();
        }
    }
}
