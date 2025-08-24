using RimWorld;
using RimWorld.Planet;
using RimWorld.SketchGen;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded
{
    public class GenStep_GiantAsteroid : GenStep_Asteroid
    {
        public override float Radius => 0.35f;
    }
}
