// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProgressRecord
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Threading;

namespace System.Management.Automation
{
  public class ProgressRecord
  {
    private const string resTableName = "ProgressRecordStrings";
    private int id;
    private int parentId = -1;
    private string activity;
    private string status;
    private string currentOperation;
    private int percent = -1;
    private int secondsRemaining = -1;
    private ProgressRecordType type;
    [TraceSource("ProgressRecord", "ProgressRecord")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ProgressRecord), nameof (ProgressRecord));

    public ProgressRecord(int activityId, string activity, string statusDescription)
    {
      using (ProgressRecord.tracer.TraceConstructor((object) this))
      {
        if (activityId < 0)
          throw ProgressRecord.tracer.NewArgumentOutOfRangeException(nameof (activityId), (object) activityId, "ProgressRecordStrings", "ArgMayNotBeNegative", (object) nameof (activityId));
        if (string.IsNullOrEmpty(activity))
          throw ProgressRecord.tracer.NewArgumentException(nameof (activity), "ProgressRecordStrings", "ArgMayNotBeNullOrEmpty", (object) nameof (activity));
        if (string.IsNullOrEmpty(statusDescription))
          throw ProgressRecord.tracer.NewArgumentException(nameof (activity), "ProgressRecordStrings", "ArgMayNotBeNullOrEmpty", (object) nameof (statusDescription));
      }
      this.id = activityId;
      this.activity = activity;
      this.status = statusDescription;
    }

    public int ActivityId
    {
      get
      {
        using (ProgressRecord.tracer.TraceProperty((object) this.id))
          return this.id;
      }
    }

    public int ParentActivityId
    {
      get
      {
        using (ProgressRecord.tracer.TraceProperty((object) this.parentId))
          return this.parentId;
      }
      set
      {
        using (ProgressRecord.tracer.TraceProperty((object) value))
          this.parentId = value != this.ActivityId ? value : throw ProgressRecord.tracer.NewArgumentException(nameof (value), "ProgressRecordStrings", "ParentActivityIdCantBeActivityId");
      }
    }

    public string Activity
    {
      get
      {
        using (ProgressRecord.tracer.TraceProperty(this.activity, new object[0]))
          return this.activity;
      }
      set
      {
        using (ProgressRecord.tracer.TraceProperty(value, new object[0]))
          this.activity = !string.IsNullOrEmpty(value) ? value : throw ProgressRecord.tracer.NewArgumentException(nameof (value), "ProgressRecordStrings", "ArgMayNotBeNullOrEmpty", (object) nameof (value));
      }
    }

    public string StatusDescription
    {
      get
      {
        using (ProgressRecord.tracer.TraceProperty(this.status, new object[0]))
          return this.status;
      }
      set
      {
        using (ProgressRecord.tracer.TraceProperty(value, new object[0]))
          this.status = !string.IsNullOrEmpty(value) ? value : throw ProgressRecord.tracer.NewArgumentException(nameof (value), "ProgressRecordStrings", "ArgMayNotBeNullOrEmpty", (object) nameof (value));
      }
    }

    public string CurrentOperation
    {
      get
      {
        using (ProgressRecord.tracer.TraceProperty(this.currentOperation, new object[0]))
          return this.currentOperation;
      }
      set
      {
        using (ProgressRecord.tracer.TraceProperty(value, new object[0]))
          this.currentOperation = value;
      }
    }

    public int PercentComplete
    {
      get
      {
        using (ProgressRecord.tracer.TraceProperty((object) this.percent))
          return this.percent;
      }
      set
      {
        using (ProgressRecord.tracer.TraceProperty((object) value))
          this.percent = value <= 100 ? value : throw ProgressRecord.tracer.NewArgumentOutOfRangeException(nameof (value), (object) value, "ProgressRecordStrings", "PercentMayNotBeMoreThan100", (object) nameof (PercentComplete));
      }
    }

    public int SecondsRemaining
    {
      get
      {
        using (ProgressRecord.tracer.TraceProperty((object) this.secondsRemaining))
          return this.secondsRemaining;
      }
      set
      {
        using (ProgressRecord.tracer.TraceProperty((object) value))
          this.secondsRemaining = value;
      }
    }

    public ProgressRecordType RecordType
    {
      get
      {
        using (ProgressRecord.tracer.TraceProperty((object) this.type))
          return this.type;
      }
      set
      {
        using (ProgressRecord.tracer.TraceProperty((object) value))
          this.type = value == ProgressRecordType.Completed || value == ProgressRecordType.Processing ? value : throw ProgressRecord.tracer.NewArgumentException(nameof (value));
      }
    }

    public override string ToString() => string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "parent = {0} id = {1} act = {2} stat = {3} cur = {4} pct = {5} sec = {6} type = {7}", (object) this.parentId, (object) this.id, (object) this.activity, (object) this.status, (object) this.currentOperation, (object) this.percent, (object) this.secondsRemaining, (object) this.type);

    internal static ProgressRecord FromPSObjectForRemoting(PSObject progressAsPSObject)
    {
      string activity = progressAsPSObject != null ? RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, "Activity") : throw ProgressRecord.tracer.NewArgumentNullException(nameof (progressAsPSObject));
      int propertyValue1 = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, "ActivityId");
      string propertyValue2 = RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, "StatusDescription");
      return new ProgressRecord(propertyValue1, activity, propertyValue2)
      {
        CurrentOperation = RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, "CurrentOperation"),
        ParentActivityId = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, "ParentActivityId"),
        PercentComplete = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, "PercentComplete"),
        RecordType = RemotingDecoder.GetPropertyValue<ProgressRecordType>(progressAsPSObject, "Type"),
        SecondsRemaining = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, "SecondsRemaining")
      };
    }

    internal PSObject ToPSObjectForRemoting()
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Activity", (object) this.Activity));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ActivityId", (object) this.ActivityId));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("StatusDescription", (object) this.StatusDescription));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("CurrentOperation", (object) this.CurrentOperation));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ParentActivityId", (object) this.ParentActivityId));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PercentComplete", (object) this.PercentComplete));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Type", (object) this.RecordType));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("SecondsRemaining", (object) this.SecondsRemaining));
      return emptyPsObject;
    }
  }
}
