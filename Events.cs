namespace UFIG;

using static Program;

public static class EventSystem {
    public static int EventToShow = 10; // for panel control, 1x is reserved for Alpha. 2x for beta, 3x for Gamma, and 1-9 for essence, 50 Above is for random events
    public static int EssenceForcedEventDone = 0; // up to 9
    public static int AlphaForcedEventDone = 0;
    public static int BetaForcedEventDone = 0;
    public static int GammaForcedEventDone = 0;
    public static bool CanShowEvent = true; // whether x sec from last event has passed
    public static bool ForcedEventWantToShow = false; // so forced event always take precidence
    public static bool RandomEventCanShow = false;

    private enum Events {
        AlphaForcedEvent,
        BetaForcedEvent,
        GammaForcedEvent,

        // the rest is W.I.P
        AlphaEventPos,
        AlphaEventNeg,
        BetaEventPos,
        BetaEventNeg,
        GammaEventPos,
        GammaEventNeg,
    }

    private static void ApplyEffect(Events Do) {
        if (Do == Events.AlphaForcedEvent) {
            if (AlphaForcedEventDone == 0) {
                Structure.AlphaFactoryCost = Structure.AlphaFactoryBaseCost * 2; // 100%
            }
            else if (AlphaForcedEventDone == 1) {
                Structure.AlphaFactoryCost = (Structure.AlphaFactoryBaseCost * 2.0) * 1.80; // 80%
            }
            else if (AlphaForcedEventDone == 2) {
                Structure.AlphaFactoryCost = (Structure.AlphaFactoryBaseCost * 2.0) * 2.30; // 130%
            }
            else if (AlphaForcedEventDone == 3) {
                Structure.AlphaFactoryCost = (Structure.AlphaFactoryBaseCost * 2.0) * 1.50; // 50%
            }
        }
        else if (Do == Events.BetaForcedEvent) {
            if (BetaForcedEventDone == 0) {
                Structure.BetaFactoryCost = Structure.BetaFactoryBaseCost * 2.50; // 150%
            }
            else if (BetaForcedEventDone == 1) {
                Structure.BetaFactoryCost = (Structure.BetaFactoryBaseCost * 2.50) * 1.80; // 80%
            }
            else if (BetaForcedEventDone == 2) {
                Structure.BetaFactoryCost = (Structure.BetaFactoryBaseCost * 2.50) * 1.40; // 40%
            }
            else if (BetaForcedEventDone == 3) {
                Structure.BetaFactoryCost = (Structure.BetaFactoryBaseCost * 2.50) * 2.50; // 150%
            }
        }
        else if (Do == Events.GammaForcedEvent) {
            if (GammaForcedEventDone == 0) {
                Structure.GammaFactoryCost = Structure.GammaFactoryBaseCost * 2.30; // 130%
            }
            else if (GammaForcedEventDone == 1) {
                Structure.GammaFactoryCost = (Structure.GammaFactoryBaseCost * 2.30) * 1.90; // 90%
            }
            else if (GammaForcedEventDone == 2) {
                Structure.GammaFactoryCost = (Structure.GammaFactoryBaseCost * 2.30) * 1.60; // 60%
            }
            else if (GammaForcedEventDone == 3) {
                Structure.GammaFactoryCost = (Structure.GammaFactoryBaseCost * 2.30) * 2.0; // 100%
            }
        }
        else return;

        // Hardcoded but kinda better looking
    }

    private static int EventCooldown = 0;

    public static void RefreshEvent() {
        if (ForcedEventWantToShow == true && CanShowEvent == false) {
            EventCooldown++;
            // increment cooldown when
            if (EventCooldown >= 10) {
                ForcedEventWantToShow = false;
                EventCooldown = 0;
                CanShowEvent = true;
                RandomEventCanShow = true;
            }
        }
    }

    public static void ForcedAlphaEventHandler() {
        if (Structure.AlphaFactory >= 10 && AlphaForcedEventDone == 0 && CanShowEvent && ForcedEventWantToShow == false) { // checks Current factory amount, whether this has been done, and if it can show events, and other ForcedEvents doesnt wanna show
            EventToShow = 10; // what Panel to use
            ApplyEffect(Events.AlphaForcedEvent);
            AlphaForcedEventDone = 1; // adds so next AlphaForcedEvent can just skip this

            // after specifying what event to show, and updating progress, declare that forced event wants to show, and reset even timer to zero
            ForcedEventWantToShow = true;
            CanShowEvent = false; // stops other from showing events
            RandomEventCanShow = false;
            EventCooldown = 0; // set to 0, 10ticks before new event can show
            Console.Beep();
            return;
        }
        else if (Structure.AlphaFactory >= 30 && AlphaForcedEventDone == 1 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 11;
            ApplyEffect(Events.AlphaForcedEvent);
            AlphaForcedEventDone = 2;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
        else if (Structure.AlphaFactory >= 60 && AlphaForcedEventDone == 2 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 12;
            ApplyEffect(Events.AlphaForcedEvent);
            AlphaForcedEventDone = 3;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
        else if (Structure.AlphaFactory >= 200 && AlphaForcedEventDone == 3 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 13;
            ApplyEffect(Events.AlphaForcedEvent);
            AlphaForcedEventDone = 4;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
    }


    public static void ForcedBetaEventHandler() {
        if (Structure.BetaFactory >= 20 && BetaForcedEventDone == 0 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 20;
            ApplyEffect(Events.BetaForcedEvent);
            BetaForcedEventDone = 1;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
        else if (Structure.BetaFactory >= 50 && BetaForcedEventDone == 1 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 21;
            ApplyEffect(Events.BetaForcedEvent);
            BetaForcedEventDone = 2;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
        else if (Structure.BetaFactory >= 80 && BetaForcedEventDone == 2 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 22;
            ApplyEffect(Events.BetaForcedEvent);
            BetaForcedEventDone = 3;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
        else if (Structure.BetaFactory >= 150 && BetaForcedEventDone == 3 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 23;
            ApplyEffect(Events.BetaForcedEvent);
            BetaForcedEventDone = 4;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
    }

    public static void ForcedGammaEventHandler() {
        if (Structure.GammaFactory >= 10 && GammaForcedEventDone == 0 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 30;
            ApplyEffect(Events.GammaForcedEvent);
            GammaForcedEventDone = 1;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
        else if (Structure.GammaFactory >= 30 && GammaForcedEventDone == 1 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 31;
            ApplyEffect(Events.GammaForcedEvent);
            GammaForcedEventDone = 2;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
        else if (Structure.GammaFactory >= 60 && GammaForcedEventDone == 2 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 32;
            ApplyEffect(Events.GammaForcedEvent);
            GammaForcedEventDone = 3;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
        else if (Structure.GammaFactory >= 100 && GammaForcedEventDone == 3 && CanShowEvent && ForcedEventWantToShow == false) {
            EventToShow = 33;
            ApplyEffect(Events.GammaForcedEvent);
            GammaForcedEventDone = 4;
            ForcedEventWantToShow = true;
            CanShowEvent = false;
            RandomEventCanShow = false;
            EventCooldown = 0;
            Console.Beep();
            return;
        }
    }

    public static void RandomEventHandler() {
        // TODO : make the random chance event thingy here
        return;
    }
    // flow : on Program or wherever this will get called on, call it first to refresh then forced event, then Random
}
