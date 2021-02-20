// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.HostUIHelperMethods
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace System.Management.Automation.Host
{
  internal static class HostUIHelperMethods
  {
    [TraceSource("PSHostUserInterface", "S.M.A.PSHostUserInterface")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSHostUserInterface", "S.M.A.PSHostUserInterface");

    internal static void BuildHotkeysAndPlainLabels(
      Collection<ChoiceDescription> choices,
      out string[,] hotkeysAndPlainLabels)
    {
      hotkeysAndPlainLabels = new string[2, choices.Count];
      for (int index = 0; index < choices.Count; ++index)
      {
        hotkeysAndPlainLabels[0, index] = string.Empty;
        int length = choices[index].Label.IndexOf('&');
        if (length >= 0)
        {
          StringBuilder stringBuilder = new StringBuilder(choices[index].Label.Substring(0, length), choices[index].Label.Length);
          if (length + 1 < choices[index].Label.Length)
          {
            stringBuilder.Append(choices[index].Label.Substring(length + 1));
            hotkeysAndPlainLabels[0, index] = choices[index].Label.Substring(length + 1, 1).Trim().ToUpper(CultureInfo.CurrentCulture);
          }
          hotkeysAndPlainLabels[1, index] = stringBuilder.ToString().Trim();
        }
        else
          hotkeysAndPlainLabels[1, index] = choices[index].Label;
        if (string.Compare(hotkeysAndPlainLabels[0, index], "?", StringComparison.Ordinal) == 0)
        {
          Exception exceptionRecord = (Exception) HostUIHelperMethods.tracer.NewArgumentException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "choices[{0}].Label", (object) index), "InternalHostUserInterfaceStrings", "InvalidChoiceHotKeyError");
          HostUIHelperMethods.tracer.TraceException(exceptionRecord);
          throw exceptionRecord;
        }
      }
    }

    internal static int DetermineChoicePicked(
      string response,
      Collection<ChoiceDescription> choices,
      string[,] hotkeysAndPlainLabels)
    {
      int num = -1;
      CultureInfo currentCulture = CultureInfo.CurrentCulture;
      for (int index = 0; index < choices.Count; ++index)
      {
        if (string.Compare(response, hotkeysAndPlainLabels[1, index], true, currentCulture) == 0)
        {
          num = index;
          break;
        }
      }
      if (num == -1)
      {
        for (int index = 0; index < choices.Count; ++index)
        {
          if (hotkeysAndPlainLabels[0, index].Length > 0 && string.Compare(response, hotkeysAndPlainLabels[0, index], true, currentCulture) == 0)
          {
            num = index;
            break;
          }
        }
      }
      return num;
    }
  }
}
