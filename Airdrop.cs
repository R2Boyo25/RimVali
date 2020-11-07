﻿using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class AirDropHandler : MapComponent
    {
        private System.Random random = new System.Random();
        private int ticks = 0;
        public AirDropHandler(Map map)
            : base(map)
        {

        }

        public bool hasDropped;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hasDropped, "hasDropped", false);
            base.ExposeData();
        }
        private void SendDrop()
        {
            GenDate.DayOfYear(ticks, Find.WorldGrid.LongLatOf(map.Tile).x);
            List<Thing> thingList = new List<Thing>();
            thingList.Add(ThingMaker.MakeThing(AvaliDefs.AvaliNexus));
            Scribe_Values.Look(ref hasDropped, "hasDropped", false);
            Map target = map;
            List<Faction> newFactions = new List<Faction>();
            IntVec3 intVec3 = DropCellFinder.TradeDropSpot(target);
            if (/*RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, AvaliDefs.RimVali) >= 5 &&*/ !hasDropped)
            {
                hasDropped = true;
                for(int a = 0; a < random.Next(4, 5); a++)
                {
                    Faction faction = FactionGenerator.NewGeneratedFaction(AvaliDefs.AvaliFaction);
                    faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Ally);
                    newFactions.Add(faction);
                }
                DropPodUtility.DropThingsNear(intVec3, target, (IEnumerable<Thing>)thingList);
                ChoiceLetter choiceLetter = LetterMaker.MakeLetter("Illuminate airdrop", "An Illuminate airdrop has landed nearby!", AvaliMod.AvaliDefs.IlluminateAirdrop,newFactions[random.Next(newFactions.Count)]);
                Find.LetterStack.ReceiveLetter(choiceLetter, null);
            }
        }
        public override void MapComponentTick()
        {
            ticks++;
            if (ticks == 120)
            {
                SendDrop();
                ticks = 0;
            }
        }
    }
}