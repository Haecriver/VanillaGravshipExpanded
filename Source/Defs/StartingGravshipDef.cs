using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    public class StartingGravshipDef : Def
    {
        public PrefabDef prefab;
        public string imagePreview;
        public List<string> gravshipTags = new();
        private Texture2D imagePreviewTex;
        public Texture2D ImagePreview => imagePreviewTex ??= ContentFinder<Texture2D>.Get(imagePreview);
    }
}
