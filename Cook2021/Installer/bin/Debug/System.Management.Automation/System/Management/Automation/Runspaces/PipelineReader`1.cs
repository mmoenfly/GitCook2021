// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PipelineReader`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  public abstract class PipelineReader<T>
  {
    public abstract event EventHandler DataReady;

    public abstract WaitHandle WaitHandle { get; }

    public abstract bool EndOfPipeline { get; }

    public abstract bool IsOpen { get; }

    public abstract int Count { get; }

    public abstract int MaxCapacity { get; }

    public abstract void Close();

    public abstract Collection<T> Read(int count);

    public abstract T Read();

    public abstract Collection<T> ReadToEnd();

    public abstract Collection<T> NonBlockingRead();

    public abstract Collection<T> NonBlockingRead(int maxRequested);

    public abstract T Peek();

    internal IEnumerator<T> GetReadEnumerator()
    {
      while (!this.EndOfPipeline)
      {
        T t = this.Read();
        if (object.Equals((object) t, (object) AutomationNull.Value))
          break;
        yield return t;
      }
    }
  }
}
