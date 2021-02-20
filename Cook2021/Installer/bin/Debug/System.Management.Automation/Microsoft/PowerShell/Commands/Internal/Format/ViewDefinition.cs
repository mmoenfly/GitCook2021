﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.ViewDefinition
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class ViewDefinition
  {
    internal DatabaseLoadingInfo loadingInfo;
    internal string name;
    internal AppliesTo appliesTo = new AppliesTo();
    internal GroupBy groupBy;
    internal FormatControlDefinitionHolder formatControlDefinitionHolder = new FormatControlDefinitionHolder();
    internal ControlBase mainControl;
    internal bool outOfBand;
    private Guid _instanceId;

    internal Guid InstanceId => this._instanceId;

    internal ViewDefinition() => this._instanceId = Guid.NewGuid();
  }
}
