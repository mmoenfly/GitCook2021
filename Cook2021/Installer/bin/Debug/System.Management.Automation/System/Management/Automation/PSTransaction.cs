// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSTransaction
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Transactions;

namespace System.Management.Automation
{
  public sealed class PSTransaction : IDisposable
  {
    private CommittableTransaction transaction;
    private RollbackSeverity rollbackPreference;
    private int subscriberCount;
    private bool isRolledBack;
    private bool isCommitted;

    internal PSTransaction(RollbackSeverity rollbackPreference, TimeSpan timeout)
    {
      this.transaction = new CommittableTransaction(timeout);
      this.rollbackPreference = rollbackPreference;
      this.subscriberCount = 1;
    }

    internal PSTransaction(CommittableTransaction transaction, RollbackSeverity severity)
    {
      this.transaction = transaction;
      this.rollbackPreference = severity;
      this.subscriberCount = 1;
    }

    public RollbackSeverity RollbackPreference => this.rollbackPreference;

    public int SubscriberCount
    {
      get
      {
        if (this.IsRolledBack)
          this.SubscriberCount = 0;
        return this.subscriberCount;
      }
      set => this.subscriberCount = value;
    }

    public PSTransactionStatus Status
    {
      get
      {
        if (this.IsRolledBack)
          return PSTransactionStatus.RolledBack;
        return this.IsCommitted ? PSTransactionStatus.Committed : PSTransactionStatus.Active;
      }
    }

    internal void Activate() => Transaction.Current = (Transaction) this.transaction;

    internal void Commit()
    {
      this.transaction.Commit();
      this.isCommitted = true;
    }

    internal void Rollback()
    {
      this.transaction.Rollback();
      this.isRolledBack = true;
    }

    internal bool IsRolledBack
    {
      get
      {
        if (!this.isRolledBack && (Transaction) this.transaction != (Transaction) null && this.transaction.TransactionInformation.Status == TransactionStatus.Aborted)
          this.isRolledBack = true;
        return this.isRolledBack;
      }
      set => this.isRolledBack = value;
    }

    internal bool IsCommitted
    {
      get => this.isCommitted;
      set => this.isCommitted = value;
    }

    ~PSTransaction() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public void Dispose(bool disposing)
    {
      if (!disposing || !((Transaction) this.transaction != (Transaction) null))
        return;
      this.transaction.Dispose();
    }
  }
}
