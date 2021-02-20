// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.OutOfProcessTextWriter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.IO;

namespace System.Management.Automation.Remoting
{
  internal sealed class OutOfProcessTextWriter
  {
    private TextWriter writer;
    private bool isStopped;
    private object syncObject = new object();

    internal OutOfProcessTextWriter(TextWriter writerToWrap) => this.writer = writerToWrap;

    internal void WriteLine(string data)
    {
      if (this.isStopped)
        return;
      lock (this.syncObject)
        this.writer.WriteLine(data);
    }

    internal void StopWriting() => this.isStopped = true;
  }
}
