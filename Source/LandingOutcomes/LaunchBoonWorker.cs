using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded
{
    public abstract class LaunchBoonWorker
    {
        private LaunchBoonDef def;
        
        protected virtual LetterDef LetterDef => def.letterDef;

        protected virtual string LetterLabel => def.letterLabel;

        protected virtual string LetterText => def.letterText;

        public LaunchBoonWorker(LaunchBoonDef def)
        {
            this.def = def;
        }

        public virtual void ApplyBoon(Gravship gravship)
        {
            SendStandardLetter(gravship.Engine, null, gravship.Engine);
        }

        protected void SendStandardLetter(Building_GravEngine Engine, string extraText, LookTargets letterTarget, TaggedString baseText = default)
        {
            TaggedString text = baseText != default ? baseText : BaseText(Engine);
            if (extraText != null)
            {
                text += "\n\n" + extraText;
            }
            text += "\n\n" + "VGE_LaunchBoonOutcomeChanceLetterExtra".Translate(GravshipHelper.LaunchBoonChanceFromQuality(Engine.launchInfo.quality));
            Find.LetterStack.ReceiveLetter(LetterLabel, text, LetterDef, letterTarget);
        }

        protected virtual TaggedString BaseText(Building_GravEngine engine)
        {
            return LetterText.Formatted(engine.RenamableLabel.Named("GRAVSHIP"), engine.launchInfo.pilot.Named("PILOT"), engine.launchInfo.copilot.Named("COPILOT"));
        }

        public virtual bool CanTrigger(Gravship gravship)
        {
            return true;
        }
    }
}
