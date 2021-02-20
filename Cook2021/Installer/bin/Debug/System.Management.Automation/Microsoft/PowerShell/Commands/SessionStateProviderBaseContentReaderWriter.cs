// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.SessionStateProviderBaseContentReaderWriter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Microsoft.PowerShell.Commands
{
  public class SessionStateProviderBaseContentReaderWriter : 
    IContentReader,
    IContentWriter,
    IDisposable
  {
    [TraceSource("SessionStateProvider", "Providers that produce a view of session state data.")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("SessionStateProvider", "Providers that produce a view of session state data.");
    private string path;
    private SessionStateProviderBase provider;
    private bool contentRead;

    internal SessionStateProviderBaseContentReaderWriter(
      string path,
      SessionStateProviderBase provider)
    {
      using (SessionStateProviderBaseContentReaderWriter.tracer.TraceConstructor((object) this))
      {
        if (string.IsNullOrEmpty(path))
          throw SessionStateProviderBaseContentReaderWriter.tracer.NewArgumentException(nameof (path));
        if (provider == null)
          throw SessionStateProviderBaseContentReaderWriter.tracer.NewArgumentNullException(nameof (provider));
        this.path = path;
        this.provider = provider;
      }
    }

    public IList Read(long readCount)
    {
      using (SessionStateProviderBaseContentReaderWriter.tracer.TraceMethod())
      {
        list = (IList) null;
        if (!this.contentRead)
        {
          object sessionStateItem = this.provider.GetSessionStateItem(this.path);
          if (sessionStateItem != null)
          {
            object valueOfItem = this.provider.GetValueOfItem(sessionStateItem);
            switch (valueOfItem)
            {
              case null:
              case IList list:
                this.contentRead = true;
                break;
              default:
                list = (IList) new object[1]
                {
                  valueOfItem
                };
                goto case null;
            }
          }
        }
        return list;
      }
    }

    public IList Write(IList content)
    {
      using (SessionStateProviderBaseContentReaderWriter.tracer.TraceMethod())
      {
        object obj = content != null ? (object) content : throw SessionStateProviderBaseContentReaderWriter.tracer.NewArgumentNullException(nameof (content));
        if (content.Count == 1)
          obj = content[0];
        this.provider.SetSessionStateItem(this.path, obj, false);
        return content;
      }
    }

    public void Seek(long offset, SeekOrigin origin) => throw SessionStateProviderBaseContentReaderWriter.tracer.NewNotSupportedException("SessionStateStrings", "IContent_Seek_NotSupported");

    public void Close()
    {
    }

    public void Dispose()
    {
      this.Close();
      GC.SuppressFinalize((object) this);
    }
  }
}
