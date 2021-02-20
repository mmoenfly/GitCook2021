// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class CmdletAttribute : CmdletCommonMetadataAttribute
  {
    private string nounName;
    private string verbName;

    public string NounName => this.nounName;

    public string VerbName => this.verbName;

    public CmdletAttribute(string verbName, string nounName)
    {
      switch (nounName)
      {
        case "":
        case null:
          throw CmdletMetadataAttribute.tracer.NewArgumentException(nameof (nounName));
        default:
          switch (verbName)
          {
            case "":
            case null:
              throw CmdletMetadataAttribute.tracer.NewArgumentException(nameof (verbName));
            default:
              this.nounName = nounName;
              this.verbName = verbName;
              return;
          }
      }
    }
  }
}
