// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSTransactionContext
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  public sealed class PSTransactionContext : IDisposable
  {
    private PSTransactionManager transactionManager;

    internal PSTransactionContext(PSTransactionManager transactionManager)
    {
      this.transactionManager = transactionManager;
      transactionManager.SetActive();
    }

    ~PSTransactionContext() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this.transactionManager.ResetActive();
    }
  }
}
