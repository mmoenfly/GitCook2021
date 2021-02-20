// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSEventArgsCollection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Management.Automation
{
  public class PSEventArgsCollection : IEnumerable<PSEventArgs>, IEnumerable
  {
    private List<PSEventArgs> eventCollection = new List<PSEventArgs>();
    private object syncRoot = new object();

    public event PSEventReceivedEventHandler PSEventReceived;

    internal void Add(PSEventArgs eventToAdd)
    {
      if (eventToAdd == null)
        throw new ArgumentNullException(nameof (eventToAdd));
      this.eventCollection.Add(eventToAdd);
      this.OnPSEventReceived(eventToAdd.Sender, eventToAdd);
    }

    public int Count => this.eventCollection.Count;

    public void RemoveAt(int index) => this.eventCollection.RemoveAt(index);

    public PSEventArgs this[int index] => this.eventCollection[index];

    private void OnPSEventReceived(object sender, PSEventArgs e)
    {
      if (this.PSEventReceived == null)
        return;
      this.PSEventReceived(sender, e);
    }

    public IEnumerator<PSEventArgs> GetEnumerator() => (IEnumerator<PSEventArgs>) this.eventCollection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.eventCollection.GetEnumerator();

    public object SyncRoot => this.syncRoot;
  }
}
