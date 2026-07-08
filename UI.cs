using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StellaForge;

// UI layout is mostly made my AI, cuz im way too lazy to do it, but the generics and Architectural decisions is still mine.
// again, the layout making itself is from AI, as for now i cannot be bothered to make a UI everysingle time i add soemthig new.

public enum Menu { MAIN }

public static class UI {
    public static MouseState PrevMouseState;
    private static MouseState CurrMouseState;

    private static SpriteBatch _spriteBatch;
    private static SpriteFont _font;
    private static Texture2D _pixel;

    public static Menu Rootmenu = Menu.MAIN;

    public static event Action? PollUiResponses; // dawg wtf is this name
    public static event Action? OnDrawCall;
    public static event Action? OnMenuChange;

    public static readonly RasterizerState ScissorState = new RasterizerState {
        ScissorTestEnable = true
    };

    public static void Init(SpriteBatch SB, SpriteFont SF, Texture2D P) {
        _spriteBatch = SB;
        _font = SF;
        _pixel = P;
        OnMenuChange += UI_Manager;
        OnMenuChange.Invoke();
    }

    public static void UI_Manager() {
        OnDrawCall = null;
        if (Rootmenu == Menu.MAIN) {
            new MAIN(_spriteBatch, _font, _pixel);
        }
    }

    public static void Draw() {
        PrevMouseState = CurrMouseState;
        CurrMouseState = Mouse.GetState();

        PollUiResponses?.Invoke();

        _spriteBatch.Begin();
        OnDrawCall?.Invoke();

        //DEBUG - remove when done
        _spriteBatch.DrawString(_font, $"X:{CurrMouseState.X} Y:{CurrMouseState.Y}", new Vector2(10, 10), Color.Red);

        _spriteBatch.End();

    }

    public static void InjectButton(GenericDrawable ToInjectTo, Action Callback) {
        ToInjectTo.OnClick += Callback;
        ToInjectTo.SpecialAction += PollClick;
        void PollClick() {
            MouseState Curr = Mouse.GetState();
            MouseState PrevState = UI.PrevMouseState;

            bool JustReleased = (Curr.LeftButton == ButtonState.Released) && (PrevMouseState.LeftButton == ButtonState.Pressed);

            Rectangle Hitbox = new Rectangle(ToInjectTo.Panelx, ToInjectTo.Panely, ToInjectTo.PanelWidth, ToInjectTo.PanelHeight);

            if (JustReleased && Hitbox.Contains(Curr.Position)) {
                ToInjectTo.CurrCol = ToInjectTo.ClickedCol;
                ToInjectTo.RaiseClickedEvent();
            }
        }
    }

    public static void InjectHoverEffect(GenericDrawable ToInjectTo) {
        void Hover() {
            MouseState MS = Mouse.GetState();
            Rectangle Hitbox = new Rectangle(ToInjectTo.Panelx, ToInjectTo.Panely, ToInjectTo.PanelWidth, ToInjectTo.PanelHeight);

            if (Hitbox.Contains(MS.Position)) {
                ToInjectTo.CurrCol = ToInjectTo.HoverCol;
            }
            else {
                ToInjectTo.CurrCol = ToInjectTo.BaseCol;
            }
        }
        ToInjectTo.SpecialAction += Hover;
    }
}

// --- //

public class MAIN : GenericRoot {
    private ResourcePanel _resourcePanel; // Resource display panel (shows Essence, Alpha, Beta, Gamma amounts)
    private FactoryPanelRoot _factoryPanelRoot; // Container for factory info panel
    private List<FactoryCard> _cardList; // List of factory cards in the scrollable area
    private GenericScrollableRoot _scrollableRoot; // Scrollable container for factory cards
    private Factories.GenericFactory _selectedFactory; // Currently selected factory for detailed view

    // Factory creation buttons
    private GenericDrawable _alphaFactoryButton; // Button to create Alpha Factory (Prototype tier)
    private GenericDrawable _betaFactoryButton; // Button to create Beta Factory (Prototype+ tier)
    private GenericDrawable _gammaFactoryButton; // Button to create Gamma Factory (Prototype++ tier)

    // Color Scheme - Dark Sci-Fi Theme
    private static readonly Color Background = new Color(30, 30, 35); // Main background color
    private static readonly Color PanelBg = new Color(45, 45, 50); // Standard panel background
    private static readonly Color PanelBgAlt = new Color(55, 55, 60); // Alternate panel background
    private static readonly Color BorderColor = new Color(100, 100, 110); // Border color for panels
    private static readonly Color TextColor = Color.White; // Main text color
    private static readonly Color AccentColor = new Color(100, 180, 220); // Accent color for highlights
    private static readonly Color CardBase = new Color(60, 60, 70); // Base color for factory cards
    private static readonly Color CardHover = new Color(80, 80, 90); // Hover color for factory cards
    private static readonly Color CardClick = new Color(100, 100, 115); // Click color for factory cards
    private static readonly Color ButtonBase = new Color(70, 70, 80); // Base color for buttons
    private static readonly Color ButtonHover = new Color(90, 90, 100); // Hover color for buttons
    private static readonly Color ButtonClick = new Color(110, 110, 125); // Click color for buttons

    // Layout dimensions
    private const int ResourcePanelW = 220; // Width of resource panel
    private const int ResourcePanelH = 120; // Height of resource panel
    private const int CardW = 250; // Width of factory cards
    private const int CardH = 45; // Height of factory cards
    private const int CardSpacing = 12; // Vertical spacing between cards
    private const int Margin = 25; // Margin from screen edges
    private const int BorderThickness = 2; // Thickness of panel borders
    private const int BorderSpacing = 6; // Spacing between border and content
    private const int CardListH = 250; // Height of the factory card scrollable area

    // Position variables
    private int _cardListX; // X position of factory card list
    private int _cardListY; // Y position of factory card list
    private int _factoryPanelX = 290; // X position of factory info panel
    private int _factoryPanelY = 25; // Y position of factory info panel
    private int _factoryPanelW = 500; // Width of factory info panel
    private int _factoryPanelH = 435; // Height of factory info panel

    // Button layout
    private const int ButtonY = 428; // Y position for all factory creation buttons
    private const int ButtonSpacing = 10; // Horizontal spacing between buttons
    private const int ButtonHeight = 35; // Height of factory creation buttons

    private int _rebuildCounter = 0; // Counter for periodic UI rebuilds
    private const int RebuildInterval = 30; // Number of frames between UI rebuilds

    public MAIN(SpriteBatch SB, SpriteFont SF, Texture2D P) : base(SB, SF, P) {
        _cardListX = Margin;
        _cardListY = ResourcePanelH + Margin + Margin;

        _resourcePanel = new ResourcePanel(this, PanelBg, BorderColor, Margin, Margin, ResourcePanelW, ResourcePanelH);
        _scrollableRoot = new GenericScrollableRoot(SB, SF, P, _cardListX, _cardListY, CardW, CardListH, this, BorderColor, BorderSpacing);
        _factoryPanelRoot = new FactoryPanelRoot(this, _factoryPanelX, _factoryPanelY, _factoryPanelW, _factoryPanelH);
        _cardList = new List<FactoryCard>();

        // Create factory creation buttons
        CreateFactoryButtons();

        BuildCards();
        GlobalState.OutpostPlayerOn.OnOutpostChange += BuildCards;
        UI.OnDrawCall += UpdatePeriodically;
    }

    private void CreateFactoryButtons() {
        // NOTE: Helper function to measure text width
        int MeasureTextWidth(string text) {
            return (int)_font.MeasureString(text).X;
        }

        // Calculate button dimensions with shorter text
        int alphaBtnWidth = MeasureTextWidth("A") + 20; // Smaller padding for single letters
        int betaBtnWidth = MeasureTextWidth("B") + 20;
        int gammaBtnWidth = MeasureTextWidth("G") + 20;

        // Position buttons with spacing, ensuring they don't clip into factory panel
        int alphaBtnX = Margin;
        int betaBtnX = alphaBtnX + alphaBtnWidth + ButtonSpacing;
        int gammaBtnX = betaBtnX + betaBtnWidth + ButtonSpacing;

        // Create Alpha Factory button (Prototype tier)
        _alphaFactoryButton = new GenericDrawable(this, ButtonBase, ButtonHover, ButtonClick, alphaBtnX, ButtonY, alphaBtnWidth, ButtonHeight);
        UI.InjectHoverEffect(_alphaFactoryButton);
        UI.InjectButton(_alphaFactoryButton, () => CreateFactory(Enums.FactoryTypes.AlphaFactory, Enums.FactoryTier.Prototype));

        // Create Beta Factory button (Prototype+ tier)
        _betaFactoryButton = new GenericDrawable(this, ButtonBase, ButtonHover, ButtonClick, betaBtnX, ButtonY, betaBtnWidth, ButtonHeight);
        UI.InjectHoverEffect(_betaFactoryButton);
        UI.InjectButton(_betaFactoryButton, () => CreateFactory(Enums.FactoryTypes.BetaFactory, Enums.FactoryTier.PrototypePlus));

        // Create Gamma Factory button (Prototype++ tier)
        _gammaFactoryButton = new GenericDrawable(this, ButtonBase, ButtonHover, ButtonClick, gammaBtnX, ButtonY, gammaBtnWidth, ButtonHeight);
        UI.InjectHoverEffect(_gammaFactoryButton);
        UI.InjectButton(_gammaFactoryButton, () => CreateFactory(Enums.FactoryTypes.GammaFactory, Enums.FactoryTier.PrototypePlusPlus));
    }

    private void CreateFactory(Enums.FactoryTypes type, Enums.FactoryTier tier) {
        Factories.FactoryCreationRelated.MakeNewFactory(GlobalState.OutpostPlayerOn, type, GlobalState.OutpostPlayerOn.FactoryStorage, tier);
    }

    private void UpdatePeriodically() {
        _rebuildCounter++;
        if (_rebuildCounter >= RebuildInterval) {
            BuildCards();
            _rebuildCounter = 0;
        }
    }

    private void BuildCards() {
        _cardListY = ResourcePanelH + Margin + Margin + _scrollableRoot.Offset + BorderSpacing;

        foreach (var card in _cardList) {
            card.Unhook();
        }
        _cardList.Clear();

        int currentY = _cardListY;
        foreach (var factory in GlobalState.OutpostPlayerOn.FactoryList) {
            var card = CreateFactoryCard(factory, currentY);
            _cardList.Add(card);
            currentY += CardH + CardSpacing;
        }
    }

    private FactoryCard CreateFactoryCard(Factories.GenericFactory factory, int y) {
        var card = new FactoryCard(factory, _scrollableRoot, _cardListX + BorderSpacing, y, CardW - BorderSpacing * 2, CardH, CardBase, CardHover, CardClick);
        UI.InjectHoverEffect(card);
        UI.InjectButton(card, () => SelectFactory(factory));
        return card;
    }

    private void SelectFactory(Factories.GenericFactory factory) {
        _selectedFactory = factory;
        _factoryPanelRoot.SetFactory(factory);
    }

    public override void HookToDraw() {
        base.HookToDraw();
        RootOnDrawCall += DrawButtons;
    }

    private void DrawButtons() {
        // Draw factory creation buttons with single letter text
        DrawFactoryButton(_alphaFactoryButton, "A");
        DrawFactoryButton(_betaFactoryButton, "B");
        DrawFactoryButton(_gammaFactoryButton, "G");
    }

    private void DrawFactoryButton(GenericDrawable button, string text) {
        // Draw button background
        _spriteBatch.Draw(_pixel, new Rectangle(button.Panelx, button.Panely, button.PanelWidth, button.PanelHeight), button.CurrCol);

        // Draw button border
        _spriteBatch.Draw(_pixel, new Rectangle(button.Panelx, button.Panely, button.PanelWidth, BorderThickness), BorderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(button.Panelx, button.Panely + button.PanelHeight - BorderThickness, button.PanelWidth, BorderThickness), BorderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(button.Panelx, button.Panely, BorderThickness, button.PanelHeight), BorderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(button.Panelx + button.PanelWidth - BorderThickness, button.Panely, BorderThickness, button.PanelHeight), BorderColor);

        // Draw button text centered
        Vector2 textSize = _font.MeasureString(text);
        Vector2 textPosition = new Vector2(
            button.Panelx + (button.PanelWidth - textSize.X) / 2,
            button.Panely + (button.PanelHeight - textSize.Y) / 2
        );
        _spriteBatch.DrawString(_font, text, textPosition, TextColor);
    }
}


public class ResourcePanel : GenericDrawable {
    private Color _borderColor;
    private const int BorderThickness = 2;

    public ResourcePanel(GenericRoot root, Color bgColor, Color borderColor, int x, int y, int width, int height)
        : base(root, bgColor, bgColor, bgColor, x, y, width, height) {
        _borderColor = borderColor;
    }

    public override void Draw() {
        base.Draw();

        var box = new Rectangle(Panelx, Panely, PanelWidth, PanelHeight);

        // Draw background
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, box, CurrCol);

        // Draw border
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely, PanelWidth, BorderThickness), _borderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely + PanelHeight - BorderThickness, PanelWidth, BorderThickness), _borderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely, BorderThickness, PanelHeight), _borderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx + PanelWidth - BorderThickness, Panely, BorderThickness, PanelHeight), _borderColor);

        // Draw resource labels and values
        var labelColor = new Color(180, 180, 190);
        var valueColor = Color.White;

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Essence:", new Vector2(Panelx + 15, Panely + 15), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Alpha:", new Vector2(Panelx + 15, Panely + 40), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Beta:", new Vector2(Panelx + 15, Panely + 65), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Gamma:", new Vector2(Panelx + 15, Panely + 90), labelColor);

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font,
                $"{GlobalState.OutpostPlayerOn.FactoryStorage.Resources[Enums.ResourceType.Essence]:F2}",
                new Vector2(Panelx + 110, Panely + 15),
                valueColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font,
                $"{GlobalState.OutpostPlayerOn.FactoryStorage.Resources[Enums.ResourceType.Alpha]:F2}",
                new Vector2(Panelx + 110, Panely + 40),
                valueColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font,
                $"{GlobalState.OutpostPlayerOn.FactoryStorage.Resources[Enums.ResourceType.Beta]:F2}",
                new Vector2(Panelx + 110, Panely + 65),
                valueColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font,
                $"{GlobalState.OutpostPlayerOn.FactoryStorage.Resources[Enums.ResourceType.Gamma]:F2}",
                new Vector2(Panelx + 110, Panely + 90),
                valueColor);
    }
}

public class FactoryPanelRoot : GenericDrawable {
    private GenericScrollableRoot _scrollableContent; // Scrollable container for factory info
    private FactoryInfoPanel _infoPanel; // Combined factory information panel
    private Factories.GenericFactory _factory; // Currently displayed factory

    // Color scheme matching MAIN
    private static readonly Color PanelBg = new Color(45, 45, 50); // Panel background color
    private static readonly Color PanelBgAlt = new Color(55, 55, 60); // Alternate panel background
    private static readonly Color BorderColor = new Color(100, 100, 110); // Border color
    private static readonly Color TextColor = Color.White; // Text color
    private static readonly Color AccentColor = new Color(100, 180, 220); // Accent color for highlights

    private const int Margin = 20; // Margin within the panel
    private const int BorderThickness = 2; // Border thickness
    private const int ContentSpacing = 320; // Spacing between scrollable content and border

    public FactoryPanelRoot(GenericRoot root, int x, int y, int width, int height)
        : base(root, PanelBg, PanelBg, PanelBg, x, y, width, height) {

        // Create scrollable root for factory info content
        _scrollableContent = new GenericScrollableRoot(root._spriteBatch, root._font, root._pixel,
            Panelx + Margin, Panely + Margin,
            width - Margin * 2, height - Margin * 2,
            root, BorderColor, ContentSpacing);

        // Create combined info panel as child of scrollable root
        _infoPanel = new FactoryInfoPanel(_scrollableContent, PanelBgAlt, BorderColor,
            ContentSpacing, 0, _scrollableContent.Rw - Margin, height + 250);
    }

    public void SetFactory(Factories.GenericFactory factory) {
        _factory = factory;
        _infoPanel.SetFactory(factory);
    }

    public override void Draw() {
        base.Draw();

        // Draw border
        var box = new Rectangle(Panelx, Panely, PanelWidth, PanelHeight);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely, PanelWidth, BorderThickness), BorderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely + PanelHeight - BorderThickness, PanelWidth, BorderThickness), BorderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely, BorderThickness, PanelHeight), BorderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx + PanelWidth - BorderThickness, Panely, BorderThickness, PanelHeight), BorderColor);

        // The scrollable content draws itself through its HookToDraw mechanism
        // No need to manually call Draw() here
    }
}

public class FactoryInfoPanel : GenericDrawable {
    private Factories.GenericFactory _factory; // Currently displayed factory
    private Color _borderColor; // Border color for the panel
    private const int BorderThickness = 2; // Border thickness
    private const int SectionSpacing = 20; // Vertical spacing between major sections

    public FactoryInfoPanel(GenericRoot root, Color bgColor, Color borderColor, int x, int y, int width, int height)
        : base(root, bgColor, bgColor, bgColor, x, y, width, height) {
        _borderColor = borderColor;
    }

    public void SetFactory(Factories.GenericFactory factory) {
        _factory = factory;
    }

    public override void Draw() {
        base.Draw();

        // Draw border
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely, PanelWidth, BorderThickness), _borderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely + PanelHeight - BorderThickness, PanelWidth, BorderThickness), _borderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely, BorderThickness, PanelHeight), _borderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx + PanelWidth - BorderThickness, Panely, BorderThickness, PanelHeight), _borderColor);

        if (_factory == null) {
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Select a factory", new Vector2(Panelx + 15, Panely + 15), Color.White);
            return;
        }

        var textColor = Color.White;
        var accentColor = new Color(100, 180, 220);
        var labelColor = new Color(180, 180, 190);
        var valueColor = new Color(100, 180, 220);
        float currentY = Panely + 15;
        float lineHeight = 20;

        // Type and Tier section
        var typeStr = GetFactoryTypeString(_factory.FactoryType);
        var tierStr = GetFactoryTierString(_factory.FactoryTier);

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Type:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, typeStr, new Vector2(Panelx + 15, currentY + lineHeight), accentColor);
        currentY += lineHeight * 2 + 5;

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Tier:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, tierStr, new Vector2(Panelx + 15, currentY + lineHeight), accentColor);
        currentY += lineHeight * 2 + SectionSpacing;

        // Traits section
        int totalTraits = _factory.TraitList.Count + _factory.TierGivenTraitList.Count;
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"Traits ({totalTraits}):", new Vector2(Panelx + 15, currentY), labelColor);
        currentY += lineHeight;

        // Draw tier-given traits first
        if (_factory.TierGivenTraitList.Count > 0) {
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Tier Traits:", new Vector2(Panelx + 20, currentY), new Color(150, 150, 160));
            currentY += lineHeight;
            foreach (var trait in _factory.TierGivenTraitList) {
                var traitStr = GetTraitString(trait.TraitIdentifier);
                // Draw trait name
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"- {traitStr}", new Vector2(Panelx + 30, currentY), textColor);
                currentY += lineHeight;

                // Draw description on next line with > prefix if available
                if (!string.IsNullOrEmpty(trait.Description)) {
                    var wrappedDesc = WrapText(trait.Description, PanelWidth - 70);
                    foreach (var line in wrappedDesc) {
                        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"> {line}", new Vector2(Panelx + 40, currentY), new Color(160, 160, 170));
                        currentY += lineHeight;
                    }
                }
            }
            currentY += 5;
        }

        // Draw random traits
        if (_factory.TraitList.Count > 0) {
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Random Traits:", new Vector2(Panelx + 20, currentY), new Color(150, 150, 160));
            currentY += lineHeight;
            foreach (var trait in _factory.TraitList) {
                var traitStr = GetTraitString(trait.TraitIdentifier);
                // Draw trait name
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"- {traitStr}", new Vector2(Panelx + 30, currentY), textColor);
                currentY += lineHeight;

                // Draw description on next line with > prefix if available
                if (!string.IsNullOrEmpty(trait.Description)) {
                    var wrappedDesc = WrapText(trait.Description, PanelWidth - 70);
                    foreach (var line in wrappedDesc) {
                        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"> {line}", new Vector2(Panelx + 40, currentY), new Color(160, 160, 170));
                        currentY += lineHeight;
                    }
                }
            }
        }

        if (totalTraits == 0) {
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "None", new Vector2(Panelx + 25, currentY), new Color(150, 150, 150));
            currentY += lineHeight;
        }

        currentY += SectionSpacing;

        // Multipliers section
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Input Mult:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{_factory.InputMult:F2}x", new Vector2(Panelx + 120, currentY), valueColor);
        currentY += lineHeight;

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Output Mult:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{_factory.OutputMult:F2}x", new Vector2(Panelx + 120, currentY), valueColor);
        currentY += lineHeight + SectionSpacing;

        // Tick Progress section
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Tick Progress:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{_factory.CurrTick}/{_factory.TickNeededToProd}", new Vector2(Panelx + 120, currentY), valueColor);
        currentY += lineHeight + SectionSpacing;

        // Inputs section
        if (_factory.Inputs.Count > 0) {
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Inputs:", new Vector2(Panelx + 15, currentY), labelColor);
            currentY += lineHeight;

            foreach (var input in _factory.Inputs) {
                var value = input.Value * _factory.InputMult;
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{input.Key}:", new Vector2(Panelx + 25, currentY), labelColor);
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{value:F2}", new Vector2(Panelx + 120, currentY), valueColor);
                currentY += lineHeight;
            }
            currentY += SectionSpacing;
        }

        // Outputs section
        if (_factory.Outputs.Count > 0) {
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Outputs:", new Vector2(Panelx + 15, currentY), labelColor);
            currentY += lineHeight;

            foreach (var output in _factory.Outputs) {
                var value = output.Value * _factory.OutputMult;
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{output.Key}:", new Vector2(Panelx + 25, currentY), labelColor);
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{value:F2}", new Vector2(Panelx + 120, currentY), valueColor);
                currentY += lineHeight;
            }
        }
    }

    // NOTE: Helper function to convert factory type enum to display string
    private string GetFactoryTypeString(Enums.FactoryTypes type) {
        return type switch {
            Enums.FactoryTypes.AlphaFactory => "Alpha Factory",
            Enums.FactoryTypes.BetaFactory => "Beta Factory",
            Enums.FactoryTypes.GammaFactory => "Gamma Factory",
            _ => "Unknown"
        };
    }

    // NOTE: Helper function to convert factory tier enum to display string
    private string GetFactoryTierString(Enums.FactoryTier tier) {
        return tier switch {
            Enums.FactoryTier.Prototype => "Prototype",
            Enums.FactoryTier.PrototypePlus => "Prototype+",
            Enums.FactoryTier.PrototypePlusPlus => "Prototype++",
            Enums.FactoryTier.Refined => "Refined",
            Enums.FactoryTier.Advanced => "Advanced",
            Enums.FactoryTier.Experimental => "Experimental",
            Enums.FactoryTier.Apex => "Apex",
            _ => "Unknown"
        };
    }

    // NOTE: Helper function to convert factory trait enum to display string
    private string GetTraitString(Enums.FactoryTrait trait) {
        return trait switch {
            Enums.FactoryTrait.PassedQualityAssurance => "Passed QA",
            Enums.FactoryTrait.ThoughtfulMakers => "Thoughtful Makers",
            Enums.FactoryTrait.BrokenOutputHatch => "Broken Hatch",
            Enums.FactoryTrait.Unstable => "Unstable",
            Enums.FactoryTrait.DangerousConstruction => "Dangerous Construction",
            Enums.FactoryTrait.Control => "Control",
            Enums.FactoryTrait.EnlighteningAura => "Enlightening Aura",
            Enums.FactoryTrait.TimeDialation => "TimeDialation",
            _ => "Unknown"
        };
    }

    // NOTE: Helper function to wrap text to prevent horizontal overflow
    private List<string> WrapText(string text, float maxWidth) {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words) {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var textSize = RootAttachedTo._font.MeasureString(testLine);

            if (textSize.X > maxWidth) {
                if (!string.IsNullOrEmpty(currentLine)) {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else {
                    // Single word is too long, force break it
                    currentLine = word;
                    lines.Add(currentLine);
                    currentLine = "";
                }
            }
            else {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine)) {
            lines.Add(currentLine);
        }

        return lines;
    }
}

public class FactoryCard : GenericDrawable {
    private Factories.GenericFactory Factory;

    private Enums.FactoryTypes Type { get; set; }
    private Enums.FactoryTier Tier { get; set; }

    private string StringedType { get; set; } = "null";
    private string StringedTier { get; set; } = "null";
    private string BuiltString { get; set; } = "null";

    private Rectangle Box;

    public FactoryCard(Factories.GenericFactory F, GenericRoot Root, int x, int y, int W, int H, Color BC, Color HC, Color CC) : base(Root, BC, HC, CC, x, y, W, H) {
        Factory = F;
        BuildString();
    }

    private void DetermineType() {
        switch (Factory.FactoryType) {
            case Enums.FactoryTypes.AlphaFactory:
                StringedType = "Alpha Factory";
                break;
            case Enums.FactoryTypes.BetaFactory:
                StringedType = "Beta Factory";
                break;
            case Enums.FactoryTypes.GammaFactory:
                StringedType = "Gamma Factory";
                break;
            default:
                StringedType = "Null";
                break;
        }
    }

    private void DetermineTier() {
        switch (Factory.FactoryTier) {
            case Enums.FactoryTier.Prototype:
                StringedTier = "Prototype";
                break;
            case Enums.FactoryTier.PrototypePlus:
                StringedTier = "Prototype+";
                break;
            case Enums.FactoryTier.PrototypePlusPlus:
                StringedTier = "Prototype++";
                break;
            default:
                StringedTier = "Null";
                break;
        }
    }

    private void BuildString() {
        DetermineType();
        DetermineTier();
        BuiltString = $"{StringedType} | {StringedTier}";
    }

    public override void Draw() {
        base.Draw();
        BuildString();

        Box = new Rectangle(Panelx, Panely, PanelWidth, PanelHeight);

        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, Box, CurrCol);

        var textColor = Color.White;
        var accentColor = new Color(100, 180, 220);
        var labelColor = new Color(180, 180, 190);

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, StringedType, new Vector2(Panelx + 12, Panely + 12), accentColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, StringedTier, new Vector2(Panelx + 12, Panely + 28), labelColor);
    }
}

// --- //

public class GenericRoot {
    public SpriteBatch _spriteBatch;
    public SpriteFont _font;
    public Texture2D _pixel;

    public List<GenericDrawable> UiComponents;

    public event Action? RootOnDrawCall;

    public GenericRoot(SpriteBatch SB, SpriteFont SF, Texture2D P) {
        _spriteBatch = SB;
        _font = SF;
        _pixel = P;
        UiComponents = new List<GenericDrawable>();
        HookToDraw();
    }


    public virtual void HookToDraw() {
        UI.OnDrawCall += () => RootOnDrawCall?.Invoke();
    }

    protected void InvokeDraw() {
        RootOnDrawCall?.Invoke();
    }

    public virtual void Init() { }
}

public class GenericScrollableRoot : GenericRoot {
    private Color _borderColor;
    private const int BorderThickness = 2;
    private int _borderSpacing;

    public GenericScrollableRoot(SpriteBatch SB, SpriteFont SF, Texture2D P, int Rootx, int Rooty, int RootW, int RootH, GenericRoot MainRoot, Color borderColor, int borderSpacing) : base(SB, SF, P) {
        Rx = Rootx;
        Ry = Rooty;
        Rw = RootW;
        Rh = RootH;
        RootAttachedTo = MainRoot;
        Border = new(Rootx, Rooty, RootW, RootH);
        _borderColor = borderColor;
        _borderSpacing = borderSpacing;
        UI.PollUiResponses += PollScroll;
    }

    public int Rx { get; private set; }
    public int Ry { get; private set; }
    public int Rw { get; private set; }
    public int Rh { get; private set; }
    public int Offset { get; private set; }

    public Rectangle Border;

    public GenericRoot RootAttachedTo { get; private set; }

    public int ScrollDelta;

    public override void HookToDraw() {
        UI.OnDrawCall += ScissorDraw;
    }

    private void PollScroll() {
        MouseState PrevMouseState = UI.PrevMouseState;
        MouseState CurrMouseState = Mouse.GetState();

        Rectangle Hitbox = new Rectangle(Rx, Ry, Rw, Rh);

        ScrollDelta = CurrMouseState.ScrollWheelValue - PrevMouseState.ScrollWheelValue;
        if (ScrollDelta != 0 && Hitbox.Contains(CurrMouseState.Position)) {
            int ShiftAmount = (ScrollDelta / 120) * 15;
            foreach (var Component in UiComponents) {
                Component.Panely += ShiftAmount;
            }
            Offset += ShiftAmount;
        }
    }

    private void ScissorDraw() {
        // Draw border with spacing
        _spriteBatch.Draw(_pixel, new Rectangle(Rx, Ry, Rw, BorderThickness), _borderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(Rx, Ry + Rh - BorderThickness, Rw, BorderThickness), _borderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(Rx, Ry, BorderThickness, Rh), _borderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(Rx + Rw - BorderThickness, Ry, BorderThickness, Rh), _borderColor);

        _spriteBatch.End();

        _spriteBatch.GraphicsDevice.ScissorRectangle = Border;

        _spriteBatch.Begin(rasterizerState: UI.ScissorState);

        this.InvokeDraw();

        _spriteBatch.End();

        _spriteBatch.Begin();
    }
}

public class GenericDrawable {
    protected GenericRoot RootAttachedTo;

    public int Panelx { get; set; }
    public int Panely { get; set; }
    public int PanelWidth { get; private set; }
    public int PanelHeight { get; private set; }
    public Color BaseCol { get; set; }
    public Color CurrCol { get; set; }
    public Color HoverCol { get; set; }
    public Color ClickedCol { get; set; }

    public event Action? OnClick;
    public event Action? SpecialAction;

    public GenericDrawable(GenericRoot RootIsOn, Color bgcol, Color HovCol, Color ClickCol, int x, int y, int width, int height) {
        RootAttachedTo = RootIsOn;
        Panelx = x;
        Panely = y;
        PanelWidth = width;
        PanelHeight = height;
        BaseCol = bgcol;
        HoverCol = HovCol;
        ClickedCol = ClickCol;
        CurrCol = BaseCol;
        Hook();
    }

    public void RaiseClickedEvent() {
        OnClick?.Invoke();
    }

    private void Hook() {
        if (RootAttachedTo != null) {
            RootAttachedTo.RootOnDrawCall += Draw;
            RootAttachedTo.UiComponents.Add(this);
        }
    }

    public virtual void Unhook() {
        if (RootAttachedTo != null) {
            RootAttachedTo.RootOnDrawCall -= Draw;
            RootAttachedTo.UiComponents.Remove(this);
            SpecialAction = null;
        }
    }

    public virtual void Draw() {
        SpecialAction?.Invoke();
    }
}
