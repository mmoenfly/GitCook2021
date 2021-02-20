// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.ShapeSelectionDirectives
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class ShapeSelectionDirectives
  {
    private int? _propertyCountForTable = new int?();
    internal List<FormatShapeSelectionOnType> formatShapeSelectionOnTypeList = new List<FormatShapeSelectionOnType>();

    internal int PropertyCountForTable
    {
      set
      {
        if (this._propertyCountForTable.HasValue)
          return;
        this._propertyCountForTable = new int?(value);
      }
      get => this._propertyCountForTable.HasValue ? this._propertyCountForTable.Value : 4;
    }
  }
}
