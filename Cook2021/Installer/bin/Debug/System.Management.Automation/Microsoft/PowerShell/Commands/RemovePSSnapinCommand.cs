// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.RemovePSSnapinCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Remove", "PSSnapin", SupportsShouldProcess = true)]
  public sealed class RemovePSSnapinCommand : PSSnapInCommandBase
  {
    private string[] _pssnapins;
    private bool _passThru;

    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    public string[] Name
    {
      get => this._pssnapins;
      set => this._pssnapins = value;
    }

    [Parameter]
    public SwitchParameter PassThru
    {
      get => (SwitchParameter) this._passThru;
      set => this._passThru = (bool) value;
    }

    protected override void ProcessRecord()
    {
      foreach (string pssnapin in this._pssnapins)
      {
        Collection<PSSnapInInfo> snapIns = this.GetSnapIns(pssnapin);
        if (snapIns.Count == 0)
        {
          this.WriteNonTerminatingError((object) pssnapin, "NoPSSnapInsFound", (Exception) PSSnapInCommandBase.tracer.NewArgumentException(pssnapin, "MshSnapInCmdletResources", "NoPSSnapInsFound", (object) pssnapin), ErrorCategory.InvalidArgument);
        }
        else
        {
          foreach (PSSnapInInfo psSnapInInfo1 in snapIns)
          {
            if (this.ShouldProcess(psSnapInInfo1.Name))
            {
              Exception innerException = (Exception) null;
              if (this.Runspace == null)
              {
                InitialSessionState initialSessionState = InitialSessionState.Create();
                initialSessionState.ImportPSSnapIn(psSnapInInfo1, out PSSnapInException _);
                initialSessionState.Unbind(this.Context);
              }
              else
              {
                try
                {
                  PSSnapInException warning = (PSSnapInException) null;
                  PSSnapInInfo psSnapInInfo2 = this.Runspace.RemovePSSnapIn(psSnapInInfo1.Name, out warning);
                  if (warning != null)
                    this.WriteNonTerminatingError((object) psSnapInInfo1.Name, "RemovePSSnapInRead", (Exception) warning, ErrorCategory.InvalidData);
                  if (this._passThru)
                  {
                    psSnapInInfo2.LoadIndirectResources(this.ResourceReader);
                    this.WriteObject((object) psSnapInInfo2);
                  }
                }
                catch (PSArgumentException ex)
                {
                  innerException = (Exception) ex;
                }
                if (innerException != null)
                  this.WriteNonTerminatingError((object) pssnapin, "RemovePSSnapIn", innerException, ErrorCategory.InvalidArgument);
              }
            }
          }
        }
      }
    }
  }
}
