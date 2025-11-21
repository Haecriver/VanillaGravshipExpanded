using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ActiveResourceTraderEffecter : CompProperties
{
    public EffecterDef effecterDef;
    public Vector3 offset;

    public CompProperties_ActiveResourceTraderEffecter() => compClass = typeof(CompActiveResourceTraderEffecter);
}