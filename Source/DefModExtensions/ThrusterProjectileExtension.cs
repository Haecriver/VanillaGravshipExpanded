using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class ThrusterProjectileExtension : DefModExtension
    {
        public float flameSize;
        public ShaderTypeDef flameShaderType;
        public List<ShaderParameter> flameShaderParameters;
        public FleckDef exhaustFleckDef;
        public float emissionsPerSecond;
        public FloatRange spawnRadiusRange;
        public Vector3 velocity;
        public FloatRange velocityRotationRange;
        public FloatRange velocityMultiplierRange;
        public FloatRange rotationOverTimeRange;
        public FloatRange scaleRange;
    }
}
