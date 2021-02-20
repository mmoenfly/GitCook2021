// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.ControlBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal abstract class ControlBase
  {
    internal static string GetControlShapeName(ControlBase control)
    {
      switch (control)
      {
        case TableControlBody _:
          return FormatShape.Table.ToString();
        case ListControlBody _:
          return FormatShape.List.ToString();
        case WideControlBody _:
          return FormatShape.Wide.ToString();
        case ComplexControlBody _:
          return FormatShape.Complex.ToString();
        default:
          return "";
      }
    }

    internal virtual ControlBase Copy() => this;
  }
}
