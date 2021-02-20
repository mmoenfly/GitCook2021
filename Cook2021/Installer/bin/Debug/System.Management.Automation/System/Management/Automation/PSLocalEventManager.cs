// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSLocalEventManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace System.Management.Automation
{
  internal class PSLocalEventManager : PSEventManager, IDisposable
  {
    private Dictionary<PSEventSubscriber, Delegate> eventSubscribers;
    private Queue<EventAction> actionQueue;
    private ExecutionContext context;
    private int nextSubscriptionId = 1;
    private double throttleLimit = 1.0;
    private int throttleChecks;
    private AssemblyBuilder eventAssembly;
    private ModuleBuilder eventModule;
    private bool debugMode;
    private int typeId;
    private object actionProcessingLock = new object();
    private EventAction processingAction;

    internal PSLocalEventManager(ExecutionContext context)
    {
      this.eventSubscribers = new Dictionary<PSEventSubscriber, Delegate>();
      this.actionQueue = new Queue<EventAction>();
      this.context = context;
    }

    public override List<PSEventSubscriber> Subscribers
    {
      get
      {
        List<PSEventSubscriber> psEventSubscriberList = new List<PSEventSubscriber>();
        lock (this.eventSubscribers)
        {
          foreach (PSEventSubscriber key in this.eventSubscribers.Keys)
            psEventSubscriberList.Add(key);
        }
        return psEventSubscriberList;
      }
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
      PSEventSubscriber subscriber = new PSEventSubscriber(this.context, this.nextSubscriptionId++, source, eventName, sourceIdentifier, action, supportEvent, forwardEvent);
      this.ProcessNewSubscriber(subscriber, source, eventName, sourceIdentifier, data, supportEvent, forwardEvent);
      return subscriber;
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
      PSEventSubscriber subscriber = new PSEventSubscriber(this.context, this.nextSubscriptionId++, source, eventName, sourceIdentifier, handlerDelegate, supportEvent, forwardEvent);
      this.ProcessNewSubscriber(subscriber, source, eventName, sourceIdentifier, data, supportEvent, forwardEvent);
      return subscriber;
    }

    private void ProcessNewSubscriber(
      PSEventSubscriber subscriber,
      object source,
      string eventName,
      string sourceIdentifier,
      PSObject data,
      bool supportEvent,
      bool forwardEvent)
    {
      Delegate handler = (Delegate) null;
      if (this.eventAssembly == null)
      {
        this.debugMode = new StackFrame(0, true).GetFileName() != null;
        this.eventAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("PSEventHandler"), AssemblyBuilderAccess.Run);
      }
      if (this.eventModule == null)
        this.eventModule = this.eventAssembly.DefineDynamicModule("PSGenericEventModule", this.debugMode);
      if (source != null)
      {
        if (!(source is Type type))
          type = source.GetType();
        BindingFlags bindingAttr = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        EventInfo eventInfo = type.GetEvent(eventName, bindingAttr);
        if (eventInfo == null)
          throw new ArgumentException(ResourceManagerCache.FormatResourceString("EventingResources", "CouldNotFindEvent", (object) eventName), nameof (eventName));
        if (sourceIdentifier != null && sourceIdentifier.StartsWith("PowerShell.", StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException(ResourceManagerCache.FormatResourceString("EventingResources", "ReservedIdentifier", (object) sourceIdentifier), nameof (sourceIdentifier));
        if (type.GetProperty("EnableRaisingEvents") != null)
          type.InvokeMember("EnableRaisingEvents", BindingFlags.SetProperty, (Binder) null, source, new object[1]
          {
            (object) true
          }, CultureInfo.CurrentCulture);
        if (source is ManagementEventWatcher managementEventWatcher)
          managementEventWatcher.Start();
        MethodInfo method = eventInfo.EventHandlerType.GetMethod("Invoke");
        if (method.ReturnType != typeof (void))
          throw new ArgumentException(ResourceManagerCache.GetResourceString("EventingResources", "NonVoidDelegateNotSupported"), nameof (eventName));
        object eventHandler = this.GenerateEventHandler((PSEventManager) this, source, sourceIdentifier, data, method);
        handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, eventHandler, "EventDelegate");
        eventInfo.AddEventHandler(source, handler);
      }
      lock (this.eventSubscribers)
        this.eventSubscribers[subscriber] = handler;
    }

    public override void UnsubscribeEvent(PSEventSubscriber subscriber)
    {
      if (subscriber == null)
        throw new ArgumentNullException(nameof (subscriber));
      Delegate handler = (Delegate) null;
      lock (this.eventSubscribers)
        handler = this.eventSubscribers[subscriber];
      if ((object) handler != null && subscriber.SourceObject != null)
      {
        if (!(subscriber.SourceObject is Type type))
          type = subscriber.SourceObject.GetType();
        BindingFlags bindingAttr = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        EventInfo eventInfo = type.GetEvent(subscriber.EventName, bindingAttr);
        if (eventInfo != null && (object) handler != null)
          eventInfo.RemoveEventHandler(subscriber.SourceObject, handler);
      }
      this.DrainPendingActions(subscriber);
      if (subscriber.Action != null)
        subscriber.Action.NotifyJobStopped();
      lock (this.eventSubscribers)
      {
        this.eventSubscribers[subscriber] = (Delegate) null;
        this.eventSubscribers.Remove(subscriber);
      }
    }

    protected override PSEventArgs CreateEvent(
      string sourceIdentifier,
      object sender,
      object[] args,
      PSObject extraData)
    {
      return new PSEventArgs((string) null, this.context.CurrentRunspace.InstanceId, this.GetNextEventId(), sourceIdentifier, sender, args, extraData);
    }

    internal override void AddForwardedEvent(PSEventArgs forwardedEvent)
    {
      forwardedEvent.EventIdentifier = this.GetNextEventId();
      this.ProcessNewEvent(forwardedEvent, false);
    }

    protected override void ProcessNewEvent(PSEventArgs newEvent, bool processSynchronously)
    {
      if (processSynchronously)
        this.ProcessNewEventImplementation(newEvent);
      else
        ThreadPool.QueueUserWorkItem((WaitCallback) (unused => this.ProcessNewEventImplementation(newEvent)));
    }

    private void ProcessNewEventImplementation(PSEventArgs newEvent)
    {
      bool flag = false;
      foreach (PSEventSubscriber eventSubscriber in this.GetEventSubscribers(newEvent.SourceIdentifier))
      {
        newEvent.ForwardEvent = eventSubscriber.ForwardEvent;
        if (eventSubscriber.Action != null)
        {
          this.AddAction(new EventAction(eventSubscriber, newEvent));
          flag = true;
        }
        else if (eventSubscriber.HandlerDelegate != null)
        {
          eventSubscriber.HandlerDelegate(newEvent.Sender, newEvent);
          flag = true;
        }
      }
      if (flag)
        return;
      if (newEvent.ForwardEvent)
      {
        this.OnForwardEvent(newEvent);
      }
      else
      {
        lock (this.ReceivedEvents.SyncRoot)
          this.ReceivedEvents.Add(newEvent);
      }
    }

    private void AddAction(EventAction action)
    {
      lock (((ICollection) this.actionQueue).SyncRoot)
        this.actionQueue.Enqueue(action);
      this.PulseEngine();
    }

    private void PulseEngine()
    {
      try
      {
        ((RunspaceBase) this.context.CurrentRunspace).Pulse();
      }
      catch (ObjectDisposedException ex)
      {
      }
    }

    internal void ProcessPendingActions()
    {
      if (this.actionQueue.Count == 0)
        return;
      if (this.IsExecutingEventAction)
        return;
      try
      {
        lock (this.actionProcessingLock)
        {
          if (this.IsExecutingEventAction)
            return;
          int num = 0;
          for (++this.throttleChecks; this.throttleLimit * (double) this.throttleChecks >= (double) num; ++num)
          {
            EventAction nextAction;
            lock (((ICollection) this.actionQueue).SyncRoot)
            {
              if (this.actionQueue.Count == 0)
                return;
              nextAction = this.actionQueue.Dequeue();
            }
            this.InvokeAction(nextAction);
          }
          if (num <= 0)
            return;
          this.throttleChecks = 0;
        }
      }
      finally
      {
        if (this.actionQueue.Count > 0)
          ThreadPool.QueueUserWorkItem((WaitCallback) (unused =>
          {
            Thread.Sleep(100);
            this.PulseEngine();
          }));
      }
    }

    internal void DrainPendingActions(PSEventSubscriber subscriber)
    {
      if (this.actionQueue.Count == 0)
        return;
      lock (this.actionProcessingLock)
      {
        lock (((ICollection) this.actionQueue).SyncRoot)
        {
          if (this.actionQueue.Count == 0)
            return;
          EventAction[] array = this.actionQueue.ToArray();
          this.actionQueue.Clear();
          foreach (EventAction nextAction in array)
          {
            if (nextAction.Sender == subscriber && nextAction != this.processingAction)
            {
              while (this.IsExecutingEventAction)
                Thread.Sleep(100);
              this.InvokeAction(nextAction);
            }
            else
              this.actionQueue.Enqueue(nextAction);
          }
        }
      }
    }

    private void InvokeAction(EventAction nextAction)
    {
      lock (this.actionProcessingLock)
      {
        this.processingAction = nextAction;
        SessionStateInternal engineSessionState = this.context.EngineSessionState;
        this.context.EngineSessionState = nextAction.Sender.Action.ScriptBlock.SessionStateInternal;
        Runspace defaultRunspace = Runspace.DefaultRunspace;
        try
        {
          Runspace.DefaultRunspace = this.context.CurrentRunspace;
          nextAction.Sender.Action.Invoke(nextAction.Sender, nextAction.Args);
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
          if (!(ex is PipelineStoppedException))
            return;
          this.AddAction(nextAction);
        }
        finally
        {
          Runspace.DefaultRunspace = defaultRunspace;
          this.context.EngineSessionState = engineSessionState;
          this.processingAction = (EventAction) null;
        }
      }
    }

    internal bool IsExecutingEventAction => this.processingAction != null;

    public override IEnumerable<PSEventSubscriber> GetEventSubscribers(
      string sourceIdentifier)
    {
      List<PSEventSubscriber> psEventSubscriberList = new List<PSEventSubscriber>();
      lock (this.eventSubscribers)
      {
        foreach (PSEventSubscriber key in this.eventSubscribers.Keys)
        {
          if (string.Equals(key.SourceIdentifier, sourceIdentifier, StringComparison.OrdinalIgnoreCase))
            psEventSubscriberList.Add(key);
        }
      }
      return (IEnumerable<PSEventSubscriber>) psEventSubscriberList;
    }

    private object GenerateEventHandler(
      PSEventManager eventManager,
      object sender,
      string sourceIdentifier,
      PSObject data,
      MethodInfo invokeSignature)
    {
      int length = invokeSignature.GetParameters().Length;
      StackFrame stackFrame = new StackFrame(0, true);
      ISymbolDocumentWriter document = (ISymbolDocumentWriter) null;
      if (this.debugMode)
        document = this.eventModule.DefineDocument(stackFrame.GetFileName(), Guid.Empty, Guid.Empty, Guid.Empty);
      TypeBuilder typeBuilder = this.eventModule.DefineType("PSEventHandler_" + (object) this.typeId, TypeAttributes.Public, typeof (PSEventHandler));
      ++this.typeId;
      ConstructorInfo constructor = typeof (PSEventHandler).GetConstructor(new Type[4]
      {
        typeof (PSEventManager),
        typeof (object),
        typeof (string),
        typeof (PSObject)
      });
      if (this.debugMode)
        this.eventAssembly.SetCustomAttribute(new CustomAttributeBuilder(typeof (DebuggableAttribute).GetConstructor(new Type[1]
        {
          typeof (DebuggableAttribute.DebuggingModes)
        }), new object[1]
        {
          (object) (DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations)
        }));
      ILGenerator ilGenerator1 = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[4]
      {
        typeof (PSEventManager),
        typeof (object),
        typeof (string),
        typeof (PSObject)
      }).GetILGenerator();
      ilGenerator1.Emit(OpCodes.Ldarg_0);
      ilGenerator1.Emit(OpCodes.Ldarg_1);
      ilGenerator1.Emit(OpCodes.Ldarg_2);
      ilGenerator1.Emit(OpCodes.Ldarg_3);
      ilGenerator1.Emit(OpCodes.Ldarg, 4);
      ilGenerator1.Emit(OpCodes.Call, constructor);
      ilGenerator1.Emit(OpCodes.Ret);
      Type[] parameterTypes = new Type[length];
      int index1 = 0;
      foreach (ParameterInfo parameter in invokeSignature.GetParameters())
      {
        parameterTypes[index1] = parameter.ParameterType;
        ++index1;
      }
      MethodBuilder methodBuilder = typeBuilder.DefineMethod("EventDelegate", MethodAttributes.Public, CallingConventions.Standard, invokeSignature.ReturnType, parameterTypes);
      int position = 1;
      foreach (ParameterInfo parameter in invokeSignature.GetParameters())
      {
        methodBuilder.DefineParameter(position, parameter.Attributes, parameter.Name);
        ++position;
      }
      ILGenerator ilGenerator2 = methodBuilder.GetILGenerator();
      LocalBuilder localBuilder = ilGenerator2.DeclareLocal(typeof (object[]));
      if (this.debugMode)
      {
        localBuilder.SetLocalSymInfo("args");
        ilGenerator2.MarkSequencePoint(document, stackFrame.GetFileLineNumber() - 1, 1, stackFrame.GetFileLineNumber(), 100);
      }
      ilGenerator2.Emit(OpCodes.Ldc_I4, length);
      ilGenerator2.Emit(OpCodes.Newarr, typeof (object));
      ilGenerator2.Emit(OpCodes.Stloc_0);
      for (int index2 = 1; index2 <= length; ++index2)
      {
        if (this.debugMode)
          ilGenerator2.MarkSequencePoint(document, stackFrame.GetFileLineNumber() - 1, 1, stackFrame.GetFileLineNumber(), 100);
        ilGenerator2.Emit(OpCodes.Ldloc_0);
        ilGenerator2.Emit(OpCodes.Ldc_I4, index2 - 1);
        ilGenerator2.Emit(OpCodes.Ldarg, index2);
        if (parameterTypes[index2 - 1].IsValueType)
          ilGenerator2.Emit(OpCodes.Box, parameterTypes[index2 - 1]);
        ilGenerator2.Emit(OpCodes.Stelem_Ref);
      }
      ilGenerator2.Emit(OpCodes.Ldarg_0);
      FieldInfo field1 = typeof (PSEventHandler).GetField(nameof (eventManager), BindingFlags.Instance | BindingFlags.NonPublic);
      ilGenerator2.Emit(OpCodes.Ldfld, field1);
      ilGenerator2.Emit(OpCodes.Ldarg_0);
      FieldInfo field2 = typeof (PSEventHandler).GetField(nameof (sourceIdentifier), BindingFlags.Instance | BindingFlags.NonPublic);
      ilGenerator2.Emit(OpCodes.Ldfld, field2);
      ilGenerator2.Emit(OpCodes.Ldarg_0);
      FieldInfo field3 = typeof (PSEventHandler).GetField(nameof (sender), BindingFlags.Instance | BindingFlags.NonPublic);
      ilGenerator2.Emit(OpCodes.Ldfld, field3);
      ilGenerator2.Emit(OpCodes.Ldloc_0);
      ilGenerator2.Emit(OpCodes.Ldarg_0);
      FieldInfo field4 = typeof (PSEventHandler).GetField("extraData", BindingFlags.Instance | BindingFlags.NonPublic);
      ilGenerator2.Emit(OpCodes.Ldfld, field4);
      MethodInfo method = typeof (PSEventManager).GetMethod("GenerateEvent");
      if (this.debugMode)
        ilGenerator2.MarkSequencePoint(document, stackFrame.GetFileLineNumber() - 1, 1, stackFrame.GetFileLineNumber(), 100);
      ilGenerator2.EmitCall(OpCodes.Callvirt, method, (Type[]) null);
      ilGenerator2.Emit(OpCodes.Pop);
      ilGenerator2.Emit(OpCodes.Ret);
      return typeBuilder.CreateType().GetConstructor(new Type[4]
      {
        typeof (PSEventManager),
        typeof (object),
        typeof (string),
        typeof (PSObject)
      }).Invoke(new object[4]
      {
        (object) eventManager,
        sender,
        (object) sourceIdentifier,
        (object) data
      });
    }

    internal override event EventHandler<PSEventArgs> ForwardEvent;

    protected virtual void OnForwardEvent(PSEventArgs e)
    {
      EventHandler<PSEventArgs> forwardEvent = this.ForwardEvent;
      if (forwardEvent == null)
        return;
      forwardEvent((object) this, e);
    }

    ~PSLocalEventManager() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      lock (this.eventSubscribers)
      {
        foreach (PSEventSubscriber key in this.eventSubscribers.Keys)
          this.UnsubscribeEvent(key);
      }
    }
  }
}
