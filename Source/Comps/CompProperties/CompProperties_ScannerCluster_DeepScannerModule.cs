using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ScannerCluster_DeepScannerModule : CompProperties_ScannerCluster_ModuleBase
{
    public string randomResourceGizmoIconPath;
    public bool allowForResourceSelection = false;

    [Unsaved] private Texture2D randomResourceGizmoIconInt;
    
    public Texture2D RandomResourceGizmoIcon
    {
        get
        {
            randomResourceGizmoIconInt ??= ContentFinder<Texture2D>.Get(randomResourceGizmoIconPath);
            return randomResourceGizmoIconInt;
        }
    }

    public CompProperties_ScannerCluster_DeepScannerModule() => compClass = typeof(CompScannerCluster_DeepScannerModule);
}