// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSListModifier
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class PSListModifier
  {
    internal const string AddKey = "Add";
    internal const string RemoveKey = "Remove";
    internal const string ReplaceKey = "Replace";
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private Collection<object> _itemsToAdd;
    private Collection<object> _itemsToRemove;
    private Collection<object> _replacementItems;

    public PSListModifier()
    {
      this._itemsToAdd = new Collection<object>();
      this._itemsToRemove = new Collection<object>();
      this._replacementItems = new Collection<object>();
    }

    public PSListModifier(Collection<object> removeItems, Collection<object> addItems)
    {
      this._itemsToAdd = addItems != null ? addItems : new Collection<object>();
      this._itemsToRemove = removeItems != null ? removeItems : new Collection<object>();
      this._replacementItems = new Collection<object>();
    }

    public PSListModifier(object replacementItems)
    {
      this._itemsToAdd = new Collection<object>();
      this._itemsToRemove = new Collection<object>();
      switch (replacementItems)
      {
        case null:
          this._replacementItems = new Collection<object>();
          break;
        case Collection<object> _:
          this._replacementItems = (Collection<object>) replacementItems;
          break;
        case IList<object> _:
          this._replacementItems = new Collection<object>((IList<object>) replacementItems);
          break;
        case IList _:
          this._replacementItems = new Collection<object>();
          IEnumerator enumerator = ((IEnumerable) replacementItems).GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
              this._replacementItems.Add(enumerator.Current);
            break;
          }
          finally
          {
            if (enumerator is IDisposable disposable)
              disposable.Dispose();
          }
        default:
          this._replacementItems = new Collection<object>();
          this._replacementItems.Add(replacementItems);
          break;
      }
    }

    public PSListModifier(Hashtable hash)
    {
      if (hash == null)
        throw PSListModifier.tracer.NewArgumentNullException(nameof (hash));
      this._itemsToAdd = new Collection<object>();
      this._itemsToRemove = new Collection<object>();
      this._replacementItems = new Collection<object>();
      foreach (DictionaryEntry dictionaryEntry in hash)
      {
        string str = dictionaryEntry.Key is string ? dictionaryEntry.Key as string : throw PSListModifier.tracer.NewArgumentException(nameof (hash), nameof (PSListModifier), "ListModifierDisallowedKey", dictionaryEntry.Key);
        bool flag1 = str.Equals(nameof (Add), StringComparison.OrdinalIgnoreCase);
        bool flag2 = str.Equals(nameof (Remove), StringComparison.OrdinalIgnoreCase);
        bool flag3 = str.Equals(nameof (Replace), StringComparison.OrdinalIgnoreCase);
        if (!flag1 && !flag2 && !flag3)
          throw PSListModifier.tracer.NewArgumentException(nameof (hash), nameof (PSListModifier), "ListModifierDisallowedKey", (object) str);
        Collection<object> collection = !flag2 ? (!flag1 ? this._replacementItems : this._itemsToAdd) : this._itemsToRemove;
        IEnumerable enumerable = LanguagePrimitives.GetEnumerable(dictionaryEntry.Value);
        if (enumerable != null)
        {
          foreach (object obj in enumerable)
            collection.Add(obj);
        }
        else
          collection.Add(dictionaryEntry.Value);
      }
    }

    public Collection<object> Add => this._itemsToAdd;

    public Collection<object> Remove => this._itemsToRemove;

    public Collection<object> Replace => this._replacementItems;

    public void ApplyTo(IList collectionToUpdate)
    {
      if (collectionToUpdate == null)
        throw PSListModifier.tracer.NewArgumentNullException(nameof (collectionToUpdate));
      if (this._replacementItems.Count > 0)
      {
        collectionToUpdate.Clear();
        foreach (object replacementItem in this._replacementItems)
          collectionToUpdate.Add(PSObject.Base(replacementItem));
      }
      else
      {
        foreach (object obj in this._itemsToRemove)
          collectionToUpdate.Remove(PSObject.Base(obj));
        foreach (object obj in this._itemsToAdd)
          collectionToUpdate.Add(PSObject.Base(obj));
      }
    }

    public void ApplyTo(object collectionToUpdate)
    {
      collectionToUpdate = collectionToUpdate != null ? PSObject.Base(collectionToUpdate) : throw new ArgumentNullException(nameof (collectionToUpdate));
      if (!(collectionToUpdate is IList collectionToUpdate1))
        throw PSListModifier.tracer.NewInvalidOperationException(nameof (PSListModifier), "UpdateFailed");
      this.ApplyTo(collectionToUpdate1);
    }

    internal Hashtable ToHashtable()
    {
      Hashtable hashtable = new Hashtable(2);
      if (this._itemsToAdd.Count > 0)
        hashtable.Add((object) "Add", (object) this._itemsToAdd);
      if (this._itemsToRemove.Count > 0)
        hashtable.Add((object) "Remove", (object) this._itemsToRemove);
      if (this._replacementItems.Count > 0)
        hashtable.Add((object) "Replace", (object) this._replacementItems);
      return hashtable;
    }
  }
}
