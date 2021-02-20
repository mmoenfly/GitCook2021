// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.DefaultSettingsSection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class DefaultSettingsSection
  {
    private bool? _multilineTables;
    internal FormatErrorPolicy formatErrorPolicy = new FormatErrorPolicy();
    internal ShapeSelectionDirectives shapeSelectionDirectives = new ShapeSelectionDirectives();
    internal List<EnumerableExpansionDirective> enumerableExpansionDirectiveList = new List<EnumerableExpansionDirective>();

    internal bool MultilineTables
    {
      set
      {
        if (this._multilineTables.HasValue)
          return;
        this._multilineTables = new bool?(value);
      }
      get => this._multilineTables.HasValue && this._multilineTables.Value;
    }
  }
}
