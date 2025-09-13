using System.Collections.Generic;
using Verse;
using PipeSystem;

namespace VanillaGravshipExpanded
{
    public class FuelSpentData : IExposable
    {
        public Dictionary<Thing, float> fuelData = new Dictionary<Thing, float>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref fuelData, "fuelData", LookMode.Reference, LookMode.Value, ref tmpThings, ref tmpAmounts);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                fuelData ??= new Dictionary<Thing, float>();
                fuelData.RemoveAll(kvp => kvp.Key == null);
            }
        }
        
        private List<Thing> tmpThings = new List<Thing>();
        private List<float> tmpAmounts = new List<float>();
    }
}
