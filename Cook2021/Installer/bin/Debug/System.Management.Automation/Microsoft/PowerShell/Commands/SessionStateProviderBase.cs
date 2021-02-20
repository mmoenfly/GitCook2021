// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.SessionStateProviderBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
  public abstract class SessionStateProviderBase : ContainerCmdletProvider, IContentCmdletProvider
  {
    [TraceSource("SessionStateProvider", "Providers that produce a view of session state data.")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("SessionStateProvider", "Providers that produce a view of session state data.");

    internal abstract object GetSessionStateItem(string name);

    internal abstract void SetSessionStateItem(string name, object value, bool writeItem);

    internal abstract void RemoveSessionStateItem(string name);

    internal abstract IDictionary GetSessionStateTable();

    internal virtual object GetValueOfItem(object item)
    {
      using (SessionStateProviderBase.tracer.TraceMethod())
      {
        object obj = item;
        if (item is DictionaryEntry dictionaryEntry)
          obj = dictionaryEntry.Value;
        return obj;
      }
    }

    internal virtual bool CanRenameItem(object item)
    {
      using (SessionStateProviderBase.tracer.TraceMethod())
      {
        bool flag = true;
        SessionStateProviderBase.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }

    protected override void GetItem(string name)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(name, new object[0]))
      {
        bool isContainer = false;
        object valueToCheck = (object) null;
        IDictionary sessionStateTable = this.GetSessionStateTable();
        if (sessionStateTable != null)
        {
          if (string.IsNullOrEmpty(name))
          {
            isContainer = true;
            valueToCheck = (object) sessionStateTable.Values;
          }
          else
            valueToCheck = sessionStateTable[(object) name];
        }
        if (valueToCheck == null || !SessionState.IsVisible(this.Context.Origin, valueToCheck))
          return;
        this.WriteItemObject(valueToCheck, name, isContainer);
      }
    }

    protected override void SetItem(string name, object value)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(name, new object[0]))
      {
        if (string.IsNullOrEmpty(name))
        {
          this.WriteError(new ErrorRecord((Exception) SessionStateProviderBase.tracer.NewArgumentNullException(nameof (name)), "SetItemNullName", ErrorCategory.InvalidArgument, (object) name));
        }
        else
        {
          try
          {
            string resourceString = ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "SetItemAction");
            if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "SetItemResourceTemplate"), (object) name, value), resourceString))
              return;
            this.SetSessionStateItem(name, value, true);
          }
          catch (SessionStateException ex)
          {
            this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
          }
          catch (PSArgumentException ex)
          {
            this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
          }
        }
      }
    }

    protected override void ClearItem(string path)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        if (string.IsNullOrEmpty(path))
        {
          this.WriteError(new ErrorRecord((Exception) SessionStateProviderBase.tracer.NewArgumentNullException(nameof (path)), "ClearItemNullPath", ErrorCategory.InvalidArgument, (object) path));
        }
        else
        {
          try
          {
            string resourceString = ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "ClearItemAction");
            if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "ClearItemResourceTemplate"), (object) path), resourceString))
              return;
            this.SetSessionStateItem(path, (object) null, false);
          }
          catch (SessionStateException ex)
          {
            this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
          }
          catch (PSArgumentException ex)
          {
            this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
          }
        }
      }
    }

    protected override void GetChildItems(string path, bool recurse)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        CommandOrigin origin = this.Context.Origin;
        if (string.IsNullOrEmpty(path))
        {
          IDictionary sessionStateTable;
          try
          {
            sessionStateTable = this.GetSessionStateTable();
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "GetTableSecurityException", ErrorCategory.ReadError, (object) path));
            return;
          }
          List<DictionaryEntry> dictionaryEntryList = new List<DictionaryEntry>(sessionStateTable.Count + 1);
          foreach (DictionaryEntry dictionaryEntry in sessionStateTable)
            dictionaryEntryList.Add(dictionaryEntry);
          dictionaryEntryList.Sort((Comparison<DictionaryEntry>) ((left, right) => StringComparer.CurrentCultureIgnoreCase.Compare((string) left.Key, (string) right.Key)));
          foreach (DictionaryEntry dictionaryEntry in dictionaryEntryList)
          {
            try
            {
              if (SessionState.IsVisible(origin, dictionaryEntry.Value))
                this.WriteItemObject(dictionaryEntry.Value, (string) dictionaryEntry.Key, false);
            }
            catch (PSArgumentException ex)
            {
              this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
              break;
            }
            catch (SecurityException ex)
            {
              this.WriteError(new ErrorRecord((Exception) ex, "GetItemSecurityException", ErrorCategory.PermissionDenied, (object) (string) dictionaryEntry.Key));
              break;
            }
          }
        }
        else
        {
          object sessionStateItem;
          try
          {
            sessionStateItem = this.GetSessionStateItem(path);
          }
          catch (PSArgumentException ex)
          {
            this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
            return;
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "GetItemSecurityException", ErrorCategory.PermissionDenied, (object) path));
            return;
          }
          if (sessionStateItem == null || !SessionState.IsVisible(origin, sessionStateItem))
            return;
          this.WriteItemObject(sessionStateItem, path, false);
        }
      }
    }

    protected override void GetChildNames(string path, ReturnContainers returnContainers)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        CommandOrigin origin = this.Context.Origin;
        if (string.IsNullOrEmpty(path))
        {
          IDictionary sessionStateTable;
          try
          {
            sessionStateTable = this.GetSessionStateTable();
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "GetChildNamesSecurityException", ErrorCategory.ReadError, (object) path));
            return;
          }
          foreach (DictionaryEntry dictionaryEntry in sessionStateTable)
          {
            try
            {
              if (SessionState.IsVisible(origin, dictionaryEntry.Value))
                this.WriteItemObject(dictionaryEntry.Key, (string) dictionaryEntry.Key, false);
            }
            catch (PSArgumentException ex)
            {
              this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
              break;
            }
            catch (SecurityException ex)
            {
              this.WriteError(new ErrorRecord((Exception) ex, "GetItemSecurityException", ErrorCategory.PermissionDenied, (object) (string) dictionaryEntry.Key));
              break;
            }
          }
        }
        else
        {
          object sessionStateItem;
          try
          {
            sessionStateItem = this.GetSessionStateItem(path);
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "GetChildNamesSecurityException", ErrorCategory.ReadError, (object) path));
            return;
          }
          if (sessionStateItem == null || !SessionState.IsVisible(origin, sessionStateItem))
            return;
          this.WriteItemObject((object) path, path, false);
        }
      }
    }

    protected override bool HasChildItems(string path)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        bool flag = false;
        if (string.IsNullOrEmpty(path))
        {
          try
          {
            if (this.GetSessionStateTable().Count > 0)
              flag = true;
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "HasChildItemsSecurityException", ErrorCategory.ReadError, (object) path));
          }
        }
        else
          flag = false;
        SessionStateProviderBase.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }

    protected override bool ItemExists(string path)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        bool flag = false;
        if (string.IsNullOrEmpty(path))
        {
          flag = true;
        }
        else
        {
          object obj = (object) null;
          try
          {
            obj = this.GetSessionStateItem(path);
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "ItemExistsSecurityException", ErrorCategory.ReadError, (object) path));
          }
          if (obj != null)
            flag = true;
        }
        SessionStateProviderBase.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }

    protected override bool IsValidPath(string path)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        bool flag = true;
        if (string.IsNullOrEmpty(path))
          flag = false;
        SessionStateProviderBase.tracer.WriteLine("result = {0}", (object) flag);
        return flag;
      }
    }

    protected override void RemoveItem(string path, bool recurse)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        if (string.IsNullOrEmpty(path))
        {
          this.WriteError(new ErrorRecord((Exception) SessionStateProviderBase.tracer.NewArgumentException(nameof (path)), "RemoveItemNullPath", ErrorCategory.InvalidArgument, (object) path));
        }
        else
        {
          string resourceString = ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "RemoveItemAction");
          if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "RemoveItemResourceTemplate"), (object) path), resourceString))
            return;
          try
          {
            this.RemoveSessionStateItem(path);
          }
          catch (SessionStateException ex)
          {
            this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "RemoveItemSecurityException", ErrorCategory.PermissionDenied, (object) path));
          }
          catch (PSArgumentException ex)
          {
            this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
          }
        }
      }
    }

    protected override void NewItem(string path, string type, object newItem)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        if (string.IsNullOrEmpty(path))
          this.WriteError(new ErrorRecord((Exception) SessionStateProviderBase.tracer.NewArgumentException(nameof (path)), "NewItemNullPath", ErrorCategory.InvalidArgument, (object) path));
        else if (newItem == null)
          this.WriteError(new ErrorRecord((Exception) SessionStateProviderBase.tracer.NewArgumentNullException("value"), "NewItemValueNotSpecified", ErrorCategory.InvalidArgument, (object) path));
        else if (this.ItemExists(path) && !(bool) this.Force)
        {
          PSArgumentException argumentException = SessionStateProviderBase.tracer.NewArgumentException(nameof (path), "SessionStateStrings", "NewItemAlreadyExists", (object) path);
          this.WriteError(new ErrorRecord(argumentException.ErrorRecord, (Exception) argumentException));
        }
        else
        {
          string resourceString = ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "NewItemAction");
          if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "NewItemResourceTemplate"), (object) path, (object) type, newItem), resourceString))
            return;
          this.SetItem(path, newItem);
        }
      }
    }

    protected override void CopyItem(string path, string copyPath, bool recurse)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
      {
        if (string.IsNullOrEmpty(path))
          this.WriteError(new ErrorRecord((Exception) SessionStateProviderBase.tracer.NewArgumentException(nameof (path)), "CopyItemNullPath", ErrorCategory.InvalidArgument, (object) path));
        else if (string.IsNullOrEmpty(copyPath))
        {
          this.GetItem(path);
        }
        else
        {
          object sessionStateItem;
          try
          {
            sessionStateItem = this.GetSessionStateItem(path);
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "CopyItemSecurityException", ErrorCategory.ReadError, (object) path));
            return;
          }
          if (sessionStateItem != null)
          {
            string resourceString = ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "CopyItemAction");
            if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "CopyItemResourceTemplate"), (object) path, (object) copyPath), resourceString))
              return;
            try
            {
              this.SetSessionStateItem(copyPath, this.GetValueOfItem(sessionStateItem), true);
            }
            catch (SessionStateException ex)
            {
              this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
            }
            catch (PSArgumentException ex)
            {
              this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
            }
          }
          else
          {
            PSArgumentException argumentException = SessionStateProviderBase.tracer.NewArgumentException(nameof (path), "SessionStateStrings", "CopyItemDoesntExist", (object) path);
            this.WriteError(new ErrorRecord(argumentException.ErrorRecord, (Exception) argumentException));
          }
        }
      }
    }

    protected override void RenameItem(string name, string newName)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(name, new object[0]))
      {
        if (string.IsNullOrEmpty(name))
        {
          this.WriteError(new ErrorRecord((Exception) SessionStateProviderBase.tracer.NewArgumentException(nameof (name)), "RenameItemNullPath", ErrorCategory.InvalidArgument, (object) name));
        }
        else
        {
          object sessionStateItem;
          try
          {
            sessionStateItem = this.GetSessionStateItem(name);
          }
          catch (SecurityException ex)
          {
            this.WriteError(new ErrorRecord((Exception) ex, "RenameItemSecurityException", ErrorCategory.ReadError, (object) name));
            return;
          }
          if (sessionStateItem != null)
          {
            if (this.ItemExists(newName))
            {
              if (!(bool) this.Force)
              {
                PSArgumentException argumentException = SessionStateProviderBase.tracer.NewArgumentException(nameof (newName), "SessionStateStrings", "NewItemAlreadyExists", (object) newName);
                this.WriteError(new ErrorRecord(argumentException.ErrorRecord, (Exception) argumentException));
                return;
              }
            }
            try
            {
              if (!this.CanRenameItem(sessionStateItem))
                return;
              string resourceString = ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "RenameItemAction");
              if (!this.ShouldProcess(string.Format((IFormatProvider) this.Host.CurrentCulture, ResourceManagerCache.GetResourceString("SessionStateProviderBaseStrings", "RenameItemResourceTemplate"), (object) name, (object) newName), resourceString))
                return;
              if (string.Equals(name, newName, StringComparison.OrdinalIgnoreCase))
              {
                this.GetItem(newName);
              }
              else
              {
                try
                {
                  this.SetSessionStateItem(newName, sessionStateItem, true);
                  this.RemoveSessionStateItem(name);
                }
                catch (SessionStateException ex)
                {
                  this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
                }
                catch (PSArgumentException ex)
                {
                  this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
                }
                catch (SecurityException ex)
                {
                  this.WriteError(new ErrorRecord((Exception) ex, "RenameItemSecurityException", ErrorCategory.PermissionDenied, (object) name));
                }
              }
            }
            catch (SessionStateException ex)
            {
              this.WriteError(new ErrorRecord(ex.ErrorRecord, (Exception) ex));
            }
          }
          else
          {
            PSArgumentException argumentException = SessionStateProviderBase.tracer.NewArgumentException(nameof (name), "SessionStateStrings", "RenameItemDoesntExist", (object) name);
            this.WriteError(new ErrorRecord(argumentException.ErrorRecord, (Exception) argumentException));
          }
        }
      }
    }

    public IContentReader GetContentReader(string path)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
        return (IContentReader) new SessionStateProviderBaseContentReaderWriter(path, this);
    }

    public IContentWriter GetContentWriter(string path)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
        return (IContentWriter) new SessionStateProviderBaseContentReaderWriter(path, this);
    }

    public void ClearContent(string path)
    {
      using (SessionStateProviderBase.tracer.TraceMethod(path, new object[0]))
        throw SessionStateProviderBase.tracer.NewNotSupportedException("SessionStateStrings", "IContent_Clear_NotSupported");
    }

    public object GetContentReaderDynamicParameters(string path) => (object) null;

    public object GetContentWriterDynamicParameters(string path) => (object) null;

    public object ClearContentDynamicParameters(string path) => (object) null;
  }
}
