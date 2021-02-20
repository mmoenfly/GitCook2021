// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProcessStreamReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Management.Automation
{
  internal class ProcessStreamReader
  {
    private StreamReader streamReader;
    private bool isOutput;
    private PipelineWriter writer;
    private string processPath;
    private ProcessOutputReader processOutputReader;
    private Thread thread;
    [TraceSource("NativeCP", "NativeCP")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("NativeCP", "NativeCP");

    internal ProcessStreamReader(
      StreamReader streamReader,
      string processPath,
      bool isOutput,
      PipelineWriter writer,
      ProcessOutputReader processOutputReader)
    {
      using (ProcessStreamReader.tracer.TraceConstructor((object) this, " isOutput {0}", (object) isOutput))
      {
        this.streamReader = streamReader;
        this.processPath = processPath;
        this.isOutput = isOutput;
        this.writer = writer;
        this.processOutputReader = processOutputReader;
      }
    }

    internal void Start()
    {
      this.thread = new Thread(new ThreadStart(this.ReaderStartProc));
      if (this.isOutput)
        this.thread.Name = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0} :Output Reader", (object) this.processPath);
      else
        this.thread.Name = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0} :Error Reader", (object) this.processPath);
      this.thread.Start();
    }

    internal void Done()
    {
      if (this.thread == null)
        return;
      this.thread.Join();
    }

    private void ReaderStartProc()
    {
      try
      {
        this.ReaderStartProcHelper();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        ProcessStreamReader.tracer.TraceException(ex);
      }
      finally
      {
        this.processOutputReader.ReaderDone(this.isOutput);
      }
    }

    private void ReaderStartProcHelper()
    {
      string line = this.streamReader.ReadLine();
      if (line == null)
        return;
      if (!line.Equals("#< CLIXML", StringComparison.Ordinal))
        this.ReadText(line);
      else
        this.ReadXml();
    }

    private void ReadText(string line)
    {
      using (ProcessStreamReader.tracer.TraceMethod())
      {
        if (this.isOutput)
        {
          for (; line != null; line = this.streamReader.ReadLine())
            this.AddObjectToWriter((object) line, MinishellStream.Output);
        }
        else
        {
          this.AddObjectToWriter((object) new ErrorRecord((Exception) new RemoteException(line), "NativeCommandError", ErrorCategory.NotSpecified, (object) line), MinishellStream.Error);
          char[] buffer = new char[4096];
          int charCount;
          while ((charCount = this.streamReader.Read(buffer, 0, buffer.Length)) != 0)
            this.AddObjectToWriter((object) new ErrorRecord((Exception) new RemoteException(new StringBuilder().Append(buffer, 0, charCount).ToString()), "NativeCommandErrorMessage", ErrorCategory.NotSpecified, (object) null), MinishellStream.Error);
        }
      }
    }

    private void ReadXml()
    {
      try
      {
        Deserializer deserializer = new Deserializer((XmlReader) new XmlTextReader((TextReader) this.streamReader));
        while (!deserializer.Done())
        {
          string streamName;
          object obj = deserializer.Deserialize(out streamName);
          MinishellStream stream = MinishellStream.Unknown;
          if (streamName != null)
            stream = StringToMinishellStreamConverter.ToMinishellStream(streamName);
          if (stream == MinishellStream.Unknown)
            stream = this.isOutput ? MinishellStream.Output : MinishellStream.Error;
          if (stream == MinishellStream.Output || obj != null)
          {
            switch (stream)
            {
              case MinishellStream.Error:
                if (obj is PSObject)
                {
                  obj = (object) ErrorRecord.FromPSObjectForRemoting(PSObject.AsPSObject(obj));
                  break;
                }
                string message;
                try
                {
                  message = (string) LanguagePrimitives.ConvertTo(obj, typeof (string), (IFormatProvider) CultureInfo.InvariantCulture);
                }
                catch (PSInvalidCastException ex)
                {
                  ProcessStreamReader.tracer.TraceException((Exception) ex);
                  continue;
                }
                obj = (object) new ErrorRecord((Exception) new RemoteException(message), "NativeCommandError", ErrorCategory.NotSpecified, (object) message);
                break;
              case MinishellStream.Verbose:
              case MinishellStream.Warning:
              case MinishellStream.Debug:
                try
                {
                  obj = LanguagePrimitives.ConvertTo(obj, typeof (string), (IFormatProvider) CultureInfo.InvariantCulture);
                  break;
                }
                catch (PSInvalidCastException ex)
                {
                  ProcessStreamReader.tracer.TraceException((Exception) ex);
                  continue;
                }
            }
            this.AddObjectToWriter(obj, stream);
          }
        }
      }
      catch (XmlException ex)
      {
        ProcessStreamReader.tracer.TraceException((Exception) ex);
        this.AddObjectToWriter((object) new ErrorRecord((Exception) new XmlException(string.Format((IFormatProvider) null, ResourceManagerCache.GetResourceString("NativeCP", "CliXmlError"), (object) (MinishellStream) (this.isOutput ? 0 : 1), (object) this.processPath, (object) ex.Message), (Exception) ex), "ProcessStreamReader_CliXmlError", ErrorCategory.SyntaxError, (object) this.processPath), MinishellStream.Error);
      }
    }

    private void AddObjectToWriter(object data, MinishellStream stream)
    {
      try
      {
        ProcessOutputObject processOutputObject = new ProcessOutputObject(data, stream);
        lock (this.writer)
          this.writer.Write((object) processOutputObject);
      }
      catch (PipelineClosedException ex)
      {
      }
      catch (ObjectDisposedException ex)
      {
      }
    }
  }
}
