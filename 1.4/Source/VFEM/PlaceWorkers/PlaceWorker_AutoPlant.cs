using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using VFE.Mechanoids.Buildings;

namespace VFE.Mechanoids.PlaceWorkers
{
    public class PlaceWorker_AutoPlant : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawFieldEdges(GetCells(center, rot, thing));
        }
        public static List<IntVec3> GetCells(IntVec3 center, Rot4 rot, Thing thing)
        {
            List<IntVec3> cells = new List<IntVec3>();
            int range = 7;
            Building_AutoPlant autoPlant;

            // Here in case the ability to uninstall is added by other mods like MinifyEverything
            if (thing is Blueprint_Install blueprint)
            {
                Thing toInstall = blueprint.MiniToInstallOrBuildingToReinstall;
                autoPlant = toInstall is MinifiedThing minified ? minified.InnerThing as Building_AutoPlant : toInstall as Building_AutoPlant;
            }
            else
            {
                autoPlant = thing as Building_AutoPlant; // Could be Blueprint_Build or Frame, so safe cast here
            }

            if (autoPlant != null)
            {
                range = autoPlant.range;
            }

            for (int offset = 0; offset < range - 1; offset++)
            {
                if (rot == Rot4.North)
                {
                    for (int x = -3; x <= 3; x++)
                    {
                        cells.Add(new IntVec3(center.x + x, 0, center.z + 2 + offset));
                    }
                }
                else if (rot == Rot4.South)
                {
                    for (int x = -3; x <= 3; x++)
                    {
                        cells.Add(new IntVec3(center.x + x, 0, center.z - 2 - offset));
                    }
                }
                else if (rot == Rot4.East)
                {
                    for (int z = -3; z <= 3; z++)
                    {
                        cells.Add(new IntVec3(center.x + 2 + offset, 0, center.z + z));
                    }
                }
                else
                {
                    for (int z = -3; z <= 3; z++)
                    {
                        cells.Add(new IntVec3(center.x - 2 - offset, 0, center.z + z));
                    }
                }
            }
            return cells;
        }
    }
}
