// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSTransactionManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Transactions;

namespace System.Management.Automation.Internal
{
  internal sealed class PSTransactionManager : IDisposable
  {
    private const string transactionStrings = "TransactionStrings";
    [TraceSource("PSTransaction", "PSTransaction")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSTransaction", "PSTransaction");
    private static bool engineProtectionEnabled = false;
    private Stack<PSTransaction> transactionStack;
    private PSTransaction baseTransaction;
    private Transaction previousActiveTransaction;

    internal PSTransactionManager()
    {
      this.transactionStack = new Stack<PSTransaction>();
      this.transactionStack.Push((PSTransaction) null);
    }

    internal static IDisposable GetEngineProtectionScope() => PSTransactionManager.engineProtectionEnabled && Transaction.Current != (Transaction) null ? (IDisposable) new TransactionScope(TransactionScopeOption.Suppress) : (IDisposable) null;

    internal static void EnableEngineProtection() => PSTransactionManager.engineProtectionEnabled = true;

    internal RollbackSeverity RollbackPreference => (this.transactionStack.Peek() ?? throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionActive"))).RollbackPreference;

    internal void CreateOrJoin() => this.CreateOrJoin(RollbackSeverity.Error, TimeSpan.FromMinutes(1.0));

    internal void CreateOrJoin(RollbackSeverity rollbackPreference, TimeSpan timeout)
    {
      PSTransaction psTransaction = this.transactionStack.Peek();
      if (psTransaction != null)
      {
        if (psTransaction.IsRolledBack || psTransaction.IsCommitted)
        {
          this.transactionStack.Pop().Dispose();
          this.transactionStack.Push(new PSTransaction(rollbackPreference, timeout));
        }
        else
          ++psTransaction.SubscriberCount;
      }
      else
        this.transactionStack.Push(new PSTransaction(rollbackPreference, timeout));
    }

    internal void CreateNew() => this.CreateNew(RollbackSeverity.Error, TimeSpan.FromMinutes(1.0));

    internal void CreateNew(RollbackSeverity rollbackPreference, TimeSpan timeout) => this.transactionStack.Push(new PSTransaction(rollbackPreference, timeout));

    internal void Commit()
    {
      PSTransaction psTransaction = this.transactionStack.Peek();
      if (psTransaction == null)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionActiveForCommit"));
      if (psTransaction.IsRolledBack)
        throw new TransactionAbortedException(ResourceManagerCache.GetResourceString("TransactionStrings", "TransactionRolledBackForCommit"));
      if (psTransaction.IsCommitted)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "CommittedTransactionForCommit"));
      if (psTransaction.SubscriberCount == 1)
      {
        psTransaction.Commit();
        psTransaction.SubscriberCount = 0;
      }
      else
        --psTransaction.SubscriberCount;
      while (this.transactionStack.Count > 2 && (this.transactionStack.Peek().IsRolledBack || this.transactionStack.Peek().IsCommitted))
        this.transactionStack.Pop().Dispose();
    }

    internal void Rollback() => this.Rollback(false);

    internal void Rollback(bool suppressErrors)
    {
      PSTransaction psTransaction = this.transactionStack.Peek();
      if (psTransaction == null)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionActiveForRollback"));
      if (psTransaction.IsRolledBack && !suppressErrors)
        throw new TransactionAbortedException(ResourceManagerCache.GetResourceString("TransactionStrings", "TransactionRolledBackForRollback"));
      if (psTransaction.IsCommitted && !suppressErrors)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "CommittedTransactionForRollback"));
      psTransaction.SubscriberCount = 0;
      psTransaction.Rollback();
      while (this.transactionStack.Count > 2 && (this.transactionStack.Peek().IsRolledBack || this.transactionStack.Peek().IsCommitted))
        this.transactionStack.Pop().Dispose();
    }

    internal void SetBaseTransaction(CommittableTransaction transaction, RollbackSeverity severity)
    {
      if (this.HasTransaction)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "BaseTransactionMustBeFirst"));
      this.transactionStack.Peek();
      while (this.transactionStack.Peek() != null && (this.transactionStack.Peek().IsRolledBack || this.transactionStack.Peek().IsCommitted))
        this.transactionStack.Pop().Dispose();
      this.baseTransaction = new PSTransaction(transaction, severity);
      this.transactionStack.Push(this.baseTransaction);
    }

    internal void ClearBaseTransaction()
    {
      if (this.baseTransaction == null)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "BaseTransactionNotSet"));
      if (this.transactionStack.Peek() != this.baseTransaction)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "BaseTransactionNotActive"));
      this.transactionStack.Pop().Dispose();
      this.baseTransaction = (PSTransaction) null;
    }

    internal PSTransaction GetCurrent() => this.transactionStack.Peek();

    internal void SetActive()
    {
      PSTransactionManager.EnableEngineProtection();
      PSTransaction psTransaction = this.transactionStack.Peek();
      if (psTransaction == null)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionForActivation"));
      if (psTransaction.IsRolledBack)
        throw new TransactionAbortedException(ResourceManagerCache.GetResourceString("TransactionStrings", "NoTransactionForActivationBecauseRollback"));
      this.previousActiveTransaction = Transaction.Current;
      psTransaction.Activate();
    }

    internal void ResetActive()
    {
      Transaction.Current = this.previousActiveTransaction;
      this.previousActiveTransaction = (Transaction) null;
    }

    internal bool HasTransaction
    {
      get
      {
        PSTransaction psTransaction = this.transactionStack.Peek();
        return psTransaction != null && !psTransaction.IsCommitted && !psTransaction.IsRolledBack;
      }
    }

    internal bool IsLastTransactionCommitted
    {
      get
      {
        PSTransaction psTransaction = this.transactionStack.Peek();
        return psTransaction != null && psTransaction.IsCommitted;
      }
    }

    internal bool IsLastTransactionRolledBack
    {
      get
      {
        PSTransaction psTransaction = this.transactionStack.Peek();
        return psTransaction != null && psTransaction.IsRolledBack;
      }
    }

    ~PSTransactionManager() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this.ResetActive();
      while (this.transactionStack.Peek() != null)
      {
        PSTransaction psTransaction = this.transactionStack.Pop();
        if (psTransaction != this.baseTransaction)
          psTransaction.Dispose();
      }
    }
  }
}
