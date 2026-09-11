using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_VacCheckpoint : CompProperties
{
    // Starting values
    public float requiredResistanceByDefault = 0.7f;
    public bool allowDraftedByDefault = true;
    // If this should apply to player, even if the building has a different faction
    public bool alwaysApplyToPlayer = false;
    // If this should apply to pawns in a mental state
    public bool applyToPawnsWithMentalState = false;
    // Determines if the gizmo only shows up in space (or other biomes with vacuum) or if the building is on top of substructure
    public bool onlyShowGizmoWhenRelevant = false;

    public CompProperties_VacCheckpoint() => compClass = typeof(CompVacCheckpoint);
}