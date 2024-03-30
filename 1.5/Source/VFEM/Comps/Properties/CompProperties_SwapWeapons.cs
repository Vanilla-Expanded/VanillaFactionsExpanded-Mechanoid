using RimWorld;
using Verse;

namespace VFEMech
{


    public class CompProperties_SwapWeapons : CompProperties_AbilityEffect
    {

        public ThingDef weapon1;
        public ThingDef weapon2;

        public CompProperties_SwapWeapons()
        {
            this.compClass = typeof(CompSwapWeapons);
        }
    }
}