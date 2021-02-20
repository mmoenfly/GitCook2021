// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterBindingValidationException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  internal class ParameterBindingValidationException : ParameterBindingException
  {
    internal ParameterBindingValidationException(
      ErrorCategory errorCategory,
      InvocationInfo invocationInfo,
      Token token,
      string parameterName,
      Type parameterType,
      Type typeSpecified,
      string resourceBaseName,
      string errorIdAndResourceId,
      params object[] args)
      : base(errorCategory, invocationInfo, token, parameterName, parameterType, typeSpecified, resourceBaseName, errorIdAndResourceId, args)
    {
    }

    internal ParameterBindingValidationException(
      Exception innerException,
      ErrorCategory errorCategory,
      InvocationInfo invocationInfo,
      Token token,
      string parameterName,
      Type parameterType,
      Type typeSpecified,
      string resourceBaseName,
      string errorIdAndResourceId,
      params object[] args)
      : base(innerException, errorCategory, invocationInfo, token, parameterName, parameterType, typeSpecified, resourceBaseName, errorIdAndResourceId, args)
    {
    }

    protected ParameterBindingValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
