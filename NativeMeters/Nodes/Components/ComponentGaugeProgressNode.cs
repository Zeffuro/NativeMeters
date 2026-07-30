using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.BaseTypes.ComponentNode;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Nodes.Components.Gauge;

namespace NativeMeters.Nodes.Components;

public abstract unsafe class ComponentGaugeProgressNode : ComponentNode<AtkComponentGaugeBar, AtkUldComponentDataGaugeBar>, IMeterProgressNode
{
    private const int GaugeMaximum = 10000;
    private const float BrightTintTextureWeight = 0.12f;
    private const float BrightTintColorWeight = 0.90f;
    private const float BrightTintColorFloor = 0.03f;
    private static readonly Vector3 LegacyAdditiveMultiplyColor = new Vector3(90.0f, 75.0f, 75.0f) / 255.0f;

    protected ImageNode BackdropImageNode { get; }
    protected NineGridNode? StaticBackdropNode { get; }
    protected NineGridNode MainFillNode { get; }
    protected NineGridNode? IncreaseFillNode { get; }
    protected NineGridNode? DecreaseFillNode { get; }
    protected NineGridNode? BorderNode { get; }
    protected ImageNode? BorderImageNode { get; }

    private readonly ComponentGaugeProgressStyle style;
    private Vector2 requestedSize;
    private float progress;
    private bool fillRightToLeft;
    private bool isInitialized;

    public override Vector2 Size
    {
        get => requestedSize;
        set
        {
            requestedSize = new Vector2(Math.Max(1.0f, value.X), Math.Max(1.0f, value.Y));
            ApplyRequestedSize();
        }
    }

    public override float Width
    {
        get => requestedSize.X;
        set => Size = requestedSize with { X = value };
    }

    public override float Height
    {
        get => requestedSize.Y;
        set => Size = requestedSize with { Y = value };
    }

    public float Progress
    {
        get => progress;
        set
        {
            progress = Math.Clamp(value, 0.0f, 1.0f);
            UpdateGaugeValue();
        }
    }

    public Vector4 BackgroundColor
    {
        get => GetNodeColor(GetBackgroundColorNode(), style.BackgroundColorMode);
        set => ApplyNodeColor(GetBackgroundColorNode(), value, style.BackgroundColorMode, GetBackgroundColorMultiplyColor());
    }

    public Vector4 BarColor
    {
        get => GetNodeColor(MainFillNode, ResolveBarColorMode());
        set
        {
            var barColorMode = ResolveBarColorMode();

            ApplyNodeColor(MainFillNode, value, barColorMode, style.MainFill.MultiplyColor);

            if (IncreaseFillNode != null)
                ApplyNodeColor(IncreaseFillNode, value, barColorMode, style.IncreaseFill?.MultiplyColor);

            if (DecreaseFillNode != null)
                ApplyNodeColor(DecreaseFillNode, value, barColorMode, style.DecreaseFill?.MultiplyColor);
        }
    }

    public ProgressBarColorTreatment ColorTreatment { get; set; } = ProgressBarColorTreatment.Auto;

    public bool FillRightToLeft
    {
        get => fillRightToLeft;
        set
        {
            if (fillRightToLeft == value)
                return;

            fillRightToLeft = value;
            UpdateGaugeValue();
        }
    }

    protected ComponentGaugeProgressNode(ComponentGaugeProgressStyle style)
    {
        this.style = style;
        requestedSize = new Vector2(style.NativeWidth ?? style.Backdrop.Size.X, style.NativeHeight ?? style.Backdrop.Size.Y);

        SetInternalComponentType(ComponentType.GaugeBar);
        this.DisableCollisionNode();

        BackdropImageNode = CreateImageNode(style.Backdrop);
        BackdropImageNode.AttachNode(this);

        if (style.StaticBackdrop != null)
        {
            StaticBackdropNode = CreateNineGridNode(style.StaticBackdrop);
            StaticBackdropNode.AttachNode(this);
        }

        if (style.IncreaseFill != null)
        {
            IncreaseFillNode = CreateNineGridNode(style.IncreaseFill);
            IncreaseFillNode.AttachNode(this);
        }

        if (style.DecreaseFill != null)
        {
            DecreaseFillNode = CreateNineGridNode(style.DecreaseFill);
            DecreaseFillNode.AttachNode(this);
        }

        MainFillNode = CreateNineGridNode(style.MainFill);
        MainFillNode.AttachNode(this);

        if (style.Border != null)
        {
            BorderNode = CreateNineGridNode(style.Border);
            BorderNode.AttachNode(this);
        }

        if (style.BorderImage != null)
        {
            BorderImageNode = CreateImageNode(style.BorderImage);
            BorderImageNode.AttachNode(this);
        }

        Data->Nodes[0] = MainFillNode.NodeId;
        Data->Nodes[1] = BackdropImageNode.NodeId;
        Data->Nodes[2] = 0;
        Data->Nodes[3] = IncreaseFillNode?.NodeId ?? 0;
        Data->Nodes[4] = DecreaseFillNode?.NodeId ?? 0;
        Data->Nodes[5] = style.UseGaugeBorderSlot ? BorderNode?.NodeId ?? BorderImageNode?.NodeId ?? 0 : 0;
        Data->MarginV = 0;
        Data->MarginH = 0;
        Data->Vertical = 0;
        Data->Indicator = 0;
        Data->Min = 0;
        Data->Max = GaugeMaximum;
        Data->Value = 0;

        InitializeComponentEvents();

        isInitialized = true;
        ApplyRequestedSize();
        UpdateGaugeValue();
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        if (!isInitialized)
            return;

        LayoutInternalNodes();
        ComponentBase->Setup();
        UpdateGaugeValue();
    }

    protected virtual void LayoutInternalNodes()
    {
        var internalWidth = GetInternalWidth();
        var heightScale = requestedSize.Y / GetInternalHeight();
        var backdropHeight = style.Backdrop.Size.Y * heightScale;
        var mainFillHeight = style.MainFill.Size.Y * heightScale;
        var transitionHeight = style.TransitionFillHeight * heightScale;

        BackdropImageNode.Position = new Vector2(style.Backdrop.Position.X, style.Backdrop.Position.Y * heightScale);
        BackdropImageNode.Size = new Vector2(GetNodeWidth(style.Backdrop.Size.X, internalWidth), backdropHeight);

        if (StaticBackdropNode != null && style.StaticBackdrop != null)
        {
            StaticBackdropNode.Position = new Vector2(style.StaticBackdrop.Position.X, style.StaticBackdrop.Position.Y * heightScale);
            StaticBackdropNode.Size = new Vector2(GetNodeWidth(style.StaticBackdrop.Size.X, internalWidth), style.StaticBackdrop.Size.Y * heightScale);
        }

        MainFillNode.Position = new Vector2(style.MainFill.Position.X, style.MainFill.Position.Y * heightScale);
        MainFillNode.Height = mainFillHeight;

        if (BorderNode != null && style.Border != null)
        {
            BorderNode.Position = new Vector2(style.Border.Position.X, style.Border.Position.Y * heightScale);
            BorderNode.Size = new Vector2(GetNodeWidth(style.Border.Size.X, internalWidth), style.Border.Size.Y * heightScale);
        }

        if (BorderImageNode != null && style.BorderImage != null)
        {
            BorderImageNode.Position = new Vector2(style.BorderImage.Position.X, style.BorderImage.Position.Y * heightScale);
            BorderImageNode.Size = new Vector2(GetNodeWidth(style.BorderImage.Size.X, internalWidth), style.BorderImage.Size.Y * heightScale);
        }

        if (IncreaseFillNode != null && style.IncreaseFill != null)
        {
            IncreaseFillNode.Position = new Vector2(style.IncreaseFill.Position.X, style.IncreaseFill.Position.Y * heightScale);
            IncreaseFillNode.Height = transitionHeight;
        }

        if (DecreaseFillNode != null && style.DecreaseFill != null)
        {
            DecreaseFillNode.Position = new Vector2(style.DecreaseFill.Position.X, style.DecreaseFill.Position.Y * heightScale);
            DecreaseFillNode.Height = transitionHeight;
        }
    }

    protected virtual void AfterGaugeValueUpdated()
    {
        var isRightToLeft = fillRightToLeft || style.FillDirection == ComponentGaugeProgressFillDirection.RightToLeft;

        if (isRightToLeft)
        {
            var fillAreaWidth = GetNodeWidth(style.MainFill.Size.X, GetInternalWidth());
            MainFillNode.X = style.MainFill.Position.X + Math.Max(0.0f, fillAreaWidth - MainFillNode.Width);
        }
        else
        {
            MainFillNode.X = style.MainFill.Position.X;
        }

        var transitionX = isRightToLeft
            ? MainFillNode.X
            : Math.Max(0.0f, MainFillNode.Width - Component->MarginX);

        if (IncreaseFillNode != null)
        {
            IncreaseFillNode.X = transitionX;
            IncreaseFillNode.Width = 0.0f;
        }

        if (DecreaseFillNode != null)
        {
            DecreaseFillNode.X = transitionX;
            DecreaseFillNode.Width = 0.0f;
        }
    }

    private ImageNode CreateImageNode(ComponentGaugeImageNodeStyle nodeStyle)
    {
        var node = new ImageNode
        {
            NodeId = nodeStyle.NodeId,
            NodeFlags = nodeStyle.NodeFlags,
            DrawFlags = nodeStyle.DrawFlags,
            PartId = nodeStyle.PartId,
            Origin = nodeStyle.Origin,
            WrapMode = nodeStyle.WrapMode,
            ImageNodeFlags = nodeStyle.ImageFlags,
            Color = nodeStyle.Color,
            MultiplyColor = nodeStyle.MultiplyColor,
            AddColor = nodeStyle.AddColor,
        };

        node.AddPart(CreateParts());
        node.Node->PartsList->Id = style.PartsListId;

        return node;
    }

    private NineGridNode CreateNineGridNode(ComponentGaugeNineGridNodeStyle nodeStyle)
    {
        var node = new NineGridNode
        {
            NodeId = nodeStyle.NodeId,
            NodeFlags = nodeStyle.NodeFlags,
            DrawFlags = nodeStyle.DrawFlags,
            PartId = nodeStyle.PartId,
            Origin = nodeStyle.Origin,
            TopOffset = nodeStyle.TopOffset,
            BottomOffset = nodeStyle.BottomOffset,
            LeftOffset = nodeStyle.LeftOffset,
            RightOffset = nodeStyle.RightOffset,
            BlendMode = nodeStyle.BlendMode,
            PartsRenderType = nodeStyle.PartsRenderType,
            Color = nodeStyle.Color,
            MultiplyColor = nodeStyle.MultiplyColor,
            AddColor = nodeStyle.AddColor,
        };

        node.AddPart(CreateParts());
        node.Node->PartsList->Id = style.PartsListId;

        return node;
    }

    private Part[] CreateParts()
    {
        var parts = new Part[style.Parts.Count];

        for (var index = 0; index < style.Parts.Count; index++)
        {
            var source = style.Parts[index];
            parts[index] = new Part
            {
                Id = source.Id,
                TexturePath = source.TexturePath,
                TextureCoordinates = source.TextureCoordinates,
                Size = source.Size,
            };
        }

        return parts;
    }

    private void ApplyRequestedSize()
    {
        if (!isInitialized)
            return;

        var internalWidth = GetInternalWidth();
        var internalSize = new Vector2(internalWidth, requestedSize.Y);

        base.Size = internalSize;
        ScaleX = style.NativeWidth.HasValue ? requestedSize.X / style.NativeWidth.Value : 1.0f;
        ScaleY = 1.0f;
    }

    private void UpdateGaugeValue()
    {
        if (IsDisposed || !isInitialized)
            return;

        var gaugeValue = (int)Math.Round(progress * GaugeMaximum);
        Component->SetGaugeValue(gaugeValue, gaugeValue, true);
        ComponentBase->Update(0.0f);
        AfterGaugeValueUpdated();
    }

    private float GetInternalWidth()
        => style.GaugeNativeWidth ?? style.NativeWidth ?? requestedSize.X;

    private float GetInternalHeight()
        => style.NativeHeight ?? style.Backdrop.Size.Y;

    private float GetNodeWidth(float styleWidth, float fallbackWidth)
        => style.NativeWidth.HasValue ? styleWidth : fallbackWidth;

    private NodeBase GetBackgroundColorNode()
        => StaticBackdropNode ?? (NodeBase)BackdropImageNode;

    private Vector3 GetBackgroundColorMultiplyColor()
        => StaticBackdropNode != null && style.StaticBackdrop != null
            ? style.StaticBackdrop.MultiplyColor
            : style.Backdrop.MultiplyColor;

    private ComponentGaugeProgressColorMode ResolveBarColorMode()
        => ColorTreatment switch
        {
            ProgressBarColorTreatment.Flat => ComponentGaugeProgressColorMode.Flat,
            ProgressBarColorTreatment.NativeTint => style.NativeBarColorMode ?? style.BarColorMode,
            ProgressBarColorTreatment.LegacyAdditive => ComponentGaugeProgressColorMode.LegacyAdditive,
            _ => style.BarColorMode,
        };

    private static Vector4 GetNodeColor(NodeBase node, ComponentGaugeProgressColorMode colorMode)
        => colorMode switch
        {
            ComponentGaugeProgressColorMode.LegacyAdditive => new Vector4(node.AddColor.X, node.AddColor.Y, node.AddColor.Z, node.Color.W),
            ComponentGaugeProgressColorMode.Additive => new Vector4(node.AddColor.X, node.AddColor.Y, node.AddColor.Z, node.Color.W),
            ComponentGaugeProgressColorMode.BrightAdditive => new Vector4(
                Math.Max(0.0f, (node.AddColor.X - BrightTintColorFloor) / BrightTintColorWeight),
                Math.Max(0.0f, (node.AddColor.Y - BrightTintColorFloor) / BrightTintColorWeight),
                Math.Max(0.0f, (node.AddColor.Z - BrightTintColorFloor) / BrightTintColorWeight),
                node.Color.W),
            ComponentGaugeProgressColorMode.Multiply => new Vector4(node.MultiplyColor.X, node.MultiplyColor.Y, node.MultiplyColor.Z, node.Color.W),
            ComponentGaugeProgressColorMode.TextureAlpha => new Vector4(1.0f, 1.0f, 1.0f, node.Color.W),
            _ => node.Color,
        };

    private static void ApplyNodeColor(NodeBase node, Vector4 value, ComponentGaugeProgressColorMode colorMode, Vector3? additiveMultiplyColor = null)
    {
        if (colorMode == ComponentGaugeProgressColorMode.Additive || colorMode == ComponentGaugeProgressColorMode.LegacyAdditive)
        {
            node.Color = new Vector4(1.0f, 1.0f, 1.0f, value.W);
            node.MultiplyColor = colorMode == ComponentGaugeProgressColorMode.LegacyAdditive
                ? LegacyAdditiveMultiplyColor
                : additiveMultiplyColor ?? Vector3.One;
            node.AddColor = new Vector3(value.X, value.Y, value.Z);
            return;
        }

        if (colorMode == ComponentGaugeProgressColorMode.BrightAdditive)
        {
            node.Color = new Vector4(1.0f, 1.0f, 1.0f, value.W);
            node.MultiplyColor = new Vector3(BrightTintTextureWeight);
            node.AddColor = Vector3.Clamp(
                new Vector3(value.X, value.Y, value.Z) * BrightTintColorWeight + new Vector3(BrightTintColorFloor),
                Vector3.Zero,
                Vector3.One);
            return;
        }

        if (colorMode == ComponentGaugeProgressColorMode.TextureAlpha)
        {
            node.Color = new Vector4(1.0f, 1.0f, 1.0f, value.W);
            node.MultiplyColor = Vector3.One;
            node.AddColor = Vector3.Zero;
            return;
        }

        if (colorMode == ComponentGaugeProgressColorMode.Multiply)
        {
            node.Color = new Vector4(1.0f, 1.0f, 1.0f, value.W);
            node.MultiplyColor = new Vector3(value.X, value.Y, value.Z);
            node.AddColor = Vector3.Zero;
            return;
        }

        node.Color = value;
        node.MultiplyColor = Vector3.One;
        node.AddColor = Vector3.Zero;
    }
}
