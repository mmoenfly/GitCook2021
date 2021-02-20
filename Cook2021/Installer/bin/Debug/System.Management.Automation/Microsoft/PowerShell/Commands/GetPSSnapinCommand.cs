// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.GetPSSnapinCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Get", "PSSnapin")]
  public sealed class GetPSSnapinCommand : PSSnapInCommandBase
  {
    private string[] _pssnapins;

    [Parameter(Mandatory = false, Position = 0)]
    public string[] Name
    {
      get => this._pssnapins;
      set => this._pssnapins = value;
    }

    [Parameter(Mandatory = false)]
    public SwitchParameter Registered
    {
      get => (SwitchParameter) this.ShouldGetAll;
      set => this.ShouldGetAll = (bool) value;
    }

    protected override void BeginProcessing()
    {
      if (this._pssnapins != null)
      {
        foreach (string pssnapin in this._pssnapins)
        {
          Exception innerException = (Exception) null;
          try
          {
            Collection<PSSnapInInfo> snapIns = this.GetSnapIns(pssnapin);
            if (snapIns.Count == 0)
            {
              this.WriteNonTerminatingError((object) pssnapin, "NoPSSnapInsFound", (Exception) PSSnapInCommandBase.tracer.NewArgumentException(pssnapin, "MshSnapInCmdletResources", "NoPSSnapInsFound", (object) pssnapin), ErrorCategory.InvalidArgument);
              continue;
            }
            foreach (PSSnapInInfo psSnapInInfo in snapIns)
            {
              psSnapInInfo.LoadIndirectResources(this.ResourceReader);
              this.WriteObject((object) psSnapInInfo);
            }
          }
          catch (SecurityException ex)
          {
            innerException = (Exception) ex;
          }
          catch (PSArgumentException ex)
          {
            innerException = (Exception) ex;
          }
          if (innerException != null)
            this.WriteNonTerminatingError((object) pssnapin, "GetPSSnapInRead", innerException, ErrorCategory.InvalidArgument);
        }
      }
      else if (this.ShouldGetAll)
      {
        Exception innerException = (Exception) null;
        try
        {
          foreach (PSSnapInInfo psSnapInInfo in PSSnapInReader.ReadAll())
          {
            psSnapInInfo.LoadIndirectResources(this.ResourceReader);
            this.WriteObject((object) psSnapInInfo);
          }
        }
        catch (SecurityException ex)
        {
          innerException = (Exception) ex;
        }
        catch (PSArgumentException ex)
        {
          innerException = (Exception) ex;
        }
        if (innerException == null)
          return;
        this.WriteNonTerminatingError((object) this, "GetPSSnapInRead", innerException, ErrorCategory.InvalidArgument);
      }
      else
      {
        foreach (PSSnapInInfo snapIn in this.GetSnapIns((string) null))
        {
          snapIn.LoadIndirectResources(this.ResourceReader);
          this.WriteObject((object) snapIn);
        }
      }
    }
  }
}
