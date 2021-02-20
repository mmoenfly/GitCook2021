// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.OutputTypeAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public sealed class OutputTypeAttribute : CmdletMetadataAttribute
  {
    private PSTypeName[] _type;
    private string _providerCmdlet;
    private string[] _parameterSetName = new string[1]
    {
      "__AllParameterSets"
    };

    public OutputTypeAttribute(params System.Type[] type)
    {
      List<PSTypeName> psTypeNameList = new List<PSTypeName>();
      if (type != null)
      {
        foreach (System.Type type1 in type)
          psTypeNameList.Add(new PSTypeName(type1));
      }
      this._type = psTypeNameList.ToArray();
    }

    public OutputTypeAttribute(params string[] type)
    {
      List<PSTypeName> psTypeNameList = new List<PSTypeName>();
      if (type != null)
      {
        foreach (string name in type)
          psTypeNameList.Add(new PSTypeName(name));
      }
      this._type = psTypeNameList.ToArray();
    }

    public PSTypeName[] Type => this._type;

    public string ProviderCmdlet
    {
      get => this._providerCmdlet;
      set => this._providerCmdlet = value;
    }

    public string[] ParameterSetName
    {
      get => this._parameterSetName;
      set => this._parameterSetName = value;
    }
  }
}
