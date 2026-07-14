using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded;

public abstract class FuelTabEntry(Building_GravEngine engine)
{
    public Building_GravEngine engine = engine;
    public List<Thing> fuelProviders = [];
    public List<Thing> thrusters = [];

    public virtual float SortingOrder => 0;

    public abstract string Title { get; }

    public virtual float TitleHeight(float width)
    {
        if (Title.NullOrEmpty())
            return 0;

        using (new TextBlock(GameFont.Medium))
            return Text.CalcHeight(Title, width);
    }

    public virtual float DrawTitle(float x, float y, float width)
    {
        if (Title.NullOrEmpty())
            return 0;

        using (new TextBlock(GameFont.Medium))
        {
            var height = Text.CalcHeight(Title, width);
            Widgets.Label(new Rect(x, y, width, height), Title);
            return height;
        }
    }

    public abstract Rect DoInterface(float x, float y, float width, int index);
}

public class SimpleMultiLineTextEntry(Building_GravEngine engine) : FuelTabEntry(engine)
{
    public List<string> text = [];
    protected List<float> textSizes = [];
    protected float totalHeight = 0f;
    public string title;

    public override string Title => title;

    public override Rect DoInterface(float x, float y, float width, int index)
    {
        if (textSizes.Empty())
        {
            totalHeight = TitleHeight(width);
            for (var i = 0; i < text.Count; i++)
            {
                var size = Text.CalcHeight(text[i], width);
                textSizes.Add(size);
                totalHeight += size;
            }
        }

        var rect = new Rect(x, y, width, totalHeight);

        if (index % 2 == 0)
            Widgets.DrawAltRect(rect);

        Widgets.BeginGroup(rect);

        var curY = DrawTitle(0f, 0f, width);
        for (var i = 0; i < text.Count; i++)
        {
            Widgets.Label(new Rect(0f, curY, width, textSizes[i]), text[i]);
            curY += textSizes[i];
        }

        Widgets.EndGroup();

        return rect;
    }
}

public class GenericFuelTabEntry(Building_GravEngine engine) : SimpleMultiLineTextEntry(engine)
{
    public float currentFuel = 0f;
    public float maxFuel = 0f;

    public override float SortingOrder => 1000f;

    public override Rect DoInterface(float x, float y, float width, int index)
    {
        if (text.Empty())
            text.Add($"{"VGE_FuelTab_OtherFuel".Translate().CapitalizeFirst()}: {currentFuel} / {maxFuel}");

        return base.DoInterface(x, y, width, index);
    }
}