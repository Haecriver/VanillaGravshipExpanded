using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class EnemyThrusterExtension : DefModExtension
    {
        public int directionInfluence = 20;

        public List<Vector3> flameOffsetsPerDirection = new List<Vector3>
        {
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero
        };

        public float flameSize;

#pragma warning disable 0649
        private ShaderTypeDef flameShaderType;
#pragma warning restore 0649

        public List<ShaderParameter> flameShaderParameters = new List<ShaderParameter>();

        public CompProperties_GravshipThruster.ExhaustSettings exhaustSettings = new CompProperties_GravshipThruster.ExhaustSettings();

        public ShaderTypeDef FlameShaderType => flameShaderType ?? ShaderTypeDefOf.MoteGlow;
    }
}
