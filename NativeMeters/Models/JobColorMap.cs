using System.Collections.Generic;
using System.Numerics;

namespace NativeMeters.Models;

// Default colors retrieved from https://github.com/lichie567/LMeter/blob/main/LMeter/Config/BarColorsConfig.cs
public static class JobColorMaps
{
    public static readonly Dictionary<uint, Vector4> DefaultColors = new()
    {
        // Tanks
        { 19, new Vector4(168f / 255f, 210f / 255f, 230f / 255f, 1f) }, // PLD
        { 21, new Vector4(207f / 255f, 38f / 255f, 33f / 255f, 1f) },   // WAR
        { 32, new Vector4(209f / 255f, 38f / 255f, 204f / 255f, 1f) },  // DRK
        { 37, new Vector4(121f / 255f, 109f / 255f, 48f / 255f, 1f) },  // GNB

        // Healers
        { 24, new Vector4(255f / 255f, 240f / 255f, 220f / 255f, 1f) }, // WHM
        { 28, new Vector4(134f / 255f, 87f / 255f, 255f / 255f, 1f) },  // SCH
        { 33, new Vector4(255f / 255f, 231f / 255f, 74f / 255f, 1f) },  // AST
        { 40, new Vector4(144f / 255f, 176f / 255f, 255f / 255f, 1f) }, // SGE

        // Melee DPS
        { 20, new Vector4(214f / 255f, 156f / 255f, 0f / 255f, 1f) },   // MNK
        { 22, new Vector4(65f / 255f, 100f / 255f, 205f / 255f, 1f) },  // DRG
        { 30, new Vector4(175f / 255f, 25f / 255f, 100f / 255f, 1f) },  // NIN
        { 34, new Vector4(228f / 255f, 109f / 255f, 4f / 255f, 1f) },   // SAM
        { 39, new Vector4(150f / 255f, 90f / 255f, 144f / 255f, 1f) },  // RPR
        { 41, new Vector4(16f / 255f, 130f / 255f, 16f / 255f, 1f) },   // VPR

        // Physical Ranged DPS
        { 23, new Vector4(145f / 255f, 186f / 255f, 94f / 255f, 1f) },  // BRD
        { 31, new Vector4(110f / 255f, 225f / 255f, 214f / 255f, 1f) }, // MCH
        { 38, new Vector4(226f / 255f, 176f / 255f, 175f / 255f, 1f) }, // DNC

        // Magical Ranged DPS
        { 25, new Vector4(165f / 255f, 121f / 255f, 214f / 255f, 1f) }, // BLM
        { 27, new Vector4(45f / 255f, 155f / 255f, 120f / 255f, 1f) },  // SMN
        { 36, new Vector4(0f / 255f, 185f / 255f, 247f / 255f, 1f) },   // BLU
        { 35, new Vector4(232f / 255f, 123f / 255f, 123f / 255f, 1f) }, // RDM
        { 42, new Vector4(252f / 255f, 146f / 255f, 225f / 255f, 1f) }, // PCT

        // Unknown/Limit Break
        { 0, new Vector4(218f / 255f, 157f / 255f, 46f / 255f, 1f) },   // UKN
    };
}
