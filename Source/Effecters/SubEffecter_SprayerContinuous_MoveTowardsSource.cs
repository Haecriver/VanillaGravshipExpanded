using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class SubEffecter_SprayerContinuous_MoveTowardsSource : SubEffecter_Sprayer
{
    public Vector3 targetOffset = Vector3.zero;
    private int ticksUntilMote;
    private int moteCount;

    public SubEffecter_SprayerContinuous_MoveTowardsSource(SubEffecterDef def, Effecter parent) : base(def, parent)
    {
        ticksUntilMote = def.initialDelayTicks;
    }

    public override void SubEffectTick(TargetInfo A, TargetInfo B)
    {
        if (moteCount >= def.maxMoteCount)
            return;

        ticksUntilMote--;
        if (ticksUntilMote <= 0)
        {
            mote = null;
            MakeMote(A, B);
            if (mote is MoteThrownMoveTowardsSource source)
                source.target = A.CenterVector3 + targetOffset;

            ticksUntilMote = def.ticksBetweenMotes;
            moteCount++;
        }
    }
}