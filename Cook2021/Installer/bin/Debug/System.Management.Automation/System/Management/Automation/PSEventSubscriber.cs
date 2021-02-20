// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSEventSubscriber
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public class PSEventSubscriber : IEquatable<PSEventSubscriber>
  {
    private ExecutionContext context;
    private int subscriptionId;
    private object sourceObject;
    private string eventName;
    private string sourceIdentifier;
    private PSEventJob action;
    private PSEventReceivedEventHandler handlerDelegate;
    private bool supportEvent;
    private bool forwardEvent;

    internal PSEventSubscriber(
      ExecutionContext context,
      int id,
      object source,
      string eventName,
      string sourceIdentifier,
      bool supportEvent,
      bool forwardEvent)
    {
      this.context = context;
      this.subscriptionId = id;
      this.sourceObject = source;
      this.eventName = eventName;
      this.sourceIdentifier = sourceIdentifier;
      this.supportEvent = supportEvent;
      this.forwardEvent = forwardEvent;
    }

    internal PSEventSubscriber(
      ExecutionContext context,
      int id,
      object source,
      string eventName,
      string sourceIdentifier,
      ScriptBlock action,
      bool supportEvent,
      bool forwardEvent)
      : this(context, id, source, eventName, sourceIdentifier, supportEvent, forwardEvent)
    {
      if (action == null)
        return;
      ScriptBlock boundScriptBlock = context.Modules.CreateBoundScriptBlock(action, true);
      PSVariable psVariable = new PSVariable("script:Error", (object) new ArrayList(), ScopedItemOptions.Constant);
      SessionStateInternal sessionStateInternal = boundScriptBlock.SessionStateInternal;
      sessionStateInternal.GetScopeByID("script").SetVariable(psVariable.Name, (object) psVariable, false, true, sessionStateInternal, CommandOrigin.Internal);
      this.action = new PSEventJob((PSEventManager) context.Events, this, boundScriptBlock, sourceIdentifier);
      if (supportEvent)
        return;
      ((LocalRunspace) context.CurrentRunspace).JobRepository.Add((Job) this.action);
    }

    internal PSEventSubscriber(
      ExecutionContext context,
      int id,
      object source,
      string eventName,
      string sourceIdentifier,
      PSEventReceivedEventHandler handlerDelegate,
      bool supportEvent,
      bool forwardEvent)
      : this(context, id, source, eventName, sourceIdentifier, supportEvent, forwardEvent)
    {
      this.handlerDelegate = handlerDelegate;
    }

    public int SubscriptionId
    {
      get => this.subscriptionId;
      set => this.subscriptionId = value;
    }

    public object SourceObject => this.sourceObject;

    public string EventName => this.eventName;

    public string SourceIdentifier => this.sourceIdentifier;

    public PSEventJob Action => this.action;

    public PSEventReceivedEventHandler HandlerDelegate => this.handlerDelegate;

    public bool SupportEvent => this.supportEvent;

    public bool ForwardEvent => this.forwardEvent;

    public bool Equals(PSEventSubscriber other) => other != null && object.Equals((object) this.SubscriptionId, (object) other.SubscriptionId);

    public override int GetHashCode() => this.SubscriptionId;
  }
}
