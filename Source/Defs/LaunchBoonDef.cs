using System;
using Verse;

namespace VanillaGravshipExpanded
{
    public class LaunchBoonDef : Def
    {
        public float weight;

        public bool negateMaintenance = false;

        [MustTranslate]
        public string letterLabel;

        [MustTranslate]
        public string letterText;

        public LetterDef letterDef;

        public Type workerClass;

        private LaunchBoonWorker worker;

        public LaunchBoonWorker Worker => worker ?? (worker = (LaunchBoonWorker)Activator.CreateInstance(workerClass, this));
    }
}