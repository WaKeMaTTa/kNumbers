﻿namespace Numbers
{
    using System.Collections.Generic;
    using System.Linq;
    using Harmony;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class PawnColumnWorker_DiseaseProgression : PawnColumnWorker
    {
        private static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting");
        private static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending");
        private static readonly Color SeverePainColor = new Color(0.9f, 0.5f, 0f);

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.Dead || pawn.health?.hediffSet == null || !pawn.health.hediffSet.HasImmunizableNotImmuneHediff())
                return;

            HediffWithComps mostSevere = FindMostSevereHediff(pawn);

            if (mostSevere == null)
                return;

            float deltaSeverity = GetTextFor(mostSevere);

            GUI.color = GetPrettyColorFor(deltaSeverity);

            Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Widgets.Label(rect2, GetTextFor(mostSevere).ToStringPercent());
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            string tip = this.GetTip(pawn, mostSevere);
            if (!tip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect2, tip);
            }

            float severityChangePerDayFromImmu = (float)AccessTools.Method(typeof(HediffComp_Immunizable), "SeverityChangePerDay")
                                                    .Invoke(mostSevere.TryGetComp<HediffComp_Immunizable>(), null);

            float severityChangePerDayFromTendD = 0f;

            if (mostSevere.TryGetComp<HediffComp_TendDuration>()?.IsTended ?? false)
            {
                severityChangePerDayFromTendD =
                    mostSevere.TryGetComp<HediffComp_TendDuration>().TProps.severityPerDayTended *
                    mostSevere.TryGetComp<HediffComp_TendDuration>().tendQuality;
            }

            float immunityPerDay = 0f;

            ImmunityRecord immunityRecord = pawn.health.immunity.GetImmunityRecord(mostSevere.def);
            if (immunityRecord != null)
                immunityPerDay = immunityRecord.ImmunityChangePerTick(pawn, true, mostSevere) * GenDate.TicksPerDay;

            GUI.color = Color.white;

            bool redFlag = !(severityChangePerDayFromTendD + severityChangePerDayFromImmu > immunityPerDay);

            Texture2D texture2D = redFlag ? SortingIcon : SortingDescendingIcon;
            GUI.color = redFlag ? HealthUtility.GoodConditionColor : HealthUtility.DarkRedColor;
            Rect position2 = new Rect(rect.xMax - texture2D.width - 1f, rect.yMax - texture2D.height - 1f, texture2D.width, texture2D.height);
            GUI.DrawTexture(position2, texture2D);
            GUI.color = Color.white;
        }

        private string GetTip(Pawn pawn, HediffWithComps severe) => severe.LabelCap + ": " + severe.SeverityLabel + "\n" + severe.TipStringExtra;

        private float GetTextFor(HediffWithComps hediff) => hediff?.TryGetComp<HediffComp_Immunizable>().Immunity - hediff?.Severity ?? -1f; //nullcheck for Comparer.

        private Color GetPrettyColorFor(float deltaSeverity)
        {
            if (deltaSeverity <= -0.4f)
                return HealthUtility.DarkRedColor;

            if (deltaSeverity <= -0.2f)
                return SeverePainColor;

            if (deltaSeverity <= -0.1f)
                return HealthUtility.ImpairedColor;

            if (deltaSeverity <= -0.05f)
                return HealthUtility.SlightlyImpairedColor;

            if (deltaSeverity <= 0.05f)
                return Color.gray;

            return HealthUtility.GoodConditionColor;
        }

        private HediffWithComps FindMostSevereHediff(Pawn pawn)
        {
            IEnumerable<HediffWithComps> tmplist =
                pawn.health.hediffSet.hediffs.Where(x => x.Visible && x is HediffWithComps && !x.FullyImmune()).Cast<HediffWithComps>();

            float delta = float.MinValue;
            HediffWithComps mostSevereHediff = null;

            foreach (HediffWithComps hediff in tmplist)
            {
                HediffComp_Immunizable hediffCompImmunizable = hediff.TryGetComp<HediffComp_Immunizable>();

                if (hediffCompImmunizable == null)
                    continue;

                if (hediffCompImmunizable.Immunity - hediff.Severity > delta)
                {
                    delta = hediffCompImmunizable.Immunity - hediff.Severity;
                    mostSevereHediff = hediff;
                }
            }

            return mostSevereHediff;
        }

        public override int GetMinHeaderHeight(PawnTable table) => Mathf.CeilToInt(Text.CalcSize(this.def.LabelCap.WordWrapAt(this.GetMinWidth(table))).y);

        public override int Compare(Pawn a, Pawn b) =>
            GetTextFor(FindMostSevereHediff(a)).CompareTo(GetTextFor(FindMostSevereHediff(b)));
    }
}
