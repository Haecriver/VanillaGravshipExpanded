using System;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class MoteThrownMoveTowardsSource : MoteThrown
{
    private bool forcedFadeout = false;
    public Vector3 target;

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);
    
        if (forcedFadeout)
            return;
    
        // If near the target, start fading out
        if (Math.Abs(exactPosition.x - target.x) < 0.5f && Math.Abs(exactPosition.z - target.z) < 0.5f)
        {
            forcedFadeout = true;
            solidTimeOverride = AgeSecs;
        }
    }
    
    public override Vector3 NextExactPosition(float deltaTime) => Vector3.MoveTowards(exactPosition, target, Speed * deltaTime);
}