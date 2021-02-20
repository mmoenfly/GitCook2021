// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Server.OutOfProcessMediator
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.IO;
using System.Management.Automation.Internal;
using System.Security.Principal;
using System.Threading;

namespace System.Management.Automation.Remoting.Server
{
  internal sealed class OutOfProcessMediator
  {
    private static IETWTracer ETWTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Transport);
    private TextReader originalStdIn;
    private OutOfProcessTextWriter originalStdOut;
    private OutOfProcessTextWriter originalStdErr;
    private OutOfProcessServerSessionTransportManager sessionTM;
    private OutOfProcessUtils.DataProcessingDelegates callbacks;
    private static object SyncObject = new object();
    private static OutOfProcessMediator SingletonInstance;

    private OutOfProcessMediator()
    {
      this.originalStdIn = Console.In;
      Console.SetIn(TextReader.Null);
      this.originalStdOut = new OutOfProcessTextWriter(Console.Out);
      Console.SetOut(TextWriter.Null);
      this.originalStdErr = new OutOfProcessTextWriter(Console.Error);
      Console.SetError(TextWriter.Null);
      this.callbacks = new OutOfProcessUtils.DataProcessingDelegates();
      this.callbacks.DataPacketReceived += new OutOfProcessUtils.DataPacketReceived(this.OnDataPacketReceived);
      this.callbacks.DataAckPacketReceived += new OutOfProcessUtils.DataAckPacketReceived(this.OnDataAckPacketReceived);
      this.callbacks.CommandCreationPacketReceived += new OutOfProcessUtils.CommandCreationPacketReceived(this.OnCommandCreationPacketReceived);
      this.callbacks.CommandCreationAckReceived += new OutOfProcessUtils.CommandCreationAckReceived(this.OnCommandCreationAckReceived);
      this.callbacks.ClosePacketReceived += new OutOfProcessUtils.ClosePacketReceived(this.OnClosePacketReceived);
      this.callbacks.CloseAckPacketReceived += new OutOfProcessUtils.CloseAckPacketReceived(this.OnCloseAckPacketReceived);
      this.callbacks.SignalPacketReceived += new OutOfProcessUtils.SignalPacketReceived(this.OnSignalPacketReceived);
      this.callbacks.SignalAckPacketReceived += new OutOfProcessUtils.SignalAckPacketReceived(this.OnSignalAckPacketReceived);
    }

    private void Start(string initialCommand)
    {
      WindowsIdentity current = WindowsIdentity.GetCurrent();
      PSSenderInfo senderInfo = new PSSenderInfo(new PSPrincipal(new PSIdentity("", true, current.Name, (PSCertificateDetails) null), current), "http://localhost");
      this.sessionTM = new OutOfProcessServerSessionTransportManager(this.originalStdOut);
      ServerRemoteSession.CreateServerRemoteSession(senderInfo, initialCommand, (AbstractServerSessionTransportManager) this.sessionTM);
      try
      {
        while (true)
        {
          string str = this.originalStdIn.ReadLine();
          if (!string.IsNullOrEmpty(str))
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.ProcessingThreadStart), (object) str);
          else
            break;
        }
        this.sessionTM.Close((Exception) null);
        throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
        {
          (object) string.Empty
        });
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        OutOfProcessMediator.ETWTracer.OperationalChannel.WriteError(PSEventId.TransportError, PSOpcode.Open, PSTask.None, (object) Guid.Empty, (object) Guid.Empty, (object) 4000, (object) ex.Message, (object) ex.StackTrace);
        OutOfProcessMediator.ETWTracer.AnalyticChannel.WriteError(PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None, (object) Guid.Empty, (object) Guid.Empty, (object) 4000, (object) ex.Message, (object) ex.StackTrace);
        this.originalStdErr.WriteLine(ex.Message);
        Environment.Exit(4000);
      }
    }

    private void ProcessingThreadStart(object state)
    {
      try
      {
        OutOfProcessUtils.ProcessData(state as string, this.callbacks);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        OutOfProcessMediator.ETWTracer.OperationalChannel.WriteError(PSEventId.TransportError, PSOpcode.Open, PSTask.None, (object) Guid.Empty, (object) Guid.Empty, (object) 4000, (object) ex.Message, (object) ex.StackTrace);
        OutOfProcessMediator.ETWTracer.AnalyticChannel.WriteError(PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None, (object) Guid.Empty, (object) Guid.Empty, (object) 4000, (object) ex.Message, (object) ex.StackTrace);
        this.originalStdErr.WriteLine(ex.Message + ex.StackTrace);
        Environment.Exit(4000);
      }
    }

    private void OnDataPacketReceived(byte[] rawData, string stream, Guid psGuid)
    {
      string stream1 = "stdin";
      if (stream.Equals(DataPriorityType.PromptResponse.ToString(), StringComparison.OrdinalIgnoreCase))
        stream1 = "pr";
      if (Guid.Empty == psGuid)
        this.sessionTM.ProcessRawData(rawData, stream1);
      else
        this.sessionTM.GetCommandTransportManager(psGuid)?.ProcessRawData(rawData, stream1);
    }

    private void OnDataAckPacketReceived(Guid psGuid) => throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
    {
      (object) "DataAck"
    });

    private void OnCommandCreationPacketReceived(Guid psGuid) => this.sessionTM.CreateCommandTransportManager(psGuid);

    private void OnCommandCreationAckReceived(Guid psGuid) => throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
    {
      (object) "CommandAck"
    });

    private void OnSignalPacketReceived(Guid psGuid)
    {
      if (psGuid == Guid.Empty)
        throw new PSRemotingTransportException(PSRemotingErrorId.IPCNoSignalForSession, new object[1]
        {
          (object) "Signal"
        });
      this.sessionTM.GetCommandTransportManager(psGuid)?.Close((Exception) null);
      this.originalStdOut.WriteLine(OutOfProcessUtils.CreateSignalAckPacket(psGuid));
    }

    private void OnSignalAckPacketReceived(Guid psGuid) => throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
    {
      (object) "SignalAck"
    });

    private void OnClosePacketReceived(Guid psGuid)
    {
      if (psGuid == Guid.Empty)
        this.sessionTM.Close((Exception) null);
      else
        this.sessionTM.GetCommandTransportManager(psGuid)?.Close((Exception) null);
      this.originalStdOut.WriteLine(OutOfProcessUtils.CreateCloseAckPacket(psGuid));
    }

    private void OnCloseAckPacketReceived(Guid psGuid) => throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
    {
      (object) "CloseAck"
    });

    internal static void Run(string initialCommand)
    {
      lock (OutOfProcessMediator.SyncObject)
      {
        if (OutOfProcessMediator.SingletonInstance != null)
          return;
        OutOfProcessMediator.SingletonInstance = new OutOfProcessMediator();
      }
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OutOfProcessMediator.AppDomainUnhandledException);
      OutOfProcessMediator.SingletonInstance.Start(initialCommand);
    }

    internal static void AppDomainUnhandledException(
      object sender,
      UnhandledExceptionEventArgs args)
    {
      Exception exceptionObject = (Exception) args.ExceptionObject;
      IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.ManagedPlugin);
      etwTracer.OperationalChannel.WriteError(PSEventId.AppDomainUnhandledException, PSOpcode.Close, PSTask.None, (object) exceptionObject.GetType().ToString(), (object) exceptionObject.Message, (object) exceptionObject.StackTrace);
      etwTracer.AnalyticChannel.WriteError(PSEventId.AppDomainUnhandledException_Analytic, PSOpcode.Close, PSTask.None, (object) exceptionObject.GetType().ToString(), (object) exceptionObject.Message, (object) exceptionObject.StackTrace);
    }
  }
}
