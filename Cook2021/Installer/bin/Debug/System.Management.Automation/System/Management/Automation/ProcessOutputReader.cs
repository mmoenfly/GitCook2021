// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProcessOutputReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Diagnostics;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class ProcessOutputReader
  {
    private Process process;
    private string processPath;
    private bool redirectOutput;
    private bool redirectError;
    private ProcessStreamReader outputReader;
    private ProcessStreamReader errorReader;
    private ObjectStream processOutput;
    private object readerLock = new object();
    private int readerCount;
    [TraceSource("NativeCP", "NativeCP")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("NativeCP", "NativeCP");

    internal ProcessOutputReader(
      Process process,
      string processPath,
      bool redirectOutput,
      bool redirectError)
    {
      using (ProcessOutputReader.tracer.TraceConstructor((object) this))
      {
        this.process = process;
        this.processPath = processPath;
        this.redirectOutput = redirectOutput;
        this.redirectError = redirectError;
      }
    }

    internal void Start()
    {
      this.processOutput = new ObjectStream(128);
      lock (this.readerLock)
      {
        if (this.redirectOutput)
        {
          ++this.readerCount;
          this.outputReader = new ProcessStreamReader(this.process.StandardOutput, this.processPath, true, this.processOutput.ObjectWriter, this);
          this.outputReader.Start();
        }
        if (!this.redirectError)
          return;
        ++this.readerCount;
        this.errorReader = new ProcessStreamReader(this.process.StandardError, this.processPath, false, this.processOutput.ObjectWriter, this);
        this.errorReader.Start();
      }
    }

    internal void Stop()
    {
      if (this.processOutput == null)
        return;
      try
      {
        this.processOutput.ObjectReader.Close();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      try
      {
        this.processOutput.Close();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }

    internal void Done()
    {
      if (this.outputReader != null)
        this.outputReader.Done();
      if (this.errorReader == null)
        return;
      this.errorReader.Done();
    }

    internal object Read() => this.processOutput.ObjectReader.Read();

    internal void ReaderDone(bool isOutput)
    {
      int num;
      lock (this.readerLock)
        num = --this.readerCount;
      if (num != 0)
        return;
      ProcessOutputReader.tracer.WriteLine("closing processOutput.ObjectWriter", new object[0]);
      this.processOutput.ObjectWriter.Close();
    }
  }
}
