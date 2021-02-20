// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ConsoleCmdletsBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  public abstract class ConsoleCmdletsBase : PSCmdlet
  {
    internal const string resBaseName = "ConsoleInfoErrorStrings";
    [TraceSource("ConsoleCmdlet", "Console related cmdlet")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("ConsoleCmdlet", "Console related cmdlet");

    internal RunspaceConfigForSingleShell Runspace
    {
      get
      {
        if (!(this.Context.RunspaceConfiguration is RunspaceConfigForSingleShell runspaceConfiguration))
          throw ConsoleCmdletsBase.tracer.NewInvalidOperationException("ConsoleInfoErrorStrings", "CmdletNotAvailable", (object) "");
        return runspaceConfiguration;
      }
    }

    internal void ThrowError(
      object targetObject,
      string errorId,
      Exception innerException,
      ErrorCategory category)
    {
      using (ConsoleCmdletsBase.tracer.TraceMethod())
        this.ThrowTerminatingError(new ErrorRecord(innerException, errorId, category, targetObject));
    }
  }
}
