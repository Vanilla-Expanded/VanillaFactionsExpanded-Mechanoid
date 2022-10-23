using RimWorld;
using System.Collections.Generic;
using Verse;
using VFE.Mechanoids.PlaceWorkers;

namespace VFE.Mechanoids.Buildings
{
    internal class Building_AutoSower : Building_AutoPlant, IPlantToGrowSettable
    {
        private ThingDef plantToPlant = ThingDefOf.Plant_Potato;
        public Building_AutoSower()
        {
            blockedByTree = true;
        }
        public IEnumerable<IntVec3> Cells => PlaceWorker_AutoPlant.GetCells(Position, Rotation, this);
        protected override void DoWorkOnCell(IntVec3 cell)
        {
            base.DoWorkOnCell(cell);
            List<Thing> thingList = cell.GetThingList(Map);
            List<Thing> toDestroy = new List<Thing>();
            foreach (Thing t in thingList)
            {
                if (t.def.plant != null && t.def.plant.harvestedThingDef == null)
                {
                    toDestroy.Add(t);
                }
                else if (t != this && ((t.def.plant != null && t.def.plant.harvestedThingDef != null) || t.def.BlocksPlanting(true)))
                {
                    return;
                }
            }
            foreach (Thing t in toDestroy)
            {
                t.Kill();
            }

            if (plantToPlant.plant.interferesWithRoof && cell.Roofed(Map))
            {
                return;
            }

            Thing otherPlant = PlantUtility.AdjacentSowBlocker(plantToPlant, cell, Map);
            if (otherPlant != null)
            {
                return;
            }

            if (cell.GetTerrain(Map).fertility < plantToPlant.plant.fertilityMin)
            {
                return;
            }

            Plant plant = (Plant)GenSpawn.Spawn(plantToPlant, cell, Map);
            plant.Growth = 0.05f;
            plant.sown = true;
            Map.mapDrawer.MapMeshDirty(cell, MapMeshFlag.Things);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingDef>(ref plantToPlant, "plantToPlant");
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = new List<Gizmo>();
            gizmos.AddRange(base.GetGizmos());
            gizmos.Add(SetPlantToGrowCommand(this));
            return gizmos;
        }
        public static Command_SetPlantToGrowAutoSower SetPlantToGrowCommand(IPlantToGrowSettable settable)
        {
            return new Command_SetPlantToGrowAutoSower
            {
                defaultDesc = "CommandSelectPlantToGrowDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc12,
                settable = settable
            };
        }
        public ThingDef GetPlantDefToGrow()
        {
            return plantToPlant;
        }

        public void SetPlantDefToGrow(ThingDef plantDef)
        {
            plantToPlant = plantDef;
        }

        public bool CanAcceptSowNow()
        {
            return true;
        }
    }
}
