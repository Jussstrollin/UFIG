using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StellaForge;

// UI layout is mostly made my AI, cuz im way too lazy to do it, but the generics and Architectural decisions is still mine.

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
    private ResourcePanel _resourcePanel;
    private FactoryPanelRoot _factoryPanelRoot;
    private List<FactoryCard> _cardList;
    private GenericScrollableRoot _scrollableRoot;
    private Factories.GenericFactory _selectedFactory;

    // Color Scheme - Dark Sci-Fi Theme
    private static readonly Color Background = new Color(30, 30, 35);
    private static readonly Color PanelBg = new Color(45, 45, 50);
    private static readonly Color PanelBgAlt = new Color(55, 55, 60);
    private static readonly Color BorderColor = new Color(100, 100, 110);
    private static readonly Color TextColor = Color.White;
    private static readonly Color AccentColor = new Color(100, 180, 220);
    private static readonly Color CardBase = new Color(60, 60, 70);
    private static readonly Color CardHover = new Color(80, 80, 90);
    private static readonly Color CardClick = new Color(100, 100, 115);

    private const int ResourcePanelW = 220;
    private const int ResourcePanelH = 120;
    private const int CardW = 250;
    private const int CardH = 45;
    private const int CardSpacing = 12;
    private const int Margin = 25;
    private const int BorderThickness = 2;
    private const int BorderSpacing = 6;
    private const int CardListH = 300;

    private int _cardListX;
    private int _cardListY;
    private int _factoryPanelX = 290;
    private int _factoryPanelY = 25;
    private int _factoryPanelW = 500;
    private int _factoryPanelH = 435;

    private int _rebuildCounter = 0;
    private const int RebuildInterval = 30;

    public MAIN(SpriteBatch SB, SpriteFont SF, Texture2D P) : base(SB, SF, P) {
        _cardListX = Margin;
        _cardListY = ResourcePanelH + Margin + Margin;

        _resourcePanel = new ResourcePanel(this, PanelBg, BorderColor, Margin, Margin, ResourcePanelW, ResourcePanelH);
        _scrollableRoot = new GenericScrollableRoot(SB, SF, P, _cardListX, _cardListY, CardW, CardListH, this, BorderColor, BorderSpacing);
        _factoryPanelRoot = new FactoryPanelRoot(this, _factoryPanelX, _factoryPanelY, _factoryPanelW, _factoryPanelH);
        _cardList = new List<FactoryCard>();

        BuildCards();
        GlobalState.OutpostPlayerOn.OnOutpostChange += BuildCards;
        UI.OnDrawCall += UpdatePeriodically;
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
    private FactoryPanelBasicInfo _basicInfoPanel;
    private FactoryPanelNumericInfo _numericInfoPanel;
    private Factories.GenericFactory _factory;

    // Color scheme matching MAIN
    private static readonly Color PanelBg = new Color(45, 45, 50);
    private static readonly Color PanelBgAlt = new Color(55, 55, 60);
    private static readonly Color BorderColor = new Color(100, 100, 110);
    private static readonly Color TextColor = Color.White;
    private static readonly Color AccentColor = new Color(100, 180, 220);

    private const int Margin = 15;
    private const int BorderThickness = 2;

    public FactoryPanelRoot(GenericRoot root, int x, int y, int width, int height)
        : base(root, PanelBg, PanelBg, PanelBg, x, y, width, height) {

        int basicInfoH = height / 2 - Margin;
        int numericInfoY = y + basicInfoH + Margin;
        int numericInfoH = height - basicInfoH - Margin - Margin;

        _basicInfoPanel = new FactoryPanelBasicInfo(root, PanelBgAlt, BorderColor, x + Margin, y + Margin, width - Margin * 2, basicInfoH);
        _numericInfoPanel = new FactoryPanelNumericInfo(root, PanelBgAlt, BorderColor, x + Margin, numericInfoY, width - Margin * 2, numericInfoH);
    }

    public void SetFactory(Factories.GenericFactory factory) {
        _factory = factory;
        _basicInfoPanel.SetFactory(factory);
        _numericInfoPanel.SetFactory(factory);
    }

    public override void Draw() {
        base.Draw();

        // Draw border
        var box = new Rectangle(Panelx, Panely, PanelWidth, PanelHeight);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely, PanelWidth, BorderThickness), BorderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely + PanelHeight - BorderThickness, PanelWidth, BorderThickness), BorderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx, Panely, BorderThickness, PanelHeight), BorderColor);
        RootAttachedTo._spriteBatch.Draw(RootAttachedTo._pixel, new Rectangle(Panelx + PanelWidth - BorderThickness, Panely, BorderThickness, PanelHeight), BorderColor);

        // Draw child panels
        _basicInfoPanel.Draw();
        _numericInfoPanel.Draw();
    }
}

public class FactoryPanelBasicInfo : GenericDrawable {
    private Factories.GenericFactory _factory;
    private Color _borderColor;
    private const int BorderThickness = 2;

    public FactoryPanelBasicInfo(GenericRoot root, Color bgColor, Color borderColor, int x, int y, int width, int height)
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
        float currentY = Panely + 15;
        float lineHeight = 22;

        // Type and Tier
        var typeStr = GetFactoryTypeString(_factory.FactoryType);
        var tierStr = GetFactoryTierString(_factory.FactoryTier);

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Type:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, typeStr, new Vector2(Panelx + 15, currentY + lineHeight), accentColor);
        currentY += lineHeight * 2 + 10;

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Tier:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, tierStr, new Vector2(Panelx + 15, currentY + lineHeight), accentColor);
        currentY += lineHeight * 2 + 15;

        // Traits
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"Traits ({_factory.TraitList.Count}):", new Vector2(Panelx + 15, currentY), labelColor);
        currentY += lineHeight;

        if (_factory.TraitList.Count > 0) {
            foreach (var trait in _factory.TraitList) {
                var traitStr = GetTraitString(trait.TraitIdentifier);
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"- {traitStr}", new Vector2(Panelx + 25, currentY), textColor);
                currentY += lineHeight;
            }
        }
        else {
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "None", new Vector2(Panelx + 25, currentY), new Color(150, 150, 150));
        }
    }

    private string GetFactoryTypeString(Enums.FactoryTypes type) {
        return type switch {
            Enums.FactoryTypes.AlphaFactory => "Alpha Factory",
            Enums.FactoryTypes.BetaFactory => "Beta Factory",
            Enums.FactoryTypes.GammaFactory => "Gamma Factory",
            _ => "Unknown"
        };
    }

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

    private string GetTraitString(Enums.FactoryTrait trait) {
        return trait switch {
            Enums.FactoryTrait.PassedQualityAssurance => "Passed QA",
            Enums.FactoryTrait.ThoughtfulMakers => "Thoughtful Makers",
            Enums.FactoryTrait.BrokenOutputHatch => "Broken Hatch",
            Enums.FactoryTrait.Unstable => "Unstable",
            Enums.FactoryTrait.DangerousConstruction => "Dangerous Construction",
            Enums.FactoryTrait.Control => "Control",
            Enums.FactoryTrait.EnlighteningAura => "Enlightening Aura",
            _ => "Unknown"
        };
    }
}

public class FactoryPanelNumericInfo : GenericDrawable {
    private Factories.GenericFactory _factory;
    private Color _borderColor;
    private const int BorderThickness = 2;

    public FactoryPanelNumericInfo(GenericRoot root, Color bgColor, Color borderColor, int x, int y, int width, int height)
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
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "No factory selected", new Vector2(Panelx + 15, Panely + 15), new Color(150, 150, 150));
            return;
        }

        var textColor = Color.White;
        var labelColor = new Color(180, 180, 190);
        var valueColor = new Color(100, 180, 220);
        float currentY = Panely + 15;
        float lineHeight = 20;

        // Multipliers
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Input Mult:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{_factory.InputMult:F2}x", new Vector2(Panelx + 120, currentY), valueColor);
        currentY += lineHeight;

        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Output Mult:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{_factory.OutputMult:F2}x", new Vector2(Panelx + 120, currentY), valueColor);
        currentY += lineHeight + 10;

        // Tick Progress
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Tick Progress:", new Vector2(Panelx + 15, currentY), labelColor);
        RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{_factory.CurrTick}/{_factory.TickNeededToProd}", new Vector2(Panelx + 120, currentY), valueColor);
        currentY += lineHeight + 15;

        // Inputs
        if (_factory.Inputs.Count > 0) {
            RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, "Inputs:", new Vector2(Panelx + 15, currentY), labelColor);
            currentY += lineHeight;

            foreach (var input in _factory.Inputs) {
                var value = input.Value * _factory.InputMult;
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{input.Key}:", new Vector2(Panelx + 25, currentY), labelColor);
                RootAttachedTo._spriteBatch.DrawString(RootAttachedTo._font, $"{value:F2}", new Vector2(Panelx + 120, currentY), valueColor);
                currentY += lineHeight;
            }
            currentY += 10;
        }

        // Outputs
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
