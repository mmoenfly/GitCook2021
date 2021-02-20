// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterInformation
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;

namespace System.Management.Automation
{
  internal class ParameterInformation
  {
    internal Type parameterType;
    internal object defaultValue;
    internal bool isOptional;
    internal bool isByRef;
    internal bool isParamArray;

    internal ParameterInformation(ParameterInfo parameter)
    {
      this.isOptional = parameter.IsOptional;
      this.defaultValue = parameter.DefaultValue;
      this.parameterType = parameter.ParameterType;
      if (this.parameterType.IsByRef)
      {
        this.isByRef = true;
        this.parameterType = this.parameterType.GetElementType();
      }
      else
        this.isByRef = false;
    }

    internal ParameterInformation(
      Type parameterType,
      bool isOptional,
      object defaultValue,
      bool isByRef)
    {
      this.parameterType = parameterType;
      this.isOptional = isOptional;
      this.defaultValue = defaultValue;
      this.isByRef = isByRef;
    }
  }
}
