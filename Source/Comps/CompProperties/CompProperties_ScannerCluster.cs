using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ScannerCluster : CompProperties_Scanner
{
    public string defaultModuleKey;
    public string defaultPassiveModuleKey;

    public string defaultGizmoIconPath;

    [Unsaved] private Texture2D defaultGizmoIconInt;
    
    public Texture2D DefaultGizmoIcon
    {
        get
        {
            defaultGizmoIconInt ??= ContentFinder<Texture2D>.Get(defaultGizmoIconPath);
            return defaultGizmoIconInt;
        }
    }

    public CompProperties_ScannerCluster() => compClass = typeof(CompScannerCluster);
}