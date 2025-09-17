using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
namespace VanillaGravshipExpanded
{
    public class GameCondition_GravitationalAnomaly : GameCondition
    {
       

        private static readonly WeatherOverlay_GravitationalAnomaly gravitationalAnomalyOverlay = new WeatherOverlay_GravitationalAnomaly();

        private static readonly List<SkyOverlay> overlays = new List<SkyOverlay> { gravitationalAnomalyOverlay };


        private Sustainer sustainer;

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return overlays;
        }

        private int curColorIndex = -1;

        private int prevColorIndex = -1;

        private float curColorTransition;

        private const float SkyColorStrength = 0.075f;

        private const float OverlayColorStrength = 0.025f;

        private const int TransitionDurationTicks_NotPermanent = 280;

        private int longTickCounter = 0;

        private static readonly Color[] Colors = new Color[8]
        {
            new Color(0f, 0f, 1f),
            new Color(0.3f, 0.3f, 1f),
            new Color(0f, 0.7f, 1f),
            new Color(0.3f, 0.7f, 1f),
            new Color(0f, 0.5f, 1f),
            new Color(0.2f, 0.2f, 1f),
            new Color(0.5f, 0f, 1f),
            new Color(0.35f, 0f, 1f)
        };

        public Color CurrentColor => Color.Lerp(Colors[prevColorIndex], Colors[curColorIndex], curColorTransition);

        private int TransitionDurationTicks => TransitionDurationTicks_NotPermanent;

        public override int TransitionTicks => 200;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curColorIndex, "curColorIndex", 0);
            Scribe_Values.Look(ref prevColorIndex, "prevColorIndex", 0);
            Scribe_Values.Look(ref curColorTransition, "curColorTransition", 0f);
            Scribe_Values.Look(ref longTickCounter, "longTickCounter");
        }

        public override void Init()
        {
            base.Init();
            curColorIndex = Rand.Range(0, Colors.Length);
            prevColorIndex = curColorIndex;
            curColorTransition = 1f;
        }

        public override void GameConditionDraw(Map map)
        {
            gravitationalAnomalyOverlay.DrawOverlay(map);
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            Color currentColor = CurrentColor;
            SkyColorSet colorSet = new SkyColorSet(Color.Lerp(Color.white, currentColor, SkyColorStrength), new Color(0.92f, 0.92f, 0.92f), Color.Lerp(Color.white, currentColor, OverlayColorStrength), 1f);
            return new SkyTarget(0f, colorSet, 1f, 1f);
        }




        public override void GameConditionTick()
        {
            var map = SingleMap;
            curColorTransition += 1f / (float)TransitionDurationTicks;
            if (curColorTransition >= 1f)
            {
                prevColorIndex = curColorIndex;
                curColorIndex = GetNewColorIndex();
                curColorTransition = 0f;
            }
            gravitationalAnomalyOverlay.TickOverlay(map, 1f);
            longTickCounter++;
            if (longTickCounter >= 2000)
            {
                if (World_ExposeData_Patch.currentGravtechProject != null)
                {
                    Find.ResearchManager.AddProgress(World_ExposeData_Patch.currentGravtechProject, 1);
                }
                longTickCounter = 0;
            }
            
        }

        private int GetNewColorIndex()
        {
            return (from x in Enumerable.Range(0, Colors.Length)
                    where x != curColorIndex
                    select x).RandomElement();
        }

       
    }
}