namespace NativeMeters.Models;

// Information gotten from Umbra: https://github.com/una-xiv/umbra/blob/main/Umbra.Game/src/Player/JobInfo/JobInfo.cs
public static class JobIconMaps
{
    public static uint GetIcon(uint rowId, JobIconType type)
    {
        return type switch
        {
            JobIconType.Default   => 62000u + rowId,
            JobIconType.Framed    => 62100u + rowId,
            JobIconType.Gearset   => 62800u + rowId,
            JobIconType.Glowing   => GetGlowingIcon(rowId),
            JobIconType.Light     => CrestIconConvert(rowId, 0),
            JobIconType.Dark      => CrestIconConvert(rowId, 1),
            JobIconType.Gold      => CrestIconConvert(rowId, 2),
            JobIconType.Orange    => CrestIconConvert(rowId, 3),
            JobIconType.Red       => CrestIconConvert(rowId, 4),
            JobIconType.Purple    => CrestIconConvert(rowId, 5),
            JobIconType.Blue      => CrestIconConvert(rowId, 6),
            JobIconType.Green     => CrestIconConvert(rowId, 7),
            _                     => 62000u + rowId
        };
    }

    private static uint CrestIconConvert(uint jobId, ushort offset)
    {
        uint o = offset * 500u;
        return jobId switch
        {
            1  => o + 91022u, 2  => o + 91023u, 3  => o + 91024u, 4  => o + 91025u,
            5  => o + 91026u, 6  => o + 91028u, 7  => o + 91029u, 8  => o + 91031u,
            9  => o + 91032u, 10 => o + 91033u, 11 => o + 91034u, 12 => o + 91035u,
            13 => o + 91036u, 14 => o + 91037u, 15 => o + 91038u, 16 => o + 91039u,
            17 => o + 91040u, 18 => o + 91041u, 19 => o + 91079u, 20 => o + 91080u,
            21 => o + 91081u, 22 => o + 91082u, 23 => o + 91083u, 24 => o + 91084u,
            25 => o + 91085u, 26 => o + 91030u, 27 => o + 91086u, 28 => o + 91087u,
            29 => o + 91121u, 30 => o + 91122u, 31 => o + 91125u, 32 => o + 91123u,
            33 => o + 91124u, 34 => o + 91127u, 35 => o + 91128u, 36 => o + 91129u,
            37 => o + 91130u, 38 => o + 91131u, 39 => o + 91132u, 40 => o + 91133u,
            41 => o + 91185u, 42 => o + 91186u, _  => o + 91169u
        };
    }

    private static uint GetGlowingIcon(uint jobId)
    {
        return jobId switch
        {
            1  => 62301u, 2  => 62302u, 3  => 62303u, 4  => 62304u,
            5  => 62305u, 6  => 62306u, 7  => 62307u, 8  => 62502u,
            9  => 62503u, 10 => 62504u, 11 => 62505u, 12 => 62506u,
            13 => 62507u, 14 => 62508u, 15 => 62509u, 16 => 62510u,
            17 => 62511u, 18 => 62512u, 19 => 62401u, 20 => 62402u,
            21 => 62403u, 22 => 62404u, 23 => 62405u, 24 => 62406u,
            25 => 62407u, 26 => 62308u, 27 => 62408u, 28 => 62409u,
            29 => 62309u, 30 => 62410u, 31 => 62411u, 32 => 62412u,
            33 => 62413u, 34 => 62414u, 35 => 62415u, 36 => 62416u,
            37 => 62417u, 38 => 62418u, 39 => 62419u, 40 => 62420u,
            41 => 62421u, 42 => 62422u, _  => 62301u
        };
    }
}