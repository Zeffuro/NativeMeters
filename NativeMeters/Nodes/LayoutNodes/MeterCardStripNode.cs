using System;
using System.Collections.Generic;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Rendering;

namespace NativeMeters.Nodes.LayoutNodes;

internal sealed class MeterCardStripNode : ResNode
{
    private readonly List<MeterRowListItemNode> cards = [];

    public void Update(MeterSettings settings, IReadOnlyList<CombatantRowData> rows)
    {
        var width = MeterRowBuilder.GetHorizontalCardWidth(Width, settings);
        var height = MathF.Floor(Math.Clamp(settings.RowHeight, 0, Height));
        var gap = MeterRowBuilder.GetHorizontalCardSpacing(Width, settings);
        var cardCount = width >= 1 && height >= 1 ? rows.Count : 0;
        var cardSize = new Vector2(width, height);

        while (cards.Count < cardCount)
        {
            var card = new MeterRowListItemNode { IsVisible = false };
            card.AttachNode(this);
            cards.Add(card);
        }

        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (i >= cardCount)
            {
                if (card.ItemData != null) card.ItemData = null;
                continue;
            }

            card.Position = new Vector2(i * (width + gap), 0);
            if (card.Size != cardSize) card.Size = cardSize;
            if (!ReferenceEquals(card.ItemData, rows[i])) card.ItemData = rows[i];
            card.Update();
        }
    }
}
