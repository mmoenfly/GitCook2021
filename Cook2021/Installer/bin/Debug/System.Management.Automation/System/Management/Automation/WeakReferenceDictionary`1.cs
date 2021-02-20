// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.WeakReferenceDictionary`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class WeakReferenceDictionary<T> : 
    IDictionary<object, T>,
    ICollection<KeyValuePair<object, T>>,
    IEnumerable<KeyValuePair<object, T>>,
    IEnumerable
  {
    private const int initialCleanupTriggerSize = 1000;
    private IEqualityComparer<WeakReference> weakEqualityComparer;
    private Dictionary<WeakReference, T> dictionary;
    private int cleanupTriggerSize = 1000;

    public WeakReferenceDictionary()
    {
      this.weakEqualityComparer = (IEqualityComparer<WeakReference>) new WeakReferenceDictionary<T>.WeakReferenceEqualityComparer();
      this.dictionary = new Dictionary<WeakReference, T>(this.weakEqualityComparer);
    }

    private void CleanUp()
    {
      if (this.Count <= this.cleanupTriggerSize)
        return;
      Dictionary<WeakReference, T> dictionary = new Dictionary<WeakReference, T>(this.weakEqualityComparer);
      foreach (KeyValuePair<WeakReference, T> keyValuePair in this.dictionary)
      {
        if (keyValuePair.Key.Target != null)
          dictionary.Add(keyValuePair.Key, keyValuePair.Value);
      }
      this.dictionary = dictionary;
      this.cleanupTriggerSize = 1000 + this.Count * 2;
    }

    public void Add(object key, T value)
    {
      this.dictionary.Add(new WeakReference(key), value);
      this.CleanUp();
    }

    public bool ContainsKey(object key) => this.dictionary.ContainsKey(new WeakReference(key));

    public ICollection<object> Keys
    {
      get
      {
        List<object> objectList = new List<object>(this.dictionary.Keys.Count);
        foreach (WeakReference key in this.dictionary.Keys)
        {
          object target = key.Target;
          if (target != null)
            objectList.Add(target);
        }
        return (ICollection<object>) objectList;
      }
    }

    public bool Remove(object key) => this.dictionary.Remove(new WeakReference(key));

    public bool TryGetValue(object key, out T value)
    {
      WeakReference weakReference1 = new WeakReference(key);
      WeakReference weakReference2 = (WeakReference) null;
      if (weakReference2 != null)
      {
        this.weakEqualityComparer.Equals(weakReference2, weakReference1);
        this.weakEqualityComparer.Equals(weakReference1, weakReference2);
        this.weakEqualityComparer.GetHashCode(weakReference2);
        this.weakEqualityComparer.GetHashCode(weakReference1);
      }
      return this.dictionary.TryGetValue(weakReference1, out value);
    }

    public ICollection<T> Values => (ICollection<T>) this.dictionary.Values;

    public T this[object key]
    {
      get => this.dictionary[new WeakReference(key)];
      set
      {
        this.dictionary[new WeakReference(key)] = value;
        this.CleanUp();
      }
    }

    private ICollection<KeyValuePair<WeakReference, T>> WeakCollection => (ICollection<KeyValuePair<WeakReference, T>>) this.dictionary;

    private static KeyValuePair<WeakReference, T> WeakKeyValuePair(
      KeyValuePair<object, T> publicKeyValuePair)
    {
      return new KeyValuePair<WeakReference, T>(new WeakReference(publicKeyValuePair.Key), publicKeyValuePair.Value);
    }

    public void Add(KeyValuePair<object, T> item)
    {
      this.WeakCollection.Add(WeakReferenceDictionary<T>.WeakKeyValuePair(item));
      this.CleanUp();
    }

    public void Clear() => this.WeakCollection.Clear();

    public bool Contains(KeyValuePair<object, T> item) => this.WeakCollection.Contains(WeakReferenceDictionary<T>.WeakKeyValuePair(item));

    public void CopyTo(KeyValuePair<object, T>[] array, int arrayIndex)
    {
      List<KeyValuePair<object, T>> keyValuePairList = new List<KeyValuePair<object, T>>(this.WeakCollection.Count);
      foreach (KeyValuePair<object, T> keyValuePair in this)
        keyValuePairList.Add(keyValuePair);
      keyValuePairList.CopyTo(array, arrayIndex);
    }

    public int Count => this.WeakCollection.Count;

    public bool IsReadOnly => this.WeakCollection.IsReadOnly;

    public bool Remove(KeyValuePair<object, T> item) => this.WeakCollection.Remove(WeakReferenceDictionary<T>.WeakKeyValuePair(item));

    public IEnumerator<KeyValuePair<object, T>> GetEnumerator()
    {
      foreach (KeyValuePair<WeakReference, T> weak in (IEnumerable<KeyValuePair<WeakReference, T>>) this.WeakCollection)
      {
        object key = weak.Key.Target;
        if (key != null)
          yield return new KeyValuePair<object, T>(key, weak.Value);
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    private class WeakReferenceEqualityComparer : IEqualityComparer<WeakReference>
    {
      public bool Equals(WeakReference x, WeakReference y)
      {
        object target1 = x.Target;
        if (target1 == null)
          return false;
        object target2 = y.Target;
        return target2 != null && object.ReferenceEquals(target1, target2);
      }

      public int GetHashCode(WeakReference obj)
      {
        object target = obj.Target;
        return target == null ? obj.GetHashCode() : target.GetHashCode();
      }
    }
  }
}
