using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded
{
    public class GravshipLaunchState
    {
        public Precept_Ritual instance;
        public TargetInfo targetInfo;
        public RitualObligation forObligation;
        public Pawn selectedPawn;
        public Dictionary<string, Pawn> forcedForRole;
        public PlanetTile targetTile;
        public GravshipLaunchState(Precept_Ritual instance, TargetInfo targetInfo, RitualObligation forObligation, Pawn selectedPawn, Dictionary<string, Pawn> forcedForRole, PlanetTile targetTile)
        {
            this.instance = instance;
            this.targetInfo = targetInfo;
            this.forObligation = forObligation;
            this.selectedPawn = selectedPawn;
            this.forcedForRole = forcedForRole;
            this.targetTile = targetTile;
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(Window), "PostClose")]
    public static class Window_PostClose_Patch
    {
        public static void Postfix(Window __instance)
        {
            if (__instance is Dialog_BeginGravshipLaunch)
            {
                Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state = null;
            }
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(TilePicker), "StopTargeting")]
    public static class TilePicker_StopTargeting_Patch
    {
        public static void Prefix(TilePicker __instance)
        {
            if (__instance.active && __instance.noTileChosen != null)
            {
                Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state = null;
            }
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(GravshipUtility), "PreLaunchConfirmation")]
    public static class GravshipUtility_PreLaunchConfirmation_Patch
    {
        public static void Prefix(Building_GravEngine engine, ref Action launchAction)
        {
            var lordJob = engine.Map.lordManager.lords.Select(x => x.LordJob).OfType<LordJob_Ritual>().FirstOrDefault(lordJob => lordJob.ritual.def.IsGravshipLaunch());
            if (lordJob is not null && LordJob_Ritual_ExposeData_Patch.targetTile.TryGetValue(lordJob, out var tile))
            {
                launchAction = delegate
                {
                    WorldComponent_GravshipController.DestroyTreesAroundSubstructure(engine.Map, engine.ValidSubstructure);
                    Find.World.renderer.wantedMode = WorldRenderMode.None;
                    engine.ConsumeFuel(tile);
                    Find.GravshipController.InitiateTakeoff(engine, tile);
                    SoundDefOf.Gravship_Launch.PlayOneShotOnCamera();
                    Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state = null;
                };
            }
        }
    }

    [HarmonyPatch(typeof(LordJob_Ritual), "ApplyOutcome")]
    public static class LordJob_Ritual_ApplyOutcome_Patch
    {
        public static void Postfix(LordJob_Ritual __instance)
        {
            Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state = null;
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(SettlementProximityGoodwillUtility), "CheckConfirmSettle")]
    public static class SettlementProximityGoodwillUtility_CheckConfirmSettle_Patch
    {
        public static PlanetTile targetTile;
        public static void Prefix(PlanetTile tile, ref Action settleAction, Action cancelAction = null, Building_GravEngine gravEngine = null)
        {
            var state = Dialog_BeginRitual_ShowRitualBeginWindow_Patch.state;
            if (gravEngine != null && state is not null)
            {
                settleAction = delegate
                {
                    CameraJumper.TryHideWorld();
                    Current.Game.CurrentMap = gravEngine.Map;
                    Find.CameraDriver.JumpToCurrentMapLoc(gravEngine.Position);
                    state.instance.ShowRitualBeginWindow(state.targetInfo, state.forObligation, state.selectedPawn, state.forcedForRole);
                    targetTile = tile;
                    state.targetTile = tile;
                };
            }
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_GravshipLaunch), "Apply")]
    public static class RitualOutcomeEffectWorker_GravshipLaunch_Apply_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var anyThingWithDefMethod = AccessTools.Method(typeof(ListerThings), nameof(ListerThings.AnyThingWithDef));
            var helperMethod = AccessTools.Method(typeof(RitualOutcomeEffectWorker_GravshipLaunch_Apply_Patch), nameof(CheckForGravAnchor));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(anyThingWithDefMethod))
                {
                    yield return new CodeInstruction(OpCodes.Call, helperMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static bool CheckForGravAnchor(ListerThings lister, ThingDef def)
        {
            if (def == ThingDefOf.GravAnchor)
            {
                return false;
            }
            return lister.AnyThingWithDef(def);
        }

        public static void Postfix(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            var engine = jobRitual.selectedTarget.Thing?.TryGetComp<CompPilotConsole>()?.engine;
            if (engine is null) return;
            var launchInfo = engine.launchInfo;
            LaunchInfo_ExposeData_Patch.launchSourceTiles[launchInfo] = engine.Map.Tile;
            if (Dialog_BeginRitual_ShowRitualBeginWindow_Patch.IsGravliftLaunch)
            {
                LaunchInfo_ExposeData_Patch.isGravliftLaunch[launchInfo] = true;
                Dialog_BeginRitual_ShowRitualBeginWindow_Patch.IsGravliftLaunch = false;
            }
            RememberResearcher(jobRitual);
        }

        private static void RememberResearcher(LordJob_Ritual lordJob)
        {
            Pawn pawn = GravdataUtility.GetResearcher(lordJob?.assignments);
            if (pawn != null)
            {
                var engine = lordJob!.selectedTarget.Thing.TryGetComp<CompPilotConsole>().engine;
                LaunchInfo_ExposeData_Patch.gravtechResearcherPawns[engine.launchInfo] = pawn;
            }
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(RitualBehaviorWorker_GravshipLaunch), "TryExecuteOn")]
    public static class RitualBehaviorWorker_GravshipLaunch_TryExecuteOn_Patch
    {
        public static void Postfix(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments, bool playerForced = false)
        {
            var lordJob = target.Map.lordManager.lords.Select(x => x.LordJob).OfType<LordJob_Ritual>().FirstOrDefault(lordJob => lordJob.ritual.def.IsGravshipLaunch());
            LordJob_Ritual_ExposeData_Patch.targetTile[lordJob] = SettlementProximityGoodwillUtility_CheckConfirmSettle_Patch.targetTile;
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(Precept_Ritual), "ShowRitualBeginWindow")]
    public static class Dialog_BeginRitual_ShowRitualBeginWindow_Patch
    {
        public static GravshipLaunchState state;
        public static bool IsGravliftLaunch = false;
        public static bool Prefix(Precept_Ritual __instance, TargetInfo targetInfo, RitualObligation forObligation = null, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
        {
            if (__instance.def.IsGravshipLaunch())
            {
                if (IsGravliftLaunch)
                {
                    int currentTileId = targetInfo.Map.Tile;
                    PlanetLayer orbitLayer = Find.WorldGrid.Orbit;
                    PlanetTile orbitTile = orbitLayer.GetClosestTile_NewTemp(currentTileId);
                    SettlementProximityGoodwillUtility_CheckConfirmSettle_Patch.targetTile = orbitTile;
                    return true;
                }
                else if (state is null)
                {
                    var comp = targetInfo.Thing.TryGetComp<CompPilotConsole>();
                    state = new(__instance, targetInfo, forObligation, selectedPawn, forcedForRole, PlanetTile.Invalid);
                    comp.StartChoosingDestination_NewTemp();
                    return false;
                }
            }
            return true;
        }
    }
}
