﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_Stat : PawnColumnWorker_Text
    {
        public const int minWidthBasedOnNarrowestColumnThatColumnBeingMass = 69;
        public const int maxWidthBasedOnColumnsWithALongAssNameLikeThisInt = 150;
        public const int margin = 5;

        protected override string GetTextFor(Pawn pawn)
        {
            Thing thing = pawn;

            if (pawn.ParentHolder is Corpse tmpCorpse  && this.def.Ext().stat != StatDefOf.LeatherAmount) //this is dumb, but corpses don't seem to have leather.
                thing = tmpCorpse;

            return this.def.Ext().stat.Worker.IsDisabledFor(thing) ? null
                 : this.def.Ext().stat.Worker.ValueToString(thing.GetStatValue(this.def.Ext().stat), true);
        }

        public override int GetMinWidth(PawnTable table) => Mathf.Max(minWidthBasedOnNarrowestColumnThatColumnBeingMass, Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap.WordWrapAt(maxWidthBasedOnColumnsWithALongAssNameLikeThisInt)).x)) + margin;

        public override int GetMinHeaderHeight(PawnTable table) => Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap.WordWrapAt(this.GetMinWidth(table))).y); //not messy at all.

        public override int Compare(Pawn a, Pawn b) => (def.Ext().stat.Worker.IsDisabledFor(a) ? 0 : a.GetStatValue(def.Ext().stat)).CompareTo(def.Ext().stat.Worker.IsDisabledFor(b) ? 0 : b.GetStatValue(def.Ext().stat));

    }
}
 