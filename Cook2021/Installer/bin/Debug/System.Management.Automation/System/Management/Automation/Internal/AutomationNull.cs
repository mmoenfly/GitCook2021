﻿// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.AutomationNull
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  public static class AutomationNull
  {
    private static readonly PSObject value1 = new PSObject();

    public static PSObject Value => AutomationNull.value1;
  }
}
