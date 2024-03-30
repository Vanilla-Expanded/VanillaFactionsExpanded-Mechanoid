using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VFEMech
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            int range = 200;
            List<IntVec3> list = new List<IntVec3>();

            for (int i = -range; i < range; i++)
            {
                for (int j = -range; j < range; j++)
                {
                    list.Add(new IntVec3(i, 0, j));
                }
            }
            list.Sort(delegate (IntVec3 A, IntVec3 B)
                        {
                            float num = A.LengthHorizontalSquared;
                            float num2 = B.LengthHorizontalSquared;
                            return num < num2 ? -1 : (num != num2) ? 1 : 0;
                        });


            GenRadial.RadialPattern = new IntVec3[list.Count];
            float[] radii = new float[list.Count];

            for (int k = 0; k < list.Count; k++)
            {
                GenRadial.RadialPattern[k] = list[k];
                radii[k] = list[k].LengthHorizontal;
            }
            AccessTools.Field(typeof(GenRadial), "RadialPatternRadii").SetValue(null, radii);
            Harmony harmony = new Harmony("OskarPotocki.VanillaFactionsExpandedMechanoids");
            harmony.PatchAll();
        }
    }
}
