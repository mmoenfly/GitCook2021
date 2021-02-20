// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.VistaCultureInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Globalization;

namespace Microsoft.PowerShell
{
  internal class VistaCultureInfo : CultureInfo
  {
    private string[] m_fallbacks;
    private VistaCultureInfo parentCI;
    private object syncObject = new object();

    public VistaCultureInfo(string name, string[] fallbacks)
      : base(name)
      => this.m_fallbacks = fallbacks;

    public override CultureInfo Parent
    {
      get
      {
        if (base.Parent != null && !string.IsNullOrEmpty(base.Parent.Name))
          return (CultureInfo) this.ImmediateParent;
        while (this.m_fallbacks != null && this.m_fallbacks.Length > 0)
        {
          string fallback = this.m_fallbacks[0];
          string[] fallbacks = (string[]) null;
          if (this.m_fallbacks.Length > 1)
          {
            fallbacks = new string[this.m_fallbacks.Length - 1];
            Array.Copy((Array) this.m_fallbacks, 1, (Array) fallbacks, 0, this.m_fallbacks.Length - 1);
          }
          try
          {
            return (CultureInfo) new VistaCultureInfo(fallback, fallbacks);
          }
          catch (ArgumentException ex)
          {
            this.m_fallbacks = fallbacks;
          }
        }
        return base.Parent;
      }
    }

    private VistaCultureInfo ImmediateParent
    {
      get
      {
        if (this.parentCI == null)
        {
          lock (this.syncObject)
          {
            if (this.parentCI == null)
            {
              string name = base.Parent.Name;
              string[] array = (string[]) null;
              if (this.m_fallbacks != null)
              {
                array = new string[this.m_fallbacks.Length];
                int newSize = 0;
                foreach (string fallback in this.m_fallbacks)
                {
                  if (!name.Equals(fallback, StringComparison.OrdinalIgnoreCase))
                  {
                    array[newSize] = fallback;
                    ++newSize;
                  }
                }
                if (this.m_fallbacks.Length != newSize)
                  Array.Resize<string>(ref array, newSize);
              }
              this.parentCI = new VistaCultureInfo(name, array);
            }
          }
        }
        return this.parentCI;
      }
    }

    public override object Clone() => (object) new VistaCultureInfo(this.Name, this.m_fallbacks);
  }
}
