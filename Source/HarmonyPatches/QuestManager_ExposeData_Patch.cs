using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(QuestManager), nameof(QuestManager.ExposeData))]
public static class QuestManager_ExposeData_Patch
{
    private static void Prefix(QuestManager __instance)
    {
        // During PostLoadInit, quests get sorted into either active or historical quests.
        // Set the quest as not ended (and restore end outcome to default value) so it gets re-added as an active one.
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Quest firstInactive = null;

            foreach (var quest in __instance.QuestsListForReading)
            {
                if (quest != null && quest.root == QuestScriptDefOf.GravEngine)
                {
                    // We have an active quest, return
                    if (!quest.ended)
                        return;
                    firstInactive ??= quest;
                }
            }

            if (firstInactive != null)
            {
                firstInactive.ended = false;
                firstInactive.endOutcome = default;
            }
        }
    }
}