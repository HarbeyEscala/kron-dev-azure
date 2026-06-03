namespace Kron.Counting.Shared.Helpers;

public sealed class Hp015ParsedRecord
{
    public DateTime TimestampUtc { get; set; }

    public int PeopleIn { get; set; }

    public int PeopleOut { get; set; }

    public bool Focus { get; set; }

    public string RawData { get; set; } = default!;
}

public static class Hp015Parser
{
    public static Hp015ParsedRecord Parse(
        string dataHex)
    {
        dataHex = dataHex.Trim();

        var year =
            2000 + Convert.ToInt32(
                dataHex.Substring(0, 2),
                16);

        var month =
            Convert.ToInt32(
                dataHex.Substring(2, 2),
                16);

        var day =
            Convert.ToInt32(
                dataHex.Substring(4, 2),
                16);

        var hour =
            Convert.ToInt32(
                dataHex.Substring(6, 2),
                16);

        var minute =
            Convert.ToInt32(
                dataHex.Substring(8, 2),
                16);

        var second =
            Convert.ToInt32(
                dataHex.Substring(10, 2),
                16);

        var focus =
            Convert.ToInt32(
                dataHex.Substring(12, 2),
                16);

        var peopleIn =
            ParseLittleEndianUInt32(
                dataHex.Substring(14, 8));

        var peopleOut =
            ParseLittleEndianUInt32(
                dataHex.Substring(22, 8));

        return new Hp015ParsedRecord
        {
            TimestampUtc =
                new DateTime(
                    year,
                    month,
                    day,
                    hour,
                    minute,
                    second,
                    DateTimeKind.Utc),

            Focus =
                focus == 1,

            PeopleIn =
                peopleIn,

            PeopleOut =
                peopleOut,

            RawData =
                dataHex
        };
    }

    private static int ParseLittleEndianUInt32(
        string hex)
    {
        var bytes =
            Convert.FromHexString(hex);

        return bytes[0]
            | (bytes[1] << 8)
            | (bytes[2] << 16)
            | (bytes[3] << 24);
    }
}