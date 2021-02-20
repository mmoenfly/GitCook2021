// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.BaseTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting.Client;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Remoting
{
  internal abstract class BaseTransportManager : IDisposable
  {
    internal const int ServerDefaultKeepAliveTimeoutMs = 240000;
    internal const int ClientDefaultOperationTimeoutMs = 180000;
    internal const int ClientCloseTimeoutMs = 60000;
    internal const int DefaultFragmentSize = 32768;
    internal const int MaximumReceivedDataSize = 52428800;
    internal const int MaximumReceivedObjectSize = 10485760;
    internal const string MAX_RECEIVED_DATA_PER_COMMAND_MB = "PSMaximumReceivedDataSizePerCommandMB";
    internal const string MAX_RECEIVED_OBJECT_SIZE_MB = "PSMaximumReceivedObjectSizeMB";
    [TraceSource("Transport", "Traces BaseWSManTransportManager")]
    private static PSTraceSource baseTracer = PSTraceSource.GetTracer("Transport", "Traces BaseWSManTransportManager");
    private static IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Transport);
    private Fragmentor fragmentor;
    private PriorityReceiveDataCollection recvdData;
    private ReceiveDataCollection.OnDataAvailableCallback onDataAvailableCallback;
    private PSRemotingCryptoHelper cryptoHelper;

    internal event EventHandler<TransportErrorOccuredEventArgs> WSManTransportErrorOccured;

    internal event EventHandler<RemoteDataEventArgs> DataReceived;

    public event EventHandler PowerShellGuidObserver;

    protected BaseTransportManager(PSRemotingCryptoHelper cryptoHelper)
    {
      this.cryptoHelper = cryptoHelper;
      this.fragmentor = new Fragmentor(32768, cryptoHelper);
      this.recvdData = new PriorityReceiveDataCollection(this.fragmentor, this is BaseClientTransportManager);
      this.onDataAvailableCallback = new ReceiveDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
    }

    internal Fragmentor Fragmentor
    {
      get => this.fragmentor;
      set => this.fragmentor = value;
    }

    internal TypeTable TypeTable
    {
      get => this.fragmentor.TypeTable;
      set => this.fragmentor.TypeTable = value;
    }

    internal static IETWTracer ETWTracer => BaseTransportManager.etwTracer;

    internal virtual void ProcessRawData(byte[] data, string stream)
    {
      try
      {
        this.ProcessRawData(data, stream, this.onDataAvailableCallback);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        BaseTransportManager.baseTracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Exception processing data. {0}", (object) ex.Message), new object[0]);
        this.RaiseErrorHandler(new TransportErrorOccuredEventArgs(new PSRemotingTransportException(ex.Message), TransportMethodEnum.ReceiveShellOutputEx));
      }
    }

    internal void ProcessRawData(
      byte[] data,
      string stream,
      ReceiveDataCollection.OnDataAvailableCallback dataAvailableCallback)
    {
      BaseTransportManager.baseTracer.WriteLine("Processing incoming data for stream {0}.", (object) stream);
      bool flag = false;
      DataPriorityType priorityType = DataPriorityType.Default;
      if (stream.Equals("stdin", StringComparison.OrdinalIgnoreCase) || stream.Equals("stdout", StringComparison.OrdinalIgnoreCase))
        flag = true;
      else if (stream.Equals("pr", StringComparison.OrdinalIgnoreCase))
      {
        priorityType = DataPriorityType.PromptResponse;
        flag = true;
      }
      if (!flag)
        BaseTransportManager.baseTracer.WriteLine("{0} is not a valid stream", (object) stream);
      else
        this.recvdData.ProcessRawData(data, priorityType, dataAvailableCallback);
    }

    internal void OnDataAvailableCallback(RemoteDataObject<PSObject> remoteObject)
    {
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.TransportReceivedObject, PSOpcode.Open, PSTask.None, (object) remoteObject.RunspacePoolId, (object) remoteObject.PowerShellId, (object) (uint) remoteObject.Destination, (object) (uint) remoteObject.DataType, (object) (uint) remoteObject.TargetInterface);
      if (this.PowerShellGuidObserver != null)
        this.PowerShellGuidObserver((object) remoteObject.PowerShellId, EventArgs.Empty);
      if (this.DataReceived == null)
        return;
      this.DataReceived((object) this, new RemoteDataEventArgs(remoteObject));
    }

    public void MigrateDataReadyEventHandlers(BaseTransportManager transportManager)
    {
      foreach (Delegate invocation in transportManager.DataReceived.GetInvocationList())
      {
        BaseTransportManager transportManager1 = this;
        transportManager1.DataReceived = (EventHandler<RemoteDataEventArgs>) Delegate.Combine((Delegate) transportManager1.DataReceived, invocation);
      }
    }

    internal virtual void RaiseErrorHandler(TransportErrorOccuredEventArgs eventArgs)
    {
      if (this.WSManTransportErrorOccured == null)
        return;
      this.WSManTransportErrorOccured((object) this, eventArgs);
    }

    internal PSRemotingCryptoHelper CryptoHelper
    {
      get => this.cryptoHelper;
      set => this.cryptoHelper = value;
    }

    internal PriorityReceiveDataCollection ReceivedDataCollection => this.recvdData;

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    internal virtual void Dispose(bool isDisposing) => this.recvdData.Dispose();
  }
}
