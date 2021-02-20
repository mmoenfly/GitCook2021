// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSRemoteEventManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class PSRemoteEventManager : PSEventManager
  {
    private string computerName;
    private Guid runspaceId;

    internal PSRemoteEventManager(string computerName, Guid runspaceId)
    {
      this.computerName = computerName;
      this.runspaceId = runspaceId;
    }

    public override List<PSEventSubscriber> Subscribers => throw new NotSupportedException(ResourceManagerCache.GetResourceString("EventingResources", "RemoteOperationNotSupported"));

    protected override PSEventArgs CreateEvent(
      string sourceIdentifier,
      object sender,
      object[] args,
      PSObject extraData)
    {
      return new PSEventArgs((string) null, this.runspaceId, this.GetNextEventId(), sourceIdentifier, sender, args, extraData);
    }

    internal override void AddForwardedEvent(PSEventArgs forwardedEvent)
    {
      forwardedEvent.EventIdentifier = this.GetNextEventId();
      forwardedEvent.ForwardEvent = false;
      if (forwardedEvent.ComputerName == null || forwardedEvent.ComputerName.Length == 0)
      {
        forwardedEvent.ComputerName = this.computerName;
        forwardedEvent.RunspaceId = this.runspaceId;
      }
      this.ProcessNewEvent(forwardedEvent, false);
    }

    protected override void ProcessNewEvent(PSEventArgs newEvent, bool processSynchronously)
    {
      lock (this.ReceivedEvents.SyncRoot)
      {
        if (newEvent.ForwardEvent)
          this.OnForwardEvent(newEvent);
        else
          this.ReceivedEvents.Add(newEvent);
      }
    }

    public override IEnumerable<PSEventSubscriber> GetEventSubscribers(
      string sourceIdentifier)
    {
      throw new NotSupportedException(ResourceManagerCache.GetResourceString("EventingResources", "RemoteOperationNotSupported"));
    }

    public override PSEventSubscriber SubscribeEvent(
      object source,
      string eventName,
      string sourceIdentifier,
      PSObject data,
      ScriptBlock action,
      bool supportEvent,
      bool forwardEvent)
    {
      throw new NotSupportedException(ResourceManagerCache.GetResourceString("EventingResources", "RemoteOperationNotSupported"));
    }

    public override PSEventSubscriber SubscribeEvent(
      object source,
      string eventName,
      string sourceIdentifier,
      PSObject data,
      PSEventReceivedEventHandler handlerDelegate,
      bool supportEvent,
      bool forwardEvent)
    {
      throw new NotSupportedException(ResourceManagerCache.GetResourceString("EventingResources", "RemoteOperationNotSupported"));
    }

    public override void UnsubscribeEvent(PSEventSubscriber subscriber) => throw new NotSupportedException(ResourceManagerCache.GetResourceString("EventingResources", "RemoteOperationNotSupported"));

    internal override event EventHandler<PSEventArgs> ForwardEvent;

    protected virtual void OnForwardEvent(PSEventArgs e)
    {
      EventHandler<PSEventArgs> forwardEvent = this.ForwardEvent;
      if (forwardEvent == null)
        return;
      forwardEvent((object) this, e);
    }
  }
}
