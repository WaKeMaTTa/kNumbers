﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_AnimalWoolGrowth : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn.ageTracker.CurLifeStage.shearable)
            {
                var comp = pawn.AllComps.FirstOrDefault(x => x is CompShearable);

                if (comp != null)
                    return ((CompShearable)comp).Fullness.ToStringPercent();
            }
            return null;
        }

        public override int Compare(Pawn a, Pawn b) => GetScoreFor(a).CompareTo(GetScoreFor(b));

        private float GetScoreFor(Pawn pawn)
        {
            if (pawn.ageTracker.CurLifeStage.shearable)
            {
                var comp = pawn.AllComps.FirstOrDefault(x => x is CompShearable);

                if (comp != null)
                    return ((CompShearable)comp).Fullness;
            }
            return -1;
        }
    }
}