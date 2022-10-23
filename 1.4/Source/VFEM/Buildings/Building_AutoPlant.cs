using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFE.Mechanoids.Buildings
{
    [StaticConstructorOnStartup]
    internal class Building_AutoPlant : Building
    {
        public int range = 7;
        public bool running = false;
        public float offset = 0;
        private static readonly float speedPerTick = 0.001f;
        protected bool blockedByTree = false;
        private static readonly Graphic baseGraphic = GraphicDatabase.Get(typeof(Graphic_Multi), "Things/Buildings/MachineryBases/AutoMachineryBase", ShaderDatabase.Cutout, new Vector2(7, 2), Color.white, Color.white);
        private Sustainer sustainer;

        private CompPowerTrader powerComp = null;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = new List<Gizmo>();
            gizmos.AddRange(base.GetGizmos());

            if (range > 2)
            {
                Command_Action command_Action = new Command_Action
                {
                    action = delegate
                {
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();
                    range--;
                    MoteMaker.ThrowText(this.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), Map, range - 1 + " tiles", Color.white);
                },
                    defaultLabel = "VFEMechLowerRange".Translate(),
                    defaultDesc = "VFEMechLowerRangeDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/DecreaseArea")
                };
                gizmos.Add(command_Action);
            }

            if (range < 31)
            {
                Command_Action command_Action2 = new Command_Action
                {
                    action = delegate
                {
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();
                    range++;
                    MoteMaker.ThrowText(this.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), Map, range - 1 + " tiles", Color.white);
                },
                    defaultLabel = "VFEMechRaiseRange".Translate(),
                    defaultDesc = "VFEMechRaiseRangeDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/IncreaseArea")
                };
                gizmos.Add(command_Action2);
            }

            Command_Toggle initiate = new Command_Toggle();
            if (running)
            {
                initiate.defaultLabel = "VFEMechStopMachine".Translate();
                initiate.defaultDesc = "VFEMechStopMachineDesc".Translate();
                initiate.icon = ContentFinder<Texture2D>.Get("UI/MachineryOff");
            }
            else
            {
                initiate.defaultLabel = "VFEMechStartMachine".Translate();
                initiate.defaultDesc = "VFEMechStartMachineDesc".Translate();
                initiate.icon = ContentFinder<Texture2D>.Get("UI/MachineryOn");
            }
            initiate.toggleAction = delegate { running = !running; if (running) { TryStartSustainer(); } };
            initiate.isActive = () => running;
            gizmos.Add(initiate);

            return gizmos;
        }
        public override void Tick()
        {
            base.Tick();

            if (powerComp.PowerOn)
            {
                if (running)
                {
                    if (sustainer != null && !sustainer.Ended)
                    {
                        sustainer.Maintain();
                    }
                    else
                    {
                        TryStartSustainer();
                    }

                    powerComp.powerOutputInt = -1900;
                    bool shouldCheck = false;
                    if (offset == 0)
                    {
                        shouldCheck = true;
                    }

                    int floor = Mathf.FloorToInt(offset);
                    offset += speedPerTick;
                    if (Mathf.FloorToInt(offset) != floor)
                    {
                        shouldCheck = true;
                    }

                    if (shouldCheck)
                    {
                        if (offset != speedPerTick && offset != speedPerTick + 1) //Ignore cell 0
                        {
                            DoWorkOnCells();
                        }

                        if (offset < range && !CheckCellsClear())
                        {
                            Messages.Message("VFEMechMachineHitObstacle".Translate(), this, MessageTypeDefOf.CautionInput);
                            running = false;
                        }
                    }
                    if (offset >= range)
                    {
                        offset = range;
                        running = false;
                    }
                }
                else if (offset > 0)
                {
                    if (sustainer != null && !sustainer.Ended)
                    {
                        sustainer.Maintain();
                    }
                    else
                    {
                        TryStartSustainer(); // Necessary for startups where the machine runs in reverse
                    }

                    offset -= speedPerTick;
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                }
                else
                {
                    powerComp.powerOutputInt = -powerComp.Props.PowerConsumption;
                    if (sustainer != null && !sustainer.Ended)
                    {
                        sustainer.End();
                    }
                }
            }
            else if (sustainer != null && !sustainer.Ended)
            {
                sustainer.End();
            }
        }

        private void DoWorkOnCells()
        {
            if (Rotation == Rot4.East || Rotation == Rot4.West)
            {
                for (int z = Position.z - 3; z <= Position.z + 3; z++)
                {
                    if (Rotation == Rot4.East)
                    {
                        DoWorkOnCell(new IntVec3(Position.x + Mathf.FloorToInt(offset), 0, z));
                    }
                    else
                    {
                        DoWorkOnCell(new IntVec3(Position.x - Mathf.FloorToInt(offset), 0, z));
                    }
                }
            }
            else
            {
                for (int x = Position.x - 3; x <= Position.x + 3; x++)
                {
                    if (Rotation == Rot4.North)
                    {
                        DoWorkOnCell(new IntVec3(x, 0, Position.z + Mathf.FloorToInt(offset)));
                    }
                    else
                    {
                        DoWorkOnCell(new IntVec3(x, 0, Position.z - Mathf.FloorToInt(offset)));
                    }
                }
            }
        }

        private bool CheckCellsClear()
        {
            if (Rotation == Rot4.East || Rotation == Rot4.West)
            {
                for (int z = Position.z - 3; z <= Position.z + 3; z++)
                {
                    if (Rotation == Rot4.East && !CheckCell(new IntVec3(Position.x + Mathf.FloorToInt(offset + 2), 0, z)))
                    {
                        return false;
                    }
                    else if (Rotation == Rot4.West && !CheckCell(new IntVec3(Position.x - Mathf.FloorToInt(offset + 2), 0, z)))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int x = Position.x - 3; x <= Position.x + 3; x++)
                {
                    if (Rotation == Rot4.North && !CheckCell(new IntVec3(x, 0, Position.z + Mathf.FloorToInt(offset + 2))))
                    {
                        return false;
                    }
                    else if (Rotation == Rot4.South && !CheckCell(new IntVec3(x, 0, Position.z - Mathf.FloorToInt(offset + 2))))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CheckCell(IntVec3 cell)
        {
            foreach (Thing t in cell.GetThingList(Map))
            {
                if (t != this && t.def.passability != Traversability.Standable && (t.def.plant == null || blockedByTree))
                {
                    return false;
                }
            }
            return true;
        }

        public override Vector3 DrawPos => base.DrawPos + OffsetDrawPos();

        private Vector3 OffsetDrawPos()
        {
            if (Rotation == Rot4.East)
            {
                return new Vector3(offset, 1f, 0);
            }
            else if (Rotation == Rot4.West)
            {
                return new Vector3(-offset, 1f, 0);
            }
            else if (Rotation == Rot4.North)
            {
                return new Vector3(0, 1f, offset);
            }

            return new Vector3(0, 1f, -offset);
        }

        public override void Draw()
        {
            base.Draw();
            baseGraphic.Draw(base.DrawPos, Rotation, this);
        }

        protected virtual void DoWorkOnCell(IntVec3 cell)
        {
            //Override this to actually be useful
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref range, "range");
            Scribe_Values.Look<bool>(ref running, "running");
            Scribe_Values.Look<float>(ref offset, "offset");
        }

        private bool TryStartSustainer()
        {
            if (sustainer != null)
            {
                sustainer.End();
            }

            SoundInfo soundInfo = SoundInfo.InMap(this);
            sustainer = SoundDef.Named("GeothermalPlant_Ambience").TrySpawnSustainer(soundInfo);
            return sustainer != null;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = this.TryGetComp<CompPowerTrader>();
        }
    }
}
