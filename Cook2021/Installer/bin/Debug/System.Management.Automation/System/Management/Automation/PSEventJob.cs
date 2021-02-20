// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSEventJob
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;

namespace System.Management.Automation
{
  public class PSEventJob : Job
  {
    private PSEventManager eventManager;
    private PSEventSubscriber subscriber;
    private int highestErrorIndex;
    private string statusMessage;
    private bool moreData;
    private ScriptBlock action;

    public PSEventJob(
      PSEventManager eventManager,
      PSEventSubscriber subscriber,
      ScriptBlock action,
      string name)
      : base(action == null ? (string) null : action.ToString(), name)
    {
      if (eventManager == null)
        throw new ArgumentNullException(nameof (eventManager));
      if (subscriber == null)
        throw new ArgumentNullException(nameof (subscriber));
      this.action = action;
      this.eventManager = eventManager;
      this.subscriber = subscriber;
    }

    public PSModuleInfo Module => this.action.Module;

    public override void StopJob() => this.eventManager.UnsubscribeEvent(this.subscriber);

    public override string StatusMessage => this.statusMessage;

    public override bool HasMoreData => this.moreData;

    public override string Location => (string) null;

    internal ScriptBlock ScriptBlock => this.action;

    internal void Invoke(PSEventSubscriber eventSubscriber, PSEventArgs eventArgs)
    {
      if (this.IsFinishedState(this.JobStateInfo.State))
        return;
      this.SetJobState(JobState.Running);
      SessionState publicSessionState = this.action.SessionStateInternal.PublicSessionState;
      publicSessionState.PSVariable.Set(nameof (eventSubscriber), (object) eventSubscriber);
      publicSessionState.PSVariable.Set("event", (object) eventArgs);
      publicSessionState.PSVariable.Set("sender", eventArgs.Sender);
      publicSessionState.PSVariable.Set(nameof (eventArgs), (object) eventArgs.SourceEventArgs);
      ArrayList resultList = new ArrayList();
      try
      {
        this.action.InvokeWithPipe(false, false, (object) AutomationNull.Value, (object) AutomationNull.Value, (object) AutomationNull.Value, (Pipe) null, ref resultList, eventArgs.SourceArgs);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (!(ex is PipelineStoppedException))
        {
          this.LogErrorsAndOutput(resultList, publicSessionState);
          this.SetJobState(JobState.Failed);
        }
        throw;
      }
      this.LogErrorsAndOutput(resultList, publicSessionState);
      this.moreData = true;
    }

    internal void NotifyJobStopped()
    {
      this.SetJobState(JobState.Stopped);
      this.moreData = false;
    }

    private void LogErrorsAndOutput(ArrayList results, SessionState actionState)
    {
      foreach (object result in results)
      {
        this.Output.Add(PSObject.AsPSObject(result));
        this.Results.Add(new PSStreamObject(PSStreamObjectType.Output, result));
      }
      this.Error.Clear();
      int num = 0;
      ArrayList arrayList = (ArrayList) actionState.PSVariable.Get("error").Value;
      arrayList.Reverse();
      foreach (ErrorRecord errorRecord in arrayList)
      {
        this.Error.Add(errorRecord);
        if (num == this.highestErrorIndex)
        {
          this.Results.Add(new PSStreamObject(PSStreamObjectType.Error, (object) errorRecord));
          ++this.highestErrorIndex;
        }
        ++num;
      }
    }
  }
}
