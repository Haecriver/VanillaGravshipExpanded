using System.Collections.Generic;
using Verse;

namespace VanillaGravshipExpanded;

// Not necessarily required (especially if inheriting from a different comp class),
// but makes it faster to create a new module that doesn't inherit from other stuff.
// Specifically, the orbital scanner module needs to inherit from actual orbital scanner.
public class CompProperties_ScannerCluster_ModuleBase : CompProperties
{
    public string clusterModuleKey;

    public string noActiveScanningMessage;
    public string missingResearchPrerequisiteMessageKey = "VGE.ScannerCluster.MissingResearchPrerequisite";
    public List<ResearchProjectDef> scanningResearchPrerequisites;

    public float? scanFindMtbDaysOverride = null;
    public float? scanFindGuaranteedDaysOverride = null;
}