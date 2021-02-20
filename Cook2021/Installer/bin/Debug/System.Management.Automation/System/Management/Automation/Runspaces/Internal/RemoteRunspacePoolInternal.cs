// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Threading;

namespace System.Management.Automation.Runspaces.Internal
{
  internal class RemoteRunspacePoolInternal : RunspacePoolInternal, IDisposable
  {
    [TraceSource("RRPool", "Powershell hosting interfaces")]
    protected new static PSTraceSource tracer = PSTraceSource.GetTracer("RRPool", "Powershell hosting interfaces");
    private IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Runspace);
    private PSPrimitiveDictionary applicationArguments;
    private PSPrimitiveDictionary applicationPrivateData;
    private ManualResetEvent applicationPrivateDataReceived = new ManualResetEvent(false);
    private RunspaceConnectionInfo connectionInfo;
    private ClientRunspacePoolDataStructureHandler dataStructureHandler;
    private RunspacePoolAsyncResult openAsyncResult;
    private RunspacePoolAsyncResult closeAsyncResult;
    private Exception closingReason;
    private bool isDisposed;
    private System.Management.Automation.Remoting.DispatchTable<object> dispatchTable;

    internal RemoteRunspacePoolInternal(
      int minRunspaces,
      int maxRunspaces,
      TypeTable typeTable,
      PSHost host,
      PSPrimitiveDictionary applicationArguments,
      RunspaceConnectionInfo connectionInfo)
      : base(minRunspaces, maxRunspaces)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceConstructor((object) this))
      {
        if (connectionInfo == null)
          throw RemoteRunspacePoolInternal.tracer.NewArgumentNullException("WSManConnectionInfo");
        this.etwTracer.OperationalChannel.WriteVerbose(PSEventId.RunspacePoolConstructor, PSOpcode.Constructor, PSTask.CreateRunspace, (object) this.instanceId, (object) this.minPoolSz.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) this.maxPoolSz.ToString((IFormatProvider) CultureInfo.InvariantCulture));
        if (connectionInfo is WSManConnectionInfo)
          this.connectionInfo = (RunspaceConnectionInfo) ((WSManConnectionInfo) connectionInfo).Copy();
        else if (connectionInfo is NewProcessConnectionInfo)
          this.connectionInfo = (RunspaceConnectionInfo) ((NewProcessConnectionInfo) connectionInfo).Copy();
        this.host = host;
        this.applicationArguments = applicationArguments;
        this.dispatchTable = new System.Management.Automation.Remoting.DispatchTable<object>();
        this.dataStructureHandler = new ClientRunspacePoolDataStructureHandler(this, typeTable);
        this.dataStructureHandler.RemoteHostCallReceived += new EventHandler<RemoteDataEventArgs<RemoteHostCall>>(this.HandleRemoteHostCalls);
        this.dataStructureHandler.StateInfoReceived += new EventHandler<RemoteDataEventArgs<RunspacePoolStateInfo>>(this.HandleStateInfoReceived);
        this.dataStructureHandler.ApplicationPrivateDataReceived += new EventHandler<RemoteDataEventArgs<PSPrimitiveDictionary>>(this.HandleApplicationPrivateDataReceived);
        this.dataStructureHandler.SessionClosing += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleSessionClosing);
        this.dataStructureHandler.SessionClosed += new EventHandler<RemoteDataEventArgs<Exception>>(this.HandleSessionClosed);
        this.dataStructureHandler.SetMaxMinRunspacesResponseRecieved += new EventHandler<RemoteDataEventArgs<PSObject>>(this.HandleResponseReceived);
        this.dataStructureHandler.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
        this.dataStructureHandler.PSEventArgsReceived += new EventHandler<RemoteDataEventArgs<PSEventArgs>>(this.HandlePSEventArgsReceived);
      }
    }

    public override RunspaceConnectionInfo ConnectionInfo
    {
      get
      {
        using (RemoteRunspacePoolInternal.tracer.TraceProperty())
          return this.connectionInfo;
      }
    }

    internal ClientRunspacePoolDataStructureHandler DataStructureHandler
    {
      get
      {
        using (RemoteRunspacePoolInternal.tracer.TraceProperty())
          return this.dataStructureHandler;
      }
    }

    internal override bool SetMaxRunspaces(int maxRunspaces)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        long callId = 0;
        lock (this.syncObject)
        {
          if (maxRunspaces < this.minPoolSz || maxRunspaces == this.maxPoolSz || (this.stateInfo.State == RunspacePoolState.Closed || this.stateInfo.State == RunspacePoolState.Closing) || this.stateInfo.State == RunspacePoolState.Broken)
            return false;
          if (this.stateInfo.State == RunspacePoolState.BeforeOpen)
          {
            this.maxPoolSz = maxRunspaces;
            return true;
          }
          callId = this.DispatchTable.CreateNewCallId();
          this.dataStructureHandler.SendSetMaxRunspacesToServer(maxRunspaces, callId);
        }
        bool response = (bool) this.DispatchTable.GetResponse(callId, (object) false);
        if (response)
        {
          lock (this.syncObject)
            this.maxPoolSz = maxRunspaces;
        }
        return response;
      }
    }

    internal override bool SetMinRunspaces(int minRunspaces)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        long callId = 0;
        lock (this.syncObject)
        {
          if (minRunspaces < 1 || minRunspaces > this.maxPoolSz || (minRunspaces == this.minPoolSz || this.stateInfo.State == RunspacePoolState.Closed) || (this.stateInfo.State == RunspacePoolState.Closing || this.stateInfo.State == RunspacePoolState.Broken))
            return false;
          if (this.stateInfo.State == RunspacePoolState.BeforeOpen)
          {
            this.minPoolSz = minRunspaces;
            return true;
          }
          callId = this.DispatchTable.CreateNewCallId();
          this.dataStructureHandler.SendSetMinRunspacesToServer(minRunspaces, callId);
        }
        bool response = (bool) this.DispatchTable.GetResponse(callId, (object) false);
        if (response)
        {
          lock (this.syncObject)
            this.minPoolSz = minRunspaces;
        }
        return response;
      }
    }

    internal override int GetAvailableRunspaces()
    {
      long callId = 0;
      lock (this.syncObject)
      {
        if (this.stateInfo.State == RunspacePoolState.Opened)
        {
          callId = this.DispatchTable.CreateNewCallId();
          this.dataStructureHandler.SendGetAvailableRunspacesToServer(callId);
        }
        else
        {
          if (this.stateInfo.State != RunspacePoolState.BeforeOpen && this.stateInfo.State != RunspacePoolState.Opening)
            throw new InvalidOperationException(ResourceManagerCache.GetResourceString("HostInterfaceExceptionsStrings", "RunspacePoolNotOpened"));
          return this.maxPoolSz;
        }
      }
      return (int) this.dispatchTable.GetResponse(callId, (object) 0);
    }

    public override void Close()
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
        this.EndClose(this.BeginClose((AsyncCallback) null, (object) null));
    }

    public override IAsyncResult BeginClose(AsyncCallback callback, object asyncState)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        bool flag1 = false;
        bool flag2 = false;
        RunspacePoolStateInfo stateInfo = new RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, (Exception) null);
        RunspacePoolAsyncResult runspacePoolAsyncResult = (RunspacePoolAsyncResult) null;
        lock (this.syncObject)
        {
          if (this.stateInfo.State == RunspacePoolState.Closed || this.stateInfo.State == RunspacePoolState.Broken)
          {
            flag2 = true;
            runspacePoolAsyncResult = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, false);
          }
          else if (this.stateInfo.State == RunspacePoolState.BeforeOpen)
          {
            stateInfo = this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closed, (Exception) null);
            flag1 = true;
            flag2 = true;
            this.closeAsyncResult = (RunspacePoolAsyncResult) null;
            runspacePoolAsyncResult = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, false);
          }
          else if (this.stateInfo.State == RunspacePoolState.Opened || this.stateInfo.State == RunspacePoolState.Opening)
          {
            stateInfo = this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closing, (Exception) null);
            this.closeAsyncResult = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, false);
            runspacePoolAsyncResult = this.closeAsyncResult;
            flag1 = true;
          }
          else if (this.stateInfo.State == RunspacePoolState.Closing)
            return (IAsyncResult) this.closeAsyncResult;
        }
        if (flag1)
          this.RaiseStateChangeEvent(stateInfo);
        if (!flag2)
          this.dataStructureHandler.CloseRunspacePoolAsync();
        else
          runspacePoolAsyncResult.SetAsCompleted((Exception) null);
        return (IAsyncResult) runspacePoolAsyncResult;
      }
    }

    internal void HandleApplicationPrivateDataReceived(
      object sender,
      RemoteDataEventArgs<PSPrimitiveDictionary> eventArgs)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
        this.SetApplicationPrivateData(eventArgs.Data);
    }

    internal void HandleStateInfoReceived(
      object sender,
      RemoteDataEventArgs<RunspacePoolStateInfo> eventArgs)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        RunspacePoolStateInfo data = eventArgs.Data;
        bool flag1 = false;
        if (data.State == RunspacePoolState.Opened)
        {
          lock (this.syncObject)
          {
            if (this.stateInfo.State == RunspacePoolState.Opening)
            {
              this.SetRunspacePoolState(data);
              flag1 = true;
            }
          }
          if (!flag1)
            return;
          this.RaiseStateChangeEvent(this.stateInfo);
          this.SetOpenAsCompleted();
        }
        else
        {
          if (data.State != RunspacePoolState.Closed && data.State != RunspacePoolState.Broken)
            return;
          bool flag2 = false;
          lock (this.syncObject)
          {
            if (this.stateInfo.State == RunspacePoolState.Closed || this.stateInfo.State == RunspacePoolState.Broken)
              return;
            if (this.stateInfo.State != RunspacePoolState.Opening && this.stateInfo.State != RunspacePoolState.Opened)
            {
              if (this.stateInfo.State != RunspacePoolState.Closing)
                goto label_16;
            }
            flag2 = true;
            this.SetRunspacePoolState(data);
          }
label_16:
          if (!flag2)
            return;
          if (this.closeAsyncResult == null)
            this.dataStructureHandler.CloseRunspacePoolAsync();
          this.RaiseStateChangeEvent(data);
          this.SetCloseAsCompleted();
        }
      }
    }

    internal void HandleRemoteHostCalls(
      object sender,
      RemoteDataEventArgs<RemoteHostCall> eventArgs)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        if (this.HostCallReceived != null)
        {
          this.HostCallReceived(sender, eventArgs);
        }
        else
        {
          RemoteHostCall data = eventArgs.Data;
          if (data.IsVoidMethod)
            data.ExecuteVoidMethod(this.host);
          else
            this.dataStructureHandler.SendHostResponseToServer(data.ExecuteNonVoidMethod(this.host));
        }
      }
    }

    internal PSHost Host => this.host;

    internal PSPrimitiveDictionary ApplicationArguments => this.applicationArguments;

    internal override PSPrimitiveDictionary GetApplicationPrivateData()
    {
      this.applicationPrivateDataReceived.WaitOne();
      return this.applicationPrivateData;
    }

    internal void SetApplicationPrivateData(PSPrimitiveDictionary applicationPrivateData)
    {
      lock (this.syncObject)
      {
        if (this.applicationPrivateDataReceived.WaitOne(0, false))
          return;
        this.applicationPrivateData = applicationPrivateData;
        this.applicationPrivateDataReceived.Set();
        foreach (Runspace runspace in this.runspaceList)
          runspace.SetApplicationPrivateData(applicationPrivateData);
      }
    }

    internal override void PropagateApplicationPrivateData(Runspace runspace)
    {
      if (!this.applicationPrivateDataReceived.WaitOne(0, false))
        return;
      runspace.SetApplicationPrivateData(this.GetApplicationPrivateData());
    }

    internal event EventHandler<RemoteDataEventArgs<RemoteHostCall>> HostCallReceived;

    internal event EventHandler<RemoteDataEventArgs<Uri>> URIRedirectionReported;

    internal void CreatePowerShellOnServerAndInvoke(ClientRemotePowerShell shell)
    {
      this.dataStructureHandler.CreatePowerShellOnServerAndInvoke(shell);
      if (shell.NoInput)
        return;
      shell.SendInput();
    }

    protected override IAsyncResult CoreOpen(
      bool isAsync,
      AsyncCallback callback,
      object asyncState)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        this.etwTracer.OperationalChannel.WriteVerbose(PSEventId.RunspacePoolOpen, PSOpcode.Open, PSTask.CreateRunspace);
        lock (this.syncObject)
        {
          this.AssertIfStateIsBeforeOpen();
          this.stateInfo = new RunspacePoolStateInfo(RunspacePoolState.Opening, (Exception) null);
        }
        this.RaiseStateChangeEvent(this.stateInfo);
        RunspacePoolAsyncResult runspacePoolAsyncResult = new RunspacePoolAsyncResult(this.instanceId, callback, asyncState, true);
        this.openAsyncResult = runspacePoolAsyncResult;
        this.dataStructureHandler.CreateRunspacePoolAndOpenAsync();
        return (IAsyncResult) runspacePoolAsyncResult;
      }
    }

    public override void Open() => this.EndOpen(this.BeginOpen((AsyncCallback) null, (object) null));

    private void SetRunspacePoolState(RunspacePoolStateInfo newStateInfo)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
        this.SetRunspacePoolState(newStateInfo, false);
    }

    private void SetRunspacePoolState(RunspacePoolStateInfo newStateInfo, bool raiseEvents)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        this.stateInfo = newStateInfo;
        if (!raiseEvents)
          return;
        this.RaiseStateChangeEvent(newStateInfo);
      }
    }

    private void HandleSessionClosing(object sender, RemoteDataEventArgs<Exception> eventArgs)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
        this.closingReason = eventArgs.Data;
    }

    private void HandleSessionClosed(object sender, RemoteDataEventArgs<Exception> eventArgs)
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        if (eventArgs.Data != null)
          this.closingReason = eventArgs.Data;
        if (this.stateInfo.State == RunspacePoolState.Opening || this.stateInfo.State == RunspacePoolState.Opened)
          this.SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Broken, this.closingReason), true);
        else if (this.stateInfo.State == RunspacePoolState.Closing)
          this.SetRunspacePoolState(new RunspacePoolStateInfo(RunspacePoolState.Closed, this.closingReason), true);
        this.SetCloseAsCompleted();
      }
    }

    private void SetOpenAsCompleted()
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        if (this.openAsyncResult == null || this.openAsyncResult.IsCompleted)
          return;
        this.openAsyncResult.SetAsCompleted(this.stateInfo.Reason);
        this.openAsyncResult = (RunspacePoolAsyncResult) null;
      }
    }

    private void SetCloseAsCompleted()
    {
      using (RemoteRunspacePoolInternal.tracer.TraceMethod())
      {
        this.DispatchTable.AbortAllCalls();
        if (this.closeAsyncResult != null)
        {
          this.closeAsyncResult.SetAsCompleted(this.stateInfo.Reason);
          this.closeAsyncResult = (RunspacePoolAsyncResult) null;
        }
        else
          this.SetOpenAsCompleted();
      }
    }

    private void HandleResponseReceived(object sender, RemoteDataEventArgs<PSObject> eventArgs)
    {
      PSObject data = eventArgs.Data;
      object propertyValue = RemotingDecoder.GetPropertyValue<object>(data, "SetMinMaxRunspacesResponse");
      this.dispatchTable.SetResponse(RemotingDecoder.GetPropertyValue<long>(data, "ci"), propertyValue);
    }

    private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs)
    {
      if (!(this.connectionInfo is WSManConnectionInfo connectionInfo))
        return;
      connectionInfo.SetConnectionUri(eventArgs.Data);
      if (this.URIRedirectionReported == null)
        return;
      this.URIRedirectionReported((object) this, eventArgs);
    }

    private void HandlePSEventArgsReceived(object sender, RemoteDataEventArgs<PSEventArgs> e) => this.OnForwardEvent(e.Data);

    private System.Management.Automation.Remoting.DispatchTable<object> DispatchTable => this.dispatchTable;

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this.isDisposed)
        return;
      this.isDisposed = true;
      this.dataStructureHandler.Dispose(disposing);
      this.applicationPrivateDataReceived.Close();
      this.etwTracer.Dispose();
    }
  }
}
