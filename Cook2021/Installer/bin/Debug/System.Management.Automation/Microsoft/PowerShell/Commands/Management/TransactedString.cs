// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Management.TransactedString
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Text;
using System.Transactions;

namespace Microsoft.PowerShell.Commands.Management
{
  public class TransactedString : IEnlistmentNotification
  {
    private StringBuilder m_Value;
    private StringBuilder m_TemporaryValue;
    private Transaction enlistedTransaction;

    public TransactedString()
      : this("")
    {
    }

    public TransactedString(string value)
    {
      this.m_Value = new StringBuilder(value);
      this.m_TemporaryValue = (StringBuilder) null;
    }

    void IEnlistmentNotification.Commit(Enlistment enlistment)
    {
      this.m_Value = new StringBuilder(this.m_TemporaryValue.ToString());
      this.m_TemporaryValue = (StringBuilder) null;
      this.enlistedTransaction = (Transaction) null;
      enlistment.Done();
    }

    void IEnlistmentNotification.Rollback(Enlistment enlistment)
    {
      this.m_TemporaryValue = (StringBuilder) null;
      this.enlistedTransaction = (Transaction) null;
      enlistment.Done();
    }

    void IEnlistmentNotification.InDoubt(Enlistment enlistment) => enlistment.Done();

    void IEnlistmentNotification.Prepare(
      PreparingEnlistment preparingEnlistment)
    {
      preparingEnlistment.Prepared();
    }

    public void Append(string text)
    {
      this.ValidateTransactionOrEnlist();
      if (this.enlistedTransaction != (Transaction) null)
        this.m_TemporaryValue.Append(text);
      else
        this.m_Value.Append(text);
    }

    public void Remove(int startIndex, int length)
    {
      this.ValidateTransactionOrEnlist();
      if (this.enlistedTransaction != (Transaction) null)
        this.m_TemporaryValue.Remove(startIndex, length);
      else
        this.m_Value.Remove(startIndex, length);
    }

    public int Length => Transaction.Current == (Transaction) null || this.enlistedTransaction != Transaction.Current ? this.m_Value.Length : this.m_TemporaryValue.Length;

    public override string ToString() => Transaction.Current == (Transaction) null || this.enlistedTransaction != Transaction.Current ? this.m_Value.ToString() : this.m_TemporaryValue.ToString();

    private void ValidateTransactionOrEnlist()
    {
      if (Transaction.Current != (Transaction) null)
      {
        if (this.enlistedTransaction == (Transaction) null)
        {
          Transaction.Current.EnlistVolatile((IEnlistmentNotification) this, EnlistmentOptions.None);
          this.enlistedTransaction = Transaction.Current;
          this.m_TemporaryValue = new StringBuilder(this.m_Value.ToString());
        }
        else if (Transaction.Current != this.enlistedTransaction)
          throw new InvalidOperationException("Cannot modify string. It has been modified by another transaction.");
      }
      else if (this.enlistedTransaction != (Transaction) null)
        throw new InvalidOperationException("Cannot modify string. It has been modified by another transaction.");
    }
  }
}
