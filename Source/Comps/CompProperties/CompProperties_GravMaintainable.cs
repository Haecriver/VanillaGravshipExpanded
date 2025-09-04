using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanillaGravshipExpanded
{
    public class CompProperties_GravMaintainable : CompProperties
    {

        public float minMaintenanceForAlert = 0.3f;
        public float fleckEmissionRate = 0.01f;

        public CompProperties_GravMaintainable()
        {
            compClass = typeof(CompGravMaintainable);
        }
    }
}
