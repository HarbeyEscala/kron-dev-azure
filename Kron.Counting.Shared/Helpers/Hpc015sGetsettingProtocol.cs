using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kron.Counting.Shared.Helpers;

/// <summary>
/// Protocol helpers for HPC015S getsetting (CRC16, hex). Aligned with NET/jron-csharp Server/HpcProtocol.cs.
/// </summary>
public static class Hpc015sGetsettingProtocol
{
        /// <summary>Getsetting request: minimum 51 body bytes + 2 CRC = 53 bytes = 106 hex chars.</summary>
        public const int GetsettingMinBodyBytes = 51;

        public const int GetsettingMinHexChars = 106;

        /// <summary>Getsetting response body length before CRC (56 bytes) + 2 CRC = 58 bytes total.</summary>
        public const int GetsettingResponseBodyBytes = 56;

        public const int GetsettingResponseTotalBytes = 58;

        /// <summary>
        /// CRC16 Modbus (polynomial 0xA001, init 0xFFFF). Returns [Lo, Hi] byte order.
        /// </summary>
        public static byte[] Crc16Modbus(byte[] buffer, int offset, int length)
        {
            ushort crc = 0xFFFF;
            int end = offset + length;
            for (int j = offset; j < end; j++)
            {
                crc ^= buffer[j];
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x01) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            byte lo = (byte)(crc & 0xFF);
            byte hi = (byte)((crc >> 8) & 0xFF);
            return new[] { lo, hi };
        }

        /// <summary>Append CRC as [Hi, Lo] after body (per HPC015S result= packet).</summary>
        public static byte[] Crc16ForResultPacket(byte[] body)
        {
            byte[] crcLoHi = Crc16Modbus(body, 0, body.Length);
            return new[] { crcLoHi[1], crcLoHi[0] };
        }

        public static byte[] HexToBytes(string hexStr)
        {
            if (string.IsNullOrWhiteSpace(hexStr)) return null;
            string raw = string.Concat(hexStr.Where(c => !char.IsWhiteSpace(c)));
            if (raw.Length % 2 != 0) return null;
            try
            {
                byte[] buf = new byte[raw.Length / 2];
                for (int i = 0; i < buf.Length; i++)
                    buf[i] = Convert.ToByte(raw.Substring(i * 2, 2), 16);
                return buf;
            }
            catch
            {
                return null;
            }
        }

        public static string BytesToHex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "";
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }

/// <summary>
/// Parsed getsetting request (data= hex). Same layout as jron-csharp GetsettingRequest.
/// </summary>
public sealed class Hpc015sGetsettingRequest
{
        public bool Ok { get; private set; }
        public string RawDataHex { get; private set; }
        public string Sn { get; private set; }
        public byte[] SnRaw { get; private set; }
        public byte[] MacRaw { get; private set; }
        public byte CommandType { get; private set; }
        public byte Speed { get; private set; }
        public byte RecordingCycle { get; private set; }
        public byte UploadCycle { get; private set; }
        public byte FixedTime { get; private set; }
        public byte[] UploadHours { get; private set; }
        public byte[] UploadMinutes { get; private set; }
        public byte Model { get; private set; }
        public byte DisableType { get; private set; }
        public byte Year { get; private set; }
        public byte Month { get; private set; }
        public byte Day { get; private set; }
        public byte Hour { get; private set; }
        public byte Minute { get; private set; }
        public byte Second { get; private set; }
        public byte Week { get; private set; }
        public byte OpenHour { get; private set; }
        public byte OpenMinute { get; private set; }
        public byte CloseHour { get; private set; }
        public byte CloseMinute { get; private set; }
        public bool CrcOk { get; private set; }

        private Hpc015sGetsettingRequest() { }

        /// <summary>
        /// Parses the hex string from data[0] on cmd=getsetting. Returns null if invalid or too short.
        /// </summary>
        public static Hpc015sGetsettingRequest Parse(string hexStr)
        {
            if (string.IsNullOrWhiteSpace(hexStr)) return null;
            string raw = string.Concat(hexStr.Where(c => !char.IsWhiteSpace(c)));
            if (raw.Length < Hpc015sGetsettingProtocol.GetsettingMinHexChars) return null;
            byte[] buf = Hpc015sGetsettingProtocol.HexToBytes(raw);
            if (buf == null) return null;
            int dataLen = buf.Length;
            int bodyLen = dataLen - 2;
            if (bodyLen < Hpc015sGetsettingProtocol.GetsettingMinBodyBytes) return null;

            byte[] crcComputed = Hpc015sGetsettingProtocol.Crc16Modbus(buf, 0, dataLen - 2);
            byte[] crcHiLo = new[] { crcComputed[1], crcComputed[0] };
            bool crcOk = (buf[dataLen - 2] == crcHiLo[0] && buf[dataLen - 1] == crcHiLo[1])
                      || (buf[dataLen - 2] == crcComputed[0] && buf[dataLen - 1] == crcComputed[1]);

            string sn = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", buf[3], buf[2], buf[1], buf[0]);
            byte[] snRaw = new byte[4];
            Array.Copy(buf, 0, snRaw, 0, 4);
            byte[] macRaw = bodyLen >= 40 ? CopySlice(buf, 19, 21) : null;

            byte[] uploadHours = dataLen >= 16
                ? new[] { buf[9], buf[11], buf[13], buf[15] }
                : new byte[4];
            byte[] uploadMinutes = dataLen >= 16
                ? new[] { buf[10], buf[12], buf[14], buf[16] }
                : new byte[4];

            return new Hpc015sGetsettingRequest
            {
                Ok = true,
                RawDataHex = raw.ToUpperInvariant(),
                Sn = sn,
                SnRaw = snRaw,
                MacRaw = macRaw,
                CommandType = buf[4],
                Speed = buf[5],
                RecordingCycle = buf[6],
                UploadCycle = buf[7],
                FixedTime = bodyLen > 8 ? buf[8] : (byte)0,
                UploadHours = uploadHours,
                UploadMinutes = uploadMinutes,
                Model = buf[17],
                DisableType = buf[18],
                Year = bodyLen > 40 ? buf[40] : (byte)0,
                Month = bodyLen > 41 ? buf[41] : (byte)0,
                Day = bodyLen > 42 ? buf[42] : (byte)0,
                Hour = bodyLen > 43 ? buf[43] : (byte)0,
                Minute = bodyLen > 44 ? buf[44] : (byte)0,
                Second = bodyLen > 45 ? buf[45] : (byte)0,
                Week = bodyLen > 46 ? buf[46] : (byte)0,
                OpenHour = buf[47],
                OpenMinute = buf[48],
                CloseHour = buf[49],
                CloseMinute = buf[50],
                CrcOk = crcOk
            };
        }

        private static byte[] CopySlice(byte[] src, int offset, int len)
        {
            byte[] r = new byte[len];
            Array.Copy(src, offset, r, 0, len);
            return r;
        }
    }

/// <summary>
/// Builds getsetting result= hex (56 body + 2 CRC). Same as jron-csharp GetsettingResponse.
/// </summary>
    public sealed class Hpc015sGetsettingResponse
    {
        public byte RespondingType { get; set; } = 0x04;
        public byte CommandType { get; set; } = 0x03;
        public byte Speed { get; set; } = 0x00;
        public byte RecordingCycle { get; set; } = 0x01; // ciclo de envio
        public byte UploadCycle { get; set; } = 0x01;  // envios al backend
        public byte FixedTime { get; set; } = 0x00;
        public byte[] UploadHours { get; set; } = new byte[4];
        public byte[] UploadMinutes { get; set; } = new byte[4];
        public byte Model { get; set; } = 0x00;
        public byte DisableType { get; set; } = 0x02;
        public byte Year { get; set; } = 0x0F;
        public byte Month { get; set; } = 0x01;
        public byte Day { get; set; } = 0x02;
        public byte Hour { get; set; } = 0x14;
        public byte Minute { get; set; } = 0x13;
        public byte Second { get; set; } = 0x1D;
        public byte Week { get; set; } = 0x00;
        public byte OpenHour { get; set; } = 0x00;
        public byte OpenMinute { get; set; } = 0x00;
        public byte CloseHour { get; set; } = 0x14;
        public byte CloseMinute { get; set; } = 0x35;
        public byte Reserved1 { get; set; } = 0x00;
        public byte Reserved2 { get; set; } = 0x00;

        private readonly byte[] _flagSwapped;
        private readonly byte[] _snRaw;

        public Hpc015sGetsettingResponse(string flagHex, byte[] snRaw)
        {
            string normalized = string.Concat((flagHex ?? "0000").Where(c => !char.IsWhiteSpace(c)).Take(4));
            if (normalized.Length < 4) normalized = normalized.PadLeft(4, '0');
            byte[] flagBuf = Hpc015sGetsettingProtocol.HexToBytes(normalized);
            _flagSwapped = (flagBuf != null && flagBuf.Length >= 2)
                ? new[] { flagBuf[1], flagBuf[0] }
                : new byte[] { 0, 0 };
            _snRaw = (snRaw != null && snRaw.Length == 4) ? snRaw : new byte[4];
        }

        public string Build()
        {
            byte[] body = new byte[Hpc015sGetsettingProtocol.GetsettingResponseBodyBytes];
            int off = 0;

            body[off++] = RespondingType;
            body[off++] = _flagSwapped[0];
            body[off++] = _flagSwapped[1];
            _snRaw.CopyTo(body, off);
            off += 4;

            body[off++] = CommandType;
            body[off++] = Speed;
            body[off++] = RecordingCycle;
            body[off++] = UploadCycle;
            body[off++] = FixedTime;

            for (int i = 0; i < 4; i++)
            {
                body[off++] = i < UploadHours.Length ? UploadHours[i] : (byte)0;
                body[off++] = i < UploadMinutes.Length ? UploadMinutes[i] : (byte)0;
            }

            body[off++] = Model;
            body[off++] = DisableType;

            off += 21;

            body[off++] = Year;
            body[off++] = Month;
            body[off++] = Day;
            body[off++] = Hour;
            body[off++] = Minute;
            body[off++] = Second;
            body[off++] = Week;

            body[off++] = OpenHour;
            body[off++] = OpenMinute;
            body[off++] = CloseHour;
            body[off++] = CloseMinute;

            body[off++] = Reserved1;
            body[off++] = Reserved2;

            if (off != Hpc015sGetsettingProtocol.GetsettingResponseBodyBytes)
                throw new InvalidOperationException("Getsetting response body size mismatch.");

            byte[] crc = Hpc015sGetsettingProtocol.Crc16ForResultPacket(body);
            byte[] packet = new byte[Hpc015sGetsettingProtocol.GetsettingResponseTotalBytes];
            Array.Copy(body, 0, packet, 0, body.Length);
            Array.Copy(crc, 0, packet, body.Length, crc.Length);
            return Hpc015sGetsettingProtocol.BytesToHex(packet);
        }
    }

/// <summary>
/// First getsetting → 0x04; same SN + same data hex within 10s → 0x05 (jron-csharp GetsettingSessionManager).
/// </summary>
    public sealed class Hpc015sGetsettingSessionManager
    {
        private struct SessionEntry
        {
            public DateTime Timestamp;
            public string DataHex;
        }

        private readonly Dictionary<string, SessionEntry> _sessions = new Dictionary<string, SessionEntry>(StringComparer.OrdinalIgnoreCase);
        private const int ConfirmWindowMs = 10000;

        public bool ShouldConfirm(string sn, string dataHex)
        {
            if (string.IsNullOrEmpty(sn)) return false;

            if (_sessions.TryGetValue(sn, out SessionEntry entry))
            {
                double elapsed = (DateTime.Now - entry.Timestamp).TotalMilliseconds;
                if (elapsed < ConfirmWindowMs && string.Equals(entry.DataHex, dataHex, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            _sessions[sn] = new SessionEntry { Timestamp = DateTime.Now, DataHex = dataHex ?? "" };
            return false;
        }
    }

    /// <summary>
    /// Formato de trazas tipo jron-csharp (Storage/LogAppender) para people-count.log.
    /// </summary>
    internal static class Hpc015sPeopleCountLogFormatter
    {
        /// <summary>
        /// Añade líneas PROCESADO / GETSETTING REQUEST / FIRST|CONFIRM / RESPONSE / desglose paquete.
        /// </summary>
        public static void AppendGetsettingExchange(StringBuilder sb, string logTs, Hpc015sGetsettingRequest parsed, byte respondingType, string resultHex)
        {
            if (parsed != null && parsed.Ok)
            {
                sb.AppendLine(string.Format("[{0}] PROCESADO getsetting SN={1} rec={2}min up={3}min CRC={4}",
                    logTs, parsed.Sn, parsed.RecordingCycle, parsed.UploadCycle, parsed.CrcOk ? "OK" : "FAIL"));
                sb.AppendLine(string.Format("[{0}] GETSETTING REQUEST campos: {1}", logTs, FormatGetsettingRequestFields(parsed)));
            }
            else
            {
                sb.AppendLine(string.Format("[{0}] PROCESADO getsetting (parse failed)", logTs));
            }

            bool isConfirm = respondingType == 0x05;
            sb.AppendLine(string.Format("[{0}] getsetting {1} => RespondingType=0x{2:X2}",
                logTs,
                isConfirm ? "CONFIRM (eco 0x05)" : "FIRST (eco 0x04)",
                respondingType));

            if (parsed != null && parsed.Ok && !string.IsNullOrEmpty(resultHex))
            {
                sb.AppendLine(string.Format("[{0}] GETSETTING RESPONSE campos: {1}", logTs, FormatGetsettingResponseFieldsLine(resultHex, parsed)));
                AppendGetsettingPacketDump(sb, resultHex);
            }
        }

        private static string FormatGetsettingRequestFields(Hpc015sGetsettingRequest p)
        {
            string time = string.Format("{0}/{1}/{2} {3}:{4}:{5}", p.Year, p.Month, p.Day, p.Hour, p.Minute, p.Second);
            string open = string.Format("{0}:{1}", p.OpenHour, p.OpenMinute);
            string close = string.Format("{0}:{1}", p.CloseHour, p.CloseMinute);
            return string.Join(" ",
                "SN=" + p.Sn,
                "commandType=" + p.CommandType,
                "speed=" + p.Speed,
                "recordingCycle=" + p.RecordingCycle,
                "uploadCycle=" + p.UploadCycle,
                "fixedTime=" + p.FixedTime,
                "model=" + p.Model,
                "disableType=" + p.DisableType,
                "time=" + time,
                "week=" + p.Week,
                "open=" + open,
                "close=" + close);
        }

        /// <summary>
        /// Interpreta el paquete result= (58 bytes) y genera una línea legible como jron FormatGetsettingResponseFields.
        /// </summary>
        private static string FormatGetsettingResponseFieldsLine(string resultHex, Hpc015sGetsettingRequest req)
        {
            byte[] packet = Hpc015sGetsettingProtocol.HexToBytes(resultHex);
            if (packet == null || packet.Length < 56)
                return "(invalid response hex)";

            byte[] b = packet;
            string time = string.Format("{0}/{1}/{2} {3}:{4}:{5}", b[43], b[44], b[45], b[46], b[47], b[48]);
            return string.Join(" ",
                string.Format("respondingType=0x{0:X2}", b[0]),
                "SN=" + req.Sn,
                "commandType=" + b[7],
                "speed=" + b[8],
                "recordingCycle=" + b[9],
                "uploadCycle=" + b[10],
                "fixedTime=" + b[11],
                "model=" + b[20],
                "disableType=" + b[21],
                "time=" + time,
                "week=" + b[49],
                "open=" + b[50] + ":" + b[51],
                "close=" + b[52] + ":" + b[53]);
        }

        /// <summary>
        /// Desglose offset/campo como LogAppender.LogGetsettingPacket en jron-csharp.
        /// </summary>
        private static void AppendGetsettingPacketDump(StringBuilder sb, string hexStr)
        {
            byte[] buf = Hpc015sGetsettingProtocol.HexToBytes(hexStr);
            if (buf == null) return;

            sb.AppendLine(string.Format("  [PACKET RESPONSE] total={0} bytes", buf.Length));
            sb.AppendLine("  result=" + hexStr);

            string Hx(byte x) { return x.ToString("X2"); }
            string HxArr(byte[] arr) { return string.Join(" ", Array.ConvertAll(arr, Hx)); }

            var fields = new[]
            {
                Tuple.Create(0, 1, "RespondingType"),
                Tuple.Create(1, 2, "Flag"),
                Tuple.Create(3, 4, "SN"),
                Tuple.Create(7, 1, "commandType"),
                Tuple.Create(8, 1, "speed"),
                Tuple.Create(9, 1, "recordingCycle"),
                Tuple.Create(10, 1, "uploadCycle"),
                Tuple.Create(11, 1, "fixedTime"),
                Tuple.Create(12, 2, "upload H1/M1"),
                Tuple.Create(14, 2, "upload H2/M2"),
                Tuple.Create(16, 2, "upload H3/M3"),
                Tuple.Create(18, 2, "upload H4/M4"),
                Tuple.Create(20, 1, "model"),
                Tuple.Create(21, 1, "disableType"),
                Tuple.Create(22, 7, "MAC1"),
                Tuple.Create(29, 7, "MAC2"),
                Tuple.Create(36, 7, "MAC3"),
                Tuple.Create(43, 1, "year"),
                Tuple.Create(44, 1, "month"),
                Tuple.Create(45, 1, "day"),
                Tuple.Create(46, 1, "hour"),
                Tuple.Create(47, 1, "minute"),
                Tuple.Create(48, 1, "second"),
                Tuple.Create(49, 1, "week"),
                Tuple.Create(50, 1, "openHour"),
                Tuple.Create(51, 1, "openMinute"),
                Tuple.Create(52, 1, "closeHour"),
                Tuple.Create(53, 1, "closeMinute"),
                Tuple.Create(54, 1, "reserved1"),
                Tuple.Create(55, 1, "reserved2"),
                Tuple.Create(56, 2, "CRC16"),
            };

            foreach (var t in fields)
            {
                int off = t.Item1;
                int len = t.Item2;
                string name = t.Item3;
                if (off + len > buf.Length) continue;
                byte[] slice = new byte[len];
                Array.Copy(buf, off, slice, 0, len);
                string hexVal = HxArr(slice);
                string dec;
                if (len == 1) dec = slice[0].ToString().PadLeft(5);
                else if (len == 2) dec = ((slice[0] << 8) | slice[1]).ToString().PadLeft(5);
                else dec = "     ";
                sb.AppendLine(string.Format("    off={0,2} len={1} {2,-14} dec={3}  hex={4}", off, len, name, dec, hexVal));
            }
        }
    }