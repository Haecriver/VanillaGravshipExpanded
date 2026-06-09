using RimWorld;
using UnityEngine;

namespace VanillaGravshipExpanded;

public class Building_VacCheckpoint : Building_Door
{
    public override bool AlwaysOpen => true;

    public override bool CanDrawMovers => false;

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this);
    }
}