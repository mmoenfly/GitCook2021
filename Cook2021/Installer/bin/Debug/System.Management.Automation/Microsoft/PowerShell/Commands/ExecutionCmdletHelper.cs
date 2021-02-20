// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ExecutionCmdletHelper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  internal abstract class ExecutionCmdletHelper : IThrottleOperation
  {
    protected Pipeline pipeline;
    protected Exception internalException;

    internal Pipeline Pipeline => this.pipeline;

    internal Exception InternalException => this.internalException;
  }
}
