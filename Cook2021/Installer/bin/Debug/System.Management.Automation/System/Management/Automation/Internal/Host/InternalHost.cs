// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.Host.InternalHost
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Internal.Host
{
  internal class InternalHost : PSHost, IHostSupportsInteractiveSession
  {
    private const string StringsBaseName = "InternalHostStrings";
    private const string ExitNonExistentNestedPromptErrorResource = "ExitNonExistentNestedPromptError";
    private const string EnterExitNestedPromptOutOfSyncResource = "EnterExitNestedPromptOutOfSync";
    private const string nestedPromptCounterVarName = "global:NestedPromptLevel";
    private const string currentlyExecutingCommandVarName = "CurrentlyExecutingCommand";
    private const string psBoundParametersVarName = "PSBoundParameters";
    private ObjectRef<PSHost> externalHostRef;
    private ObjectRef<InternalHostUserInterface> internalUIRef;
    private ExecutionContext executionContext;
    private string nameResult;
    private Version versionResult;
    private Guid idResult;
    private int nestedPromptCount;
    private Stack<InternalHost.PromptContextData> contextStack = new Stack<InternalHost.PromptContextData>();
    private readonly Guid zeroGuid;
    [TraceSource("InternalHost", "S.M.A.InternalHost")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (InternalHost), "S.M.A.InternalHost");

    internal InternalHost(PSHost externalHost, ExecutionContext executionContext)
    {
      this.externalHostRef = new ObjectRef<PSHost>(externalHost);
      this.executionContext = executionContext;
      this.internalUIRef = new ObjectRef<InternalHostUserInterface>(new InternalHostUserInterface(externalHost.UI, this));
      this.zeroGuid = new Guid(0, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0);
      this.idResult = this.zeroGuid;
    }

    public override string Name
    {
      get
      {
        if (string.IsNullOrEmpty(this.nameResult))
        {
          this.nameResult = this.externalHostRef.Value.Name;
          if (string.IsNullOrEmpty(this.nameResult))
            throw InternalHost.tracer.NewNotImplementedException();
        }
        return this.nameResult;
      }
    }

    public override Version Version
    {
      get
      {
        if (this.versionResult == (Version) null)
        {
          this.versionResult = this.externalHostRef.Value.Version;
          if (this.versionResult == (Version) null)
            throw InternalHost.tracer.NewNotImplementedException();
        }
        return this.versionResult;
      }
    }

    public override Guid InstanceId
    {
      get
      {
        if (this.idResult == this.zeroGuid)
        {
          this.idResult = this.externalHostRef.Value.InstanceId;
          if (this.idResult == this.zeroGuid)
            throw InternalHost.tracer.NewNotImplementedException();
        }
        return this.idResult;
      }
    }

    public override PSHostUserInterface UI => (PSHostUserInterface) this.internalUIRef.Value;

    internal InternalHostUserInterface InternalUI => this.internalUIRef.Value;

    public override CultureInfo CurrentCulture
    {
      get
      {
        CultureInfo cultureInfo = this.externalHostRef.Value.CurrentCulture;
        if (cultureInfo == null)
        {
          InternalHost.tracer.WriteLine("warning: host returned null -- using InvariantCulture", new object[0]);
          cultureInfo = CultureInfo.InvariantCulture;
        }
        return cultureInfo;
      }
    }

    public override CultureInfo CurrentUICulture
    {
      get
      {
        CultureInfo cultureInfo = this.externalHostRef.Value.CurrentUICulture;
        if (cultureInfo == null)
        {
          InternalHost.tracer.WriteLine("warning: host returned null -- using installed ui culture", new object[0]);
          cultureInfo = CultureInfo.InstalledUICulture;
        }
        return cultureInfo;
      }
    }

    public override void SetShouldExit(int exitCode) => this.externalHostRef.Value.SetShouldExit(exitCode);

    public override void EnterNestedPrompt() => this.EnterNestedPrompt((InternalCommand) null);

    internal void EnterNestedPrompt(InternalCommand callingCommand)
    {
      LocalRunspace localRunspace = (LocalRunspace) null;
      try
      {
        localRunspace = this.Runspace as LocalRunspace;
      }
      catch (PSNotImplementedException ex)
      {
      }
      if (localRunspace != null)
      {
        Pipeline currentlyRunningPipeline = this.Runspace.GetCurrentlyRunningPipeline();
        if (currentlyRunningPipeline != null && currentlyRunningPipeline == localRunspace.PulsePipeline)
          throw new InvalidOperationException();
      }
      if (this.nestedPromptCount < 0)
        throw InternalHost.tracer.NewInvalidOperationException("InternalHostStrings", "EnterExitNestedPromptOutOfSync");
      ++this.nestedPromptCount;
      this.executionContext.SetVariable("global:NestedPromptLevel", (object) this.nestedPromptCount);
      InternalHost.PromptContextData promptContextData = new InternalHost.PromptContextData();
      promptContextData.SavedContextData = this.executionContext.SaveContextData();
      promptContextData.SavedCurrentlyExecutingCommandVarValue = this.executionContext.GetVariable("CurrentlyExecutingCommand");
      promptContextData.SavedPSBoundParametersVarValue = this.executionContext.GetVariable("PSBoundParameters");
      promptContextData.RunspaceAvailability = this.Context.CurrentRunspace.RunspaceAvailability;
      if (callingCommand != null)
      {
        PSObject psObject = PSObject.AsPSObject((object) callingCommand);
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("CommandInfo", (object) callingCommand.CommandInfo));
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("StackTrace", (object) new StackTrace()));
        this.executionContext.SetVariable("CurrentlyExecutingCommand", (object) psObject);
      }
      this.contextStack.Push(promptContextData);
      this.executionContext.StepScript = false;
      this.executionContext.PSDebug = 0;
      this.executionContext.ResetShellFunctionErrorOutputPipe();
      this.Context.CurrentRunspace.UpdateRunspaceAvailability(RunspaceAvailability.AvailableForNestedCommand, true);
      try
      {
        this.externalHostRef.Value.EnterNestedPrompt();
      }
      catch
      {
        this.ExitNestedPromptHelper();
        throw;
      }
    }

    private void ExitNestedPromptHelper()
    {
      --this.nestedPromptCount;
      this.executionContext.SetVariable("global:NestedPromptLevel", (object) this.nestedPromptCount);
      if (this.contextStack.Count <= 0)
        return;
      InternalHost.PromptContextData promptContextData = this.contextStack.Pop();
      promptContextData.SavedContextData.RestoreContextData(this.executionContext);
      this.executionContext.SetVariable("CurrentlyExecutingCommand", promptContextData.SavedCurrentlyExecutingCommandVarValue);
      this.executionContext.SetVariable("PSBoundParameters", promptContextData.SavedPSBoundParametersVarValue);
      this.Context.CurrentRunspace.UpdateRunspaceAvailability(promptContextData.RunspaceAvailability, true);
    }

    public override void ExitNestedPrompt()
    {
      if (this.nestedPromptCount != 0)
      {
        try
        {
          this.externalHostRef.Value.ExitNestedPrompt();
        }
        finally
        {
          this.ExitNestedPromptHelper();
        }
        ExitNestedPromptException nestedPromptException = new ExitNestedPromptException();
        InternalHost.tracer.TraceException((Exception) nestedPromptException);
        throw nestedPromptException;
      }
    }

    public override PSObject PrivateData => this.externalHostRef.Value.PrivateData;

    public override void NotifyBeginApplication() => this.externalHostRef.Value.NotifyBeginApplication();

    public override void NotifyEndApplication() => this.externalHostRef.Value.NotifyEndApplication();

    private IHostSupportsInteractiveSession GetIHostSupportsInteractiveSession()
    {
      if (!(this.externalHostRef.Value is IHostSupportsInteractiveSession interactiveSession))
        throw new PSNotImplementedException();
      return interactiveSession;
    }

    public void PushRunspace(Runspace runspace) => this.GetIHostSupportsInteractiveSession().PushRunspace(runspace);

    public void PopRunspace() => this.GetIHostSupportsInteractiveSession().PopRunspace();

    public bool IsRunspacePushed => this.GetIHostSupportsInteractiveSession().IsRunspacePushed;

    public Runspace Runspace => this.GetIHostSupportsInteractiveSession().Runspace;

    internal bool HostInNestedPrompt() => this.nestedPromptCount > 0;

    internal void SetHostRef(PSHost psHost)
    {
      this.externalHostRef.Override(psHost);
      this.internalUIRef.Override(new InternalHostUserInterface(psHost.UI, this));
    }

    internal void RevertHostRef()
    {
      if (this.IsHostRefSet)
        return;
      this.externalHostRef.Revert();
      this.internalUIRef.Revert();
    }

    internal bool IsHostRefSet => this.externalHostRef.IsOverridden;

    internal ExecutionContext Context => this.executionContext;

    internal PSHost ExternalHost => this.externalHostRef.Value;

    internal int NestedPromptCount => this.nestedPromptCount;

    private struct PromptContextData
    {
      public object SavedCurrentlyExecutingCommandVarValue;
      public object SavedPSBoundParametersVarValue;
      public ExecutionContext.SavedContextData SavedContextData;
      public RunspaceAvailability RunspaceAvailability;
    }
  }
}
