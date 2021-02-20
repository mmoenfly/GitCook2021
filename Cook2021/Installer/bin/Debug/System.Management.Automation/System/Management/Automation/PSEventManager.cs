// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSEventManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  public abstract class PSEventManager
  {
    private int nextEventId = 1;
    private PSEventArgsCollection receivedEvents = new PSEventArgsCollection();

    protected int GetNextEventId() => this.nextEventId++;

    public PSEventArgsCollection ReceivedEvents => this.receivedEvents;

    public abstract List<PSEventSubscriber> Subscribers { get; }

    protected abstract PSEventArgs CreateEvent(
      string sourceIdentifier,
      object sender,
      object[] args,
      PSObject extraData);

    public PSEventArgs GenerateEvent(
      string sourceIdentifier,
      object sender,
      object[] args,
      PSObject extraData)
    {
      return this.GenerateEvent(sourceIdentifier, sender, args, extraData, false);
    }

    internal PSEventArgs GenerateEvent(
      string sourceIdentifier,
      object sender,
      object[] args,
      PSObject extraData,
      bool processSynchronously)
    {
      PSEventArgs newEvent = this.CreateEvent(sourceIdentifier, sender, args, extraData);
      this.ProcessNewEvent(newEvent, processSynchronously);
      return newEvent;
    }

    internal abstract void AddForwardedEvent(PSEventArgs forwardedEvent);

    protected abstract void ProcessNewEvent(PSEventArgs newEvent, bool processSynchronously);

    public abstract IEnumerable<PSEventSubscriber> GetEventSubscribers(
      string sourceIdentifier);

    public abstract PSEventSubscriber SubscribeEvent(
      object source,
      string eventName,
      string sourceIdentifier,
      PSObject data,
      ScriptBlock action,
      bool supportEvent,
      bool forwardEvent);

    public abstract PSEventSubscriber SubscribeEvent(
      object source,
      string eventName,
      string sourceIdentifier,
      PSObject data,
      PSEventReceivedEventHandler handlerDelegate,
      bool supportEvent,
      bool forwardEvent);

    public abstract void UnsubscribeEvent(PSEventSubscriber subscriber);

    internal abstract event EventHandler<PSEventArgs> ForwardEvent;
  }
}
