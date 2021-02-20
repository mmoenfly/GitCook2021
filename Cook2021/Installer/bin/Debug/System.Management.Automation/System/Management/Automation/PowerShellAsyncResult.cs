// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PowerShellAsyncResult
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal sealed class PowerShellAsyncResult : AsyncResult
  {
    private bool isAssociatedWithAsyncInvoke;
    private PSDataCollection<PSObject> output;

    internal bool IsAssociatedWithAsyncInvoke => this.isAssociatedWithAsyncInvoke;

    internal PSDataCollection<PSObject> Output => this.output;

    internal PowerShellAsyncResult(
      Guid ownerId,
      AsyncCallback callback,
      object state,
      PSDataCollection<PSObject> output,
      bool isCalledFromBeginInvoke)
      : base(ownerId, callback, state)
    {
      this.isAssociatedWithAsyncInvoke = isCalledFromBeginInvoke;
      this.output = output;
    }
  }
}
