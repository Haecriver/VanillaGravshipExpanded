using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded
{
    [HarmonyPatch(typeof(DamageWorker_Extinguish), nameof(DamageWorker_Extinguish.Apply))]
    public static class VanillaGravshipExpanded_DamageWorker_Extinguish_Apply_Patch
    {
        public static void Postfix(DamageInfo dinfo, Thing victim)
        {
            Astrofire fire = victim as Astrofire;
            if (fire == null || fire.Destroyed)
            {
                Thing thing = victim?.GetAttachment(VGEDefOf.VGE_Astrofire);
                if (thing != null)
                {
                    fire = (Astrofire)thing;
                }
            }
            if (fire != null && !fire.Destroyed)
            {
               
                fire.fireSize -= dinfo.Amount * 0.01f;
                if (fire.fireSize < 0.1f)
                {
                    fire.Destroy();
                }
            }
        }
    }
}
