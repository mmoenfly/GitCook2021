// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Indexer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;

namespace System.Management.Automation.Remoting
{
  internal class Indexer : IEnumerable, IEnumerator
  {
    private int[] _current;
    private int[] _lengths;

    public object Current => (object) this._current;

    internal Indexer(int[] lengths)
    {
      this._lengths = lengths;
      this._current = new int[lengths.Length];
    }

    private bool CheckLengthsNonNegative(int[] lengths)
    {
      for (int index = 0; index < lengths.Length; ++index)
      {
        if (lengths[index] < 0)
          return false;
      }
      return true;
    }

    public IEnumerator GetEnumerator()
    {
      this.Reset();
      return (IEnumerator) this;
    }

    public void Reset()
    {
      for (int index = 0; index < this._current.Length; ++index)
        this._current[index] = 0;
      if (this._current.Length <= 0)
        return;
      this._current[this._current.Length - 1] = -1;
    }

    public bool MoveNext()
    {
      for (int index = this._lengths.Length - 1; index >= 0; --index)
      {
        if (this._current[index] < this._lengths[index] - 1)
        {
          ++this._current[index];
          return true;
        }
        this._current[index] = 0;
      }
      return false;
    }
  }
}
