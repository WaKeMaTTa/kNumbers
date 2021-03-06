﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Numbers
{
    public class WorldComponent_Numbers : WorldComponent
    {
        public WorldComponent_Numbers(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            primaryFilter = PrimaryFilter.First();

            List<string> storedPawnTables = LoadedModManager.GetMod<Numbers>().GetSettings<Numbers_Settings>().storedPawnTableDefs;
            foreach (string item in storedPawnTables)
            {
                if (item.Split(',')[1] == "Default" && sessionTable.All(x => x.Key.defName != item.Split(',')[0]))
                {
                    PawnTableDef pawnTableDef = HorribleStringParsersForSaving.TurnCommaDelimitedStringIntoPawnTableDef(item);
                    sessionTable.Add(pawnTableDef, pawnTableDef.columns);
                }
            }
            NotifySettingsChanged();
        }

        public KeyValuePair<PawnTableDef, Func<Pawn, bool>> primaryFilter;

        public Dictionary<PawnTableDef, List<PawnColumnDef>> sessionTable = new Dictionary<PawnTableDef, List<PawnColumnDef>>();

        public override void ExposeData()
        {
            foreach (PawnTableDef type in DefDatabase<PawnTableDef>.AllDefsListForReading.Where(x => x.HasModExtension<DefModExtension_PawnTableDefs>()))
            {
                if (sessionTable.TryGetValue(type, out List<PawnColumnDef> workList))
                {
                    Scribe_Collections.Look(ref workList, "Numbers_" + type, LookMode.Def);
                    sessionTable[type] = workList;
                }
            }
        }

        public static readonly Dictionary<PawnTableDef, Func<Pawn, bool>> PrimaryFilter = new Dictionary<PawnTableDef, Func<Pawn, bool>>
        {
            //{ "All",            (pawn) => true },
            { NumbersDefOf.Numbers_MainTable,      pawn => !pawn.Dead && pawn.IsColonist },
            { NumbersDefOf.Numbers_Enemies,        pawn => !pawn.Dead && pawn.IsEnemy() },
            { NumbersDefOf.Numbers_Prisoners,      pawn => !pawn.Dead && pawn.IsPrisoner },
            { NumbersDefOf.Numbers_Guests,         pawn => !pawn.Dead && pawn.IsGuest() },
            { NumbersDefOf.Numbers_Animals,        pawn => !pawn.Dead && pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer },
            { NumbersDefOf.Numbers_WildAnimals,    pawn => !pawn.Dead && pawn.IsWildAnimal() },
            { NumbersDefOf.Numbers_Corpses,        pawn => pawn.Dead && !pawn.RaceProps.Animal },
            { NumbersDefOf.Numbers_AnimalCorpses,  pawn => pawn.Dead && pawn.RaceProps.Animal }
        };

        internal void NotifySettingsChanged()
        {
            if (Numbers_Settings.coolerThanTheWildlifeTab)
                DefDatabase<MainButtonDef>.GetNamed("Wildlife").tabWindowClass = typeof(MainTabWindow_NumbersWildLife);
            else
                DefDatabase<MainButtonDef>.GetNamed("Wildlife").tabWindowClass = typeof(MainTabWindow_Wildlife);

            DefDatabase<MainButtonDef>.GetNamed("Wildlife").Notify_ClearingAllMapsMemory();
        }
    }
}
