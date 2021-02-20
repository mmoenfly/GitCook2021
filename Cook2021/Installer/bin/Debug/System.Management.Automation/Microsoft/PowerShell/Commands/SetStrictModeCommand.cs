// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.SetStrictModeCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Set", "StrictMode", DefaultParameterSetName = "Version")]
  public class SetStrictModeCommand : PSCmdlet
  {
    private SwitchParameter off;
    private Version version;

    [Parameter(Mandatory = true, ParameterSetName = "Off")]
    public SwitchParameter Off
    {
      get => this.off;
      set => this.off = value;
    }

    [SetStrictModeCommand.ValidateVersion]
    [Alias(new string[] {"v"})]
    [Parameter(Mandatory = true, ParameterSetName = "Version")]
    [SetStrictModeCommand.ArgumentToVersionTransformation]
    public Version Version
    {
      get => this.version;
      set => this.version = value;
    }

    protected override void EndProcessing()
    {
      if (this.off.IsPresent)
        this.version = new Version(0, 0);
      this.Context.EngineSessionState.CurrentScope.StrictModeVersion = this.version;
    }

    private sealed class ArgumentToVersionTransformationAttribute : ArgumentTransformationAttribute
    {
      public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
      {
        object valueToConvert = PSObject.Base(inputData);
        if (valueToConvert is string str)
        {
          if (str.Equals("latest", StringComparison.OrdinalIgnoreCase))
            return (object) PSVersionInfo.PSVersion;
          if (str.Contains("."))
            return inputData;
        }
        int result;
        return valueToConvert is double || !LanguagePrimitives.TryConvertTo<int>(valueToConvert, out result) ? inputData : (object) new Version(result, 0);
      }
    }

    private sealed class ValidateVersionAttribute : ValidateArgumentsAttribute
    {
      protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
      {
        Version version = arguments as Version;
        if (version == (Version) null || !PSVersionInfo.IsValidPSVersion(version))
          throw new ValidationMetadataException("InvalidPSVersion", (Exception) null, "Metadata", "ValidateVersionFailure", new object[1]
          {
            arguments
          });
      }
    }
  }
}
