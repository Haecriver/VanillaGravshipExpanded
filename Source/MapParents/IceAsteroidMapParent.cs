
using RimWorld.Planet;
using Verse;
namespace VanillaGravshipExpanded
{
    public class IceAsteroidMapParent : SpaceMapParent
    {
        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (preciousResource != null)
            {
                text += "\n" + "VGE_SoildIceAsteroid".Translate(NamedArgumentUtility.Named(preciousResource, "RESOURCE"));
            }
            return text.Trim();
        }
    }
}
