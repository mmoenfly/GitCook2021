// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ServerRemoteSessionDSHandlerStateMachine
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Timers;

namespace System.Management.Automation.Remoting
{
  internal class ServerRemoteSessionDSHandlerStateMachine
  {
    [TraceSource("ServerRemoteSessionDSHandlerStateMachine", "ServerRemoteSessionDSHandlerStateMachine")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (ServerRemoteSessionDSHandlerStateMachine), nameof (ServerRemoteSessionDSHandlerStateMachine));
    private ServerRemoteSession _session;
    private object _syncObject;
    private Queue<RemoteSessionStateMachineEventArgs> processPendingEventsQueue = new Queue<RemoteSessionStateMachineEventArgs>();
    private bool eventsInProcess;
    private EventHandler<RemoteSessionStateMachineEventArgs>[,] _stateMachineHandle;
    private RemoteSessionState _state;
    private Timer _keyExchangeTimer;

    internal ServerRemoteSessionDSHandlerStateMachine(ServerRemoteSession session)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceConstructor((object) this))
      {
        this._session = session != null ? session : throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (session));
        this._syncObject = new object();
        this._stateMachineHandle = new EventHandler<RemoteSessionStateMachineEventArgs>[15, 24];
        for (int index = 0; index < this._stateMachineHandle.GetLength(0); ++index)
        {
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle1;
          IntPtr num1;
          (stateMachineHandle1 = this._stateMachineHandle)[(int) (num1 = (IntPtr) index), 16] = stateMachineHandle1[(int) num1, 16] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoFatalError);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle2;
          IntPtr num2;
          (stateMachineHandle2 = this._stateMachineHandle)[(int) (num2 = (IntPtr) index), 8] = stateMachineHandle2[(int) num2, 8] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoClose);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle3;
          IntPtr num3;
          (stateMachineHandle3 = this._stateMachineHandle)[(int) (num3 = (IntPtr) index), 10] = stateMachineHandle3[(int) num3, 10] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoCloseFailed);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle4;
          IntPtr num4;
          (stateMachineHandle4 = this._stateMachineHandle)[(int) (num4 = (IntPtr) index), 9] = stateMachineHandle4[(int) num4, 9] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoCloseCompleted);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle5;
          IntPtr num5;
          (stateMachineHandle5 = this._stateMachineHandle)[(int) (num5 = (IntPtr) index), 13] = stateMachineHandle5[(int) num5, 13] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationTimeout);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle6;
          IntPtr num6;
          (stateMachineHandle6 = this._stateMachineHandle)[(int) (num6 = (IntPtr) index), 14] = stateMachineHandle6[(int) num6, 14] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoSendFailed);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle7;
          IntPtr num7;
          (stateMachineHandle7 = this._stateMachineHandle)[(int) (num7 = (IntPtr) index), 15] = stateMachineHandle7[(int) num7, 15] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoReceiveFailed);
        }
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle8;
        (stateMachineHandle8 = this._stateMachineHandle)[1, 1] = stateMachineHandle8[1, 1] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoStart);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle9;
        (stateMachineHandle9 = this._stateMachineHandle)[7, 5] = stateMachineHandle9[7, 5] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationReceived);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle10;
        (stateMachineHandle10 = this._stateMachineHandle)[6, 3] = stateMachineHandle10[6, 3] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationSending);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle11;
        (stateMachineHandle11 = this._stateMachineHandle)[4, 4] = stateMachineHandle11[4, 4] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationCompleted);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle12;
        (stateMachineHandle12 = this._stateMachineHandle)[5, 6] = stateMachineHandle12[5, 6] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoEstablished);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle13;
        (stateMachineHandle13 = this._stateMachineHandle)[5, 7] = stateMachineHandle13[5, 7] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationPending);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle14;
        (stateMachineHandle14 = this._stateMachineHandle)[10, 17] = stateMachineHandle14[10, 17] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoMessageReceived);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle15;
        (stateMachineHandle15 = this._stateMachineHandle)[6, 12] = stateMachineHandle15[6, 12] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationFailed);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle16;
        (stateMachineHandle16 = this._stateMachineHandle)[2, 11] = stateMachineHandle16[2, 11] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoConnectFailed);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle17;
        (stateMachineHandle17 = this._stateMachineHandle)[10, 20] = stateMachineHandle17[10, 20] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle18;
        (stateMachineHandle18 = this._stateMachineHandle)[10, 22] = stateMachineHandle18[10, 22] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle19;
        (stateMachineHandle19 = this._stateMachineHandle)[10, 21] = stateMachineHandle19[10, 21] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle20;
        (stateMachineHandle20 = this._stateMachineHandle)[13, 20] = stateMachineHandle20[13, 20] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle21;
        (stateMachineHandle21 = this._stateMachineHandle)[13, 18] = stateMachineHandle21[13, 18] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle22;
        (stateMachineHandle22 = this._stateMachineHandle)[13, 21] = stateMachineHandle22[13, 21] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle23;
        (stateMachineHandle23 = this._stateMachineHandle)[12, 19] = stateMachineHandle23[12, 19] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle24;
        (stateMachineHandle24 = this._stateMachineHandle)[12, 18] = stateMachineHandle24[12, 18] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoKeyExchange);
        for (int index1 = 0; index1 < this._stateMachineHandle.GetLength(0); ++index1)
        {
          for (int index2 = 0; index2 < this._stateMachineHandle.GetLength(1); ++index2)
          {
            if (this._stateMachineHandle[index1, index2] == null)
              this._stateMachineHandle[index1, index2] += new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoClose);
          }
        }
        this.SetState(RemoteSessionState.Idle, (Exception) null);
      }
    }

    internal RemoteSessionState State
    {
      get
      {
        using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceProperty())
          return this._state;
      }
    }

    internal bool CanByPassRaiseEvent(RemoteSessionStateMachineEventArgs arg) => arg.StateEvent == RemoteSessionEvent.MessageReceived && (this._state == RemoteSessionState.Established || this._state == RemoteSessionState.EstablishedAndKeySent || (this._state == RemoteSessionState.EstablishedAndKeyReceived || this._state == RemoteSessionState.EstablishedAndKeyExchanged));

    internal void RaiseEvent(RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        lock (this._syncObject)
        {
          ServerRemoteSessionDSHandlerStateMachine._trace.WriteLine("Event received : {0}", (object) fsmEventArg.StateEvent);
          this.processPendingEventsQueue.Enqueue(fsmEventArg);
          if (this.eventsInProcess)
            return;
          this.eventsInProcess = true;
        }
        this.ProcessEvents();
      }
    }

    private void ProcessEvents()
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        RemoteSessionStateMachineEventArgs fsmEventArg = (RemoteSessionStateMachineEventArgs) null;
        do
        {
          lock (this._syncObject)
          {
            if (this.processPendingEventsQueue.Count == 0)
            {
              this.eventsInProcess = false;
              break;
            }
            fsmEventArg = this.processPendingEventsQueue.Dequeue();
          }
          this.RaiseEventPrivate(fsmEventArg);
        }
        while (this.eventsInProcess);
      }
    }

    private void RaiseEventPrivate(RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        EventHandler<RemoteSessionStateMachineEventArgs> eventHandler = this._stateMachineHandle[(int) this._state, (int) fsmEventArg.StateEvent];
        if (eventHandler == null)
          return;
        ServerRemoteSessionDSHandlerStateMachine._trace.WriteLine("Before calling state machine event handler: state = {0}, event = {1}", (object) this._state, (object) fsmEventArg.StateEvent);
        eventHandler((object) this, fsmEventArg);
        ServerRemoteSessionDSHandlerStateMachine._trace.WriteLine("After calling state machine event handler: state = {0}, event = {1}", (object) this._state, (object) fsmEventArg.StateEvent);
      }
    }

    private void DoStart(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.DoNegotiationPending(sender, fsmEventArg);
      }
    }

    private void DoNegotiationPending(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.SetState(RemoteSessionState.NegotiationPending, (Exception) null);
      }
    }

    private void DoNegotiationReceived(
      object sender,
      RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        if (fsmEventArg.StateEvent != RemoteSessionEvent.NegotiationReceived)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentException(nameof (fsmEventArg));
        if (fsmEventArg.RemoteSessionCapability == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentException(nameof (fsmEventArg));
        this.SetState(RemoteSessionState.NegotiationReceived, (Exception) null);
      }
    }

    private void DoNegotiationSending(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.SetState(RemoteSessionState.NegotiationSending, (Exception) null);
        this._session.SessionDataStructureHandler.SendNegotiationAsync();
      }
    }

    private void DoNegotiationCompleted(
      object sender,
      RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.SetState(RemoteSessionState.NegotiationSent, (Exception) null);
      }
    }

    private void DoEstablished(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        if (fsmEventArg.StateEvent != RemoteSessionEvent.NegotiationCompleted)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentException(nameof (fsmEventArg));
        if (this._state != RemoteSessionState.NegotiationSent)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewInvalidOperationException();
        this.SetState(RemoteSessionState.Established, (Exception) null);
      }
    }

    internal void DoMessageReceived(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        if (fsmEventArg.RemoteData == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentException(nameof (fsmEventArg));
        RemotingTargetInterface targetInterface = fsmEventArg.RemoteData.TargetInterface;
        RemotingDataType dataType = fsmEventArg.RemoteData.DataType;
        switch (targetInterface)
        {
          case RemotingTargetInterface.Session:
            if (dataType != RemotingDataType.CreateRunspacePool)
              break;
            this._session.SessionDataStructureHandler.RaiseDataReceivedEvent(new RemoteDataEventArgs(fsmEventArg.RemoteData));
            break;
          case RemotingTargetInterface.RunspacePool:
            Guid runspacePoolId = fsmEventArg.RemoteData.RunspacePoolId;
            ServerRunspacePoolDriver runspacePoolDriver = this._session.GetRunspacePoolDriver(runspacePoolId);
            if (runspacePoolDriver != null)
            {
              runspacePoolDriver.DataStructureHandler.ProcessReceivedData(fsmEventArg.RemoteData);
              break;
            }
            ServerRemoteSessionDSHandlerStateMachine._trace.WriteLine("Server received data for Runspace (id: {0}), \r\n                                but the Runspace cannot be found", (object) runspacePoolId);
            this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.FatalError, (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.RunspaceCannotBeFound, new object[1]
            {
              (object) runspacePoolId
            })));
            break;
          case RemotingTargetInterface.PowerShell:
            this._session.GetRunspacePoolDriver(fsmEventArg.RemoteData.RunspacePoolId).DataStructureHandler.DispatchMessageToPowerShell(fsmEventArg.RemoteData);
            break;
          default:
            ServerRemoteSessionDSHandlerStateMachine._trace.WriteLine("Server received data unknown targetInterface: {0}", (object) targetInterface);
            this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.FatalError, (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.ReceivedUnsupportedRemotingTargetInterfaceType, new object[1]
            {
              (object) targetInterface
            })));
            break;
        }
      }
    }

    private void DoConnectFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        if (fsmEventArg.StateEvent != RemoteSessionEvent.ConnectFailed)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentException(nameof (fsmEventArg));
        throw ServerRemoteSessionDSHandlerStateMachine._trace.NewInvalidOperationException();
      }
    }

    private void DoFatalError(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        if (fsmEventArg.StateEvent != RemoteSessionEvent.FatalError)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentException(nameof (fsmEventArg));
        this.DoClose((object) this, fsmEventArg);
      }
    }

    private void DoClose(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        switch (this._state)
        {
          case RemoteSessionState.Connecting:
          case RemoteSessionState.Connected:
          case RemoteSessionState.NegotiationSending:
          case RemoteSessionState.NegotiationSent:
          case RemoteSessionState.NegotiationReceived:
          case RemoteSessionState.Established:
          case RemoteSessionState.EstablishedAndKeySent:
          case RemoteSessionState.EstablishedAndKeyReceived:
          case RemoteSessionState.EstablishedAndKeyExchanged:
            this.SetState(RemoteSessionState.ClosingConnection, fsmEventArg.Reason);
            this._session.SessionDataStructureHandler.CloseConnectionAsync(fsmEventArg.Reason);
            goto case RemoteSessionState.ClosingConnection;
          case RemoteSessionState.ClosingConnection:
          case RemoteSessionState.Closed:
            this.CleanAll();
            break;
          default:
            this.SetState(RemoteSessionState.Closed, (Exception) new PSRemotingTransportException(fsmEventArg.Reason, PSRemotingErrorId.ForceClosed, new object[0]));
            goto case RemoteSessionState.ClosingConnection;
        }
      }
    }

    private void DoCloseFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.SetState(RemoteSessionState.Closed, fsmEventArg.Reason);
        this.CleanAll();
      }
    }

    private void DoCloseCompleted(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.SetState(RemoteSessionState.Closed, fsmEventArg.Reason);
        this._session.Close(fsmEventArg);
        this.CleanAll();
      }
    }

    private void DoNegotiationFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.RaiseEventPrivate(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close));
      }
    }

    private void DoNegotiationTimeout(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        if (this._state == RemoteSessionState.Established)
          return;
        this.RaiseEventPrivate(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close));
      }
    }

    private void DoSendFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.RaiseEventPrivate(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close));
      }
    }

    private void DoReceiveFailed(object sender, RemoteSessionStateMachineEventArgs fsmEventArg)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (fsmEventArg == null)
          throw ServerRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (fsmEventArg));
        this.RaiseEventPrivate(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close));
      }
    }

    private void DoKeyExchange(object sender, RemoteSessionStateMachineEventArgs eventArgs)
    {
      switch (eventArgs.StateEvent)
      {
        case RemoteSessionEvent.KeySent:
          if (this._state != RemoteSessionState.EstablishedAndKeyReceived)
            break;
          this.SetState(RemoteSessionState.EstablishedAndKeyExchanged, eventArgs.Reason);
          break;
        case RemoteSessionEvent.KeySendFailed:
          this.DoClose((object) this, eventArgs);
          break;
        case RemoteSessionEvent.KeyReceived:
          if (this._state == RemoteSessionState.EstablishedAndKeyRequested && this._keyExchangeTimer != null)
          {
            this._keyExchangeTimer.Enabled = false;
            this._keyExchangeTimer.Dispose();
            this._keyExchangeTimer = (Timer) null;
          }
          this.SetState(RemoteSessionState.EstablishedAndKeyReceived, eventArgs.Reason);
          this._session.SendEncryptedSessionKey();
          break;
        case RemoteSessionEvent.KeyReceiveFailed:
          if (this._state == RemoteSessionState.Established)
            break;
          this.DoClose((object) this, eventArgs);
          break;
        case RemoteSessionEvent.KeyRequested:
          if (this._state != RemoteSessionState.Established)
            break;
          this.SetState(RemoteSessionState.EstablishedAndKeyRequested, eventArgs.Reason);
          this._keyExchangeTimer = new Timer();
          this._keyExchangeTimer.AutoReset = false;
          this._keyExchangeTimer.Elapsed += new ElapsedEventHandler(this.HandleKeyExchangeTimeout);
          this._keyExchangeTimer.Interval = 240000.0;
          break;
      }
    }

    private void HandleKeyExchangeTimeout(object sender, ElapsedEventArgs eventArgs)
    {
      this._keyExchangeTimer.Dispose();
      this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed, (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.ServerKeyExchangeFailed, new object[0])));
    }

    private void CleanAll()
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
        ;
    }

    private void SetState(RemoteSessionState newState, Exception reasion)
    {
      using (ServerRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        RemoteSessionState state = this._state;
        if (newState == state)
          return;
        this._state = newState;
        ServerRemoteSessionDSHandlerStateMachine._trace.WriteLine("state machine state transition: from state {0} to state {1}", (object) state, (object) this._state);
      }
    }
  }
}
