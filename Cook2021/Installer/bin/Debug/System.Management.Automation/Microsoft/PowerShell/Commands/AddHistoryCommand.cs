// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.AddHistoryCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Add", "History")]
  public class AddHistoryCommand : PSCmdlet
  {
    private const string ErrorBase = "History";
    private PSObject[] _inputObjects;
    private bool _passthru;
    [TraceSource("History", "Add History class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("History", "Add History class");

    [Parameter(Position = 0, ValueFromPipeline = true)]
    public PSObject[] InputObject
    {
      set
      {
        using (AddHistoryCommand._trace.TraceProperty())
          this._inputObjects = value;
      }
      get
      {
        using (AddHistoryCommand._trace.TraceProperty())
          return this._inputObjects;
      }
    }

    [Parameter]
    public SwitchParameter Passthru
    {
      get
      {
        using (AddHistoryCommand._trace.TraceProperty())
          return (SwitchParameter) this._passthru;
      }
      set
      {
        using (AddHistoryCommand._trace.TraceProperty())
          this._passthru = (bool) value;
      }
    }

    protected override void BeginProcessing()
    {
      using (AddHistoryCommand._trace.TraceMethod())
        ((LocalPipeline) this.Context.CurrentRunspace.GetCurrentlyRunningPipeline()).AddHistoryEntryFromAddHistoryCmdlet();
    }

    protected override void ProcessRecord()
    {
      using (AddHistoryCommand._trace.TraceMethod())
      {
        History history = ((LocalRunspace) this.Context.CurrentRunspace).History;
        if (this.InputObject == null)
          return;
        foreach (PSObject mshObject in this.InputObject)
        {
          HistoryInfo historyInfoObject = this.GetHistoryInfoObject(mshObject);
          if (historyInfoObject != null)
          {
            long id = history.AddEntry(0L, historyInfoObject.CommandLine, historyInfoObject.ExecutionStatus, historyInfoObject.StartExecutionTime, historyInfoObject.EndExecutionTime, false);
            if ((bool) this.Passthru)
              this.WriteObject((object) history.GetEntry(id));
          }
        }
      }
    }

    private HistoryInfo GetHistoryInfoObject(PSObject mshObject)
    {
      using (AddHistoryCommand._trace.TraceMethod())
      {
        if (mshObject != null && AddHistoryCommand.GetPropertyValue(mshObject, "CommandLine") is string propertyValue)
        {
          object propertyValue1 = AddHistoryCommand.GetPropertyValue(mshObject, "ExecutionStatus");
          switch (propertyValue1)
          {
            case PipelineState status:
label_7:
              object propertyValue2 = AddHistoryCommand.GetPropertyValue(mshObject, "StartExecutionTime");
              switch (propertyValue2)
              {
                case DateTime startTime:
label_10:
                  object propertyValue3 = AddHistoryCommand.GetPropertyValue(mshObject, "EndExecutionTime");
                  switch (propertyValue3)
                  {
                    case DateTime endTime:
label_13:
                      return new HistoryInfo(0L, propertyValue, status, startTime, endTime);
                    case string _:
                      try
                      {
                        endTime = DateTime.Parse((string) propertyValue3, (IFormatProvider) CultureInfo.CurrentCulture);
                        goto label_13;
                      }
                      catch (FormatException ex)
                      {
                        AddHistoryCommand._trace.TraceException((Exception) ex);
                        break;
                      }
                  }
                  break;
                case string _:
                  try
                  {
                    startTime = DateTime.Parse((string) propertyValue2, (IFormatProvider) CultureInfo.CurrentCulture);
                    goto label_10;
                  }
                  catch (FormatException ex)
                  {
                    AddHistoryCommand._trace.TraceException((Exception) ex);
                    break;
                  }
              }
              break;
            case PSObject _:
              object baseObject = (propertyValue1 as PSObject).BaseObject;
              if (baseObject is int)
              {
                status = (PipelineState) baseObject;
                switch (status)
                {
                  case PipelineState.NotStarted:
                  case PipelineState.Running:
                  case PipelineState.Stopping:
                  case PipelineState.Stopped:
                  case PipelineState.Completed:
                  case PipelineState.Failed:
                    goto label_7;
                }
              }
              else
                break;
              break;
            case string _:
              try
              {
                status = (PipelineState) Enum.Parse(typeof (PipelineState), (string) propertyValue1);
                goto label_7;
              }
              catch (ArgumentException ex)
              {
                AddHistoryCommand._trace.TraceException((Exception) ex);
                break;
              }
          }
        }
        this.WriteError(new ErrorRecord((Exception) new InvalidDataException(ResourceManagerCache.FormatResourceString("History", "AddHistoryInvalidInput")), "AddHistoryInvalidInput", ErrorCategory.InvalidData, (object) mshObject));
        return (HistoryInfo) null;
      }
    }

    private static object GetPropertyValue(PSObject mshObject, string propertyName)
    {
      using (AddHistoryCommand._trace.TraceMethod())
        return mshObject.Properties[propertyName]?.Value;
    }
  }
}
