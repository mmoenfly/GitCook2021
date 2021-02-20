// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RangeEnumerator
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;

namespace System.Management.Automation
{
  internal class RangeEnumerator : IEnumerator
  {
    private int _lowerBound;
    private int _upperBound;
    private int _current;
    private int increment = 1;
    private bool firstElement = true;

    internal int LowerBound => this._lowerBound;

    internal int UpperBound => this._upperBound;

    public object Current => (object) this._current;

    internal int CurrentValue => this._current;

    public RangeEnumerator(int lowerBound, int upperBound)
    {
      this._lowerBound = lowerBound;
      this._current = this._lowerBound;
      this._upperBound = upperBound;
      if (lowerBound <= upperBound)
        return;
      this.increment = -1;
    }

    public void Reset()
    {
      this._current = this._lowerBound;
      this.firstElement = true;
    }

    public bool MoveNext()
    {
      if (this.firstElement)
      {
        this.firstElement = false;
        return true;
      }
      if (this._current == this._upperBound)
        return false;
      this._current += this.increment;
      return true;
    }
  }
}
