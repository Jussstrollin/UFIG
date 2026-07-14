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
        const int FactoryList_W = 30;
        const int FactoryList_H = 13;
        const int FactoryList_X = 1;
        const int FactoryList_Y = 1 + ResourcePanel_H + Padding;
        private void SelectFactory(Factories.GenericFactory ToSel) {
            CurrentFactoryRaised = ToSel;
        }

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

            // if (GlobalState.OutpostPlayerOn != null) {
            //     GlobalState.OutpostPlayerOn.OnOutpostChange += UpdateResourcePanel;
            // }

            Children.Add(FactoryList);
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
                Factorycard.MouseButtonClicked += (sender, args) => {
                    SelectFactory(Factory);
                };

                Factorycard.Surface.Print(0, 0, $"[{Factory.FactoryType.ToString()}] : {Factory.FactoryTier.ToString()}");

                FactoryCardList.Add(Factorycard);
                FactoryList.Children.Add(Factorycard);
            }
        }

        public override void UpdateSurfaces() {
            UpdateResourcePanel();
            UpdateFactoryList();
        }
    }
}
