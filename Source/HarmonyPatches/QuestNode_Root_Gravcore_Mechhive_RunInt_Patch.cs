using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace VanillaGravshipExpanded;

[HarmonyPatch(typeof(QuestNode_Root_Gravcore_Mechhive), nameof(QuestNode_Root_Gravcore_Mechhive.RunInt))]
public static class QuestNode_Root_Gravcore_Mechhive_RunInt_Patch
{
    private static void Postfix()
    {
        var quest = QuestGen.quest;
        if (quest == null)
        {
            Log.Error("[VGE] QuestGen.quest is null after QuestNode_Root_Gravcore_Mechhive:RunInt method.");
            return;
        }

        // Reverse for loop, since we're removing elements from the list (in RemovePart method).
        // We could directly remove from the list (and set the part's parent to null),
        // but for safety call RemoveParent in case some mods patch it to do something with the removed part.
        for (var i = quest.PartsListForReading.Count - 1; i >= 0; i--)
        {
            var part = quest.PartsListForReading[i];
            if (part is QuestPart_QuestEndParent)
                quest.RemovePart(part);
        }
    }
}