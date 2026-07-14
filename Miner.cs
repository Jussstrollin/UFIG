using System;
using System.Collections.Generic;

namespace StellaForge;

public class Miners {
    public class MinerTrait {
        private static readonly Random RNG = new Random();

        public static readonly List<Enums.MinerTrait> MinerTraitPool = new() {
            Enums.MinerTrait.Bedrock, Enums.MinerTrait.SoftSoil, Enums.MinerTrait.PristineCondition, Enums.MinerTrait.TooMuchSuck, Enums.MinerTrait.PoorMounting
        };

        public static Enums.MinerTrait? RollMinerTrait() {
            long Value = RNG.NextInt64(100);

            if (Value >= 0 && Value < 20) {
                return Enums.MinerTrait.PristineCondition;
            }
            else if (Value >= 20 && Value < 40) {
                return Enums.MinerTrait.PoorMounting;
            }
            else {
                return null;
            }
        }

        public class GenericMinerTrait {
            public GenericMinerTrait(GenericMiner Miner, double? OM, int? TNP, Enums.MinerTrait TID) {
                MinerOn = Miner;
                OutputMultiplier = OM;
                TickNeededToProd = TNP;
                TraitIdentifier = TID;
                HookSelf();
                ApplyEffects();
            }

            public GenericMiner MinerOn { get; set; }
            public double? OutputMultiplier { get; set; }
            public double? TickNeededToProd { get; set; }
            public Enums.MinerTrait TraitIdentifier { get; set; }

            public void ApplyEffects() {
                if (OutputMultiplier != null) {
                    if (OutputMultiplier >= -1.0) {
                        MinerOn.OutputMult *= (double)(1.0 + OutputMultiplier);
                    }
                }
                if (TickNeededToProd != null) {
                    MinerOn.TickNeededToProd += (int)TickNeededToProd;
                }
            }

            private void HookSelf() {
                MinerOn.TraitList.Add(this);
            }

            public void RemoveSelf() {
                if (OutputMultiplier != null) {
                    if (OutputMultiplier >= -1.0) {
                        MinerOn.OutputMult /= (double)(1.0 + OutputMultiplier);
                    }
                }
                if (TickNeededToProd != null) {
                    MinerOn.TickNeededToProd -= (int)TickNeededToProd;
                }
            }

            public virtual void OptionalSetup() { }

            public virtual void TickEffect() { }
        }

        // REMEMBER!! Lower TickNeededToProd means its faster, and higher Value means its SLOWER!!

        public class PristineCondition : GenericMinerTrait {
            public PristineCondition(GenericMiner Miner) : base(Miner, 0.10, -(int)(Miner.TickNeededToProd * 0.30), Enums.MinerTrait.PristineCondition) {
            }
        }

        public class PoorMounting : GenericMinerTrait {
            public PoorMounting(GenericMiner Miner) : base(Miner, -0.08, Miner is EssenceMiner ? (int)((Miner.TickNeededToProd * 1.20) - Miner.TickNeededToProd) : null, Enums.MinerTrait.PoorMounting) {
            }
        }
    }

    public enum MinerType {
        EssenceMiner,
    }

    public class GenericMiner {
        public Dictionary<Enums.ResourceType, double> Outputs;
        public double OutputMult { get; set; } = 1.0;

        public List<MinerTrait.GenericMinerTrait> TraitList;
        public int MaxTrait { get; set; } = 4;

        protected MainFactory AttachedTo { get; set; }
        protected int CurrTick { get; set; } = 0;
        public int TickNeededToProd { get; set; }
        public event Action? OnTickEffect;

        public GenericMiner(MainFactory ToAttachTo) {
            AttachedTo = ToAttachTo;
            TraitList = new();
            Outputs = new();
            AttachedTo.InvokeChangeHasHappened();
        }

        public virtual void Tick() {
            CurrTick++;
            OnTickEffect?.Invoke();
            if (CurrTick >= TickNeededToProd) {
                foreach (var Material in Outputs) {
                    AttachedTo.FactoryStorage.TryAppend(Material.Key, Material.Value * OutputMult);
                }
                CurrTick = 0;
            }
        }
    }

    public class EssenceMiner : GenericMiner {
        public EssenceMiner(MainFactory FT)
            : base(FT) {
            Outputs.Add(Enums.ResourceType.Essence, _MinerOutput);
            TickNeededToProd = 1;
        }

        private static readonly Random Rng = new Random();

        private const int _Base = 1;
        private double _MinerOutput = _Base + (Rng.NextDouble() + 1.0);
    }

    public static void AddMiner(MinerType ToAdd, MainFactory FT) {
        GenericMiner NewMiner = null;

        if (ToAdd == MinerType.EssenceMiner) {
            NewMiner = new EssenceMiner(FT);
        }

        if (NewMiner != null) {
            for (int i = 0; i < NewMiner.MaxTrait; i++) {
                Enums.MinerTrait? Trait = MinerTrait.RollMinerTrait();

                if (Trait == null) {
                    continue;
                }

                bool AlreadyActive = false;
                foreach (var ActiveTrait in NewMiner.TraitList) {
                    if (ActiveTrait.TraitIdentifier == Trait) {
                        AlreadyActive = true;
                        break;
                    }
                }

                if (AlreadyActive) {
                    continue;
                }

                switch (Trait) {
                    case Enums.MinerTrait.PristineCondition:
                        new MinerTrait.PristineCondition(NewMiner);
                        break;
                    case Enums.MinerTrait.PoorMounting:
                        new MinerTrait.PoorMounting(NewMiner);
                        break;
                }
            }
        }

        if (NewMiner != null) {
            FT.OnMinerTick += NewMiner.Tick;
            FT.MinersList.Add(NewMiner);
        }
    }
}
