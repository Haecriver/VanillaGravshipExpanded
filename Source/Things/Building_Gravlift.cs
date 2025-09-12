using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Building_Gravlift : Building
    {
        private static readonly Texture2D LaunchToOrbitIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/Gizmo_LaunchVertically", true);
        
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            bool isInOrbit = Tile.LayerDef == PlanetLayerDefOf.Orbit;
            var gravEngine = this.GetComp<CompGravshipFacility>().LinkedBuildings.OfType<Building_GravEngine>().FirstOrDefault();
            
            if (gravEngine == null)
            {
                yield break;
            }
            var comp = gravEngine.GravshipComponents.Select(c => c.parent.TryGetComp<CompPilotConsole>()).FirstOrDefault(c => c != null);
            if (comp == null)
            {
                yield break;
            }
            var launchGizmo = new Command_Action
            {
                defaultLabel = "VGE_LaunchToOrbit".Translate(),
                defaultDesc = "VGE_LaunchToOrbitDesc".Translate(),
                icon = LaunchToOrbitIcon,
                action = delegate
                {
                    if (isInOrbit)
                    {
                        Messages.Message("VGE_CantLaunchAlreadyInOrbit".Translate(), MessageTypeDefOf.RejectInput);
                    }
                    else
                    {
                        Dialog_BeginRitual_ShowRitualBeginWindow_Patch.IsGravliftLaunch = true;
                        ShowLaunchRitual(comp);
                    }
                },
                disabled = isInOrbit,
                disabledReason = isInOrbit ? "VGE_CantLaunchAlreadyInOrbit".Translate() : null
            };
            if (launchGizmo.disabled is false)
            {
                var reason = comp.CanUseNow();
                if (reason.Accepted is false)
                {
                    launchGizmo.Disable(reason.Reason);
                }
            }
            yield return launchGizmo;
        }
        
        private void ShowLaunchRitual(CompPilotConsole console)
        {
            var ritualDef = VGEDefOf.GravshipLaunch;
            if (console.engine.def == VGEDefOf.VGE_GravjumperEngine)
            {
                ritualDef = VGEDefOf.VGE_GravjumperLaunch;
            }
            if (console.engine.def == VGEDefOf.VGE_GravhulkEngine)
            {
                ritualDef = VGEDefOf.VGE_GravhulkLaunch;
            }
            var target = console.parent;
            var precept = (Precept_Ritual)Faction.OfPlayer.ideos.GetPrecept(ritualDef);
            precept.ShowRitualBeginWindow(target, null, null);
        }
    }
}
