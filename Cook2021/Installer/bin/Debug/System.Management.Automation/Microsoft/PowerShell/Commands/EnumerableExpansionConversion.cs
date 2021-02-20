// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.EnumerableExpansionConversion
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System;

namespace Microsoft.PowerShell.Commands
{
  internal static class EnumerableExpansionConversion
  {
    internal const string CoreOnlyString = "CoreOnly";
    internal const string EnumOnlyString = "EnumOnly";
    internal const string BothString = "Both";

    internal static bool Convert(string expansionString, out EnumerableExpansion expansion)
    {
      expansion = EnumerableExpansion.EnumOnly;
      if (string.Equals(expansionString, "CoreOnly", StringComparison.OrdinalIgnoreCase))
      {
        expansion = EnumerableExpansion.CoreOnly;
        return true;
      }
      if (string.Equals(expansionString, "EnumOnly", StringComparison.OrdinalIgnoreCase))
      {
        expansion = EnumerableExpansion.EnumOnly;
        return true;
      }
      if (!string.Equals(expansionString, "Both", StringComparison.OrdinalIgnoreCase))
        return false;
      expansion = EnumerableExpansion.Both;
      return true;
    }
  }
}
