using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(GenThing), "ItemCenterAt")]
    public static class GenThing_ItemCenterAt_Patch
    {
        private static int MaxItemsPerVisualStack => 5;

        public static bool Prefix(ref Vector3 __result, Thing thing)
        {
            List<Thing> thingList = thing.Position.GetThingList(thing.Map);
            bool onTargetShelf = false;
            foreach (Thing t in thingList)
            {
                if (t.def == VGEDefOf.VGE_GravshipShelf)
                {
                    onTargetShelf = true;
                    break;
                }
            }
            if (!onTargetShelf)
            {
                return true;
            }
            int num = 0;
            int itemsWithLowerID = 0;
            bool allSameDef = true;
            ThingDef firstItemDef = null;

            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing2 = thingList[i];
                if (thing2.def.category == ThingCategory.Item)
                {
                    if (firstItemDef == null)
                    {
                        firstItemDef = thing2.def;
                    }
                    num++;
                    if (thing2.thingIDNumber < thing.thingIDNumber)
                    {
                        itemsWithLowerID++;
                    }
                    if (thing2.def != firstItemDef)
                    {
                        allSameDef = false;
                    }
                }
            }

            if (num > 1 && allSameDef)
            {
                int pileIndex = itemsWithLowerID / MaxItemsPerVisualStack;
                int indexInPile = itemsWithLowerID % MaxItemsPerVisualStack;

                Vector2 pileOffset = new Vector2(pileIndex * 0.30f, pileIndex * -0.20f);
                Vector2 inPileOffset = new Vector2(indexInPile * 0.05f, indexInPile * 0.10f);

                Vector3 basePos = thing.Position.ToVector3Shifted();
                float altitude = thing.def.Altitude + (itemsWithLowerID * 0.03658537f / 10f);

                __result = new Vector3(
                    basePos.x + pileOffset.x + inPileOffset.x - 0.2f,
                    altitude,
                    basePos.z + pileOffset.y + inPileOffset.y + 0.1f
                );

                return false;
            }

            return true;
        }
    }
}
