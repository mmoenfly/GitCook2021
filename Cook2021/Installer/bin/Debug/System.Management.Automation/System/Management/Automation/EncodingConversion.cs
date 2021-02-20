// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.EncodingConversion
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;
using System.Text;

namespace System.Management.Automation
{
  internal static class EncodingConversion
  {
    internal const string Unicode = "unicode";
    internal const string BigEndianUnicode = "bigendianunicode";
    internal const string Ascii = "ascii";
    internal const string Utf8 = "utf8";
    internal const string Utf7 = "utf7";
    internal const string Utf32 = "utf32";
    internal const string Default = "default";
    internal const string OEM = "oem";
    [TraceSource("EncodingConversion", "EncodingConversion")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (EncodingConversion), nameof (EncodingConversion));

    internal static Encoding Convert(Cmdlet cmdlet, string encoding)
    {
      switch (encoding)
      {
        case "":
        case null:
          return Encoding.Unicode;
        default:
          if (string.Equals(encoding, "unicode", StringComparison.OrdinalIgnoreCase))
            return Encoding.Unicode;
          if (string.Equals(encoding, "bigendianunicode", StringComparison.OrdinalIgnoreCase))
            return Encoding.BigEndianUnicode;
          if (string.Equals(encoding, "ascii", StringComparison.OrdinalIgnoreCase))
            return Encoding.ASCII;
          if (string.Equals(encoding, "utf8", StringComparison.OrdinalIgnoreCase))
            return Encoding.UTF8;
          if (string.Equals(encoding, "utf7", StringComparison.OrdinalIgnoreCase))
            return Encoding.UTF7;
          if (string.Equals(encoding, "utf32", StringComparison.OrdinalIgnoreCase))
            return Encoding.UTF32;
          if (string.Equals(encoding, "default", StringComparison.OrdinalIgnoreCase))
            return Encoding.Default;
          if (string.Equals(encoding, "oem", StringComparison.OrdinalIgnoreCase))
            return Encoding.GetEncoding((int) EncodingConversion.NativeMethods.GetOEMCP());
          string str = string.Join(", ", new string[8]
          {
            "unicode",
            "bigendianunicode",
            "ascii",
            "utf8",
            "utf7",
            "utf32",
            "default",
            "oem"
          });
          string message = ResourceManagerCache.FormatResourceString("PathUtils", "OutFile_WriteToFileEncodingUnknown", (object) encoding, (object) str);
          cmdlet.ThrowTerminatingError(new ErrorRecord((Exception) EncodingConversion.tracer.NewArgumentException("Encoding"), "WriteToFileEncodingUnknown", ErrorCategory.InvalidArgument, (object) null)
          {
            ErrorDetails = new ErrorDetails(message)
          });
          return (Encoding) null;
      }
    }

    private static class NativeMethods
    {
      [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
      internal static extern uint GetOEMCP();
    }
  }
}
