// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.InformationalRecord
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public abstract class InformationalRecord
  {
    [TraceSource("InformationalRecord", "InformationalRecord")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (InformationalRecord), nameof (InformationalRecord));
    private string message;
    private InvocationInfo invocationInfo;
    private ReadOnlyCollection<int> pipelineIterationInfo;
    private bool serializeExtendedInfo;

    internal InformationalRecord(string message)
    {
      this.message = message;
      this.invocationInfo = (InvocationInfo) null;
      this.pipelineIterationInfo = (ReadOnlyCollection<int>) null;
      this.serializeExtendedInfo = false;
    }

    internal InformationalRecord(PSObject serializedObject)
    {
      using (InformationalRecord.tracer.TraceMethod())
      {
        this.message = (string) SerializationUtilities.GetPropertyValue(serializedObject, "InformationalRecord_Message");
        this.serializeExtendedInfo = (bool) SerializationUtilities.GetPropertyValue(serializedObject, "InformationalRecord_SerializeInvocationInfo");
        if (this.serializeExtendedInfo)
        {
          this.invocationInfo = new InvocationInfo(serializedObject);
          this.pipelineIterationInfo = new ReadOnlyCollection<int>((IList<int>) ((ArrayList) SerializationUtilities.GetPsObjectPropertyBaseObject(serializedObject, "InformationalRecord_PipelineIterationInfo")).ToArray(Type.GetType("System.Int32")));
        }
        else
          this.invocationInfo = (InvocationInfo) null;
      }
    }

    public string Message => this.message;

    public InvocationInfo InvocationInfo => this.invocationInfo;

    public ReadOnlyCollection<int> PipelineIterationInfo => this.pipelineIterationInfo;

    internal void SetInvocationInfo(InvocationInfo invocationInfo)
    {
      this.invocationInfo = invocationInfo;
      if (invocationInfo.PipelineIterationInfo == null)
        return;
      this.pipelineIterationInfo = new ReadOnlyCollection<int>((IList<int>) (int[]) invocationInfo.PipelineIterationInfo.Clone());
    }

    internal bool SerializeExtendedInfo
    {
      get => this.serializeExtendedInfo;
      set => this.serializeExtendedInfo = value;
    }

    public override string ToString() => this.Message;

    internal virtual void ToPSObjectForRemoting(PSObject psObject)
    {
      using (InformationalRecord.tracer.TraceMethod())
      {
        RemotingEncoder.AddNoteProperty<string>(psObject, "InformationalRecord_Message", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.Message));
        if (!this.SerializeExtendedInfo || this.invocationInfo == null)
        {
          SerializationUtilities.AddProperty(psObject, "InformationalRecord_SerializeInvocationInfo", (object) false);
        }
        else
        {
          SerializationUtilities.AddProperty(psObject, "InformationalRecord_SerializeInvocationInfo", (object) true);
          this.invocationInfo.ToPSObjectForRemoting(psObject);
          RemotingEncoder.AddNoteProperty<object>(psObject, "InformationalRecord_PipelineIterationInfo", (RemotingEncoder.ValueGetterDelegate<object>) (() => (object) this.PipelineIterationInfo));
        }
      }
    }
  }
}
