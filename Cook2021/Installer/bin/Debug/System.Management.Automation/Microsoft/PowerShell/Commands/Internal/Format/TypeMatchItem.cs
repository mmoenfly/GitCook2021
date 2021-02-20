// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.TypeMatchItem
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class TypeMatchItem
  {
    private object _item;
    private AppliesTo _appliesTo;
    private PSObject _currentObject;

    internal TypeMatchItem(object obj, AppliesTo a)
    {
      this._item = obj;
      this._appliesTo = a;
    }

    internal TypeMatchItem(object obj, AppliesTo a, PSObject currentObject)
    {
      this._item = obj;
      this._appliesTo = a;
      this._currentObject = currentObject;
    }

    internal object Item => this._item;

    internal AppliesTo AppliesTo => this._appliesTo;

    internal PSObject CurrentObject => this._currentObject;
  }
}
