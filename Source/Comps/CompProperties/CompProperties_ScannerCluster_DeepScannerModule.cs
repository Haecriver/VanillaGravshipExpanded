using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ScannerCluster_DeepScannerModule : CompProperties_ScannerCluster_ModuleBase
{
    public string randomResourceGizmoIconPath;
    public bool allowForResourceSelection = false;

    public Texture2D RandomResourceGizmoIcon
    {
        get
        {
            field ??= ContentFinder<Texture2D>.Get(randomResourceGizmoIconPath);
            return field;
        }
    }

    public CompProperties_ScannerCluster_DeepScannerModule() => compClass = typeof(CompScannerCluster_DeepScannerModule);
}