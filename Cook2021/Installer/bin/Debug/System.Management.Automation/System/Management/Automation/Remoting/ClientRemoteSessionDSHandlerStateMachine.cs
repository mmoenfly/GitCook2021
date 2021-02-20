// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ClientRemoteSessionDSHandlerStateMachine
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Timers;

namespace System.Management.Automation.Remoting
{
  internal class ClientRemoteSessionDSHandlerStateMachine
  {
    [TraceSource("CRSessionFSM", "CRSessionFSM")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("CRSessionFSM", "CRSessionFSM");
    private EventHandler<RemoteSessionStateMachineEventArgs>[,] _stateMachineHandle;
    private Queue<RemoteSessionStateEventArgs> _clientRemoteSessionStateChangeQueue;
    private RemoteSessionState _state;
    private Queue<RemoteSessionStateMachineEventArgs> processPendingEventsQueue = new Queue<RemoteSessionStateMachineEventArgs>();
    private object syncObject = new object();
    private bool eventsInProcess;
    private Timer _keyExchangeTimer;
    private Guid id;

    private void ProcessEvents()
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        RemoteSessionStateMachineEventArgs machineEventArgs = (RemoteSessionStateMachineEventArgs) null;
        do
        {
          lock (this.syncObject)
          {
            if (this.processPendingEventsQueue.Count == 0)
            {
              this.eventsInProcess = false;
              break;
            }
            machineEventArgs = this.processPendingEventsQueue.Dequeue();
          }
          this.RaiseEventPrivate(machineEventArgs);
          this.RaiseStateMachineEvents();
        }
        while (this.eventsInProcess);
      }
    }

    private void RaiseStateMachineEvents()
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        while (this._clientRemoteSessionStateChangeQueue.Count > 0)
        {
          RemoteSessionStateEventArgs e = this._clientRemoteSessionStateChangeQueue.Dequeue();
          if (this.StateChanged != null)
            this.StateChanged((object) this, e);
        }
      }
    }

    private void SetStateHandler(object sender, RemoteSessionStateMachineEventArgs eventArgs)
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        switch (eventArgs.StateEvent)
        {
          case RemoteSessionEvent.NegotiationSendCompleted:
            this.SetState(RemoteSessionState.NegotiationSent, (Exception) null);
            break;
          case RemoteSessionEvent.NegotiationReceived:
            if (eventArgs.RemoteSessionCapability == null)
              throw ClientRemoteSessionDSHandlerStateMachine._trace.NewArgumentException(nameof (eventArgs));
            this.SetState(RemoteSessionState.NegotiationReceived, (Exception) null);
            break;
          case RemoteSessionEvent.NegotiationCompleted:
            this.SetState(RemoteSessionState.Established, (Exception) null);
            break;
          case RemoteSessionEvent.CloseCompleted:
            this.SetState(RemoteSessionState.Closed, eventArgs.Reason);
            break;
          case RemoteSessionEvent.CloseFailed:
            this.SetState(RemoteSessionState.Closed, eventArgs.Reason);
            break;
          case RemoteSessionEvent.ConnectFailed:
            this.SetState(RemoteSessionState.ClosingConnection, eventArgs.Reason);
            break;
          case RemoteSessionEvent.KeySent:
            if (this._state != RemoteSessionState.Established && this._state != RemoteSessionState.EstablishedAndKeyRequested)
              break;
            this.SetState(RemoteSessionState.EstablishedAndKeySent, eventArgs.Reason);
            this._keyExchangeTimer = new Timer();
            this._keyExchangeTimer.AutoReset = false;
            this._keyExchangeTimer.Elapsed += new ElapsedEventHandler(this.HandleKeyExchangeTimeout);
            this._keyExchangeTimer.Interval = 180000.0;
            break;
          case RemoteSessionEvent.KeyReceived:
            if (this._state != RemoteSessionState.EstablishedAndKeySent)
              break;
            if (this._keyExchangeTimer != null)
            {
              this._keyExchangeTimer.Enabled = false;
              this._keyExchangeTimer.Dispose();
              this._keyExchangeTimer = (Timer) null;
            }
            this.SetState(RemoteSessionState.EstablishedAndKeyExchanged, eventArgs.Reason);
            break;
          case RemoteSessionEvent.KeyRequested:
            if (this._state != RemoteSessionState.Established)
              break;
            this.SetState(RemoteSessionState.EstablishedAndKeyRequested, eventArgs.Reason);
            break;
        }
      }
    }

    private void HandleKeyExchangeTimeout(object sender, ElapsedEventArgs eventArgs)
    {
      this._keyExchangeTimer.Dispose();
      this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.KeyReceiveFailed, (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.ClientKeyExchangeFailed, new object[0])));
    }

    private void SetStateToClosedHandler(
      object sender,
      RemoteSessionStateMachineEventArgs eventArgs)
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        if (eventArgs.StateEvent == RemoteSessionEvent.NegotiationTimeout && this.State == RemoteSessionState.Established)
          return;
        this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, eventArgs.Reason));
      }
    }

    internal ClientRemoteSessionDSHandlerStateMachine()
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceConstructor((object) this))
      {
        this._clientRemoteSessionStateChangeQueue = new Queue<RemoteSessionStateEventArgs>();
        this._stateMachineHandle = new EventHandler<RemoteSessionStateMachineEventArgs>[15, 24];
        for (int index = 0; index < this._stateMachineHandle.GetLength(0); ++index)
        {
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle1;
          IntPtr num1;
          (stateMachineHandle1 = this._stateMachineHandle)[(int) (num1 = (IntPtr) index), 16] = stateMachineHandle1[(int) num1, 16] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoFatal);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle2;
          IntPtr num2;
          (stateMachineHandle2 = this._stateMachineHandle)[(int) (num2 = (IntPtr) index), 8] = stateMachineHandle2[(int) num2, 8] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoClose);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle3;
          IntPtr num3;
          (stateMachineHandle3 = this._stateMachineHandle)[(int) (num3 = (IntPtr) index), 10] = stateMachineHandle3[(int) num3, 10] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle4;
          IntPtr num4;
          (stateMachineHandle4 = this._stateMachineHandle)[(int) (num4 = (IntPtr) index), 9] = stateMachineHandle4[(int) num4, 9] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle5;
          IntPtr num5;
          (stateMachineHandle5 = this._stateMachineHandle)[(int) (num5 = (IntPtr) index), 13] = stateMachineHandle5[(int) num5, 13] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle6;
          IntPtr num6;
          (stateMachineHandle6 = this._stateMachineHandle)[(int) (num6 = (IntPtr) index), 14] = stateMachineHandle6[(int) num6, 14] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle7;
          IntPtr num7;
          (stateMachineHandle7 = this._stateMachineHandle)[(int) (num7 = (IntPtr) index), 15] = stateMachineHandle7[(int) num7, 15] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler);
          EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle8;
          IntPtr num8;
          (stateMachineHandle8 = this._stateMachineHandle)[(int) (num8 = (IntPtr) index), 1] = stateMachineHandle8[(int) num8, 1] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoStart);
        }
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle9;
        (stateMachineHandle9 = this._stateMachineHandle)[1, 3] = stateMachineHandle9[1, 3] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoNegotiationSending);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle10;
        (stateMachineHandle10 = this._stateMachineHandle)[4, 4] = stateMachineHandle10[4, 4] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle11;
        (stateMachineHandle11 = this._stateMachineHandle)[5, 5] = stateMachineHandle11[5, 5] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle12;
        (stateMachineHandle12 = this._stateMachineHandle)[6, 6] = stateMachineHandle12[6, 6] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle13;
        (stateMachineHandle13 = this._stateMachineHandle)[6, 12] = stateMachineHandle13[6, 12] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle14;
        (stateMachineHandle14 = this._stateMachineHandle)[2, 11] = stateMachineHandle14[2, 11] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle15;
        (stateMachineHandle15 = this._stateMachineHandle)[8, 9] = stateMachineHandle15[8, 9] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle16;
        (stateMachineHandle16 = this._stateMachineHandle)[10, 22] = stateMachineHandle16[10, 22] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle17;
        (stateMachineHandle17 = this._stateMachineHandle)[10, 18] = stateMachineHandle17[10, 18] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle18;
        (stateMachineHandle18 = this._stateMachineHandle)[10, 19] = stateMachineHandle18[10, 19] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle19;
        (stateMachineHandle19 = this._stateMachineHandle)[11, 20] = stateMachineHandle19[11, 20] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle20;
        (stateMachineHandle20 = this._stateMachineHandle)[13, 18] = stateMachineHandle20[13, 18] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle21;
        (stateMachineHandle21 = this._stateMachineHandle)[11, 21] = stateMachineHandle21[11, 21] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler);
        EventHandler<RemoteSessionStateMachineEventArgs>[,] stateMachineHandle22;
        (stateMachineHandle22 = this._stateMachineHandle)[13, 19] = stateMachineHandle22[13, 19] + new EventHandler<RemoteSessionStateMachineEventArgs>(this.SetStateToClosedHandler);
        for (int index1 = 0; index1 < this._stateMachineHandle.GetLength(0); ++index1)
        {
          for (int index2 = 0; index2 < this._stateMachineHandle.GetLength(1); ++index2)
          {
            if (this._stateMachineHandle[index1, index2] == null)
              this._stateMachineHandle[index1, index2] += new EventHandler<RemoteSessionStateMachineEventArgs>(this.DoClose);
          }
        }
        this.id = Guid.NewGuid();
        this.SetState(RemoteSessionState.Idle, (Exception) null);
      }
    }

    internal bool CanByPassRaiseEvent(RemoteSessionStateMachineEventArgs arg) => arg.StateEvent == RemoteSessionEvent.MessageReceived && (this._state == RemoteSessionState.Established || this._state == RemoteSessionState.EstablishedAndKeyReceived || (this._state == RemoteSessionState.EstablishedAndKeySent || this._state == RemoteSessionState.EstablishedAndKeyExchanged));

    internal void RaiseEvent(RemoteSessionStateMachineEventArgs arg)
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        lock (this.syncObject)
        {
          ClientRemoteSessionDSHandlerStateMachine._trace.WriteLine("Event recieved : {0} for {1}", (object) arg.StateEvent, (object) this.id);
          this.processPendingEventsQueue.Enqueue(arg);
          if (this.eventsInProcess)
            return;
          this.eventsInProcess = true;
        }
        this.ProcessEvents();
      }
    }

    private void RaiseEventPrivate(RemoteSessionStateMachineEventArgs arg)
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        if (arg == null)
          throw ClientRemoteSessionDSHandlerStateMachine._trace.NewArgumentNullException(nameof (arg));
        EventHandler<RemoteSessionStateMachineEventArgs> eventHandler = this._stateMachineHandle[(int) this.State, (int) arg.StateEvent];
        if (eventHandler == null)
          return;
        ClientRemoteSessionDSHandlerStateMachine._trace.WriteLine("Before calling state machine event handler: state = {0}, event = {1}, id = {2}", (object) this.State, (object) arg.StateEvent, (object) this.id);
        eventHandler((object) this, arg);
        ClientRemoteSessionDSHandlerStateMachine._trace.WriteLine("After calling state machine event handler: state = {0}, event = {1}, id = {2}", (object) this.State, (object) arg.StateEvent, (object) this.id);
      }
    }

    internal RemoteSessionState State
    {
      get
      {
        using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceProperty())
          return this._state;
      }
    }

    internal event EventHandler<RemoteSessionStateEventArgs> StateChanged;

    private void DoStart(object sender, RemoteSessionStateMachineEventArgs arg)
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
        if (this.State != RemoteSessionState.Idle)
          return;
        this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.NegotiationSending));
      }
    }

    private void DoNegotiationSending(object sender, RemoteSessionStateMachineEventArgs arg)
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
        this.SetState(RemoteSessionState.NegotiationSending, (Exception) null);
    }

    private void DoClose(object sender, RemoteSessionStateMachineEventArgs arg)
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceEventHandlers())
      {
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
            this.SetState(RemoteSessionState.ClosingConnection, arg.Reason);
            goto case RemoteSessionState.ClosingConnection;
          case RemoteSessionState.ClosingConnection:
          case RemoteSessionState.Closed:
            this.CleanAll();
            break;
          default:
            this.SetState(RemoteSessionState.Closed, (Exception) new PSRemotingTransportException(arg.Reason, PSRemotingErrorId.ForceClosed, new object[0]));
            goto case RemoteSessionState.ClosingConnection;
        }
      }
    }

    private void DoFatal(object sender, RemoteSessionStateMachineEventArgs eventArgs) => this.RaiseEvent(new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, (Exception) new PSRemotingDataStructureException(eventArgs.Reason, PSRemotingErrorId.FatalErrorCausingClose, new object[0])));

    private void CleanAll()
    {
    }

    private void SetState(RemoteSessionState newState, Exception reason)
    {
      using (ClientRemoteSessionDSHandlerStateMachine._trace.TraceMethod())
      {
        RemoteSessionState state = this._state;
        if (newState == state)
          return;
        this._state = newState;
        ClientRemoteSessionDSHandlerStateMachine._trace.WriteLine("state machine state transition: from state {0} to state {1}", (object) state, (object) this._state);
        this._clientRemoteSessionStateChangeQueue.Enqueue(new RemoteSessionStateEventArgs(new RemoteSessionStateInfo(this._state, reason)));
      }
    }
  }
}
