// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ObjectEventRegistrationBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
  public abstract class ObjectEventRegistrationBase : PSCmdlet
  {
    private string sourceIdentifier = Guid.NewGuid().ToString();
    private ScriptBlock action;
    private PSObject messageData;
    private SwitchParameter supportEvent = new SwitchParameter();
    private SwitchParameter forward = new SwitchParameter();

    [Parameter(Position = 100)]
    public string SourceIdentifier
    {
      get => this.sourceIdentifier;
      set => this.sourceIdentifier = value;
    }

    [Parameter(Position = 101)]
    public ScriptBlock Action
    {
      get => this.action;
      set => this.action = value;
    }

    [Parameter]
    public PSObject MessageData
    {
      get => this.messageData;
      set => this.messageData = value;
    }

    [Parameter]
    public SwitchParameter SupportEvent
    {
      get => this.supportEvent;
      set => this.supportEvent = value;
    }

    [Parameter]
    public SwitchParameter Forward
    {
      get => this.forward;
      set => this.forward = value;
    }

    protected abstract object GetSourceObject();

    protected abstract string GetSourceObjectEventName();

    protected override void BeginProcessing()
    {
      if (!(bool) this.forward || this.action == null)
        return;
      this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.GetResourceString("EventingResources", "ActionAndForwardNotSupported")), "ACTION_AND_FORWARD_NOT_SUPPORTED", ErrorCategory.InvalidOperation, (object) null));
    }

    protected override void EndProcessing()
    {
      object obj = PSObject.Base(this.GetSourceObject());
      string sourceObjectEventName = this.GetSourceObjectEventName();
      try
      {
        if ((obj != null || sourceObjectEventName != null) && this.Events.GetEventSubscribers(this.sourceIdentifier).GetEnumerator().MoveNext())
        {
          this.WriteError(new ErrorRecord((Exception) new ArgumentException(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, ResourceManagerCache.GetResourceString("EventingResources", "SubscriberExists"), (object) this.sourceIdentifier)), "SUBSCRIBER_EXISTS", ErrorCategory.InvalidArgument, obj));
        }
        else
        {
          PSEventSubscriber psEventSubscriber = this.Events.SubscribeEvent(obj, sourceObjectEventName, this.sourceIdentifier, this.messageData, this.action, (bool) this.supportEvent, (bool) this.forward);
          if (this.action == null || (bool) this.supportEvent)
            return;
          this.WriteObject((object) psEventSubscriber.Action);
        }
      }
      catch (ArgumentException ex)
      {
        this.WriteError(new ErrorRecord((Exception) ex, "INVALID_REGISTRATION", ErrorCategory.InvalidArgument, obj));
      }
    }
  }
}
