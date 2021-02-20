// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.FormatTable
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections.Generic;
using System.Management.Automation.Host;

namespace System.Management.Automation.Runspaces
{
  public sealed class FormatTable
  {
    [TraceSource("FormatTable", "FormatTable")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (FormatTable), nameof (FormatTable));
    private TypeInfoDataBaseManager formatDBMgr;

    internal FormatTable() => this.formatDBMgr = new TypeInfoDataBaseManager();

    public FormatTable(IEnumerable<string> formatFiles)
      : this(formatFiles, (AuthorizationManager) null, (PSHost) null)
    {
    }

    internal FormatTable(
      IEnumerable<string> formatFiles,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      if (formatFiles == null)
        throw FormatTable.tracer.NewArgumentNullException(nameof (formatFiles));
      this.formatDBMgr = new TypeInfoDataBaseManager(formatFiles, true, authorizationManager, host);
    }

    internal TypeInfoDataBaseManager FormatDBManager => this.formatDBMgr;

    internal void Add(string formatFile, bool shouldPrepend) => this.formatDBMgr.Add(formatFile, shouldPrepend);

    internal void Remove(string formatFile) => this.formatDBMgr.Remove(formatFile);
  }
}
