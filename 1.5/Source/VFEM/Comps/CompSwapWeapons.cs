
using Verse;
using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;




namespace VFEMech
{
    class CompSwapWeapons : CompAbilityEffect
    {

        public bool firstWeapon = true;

        new public CompProperties_SwapWeapons Props
        {
            get
            {
                return (CompProperties_SwapWeapons)this.props;
            } 
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (this.parent.pawn.Faction == Faction.OfPlayer)
            {
                if (firstWeapon)
                {
                    this.parent.pawn.equipment.DestroyEquipment(this.parent.pawn.equipment.Primary);
                    ThingDef stuff = GenStuff.RandomStuffFor(Props.weapon2);
                    ThingWithComps newEq = (ThingWithComps)ThingMaker.MakeThing(Props.weapon2, stuff);
                    this.parent.pawn.equipment.AddEquipment(newEq);
                    firstWeapon = false;
                }
                else
                {
                    this.parent.pawn.equipment.DestroyEquipment(this.parent.pawn.equipment.Primary);
                    ThingDef stuff = GenStuff.RandomStuffFor(Props.weapon1);
                    ThingWithComps newEq = (ThingWithComps)ThingMaker.MakeThing(Props.weapon1, stuff);
                    this.parent.pawn.equipment.AddEquipment(newEq);
                    firstWeapon = true;
                }
            }
        }
        public override bool ShouldHideGizmo => this.parent.pawn.Faction != Faction.OfPlayer;


        public override void PostExposeData()
        {
            Scribe_Values.Look(ref firstWeapon, "firstWeapon", true);
            base.PostExposeData();
        }





    }
}
