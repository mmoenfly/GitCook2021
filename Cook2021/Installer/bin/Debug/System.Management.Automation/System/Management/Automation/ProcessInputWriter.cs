// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProcessInputWriter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Management.Automation.Internal;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Management.Automation
{
  internal class ProcessInputWriter
  {
    private InternalCommand command;
    private ArrayList inputList = new ArrayList();
    private StreamWriter streamWriter;
    private NativeCommandIOFormat inputFormat;
    private Thread inputThread;
    private bool stopping;
    [TraceSource("NativeCP", "NativeCP")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("NativeCP", "NativeCP");

    internal ProcessInputWriter(InternalCommand command)
    {
      using (ProcessInputWriter.tracer.TraceConstructor((object) this))
        this.command = command;
    }

    internal void Add(object input) => this.inputList.Add(input);

    internal int Count => this.inputList.Count;

    internal void Start(Process process, NativeCommandIOFormat inputFormat)
    {
      if (!(this.command.Context.GetVariable("OutputEncoding") is Encoding encoding))
        encoding = Encoding.ASCII;
      this.streamWriter = new StreamWriter(process.StandardInput.BaseStream, encoding);
      this.inputFormat = inputFormat;
      if (inputFormat == NativeCommandIOFormat.Text)
        this.ConvertToString();
      this.inputThread = new Thread(new ThreadStart(this.WriterThreadProc));
      this.inputThread.Start();
    }

    internal void Stop() => this.stopping = true;

    internal void Done()
    {
      if (this.inputThread == null)
        return;
      this.inputThread.Join();
    }

    private void WriterThreadProc()
    {
      try
      {
        if (this.inputFormat == NativeCommandIOFormat.Text)
          this.WriteTextInput();
        else
          this.WriteXmlInput();
      }
      catch (IOException ex)
      {
        ProcessInputWriter.tracer.TraceException((Exception) ex);
      }
    }

    private void WriteTextInput()
    {
      try
      {
        foreach (object input in this.inputList)
        {
          if (this.stopping)
            break;
          this.streamWriter.Write(PSObject.ToStringParser(this.command.Context, input));
        }
      }
      finally
      {
        this.streamWriter.Close();
      }
    }

    private void WriteXmlInput()
    {
      try
      {
        this.streamWriter.WriteLine("#< CLIXML");
        Serializer serializer = new Serializer((XmlWriter) new XmlTextWriter((TextWriter) this.streamWriter));
        foreach (object input in this.inputList)
        {
          if (this.stopping)
            return;
          serializer.Serialize(input);
        }
        serializer.Done();
      }
      finally
      {
        this.streamWriter.Close();
      }
    }

    private void ConvertToString()
    {
      PipelineProcessor pipelineProcessor = new PipelineProcessor();
      pipelineProcessor.Add(this.command.Context.CreateCommand("out-string"));
      this.inputList = new ArrayList((ICollection) pipelineProcessor.Execute((Array) this.inputList.ToArray()));
    }
  }
}
