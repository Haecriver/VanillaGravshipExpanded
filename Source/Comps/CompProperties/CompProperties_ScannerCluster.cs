using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public class CompProperties_ScannerCluster : CompProperties_Scanner
{
    public string defaultModuleKey;
    public string defaultPassiveModuleKey;

    public string defaultGizmoIconPath;

    public Texture2D DefaultGizmoIcon
    {
        get
        {
            field ??= ContentFinder<Texture2D>.Get(defaultGizmoIconPath);
            return field;
        }
    }

    public CompProperties_ScannerCluster() => compClass = typeof(CompScannerCluster);
}