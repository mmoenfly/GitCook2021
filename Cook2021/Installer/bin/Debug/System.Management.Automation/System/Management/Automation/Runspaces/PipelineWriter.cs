// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PipelineWriter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Threading;

namespace System.Management.Automation.Runspaces
{
  public abstract class PipelineWriter
  {
    public abstract WaitHandle WaitHandle { get; }

    public abstract bool IsOpen { get; }

    public abstract int Count { get; }

    public abstract int MaxCapacity { get; }

    public abstract void Close();

    public abstract void Flush();

    public abstract int Write(object obj);

    public abstract int Write(object obj, bool enumerateCollection);
  }
}
