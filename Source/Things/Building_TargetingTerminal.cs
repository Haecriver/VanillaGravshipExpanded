using RimWorld;
using System.Collections.Generic;
using VEF.Graphics;
using Verse;

namespace VanillaGravshipExpanded
{
    public class Building_TargetingTerminal : Building, ITurretLinker
    {
        public Building_GravshipTurret linkedTurret;
        public List<Building_GravshipTurret> linkedTurrets = new List<Building_GravshipTurret>();
        public virtual bool MannedByPlayer => MannableComp?.MannedNow ?? false;
        public virtual float GravshipTargeting => MannableComp?.ManningPawn?.GetStatValue(VGEDefOf.VGE_GravshipTargeting) ?? 0f;

        public Thing LinkerThing => this;
        public IntVec3 LinkerPosition => Position;
        public Pawn ManningPawn => MannableComp?.ManningPawn;

        public virtual IEnumerable<Building_GravshipTurret> LinkedTurrets => linkedTurrets;
        public virtual int MaxLinkedTurrets => 1;
        public virtual float LinkRange => 36f;
        public virtual string OnlyArtilleryErrorKey => "VGE_TargetingTerminalCanOnlyLinkWithGravshipArtillery";
        public virtual string LinkGizmoDesc => "VGE_LinkWithTurretDesc".Translate();
        public virtual string UnlinkGizmoDesc => "VGE_UnlinkWithTurretDesc".Translate();
        public virtual string SelectGizmoDesc => "VGE_SelectLinkedTurretDesc".Translate();
        private CompMannable mannableComp;
        private CustomOverlayDrawer overlayDrawer;
        public CompMannable MannableComp
        {
            get
            {
                if (mannableComp == null)
                {
                    mannableComp = this.GetComp<CompMannable>();
                }
                return mannableComp;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref linkedTurret, "linkedTurret");
            Scribe_Collections.Look(ref linkedTurrets, "linkedTurrets", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                linkedTurrets ??= new List<Building_GravshipTurret>();
                if (linkedTurret != null && !linkedTurrets.Contains(linkedTurret))
                {
                    linkedTurrets.Add(linkedTurret);
                }
                linkedTurrets.RemoveAll(x => x == null);
                linkedTurret = null;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            overlayDrawer = map.GetComponent<CustomOverlayDrawer>();
            if (linkedTurrets.Count == 0)
                EnableOverlay();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            overlayDrawer = null;
        }

        public override void Tick()
        {
            base.Tick();

            for (int i = linkedTurrets.Count - 1; i >= 0; i--)
            {
                var turret = linkedTurrets[i];
                if (turret.Destroyed || !turret.Spawned)
                {
                    Unlink(turret);
                }
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            foreach (var turret in linkedTurrets)
            {
                GenDraw.DrawLineBetween(this.TrueCenter(), turret.TrueCenter(), SimpleColor.White);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            foreach (var gizmo in this.GetLinkerGizmos(LinkRange))
            {
                yield return gizmo;
            }
        }

        public virtual void LinkTo(Building_GravshipTurret turret)
        {
            if (linkedTurrets.Count >= MaxLinkedTurrets)
            {
                Unlink(linkedTurrets[0]);
            }
            linkedTurrets.Add(turret);
            turret.LinkTo(this);
        }

        public virtual void Unlink(Building_GravshipTurret turret)
        {
            if (linkedTurrets.Remove(turret))
            {
                turret.unlinking = true;
                turret.Unlink();
                turret.unlinking = false;
                if (linkedTurrets.Count == 0 && !Destroyed && Spawned)
                {
                    EnableOverlay();
                }
            }
        }

        public void Unlink()
        {
            for (int i = linkedTurrets.Count - 1; i >= 0; i--)
            {
                Unlink(linkedTurrets[i]);
            }
        }

        public void EnableOverlay() => overlayDrawer?.Enable(this, VGEDefOf.VGE_NoLinkedTurretOverlay);

        public void DisableOverlay() => overlayDrawer?.Disable(this, VGEDefOf.VGE_NoLinkedTurretOverlay);
    }
}
