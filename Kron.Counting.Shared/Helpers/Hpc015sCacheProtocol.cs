namespace Kron.Counting.Shared.Helpers;

public static class Hpc015sCacheProtocol
{
    public static string BuildCacheSuccessResponseHex(
        string flagHex)
    {
        string normalized =
            string.Concat(
                (flagHex ?? "0000")
                    .Where(c => !char.IsWhiteSpace(c))
                    .Take(4));

        if (normalized.Length < 4)
        {
            normalized =
                normalized.PadLeft(4, '0');
        }

        byte b0 =
            Convert.ToByte(
                normalized.Substring(0, 2),
                16);

        byte b1 =
            Convert.ToByte(
                normalized.Substring(2, 2),
                16);

        byte[] swapped =
        [
            b1,
            b0
        ];

        var now = DateTime.Now;

        byte[] body = new byte[15];

        int off = 0;

        body[off++] = 0x01;

        body[off++] = swapped[0];
        body[off++] = swapped[1];

        body[off++] = 0x01;

        body[off++] = (byte)(now.Year % 100);
        body[off++] = (byte)now.Month;
        body[off++] = (byte)now.Day;

        body[off++] = (byte)now.Hour;
        body[off++] = (byte)now.Minute;
        body[off++] = (byte)now.Second;

        body[off++] = 0;
        body[off++] = 0;
        body[off++] = 0;

        body[off++] = 23;
        body[off++] = 59;

        byte[] crc =
            Hpc015sGetsettingProtocol
                .Crc16ForResultPacket(
                    body);

        byte[] packet = new byte[17];

        Array.Copy(
            body,
            0,
            packet,
            0,
            body.Length);

        Array.Copy(
            crc,
            0,
            packet,
            body.Length,
            crc.Length);

        return Hpc015sGetsettingProtocol
            .BytesToHex(packet);
    }
}