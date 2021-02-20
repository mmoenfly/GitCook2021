// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.StringUtil
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;
using System.Threading;

namespace System.Management.Automation.Internal
{
  internal static class StringUtil
  {
    internal static string Format(string formatSpec, object o) => string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, formatSpec, o);

    internal static string Format(string formatSpec, object o1, object o2) => string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, formatSpec, o1, o2);

    internal static string Format(string formatSpec, params object[] o) => string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, formatSpec, o);

    internal static string TruncateToBufferCellWidth(
      PSHostRawUserInterface rawUI,
      string toTruncate,
      int maxWidthInBufferCells)
    {
      int length = Math.Min(toTruncate.Length, maxWidthInBufferCells);
      string source;
      while (true)
      {
        source = toTruncate.Substring(0, length);
        if (rawUI.LengthInBufferCells(source) > maxWidthInBufferCells)
          --length;
        else
          break;
      }
      return source;
    }
  }
}
