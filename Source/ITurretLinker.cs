using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded
{
    public interface ITurretLinker : ILoadReferenceable
    {
        Thing LinkerThing { get; }
        IntVec3 LinkerPosition { get; }
        bool MannedByPlayer { get; }
        float GravshipTargeting { get; }
        Pawn ManningPawn { get; }
        IEnumerable<Building_GravshipTurret> LinkedTurrets { get; }
        int MaxLinkedTurrets { get; }
        float LinkRange { get; }
        string OnlyArtilleryErrorKey { get; }
        string LinkGizmoDesc { get; }
        string UnlinkGizmoDesc { get; }
        string SelectGizmoDesc { get; }
        void LinkTo(Building_GravshipTurret turret);
        void Unlink(Building_GravshipTurret turret);

        void EnableOverlay();
        void DisableOverlay();
    }
}
