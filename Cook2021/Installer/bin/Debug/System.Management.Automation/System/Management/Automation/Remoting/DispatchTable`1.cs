// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.DispatchTable`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Threading;

namespace System.Management.Automation.Remoting
{
  internal class DispatchTable<T> where T : class
  {
    internal const long VoidCallId = -100;
    private Dictionary<long, AsyncObject<T>> _responseAsyncObjects = new Dictionary<long, AsyncObject<T>>();
    private long _nextCallId;

    internal long CreateNewCallId()
    {
      long key = Interlocked.Increment(ref this._nextCallId);
      AsyncObject<T> asyncObject = new AsyncObject<T>();
      lock (this._responseAsyncObjects)
        this._responseAsyncObjects[key] = asyncObject;
      return key;
    }

    private AsyncObject<T> GetResponseAsyncObject(long callId) => this._responseAsyncObjects[callId];

    internal T GetResponse(long callId, T defaultValue)
    {
      AsyncObject<T> asyncObject = (AsyncObject<T>) null;
      lock (this._responseAsyncObjects)
        asyncObject = this.GetResponseAsyncObject(callId);
      T obj = asyncObject.Value;
      lock (this._responseAsyncObjects)
        this._responseAsyncObjects.Remove(callId);
      return (object) obj == null ? defaultValue : obj;
    }

    internal void SetResponse(long callId, T remoteHostResponse)
    {
      lock (this._responseAsyncObjects)
      {
        if (!this._responseAsyncObjects.ContainsKey(callId))
          return;
        this.GetResponseAsyncObject(callId).Value = remoteHostResponse;
      }
    }

    private void AbortCall(long callId)
    {
      if (!this._responseAsyncObjects.ContainsKey(callId))
        return;
      this.GetResponseAsyncObject(callId).Value = default (T);
    }

    private void AbortCalls(List<long> callIds)
    {
      foreach (long callId in callIds)
        this.AbortCall(callId);
    }

    private List<long> GetAllCalls()
    {
      List<long> longList = new List<long>();
      foreach (KeyValuePair<long, AsyncObject<T>> responseAsyncObject in this._responseAsyncObjects)
        longList.Add(responseAsyncObject.Key);
      return longList;
    }

    internal void AbortAllCalls()
    {
      lock (this._responseAsyncObjects)
        this.AbortCalls(this.GetAllCalls());
    }
  }
}
