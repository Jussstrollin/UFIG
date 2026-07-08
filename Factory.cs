using System;
using System.Collections.Generic;

namespace StellaForge;

#nullable enable

public class Factories {
    public class Traits {
        private static Random RNG = new();

        public static readonly List<Enums.FactoryTrait> LuckGivenTraitPool = new() { Enums.FactoryTrait.PassedQualityAssurance, Enums.FactoryTrait.ThoughtfulMakers, Enums.FactoryTrait.BrokenOutputHatch };

        public static Enums.FactoryTrait? RollTraitFor() {
            // TODO: Add a weight value depending on the Factory's LuckValue to getting Positive or Negative trait, both fields and values will be in the future
            long Value = RNG.NextInt64(100);
            // PassedQA = 20%
            // ThoughtfulMakers = 20%
            // null / reserved for future = 60%
            if (Value >= 0 && Value < 20) {
                return LuckGivenTraitPool[0];
            }
            else if (Value >= 20 && Value < 40) {
                return LuckGivenTraitPool[1];
            }
            else if (Value >= 40 && Value < 50) {
                return LuckGivenTraitPool[2];
            }
            else {
                return null;
            }
        }



        public class GenericTrait {
            public GenericTrait(GenericFactory Factory, double? OM, double? IM, int? TNP, Enums.FactoryTrait TID, bool IGBT) {
                FactoryOn = Factory;
                OutputMultipler = OM;
                InputMultiplier = IM;
                TickNeededToProd = TNP;
                TraitIdentifier = TID;
                IsGivenByTier = IGBT;
                ApplyEffects();
                AddSelfToList();
            }

            protected GenericFactory FactoryOn { get; set; }
            protected double? OutputMultipler { get; set; } = null;
            protected double? InputMultiplier { get; set; } = null;
            protected int? TickNeededToProd { get; set; } = null;
            public Enums.FactoryTrait TraitIdentifier { get; set; }
            protected bool IsGivenByTier { get; set; } = false;
            public string? Description { get; set; }

            private void AddSelfToList() {
                if (IsGivenByTier) {
                    FactoryOn.TierGivenTraitList.Add(this);
                }
                else {
                    FactoryOn.TraitList.Add(this);
                }
            }

            private void ApplyEffects() {
                if (OutputMultipler != null) {
                    if (OutputMultipler >= -1.0) {
                        FactoryOn.OutputMult *= (double)(1.0 + OutputMultipler);
                    }
                }
                if (InputMultiplier != null) {
                    if (InputMultiplier >= -1.0) {
                        FactoryOn.InputMult *= (double)(1.0 + InputMultiplier);
                    }
                }
                if (TickNeededToProd != null) {
                    FactoryOn.TickNeededToProd += (int)TickNeededToProd;
                }

                FactoryOn.OnTickEffect += TickEffect;
            }

            public virtual void RemoveSelf() {
                bool Removed = false;

                if (IsGivenByTier) {
                    Removed = FactoryOn.TierGivenTraitList.Remove(this);
                }
                else {
                    Removed = FactoryOn.TraitList.Remove(this);
                }

                if (Removed) {
                    if (OutputMultipler != null) {
                        if (OutputMultipler > -1.0) {
                            FactoryOn.OutputMult /= (double)(1.0 + OutputMultipler);
                        }
                    }
                    if (InputMultiplier != null) {
                        if (InputMultiplier > -1.0) {
                            FactoryOn.InputMult /= (double)(1.0 + InputMultiplier);
                        }
                    }
                    if (TickNeededToProd != null) {
                        FactoryOn.TickNeededToProd -= (int)TickNeededToProd;
                    }

                    FactoryOn.OnTickEffect -= TickEffect;
                }
            }

            public virtual void TickEffect() { }
        }

        public class PassedQualityAssurance : GenericTrait {
            public PassedQualityAssurance(GenericFactory FactoryIsOn, bool IsGivenByTier) : base(FactoryIsOn, 0.03, null, null, Enums.FactoryTrait.PassedQualityAssurance, IsGivenByTier) {
                Description = "Well, it turned out Good enough.";
            }
        }

        public class ThoughtfulMakers : GenericTrait {
            public ThoughtfulMakers(GenericFactory FactoryIsOn, bool IsGivenByTier) : base(FactoryIsOn, -0.10, 0.10, null, Enums.FactoryTrait.ThoughtfulMakers, IsGivenByTier) {
                Description = "Whoever made this seemed to be in a good mood, the quality is better than usual.";
            }
        }

        public class BrokenOutputHatch : GenericTrait {
            public BrokenOutputHatch(GenericFactory FactoryIsOn, bool IsGivenByTier) : base(FactoryIsOn, null, -0.05, null, Enums.FactoryTrait.BrokenOutputHatch, IsGivenByTier) {
                Description = "Whoops, someone broke the output hatch, I sure hope it doesnt affect production.";
            }
        }

        public class TimeDialation : GenericTrait {
            public TimeDialation(GenericFactory FactoryIsOn, bool IsGivenByTier, int MinInterval, int MaxInterval, int AmountToReduce, double ChanceToTrigger) : base(FactoryIsOn, null, null, null, Enums.FactoryTrait.TimeDialation, IsGivenByTier) {
                Description = "An Unknown Anomaly causes Time Reversal on a factory everynow and then, it doesnt seem to harm the employees, just reverts progress back a few unit of Time.";
                Min = MinInterval;
                Max = MaxInterval;
                ReduceAmount = AmountToReduce;
                ChanceValue = (int)(ChanceToTrigger * 100);
                Reroll();
            }

            private int ChanceValue;

            private int Min { get; set; }
            private int Max { get; set; }

            private int ReduceAmount;
            private int interval;

            private void Reroll() {
                interval = (int)RNG.NextInt64(Min, Max);
            }

            public override void TickEffect() {
                interval--;
                if (interval <= 0) {
                    if ((int)RNG.NextInt64(0, 100) < ChanceValue) {
                        // yes, allow it to get to negative, if its a strong rollback, it should feel like the factory is incapacitated for a while without having to actually rollback produced resources.
                        FactoryOn.CurrTick -= ReduceAmount;
                        Reroll();
                    }
                }
            }
        }
    }

    public class TierApplier {
        private static Random RNG = new();

        public class GenericTierEffect {
            public GenericTierEffect(GenericFactory Factory, double? Om, double? Im, int? TNP, Enums.FactoryTier TI, int MaxTrait) {
                OutputMultipler = Om;
                InputMultiplier = Im;
                TickNeededToProd = TNP;
                FactoryAimingAt = Factory;
                TierIdentifier = TI;
                MaxTraitToApply = MaxTrait;
                TagFactory();
                TickInject();
            }

            protected GenericFactory FactoryAimingAt;
            protected double? OutputMultipler { get; set; } = null;
            protected double? InputMultiplier { get; set; } = null;
            protected int? TickNeededToProd { get; set; } = null;
            protected Enums.FactoryTier TierIdentifier { get; set; }

            protected int MaxTraitToApply { get; set; }

            private void TagFactory() {
                FactoryAimingAt.FactoryTier = this.TierIdentifier;
            }

            public virtual void ApplyEffect() {
                if (OutputMultipler != null) {
                    if (OutputMultipler >= -1.0) {
                        FactoryAimingAt.OutputMult *= (double)(1.0 + OutputMultipler);
                    }
                }
                if (InputMultiplier != null) {
                    if (InputMultiplier >= -1.0) {
                        FactoryAimingAt.InputMult *= (double)(1.0 + InputMultiplier);
                    }
                }
                if (TickNeededToProd != null) {
                    FactoryAimingAt.TickNeededToProd += (int)TickNeededToProd;
                }

                FactoryAimingAt.MaxTrait = MaxTraitToApply;
            }

            public virtual void TickInject() { }
        }

        public class PrototypeTier : GenericTierEffect {
            public PrototypeTier(GenericFactory F) : base(F, -0.10, null, null, Enums.FactoryTier.Prototype, 2) {
                base.ApplyEffect();
                new Traits.TimeDialation(F, GivenByTier, MinInterval, MaxInterval, TickReduceAmount, 0.30);
            }

            private const int MinInterval = 30;
            private const int MaxInterval = 50;
            private const int TickReduceAmount = 10;
            private const bool GivenByTier = true;
        }

        public class PrototypePlusTier : GenericTierEffect {
            public PrototypePlusTier(GenericFactory F) : base(F, -0.05, null, null, Enums.FactoryTier.PrototypePlus, 2) {
                base.ApplyEffect();
                new Traits.TimeDialation(F, GivenByTier, MinInterval, MaxInterval, TickReduceAmount, 0.10);
            }

            private const int MinInterval = 60;
            private const int MaxInterval = 80;
            private const int TickReduceAmount = 5;
            private const bool GivenByTier = true;
        }

        public class PrototypePlusPlusTier : GenericTierEffect {
            public PrototypePlusPlusTier(GenericFactory F) : base(F, null, null, null, Enums.FactoryTier.PrototypePlusPlus, 2) {
                base.ApplyEffect();
            }
        }
    }

    public class FactoryCreationRelated {
        public static readonly Dictionary<Enums.FactoryTypes, Func<MainFactory, Storage, GenericFactory>> FactoryMap = new() {
            { Enums.FactoryTypes.AlphaFactory, (MF, S) => new AlphaFactory(MF, S) },
            { Enums. FactoryTypes.BetaFactory, (MF, S) => new BetaFactory(MF, S) },
            { Enums.FactoryTypes.GammaFactory, (MF, S) => new GammaFactory(MF, S) }
        };
        public static readonly Dictionary<Enums.FactoryTier, Func<GenericFactory, TierApplier.GenericTierEffect>> TierMap = new() {
            { Enums.FactoryTier.Prototype, (ToAttachTo) => new TierApplier.PrototypeTier(ToAttachTo) },
            { Enums.FactoryTier.PrototypePlus, (ToAttachTo) => new TierApplier.PrototypePlusTier(ToAttachTo) },
            { Enums.FactoryTier.PrototypePlusPlus, (ToAttachTo) => new TierApplier.PrototypePlusPlusTier(ToAttachTo) }
        };
        public static readonly Dictionary<Enums.FactoryTrait, Func<GenericFactory, Traits.GenericTrait>> TraitMap = new() {
            { Enums.FactoryTrait.PassedQualityAssurance, (ToAttachTo) => new Traits.PassedQualityAssurance(ToAttachTo, false) },
            { Enums.FactoryTrait.ThoughtfulMakers, (ToAttachTo) => new Traits.ThoughtfulMakers(ToAttachTo, false) },
            { Enums.FactoryTrait.BrokenOutputHatch, (ToAttachTo) => new Traits.BrokenOutputHatch(ToAttachTo, false) }
        };

        public static int MakeNewFactory(
            MainFactory MainFactoryToAttachOn,
            Enums.FactoryTypes ToMake,
            Storage StorageRef,
            Enums.FactoryTier FT
        ) {
            GenericFactory? NewFactory = null;

            if (FactoryMap.TryGetValue(ToMake, out var FactoryCreator)) {
                NewFactory = FactoryCreator(MainFactoryToAttachOn, StorageRef);
            }
            else {
                return 1;
            }

            GetThisBoiATier(NewFactory, FT);

            GetThisBoiATrait(NewFactory);

            MainFactoryToAttachOn.FactoryList.Add(NewFactory);
            return 0;
        }

        private static void GetThisBoiATier(GenericFactory WhoToGive, Enums.FactoryTier WhatToGive) {
            if (TierMap.TryGetValue(WhatToGive, out var TierCreator)) {
                TierCreator(WhoToGive);
            }
        }

        private static void GetThisBoiATrait(GenericFactory WhoToGive) {
            if (WhoToGive.TraitList.Count != 0) { Console.WriteLine("Failed to give boi a trait: Factory already have a trait!"); return; }
            for (int i = 0; i < WhoToGive.MaxTrait; i++) {
                Enums.FactoryTrait? Trait = Traits.RollTraitFor();

                if (Trait == null) {
                    continue;
                }

                bool IsActive = false;
                foreach (var TraitFound in WhoToGive.TraitList) {
                    if (TraitFound.TraitIdentifier == Trait) {
                        IsActive = true;
                        break;
                    }
                }

                if (IsActive) {
                    continue;
                }

                if (TraitMap.TryGetValue(Trait.Value, out var TraitCreator)) {
                    TraitCreator(WhoToGive);
                }
            }
        }

        public static void RerollTraits(GenericFactory ToReroll) {
            if (ToReroll.TraitList.Count != 0) {
                for (int i = ToReroll.TraitList.Count - 1; i >= 0; i--) {
                    ToReroll.TraitList[i].RemoveSelf();
                }
            }
            GetThisBoiATrait(ToReroll);
        }
    }

    public class GenericFactory {
        // word salad here but basically
        // protected : children can access it so BallzClass : GenericFactory, BallzClass Can access it.
        // virtual : children can implement their own function logic, just that others can call Generic.Tick() no matter the implementation.

        public Dictionary<Enums.ResourceType, double> Inputs;
        public Dictionary<Enums.ResourceType, double> Outputs;
        public double InputMult = 1.0d;
        public double OutputMult = 1.0d;
        protected Storage _StorageRef;
        protected MainFactory ToAttachTo;

        public int MaxTrait = 0;
        public List<Traits.GenericTrait> TraitList;
        public List<Traits.GenericTrait> TierGivenTraitList;

        public Enums.FactoryTypes FactoryType { get; set; }
        public Enums.FactoryTier FactoryTier { get; set; }

        public event Action? OnTickEffect;

        public GenericFactory(MainFactory TAT, Storage StorageRef) {
            ToAttachTo = TAT;
            _StorageRef = StorageRef;
            Inputs = new();
            Outputs = new();
            TraitList = new();
            TierGivenTraitList = new();
            TAT.InvokeChangeHasHappened();
            ToAttachTo.OnFactoryTick += Tick;
        }

        public int CurrTick { get; set; } = 0;
        public int TickNeededToProd { get; set; }

        public virtual void Tick() {
            CurrTick++;
            OnTickEffect?.Invoke();
            if (CurrTick >= TickNeededToProd) {
                Storage.ReturnType Status = Storage.ReturnType.SUCCESS;
                foreach (var Input in Inputs) {
                    var _confirmation = ToAttachTo.FactoryStorage.TryDeduct(Input.Key, Input.Value * InputMult, true);
                    if (_confirmation != Storage.ReturnType.SUCCESS) {
                        Status = Storage.ReturnType.FAIL;
                    }
                }

                if (Status == Storage.ReturnType.SUCCESS) {
                    foreach (var Item in Inputs) {
                        ToAttachTo.FactoryStorage.TryDeduct(Item.Key, Item.Value * InputMult, false);
                    }
                    foreach (var Item in Outputs) {
                        ToAttachTo.FactoryStorage.TryAppend(Item.Key, Item.Value * OutputMult);
                    }
                }
                CurrTick = 0;
            }
        }

        public virtual void KillFactory() {
            var Removed = ToAttachTo.FactoryList.Remove(this);

            if (Removed) {
                ToAttachTo.OnFactoryTick -= Tick;
                OnTickEffect = null;

                // Clean up traits
                foreach (var Trait in TraitList) {
                    Trait.RemoveSelf();
                }
                foreach (var Trait in TierGivenTraitList) {
                    Trait.RemoveSelf();
                }

                // Clear collections
                Inputs.Clear();
                Outputs.Clear();
                TraitList.Clear();
                TierGivenTraitList.Clear();

                // Clear references
                ToAttachTo = null!;
                _StorageRef = null!;
            }
        }
    }

    public class AlphaFactory : GenericFactory {
        public AlphaFactory(MainFactory TAT, Storage storageRef)
            : base(TAT, storageRef) {
            FactoryType = Enums.FactoryTypes.AlphaFactory;
            Inputs.Add(Enums.ResourceType.Essence, 1.0d);
            Outputs.Add(Enums.ResourceType.Alpha, 1.0d);
            TickNeededToProd = 3;
        }
    }

    public class BetaFactory : GenericFactory {
        public BetaFactory(MainFactory TAT, Storage storageRef)
            : base(TAT, storageRef) {
            FactoryType = Enums.FactoryTypes.BetaFactory;
            Inputs.Add(Enums.ResourceType.Alpha, 1.0d);
            Outputs.Add(Enums.ResourceType.Beta, 0.5d);
            TickNeededToProd = 8;
        }
    }

    public class GammaFactory : GenericFactory {
        public GammaFactory(MainFactory TAT, Storage storageRef)
            : base(TAT, storageRef) {
            FactoryType = Enums.FactoryTypes.GammaFactory;
            Inputs.Add(Enums.ResourceType.Alpha, 1.0d);
            Inputs.Add(Enums.ResourceType.Beta, 0.5d);
            Inputs.Add(Enums.ResourceType.Essence, 2.0);
            Outputs.Add(Enums.ResourceType.Gamma, 1.0d);
            TickNeededToProd = 10;
        }
    }
}

