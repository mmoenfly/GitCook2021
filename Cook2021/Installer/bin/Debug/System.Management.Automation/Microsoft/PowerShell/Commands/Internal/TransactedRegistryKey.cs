// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.TransactedRegistryKey
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using System.Transactions;

namespace Microsoft.PowerShell.Commands.Internal
{
  [ComVisible(true)]
  public sealed class TransactedRegistryKey : MarshalByRefObject, IDisposable
  {
    private const string resBaseName = "RegistryProviderStrings";
    private const int STATE_DIRTY = 1;
    private const int STATE_SYSTEMKEY = 2;
    private const int STATE_WRITEACCESS = 4;
    private const int MaxKeyLength = 255;
    private const int MaxValueNameLength = 16383;
    private const int MaxValueDataLength = 1048576;
    private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;
    private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;
    private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;
    private static readonly string[] hkeyNames = new string[7]
    {
      "HKEY_CLASSES_ROOT",
      "HKEY_CURRENT_USER",
      "HKEY_LOCAL_MACHINE",
      "HKEY_USERS",
      "HKEY_PERFORMANCE_DATA",
      "HKEY_CURRENT_CONFIG",
      "HKEY_DYN_DATA"
    };
    private SafeRegistryHandle hkey;
    private int state;
    private string keyName;
    private RegistryKeyPermissionCheck checkMode;
    private Transaction myTransaction;
    private SafeTransactionHandle myTransactionHandle;

    private int RegOpenKeyTransactedWrapper(
      SafeRegistryHandle hKey,
      string lpSubKey,
      int ulOptions,
      int samDesired,
      out SafeRegistryHandle hkResult,
      SafeTransactionHandle hTransaction,
      IntPtr pExtendedParameter)
    {
      SafeRegistryHandle hkResult1 = (SafeRegistryHandle) null;
      int num = Win32Native.RegOpenKeyTransacted(this.hkey, lpSubKey, ulOptions, samDesired, out hkResult1, hTransaction, pExtendedParameter);
      if (num == 0 && !hkResult1.IsInvalid)
      {
        int lpcSubKeys = 0;
        int lpcValues = 0;
        num = Win32Native.RegQueryInfoKey(hkResult1, (StringBuilder) null, (int[]) null, Win32Native.NULL, ref lpcSubKeys, (int[]) null, (int[]) null, ref lpcValues, (int[]) null, (int[]) null, (int[]) null, (int[]) null);
        if (6700 == num)
        {
          SafeRegistryHandle hkResult2 = (SafeRegistryHandle) null;
          SafeRegistryHandle hkResult3 = (SafeRegistryHandle) null;
          num = Win32Native.RegOpenKeyEx(this.hkey, lpSubKey, ulOptions, samDesired, out hkResult2);
          if (num == 0)
          {
            num = Win32Native.RegOpenKeyTransacted(hkResult2, (string) null, ulOptions, samDesired, out hkResult3, hTransaction, pExtendedParameter);
            if (num == 0)
            {
              hkResult1.Dispose();
              hkResult1 = hkResult3;
            }
            hkResult2.Dispose();
          }
        }
      }
      hkResult = hkResult1;
      return num;
    }

    private TransactedRegistryKey(
      SafeRegistryHandle hkey,
      bool writable,
      bool systemkey,
      Transaction transaction,
      SafeTransactionHandle txHandle)
    {
      this.hkey = hkey;
      this.keyName = "";
      if (systemkey)
        this.state |= 2;
      if (writable)
        this.state |= 4;
      if ((Transaction) null != transaction)
      {
        this.myTransaction = transaction.Clone();
        this.myTransactionHandle = txHandle;
      }
      else
      {
        this.myTransaction = (Transaction) null;
        this.myTransactionHandle = (SafeTransactionHandle) null;
      }
    }

    private SafeTransactionHandle GetTransactionHandle()
    {
      SafeTransactionHandle transactionHandle;
      if ((Transaction) null != this.myTransaction)
      {
        if (!this.myTransaction.Equals((object) Transaction.Current))
          throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "InvalidOperation_MustUseSameTransaction"));
        transactionHandle = this.myTransactionHandle;
      }
      else
        transactionHandle = SafeTransactionHandle.Create();
      return transactionHandle;
    }

    public void Close() => this.Dispose(true);

    private void Dispose(bool disposing)
    {
      if (this.hkey != null)
      {
        if (!this.IsSystemKey())
        {
          try
          {
            this.hkey.Dispose();
          }
          catch (IOException ex)
          {
          }
          finally
          {
            this.hkey = (SafeRegistryHandle) null;
          }
        }
      }
      if (!((Transaction) null != this.myTransaction))
        return;
      try
      {
        this.myTransaction.Dispose();
      }
      catch (TransactionException ex)
      {
      }
      finally
      {
        this.myTransaction = (Transaction) null;
      }
    }

    public void Flush()
    {
      this.VerifyTransaction();
      if (this.hkey == null || !this.IsDirty())
        return;
      int num = Win32Native.RegFlushKey(this.hkey);
      if (num != 0)
        throw new IOException(Win32Native.GetMessage(num), num);
    }

    public void Dispose() => this.Dispose(true);

    public TransactedRegistryKey CreateSubKey(string subkey) => this.CreateSubKey(subkey, this.checkMode);

    [ComVisible(false)]
    public TransactedRegistryKey CreateSubKey(
      string subkey,
      RegistryKeyPermissionCheck permissionCheck)
    {
      return this.CreateSubKeyInternal(subkey, permissionCheck, (object) null);
    }

    [ComVisible(false)]
    public TransactedRegistryKey CreateSubKey(
      string subkey,
      RegistryKeyPermissionCheck permissionCheck,
      TransactedRegistrySecurity registrySecurity)
    {
      return this.CreateSubKeyInternal(subkey, permissionCheck, (object) registrySecurity);
    }

    [ComVisible(false)]
    private unsafe TransactedRegistryKey CreateSubKeyInternal(
      string subkey,
      RegistryKeyPermissionCheck permissionCheck,
      object registrySecurityObj)
    {
      TransactedRegistryKey.ValidateKeyName(subkey);
      if (string.Empty == subkey)
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegKeyStrEmpty"));
      TransactedRegistryKey.ValidateKeyMode(permissionCheck);
      this.EnsureWriteable();
      subkey = TransactedRegistryKey.FixupName(subkey);
      TransactedRegistryKey transactedRegistryKey1 = this.InternalOpenSubKey(subkey, permissionCheck != RegistryKeyPermissionCheck.ReadSubTree);
      if (transactedRegistryKey1 != null)
      {
        this.CheckSubKeyWritePermission(subkey);
        this.CheckSubTreePermission(subkey, permissionCheck);
        transactedRegistryKey1.checkMode = permissionCheck;
        return transactedRegistryKey1;
      }
      this.CheckSubKeyCreatePermission(subkey);
      Win32Native.SECURITY_ATTRIBUTES lpSecurityAttributes = (Win32Native.SECURITY_ATTRIBUTES) null;
      if (registrySecurityObj is TransactedRegistrySecurity registrySecurity)
      {
        lpSecurityAttributes = new Win32Native.SECURITY_ATTRIBUTES();
        lpSecurityAttributes.nLength = Marshal.SizeOf((object) lpSecurityAttributes);
        byte[] descriptorBinaryForm = registrySecurity.GetSecurityDescriptorBinaryForm();
        byte* pDest = stackalloc byte[descriptorBinaryForm.Length];
        Buffer.memcpy(descriptorBinaryForm, 0, pDest, 0, descriptorBinaryForm.Length);
        lpSecurityAttributes.pSecurityDescriptor = pDest;
      }
      int lpdwDisposition = 0;
      SafeRegistryHandle hkResult = (SafeRegistryHandle) null;
      SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
      int keyTransacted = Win32Native.RegCreateKeyTransacted(this.hkey, subkey, 0, (string) null, 0, TransactedRegistryKey.GetRegistryKeyAccess(permissionCheck != RegistryKeyPermissionCheck.ReadSubTree), lpSecurityAttributes, out hkResult, out lpdwDisposition, transactionHandle, IntPtr.Zero);
      if (keyTransacted == 0 && !hkResult.IsInvalid)
      {
        TransactedRegistryKey transactedRegistryKey2 = new TransactedRegistryKey(hkResult, permissionCheck != RegistryKeyPermissionCheck.ReadSubTree, false, Transaction.Current, transactionHandle);
        this.CheckSubTreePermission(subkey, permissionCheck);
        transactedRegistryKey2.checkMode = permissionCheck;
        transactedRegistryKey2.keyName = subkey.Length != 0 ? this.keyName + "\\" + subkey : this.keyName;
        return transactedRegistryKey2;
      }
      if (keyTransacted != 0)
        this.Win32Error(keyTransacted, this.keyName + "\\" + subkey);
      return (TransactedRegistryKey) null;
    }

    public void DeleteSubKey(string subkey) => this.DeleteSubKey(subkey, true);

    public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
    {
      TransactedRegistryKey.ValidateKeyName(subkey);
      this.EnsureWriteable();
      subkey = TransactedRegistryKey.FixupName(subkey);
      this.CheckSubKeyWritePermission(subkey);
      TransactedRegistryKey transactedRegistryKey = this.InternalOpenSubKey(subkey, false);
      if (transactedRegistryKey != null)
      {
        try
        {
          if (transactedRegistryKey.InternalSubKeyCount() > 0)
            throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "InvalidOperation_RegRemoveSubKey"));
        }
        finally
        {
          transactedRegistryKey.Close();
        }
        SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
        int errorCode = Win32Native.RegDeleteKeyTransacted(this.hkey, subkey, 0, 0U, transactionHandle, IntPtr.Zero);
        switch (errorCode)
        {
          case 0:
            break;
          case 2:
            if (!throwOnMissingSubKey)
              break;
            throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ArgumentException_RegSubKeyAbsent"));
          default:
            this.Win32Error(errorCode, (string) null);
            break;
        }
      }
      else if (throwOnMissingSubKey)
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ArgumentException_RegSubKeyAbsent"));
    }

    public void DeleteSubKeyTree(string subkey)
    {
      TransactedRegistryKey.ValidateKeyName(subkey);
      if ((string.IsNullOrEmpty(subkey) || subkey.Length == 0) && this.IsSystemKey())
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ArgRegKeyDelHive"));
      this.EnsureWriteable();
      SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
      subkey = TransactedRegistryKey.FixupName(subkey);
      this.CheckSubTreeWritePermission(subkey);
      TransactedRegistryKey transactedRegistryKey = this.InternalOpenSubKey(subkey, true);
      if (transactedRegistryKey == null)
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSubKeyAbsent"));
      try
      {
        if (transactedRegistryKey.InternalSubKeyCount() > 0)
        {
          foreach (string subKeyName in transactedRegistryKey.InternalGetSubKeyNames())
            transactedRegistryKey.DeleteSubKeyTreeInternal(subKeyName);
        }
      }
      finally
      {
        transactedRegistryKey.Close();
      }
      int errorCode = Win32Native.RegDeleteKeyTransacted(this.hkey, subkey, 0, 0U, transactionHandle, IntPtr.Zero);
      if (errorCode == 0)
        return;
      this.Win32Error(errorCode, (string) null);
    }

    private void DeleteSubKeyTreeInternal(string subkey)
    {
      SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
      TransactedRegistryKey transactedRegistryKey = this.InternalOpenSubKey(subkey, true);
      if (transactedRegistryKey == null)
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSubKeyAbsent"));
      try
      {
        if (transactedRegistryKey.InternalSubKeyCount() > 0)
        {
          foreach (string subKeyName in transactedRegistryKey.InternalGetSubKeyNames())
            transactedRegistryKey.DeleteSubKeyTreeInternal(subKeyName);
        }
      }
      finally
      {
        transactedRegistryKey.Close();
      }
      int errorCode = Win32Native.RegDeleteKeyTransacted(this.hkey, subkey, 0, 0U, transactionHandle, IntPtr.Zero);
      if (errorCode == 0)
        return;
      this.Win32Error(errorCode, (string) null);
    }

    public void DeleteValue(string name) => this.DeleteValue(name, true);

    public void DeleteValue(string name, bool throwOnMissingValue)
    {
      this.EnsureWriteable();
      this.CheckValueWritePermission(name);
      this.VerifyTransaction();
      int errorCode = Win32Native.RegDeleteValue(this.hkey, name);
      if (errorCode == 2 || errorCode == 206)
      {
        if (throwOnMissingValue)
          throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSubKeyValueAbsent"));
        errorCode = 0;
      }
      if (errorCode == 0)
        return;
      this.Win32Error(errorCode, (string) null);
    }

    internal static TransactedRegistryKey GetBaseKey(IntPtr hKey)
    {
      int index = (int) hKey & 268435455;
      return new TransactedRegistryKey(new SafeRegistryHandle(hKey, false), true, true, (Transaction) null, (SafeTransactionHandle) null)
      {
        checkMode = RegistryKeyPermissionCheck.Default,
        keyName = TransactedRegistryKey.hkeyNames[index]
      };
    }

    public TransactedRegistryKey OpenSubKey(string name, bool writable)
    {
      TransactedRegistryKey.ValidateKeyName(name);
      this.EnsureNotDisposed();
      name = TransactedRegistryKey.FixupName(name);
      this.CheckOpenSubKeyPermission(name, writable);
      SafeRegistryHandle hkResult = (SafeRegistryHandle) null;
      SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
      int num = this.RegOpenKeyTransactedWrapper(this.hkey, name, 0, TransactedRegistryKey.GetRegistryKeyAccess(writable), out hkResult, transactionHandle, IntPtr.Zero);
      if (num == 0 && !hkResult.IsInvalid)
        return new TransactedRegistryKey(hkResult, writable, false, Transaction.Current, transactionHandle)
        {
          checkMode = this.GetSubKeyPermissonCheck(writable),
          keyName = this.keyName + "\\" + name
        };
      if (num == 5 || num == 1346)
        throw new SecurityException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Security_RegistryPermission"));
      return (TransactedRegistryKey) null;
    }

    [ComVisible(false)]
    public TransactedRegistryKey OpenSubKey(
      string name,
      RegistryKeyPermissionCheck permissionCheck)
    {
      TransactedRegistryKey.ValidateKeyMode(permissionCheck);
      return this.InternalOpenSubKey(name, permissionCheck, TransactedRegistryKey.GetRegistryKeyAccess(permissionCheck));
    }

    [ComVisible(false)]
    public TransactedRegistryKey OpenSubKey(
      string name,
      RegistryKeyPermissionCheck permissionCheck,
      RegistryRights rights)
    {
      return this.InternalOpenSubKey(name, permissionCheck, (int) rights);
    }

    private TransactedRegistryKey InternalOpenSubKey(
      string name,
      RegistryKeyPermissionCheck permissionCheck,
      int rights)
    {
      TransactedRegistryKey.ValidateKeyName(name);
      TransactedRegistryKey.ValidateKeyMode(permissionCheck);
      TransactedRegistryKey.ValidateKeyRights(rights);
      this.EnsureNotDisposed();
      name = TransactedRegistryKey.FixupName(name);
      this.CheckOpenSubKeyPermission(name, permissionCheck);
      SafeRegistryHandle hkResult = (SafeRegistryHandle) null;
      SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
      int num = this.RegOpenKeyTransactedWrapper(this.hkey, name, 0, rights, out hkResult, transactionHandle, IntPtr.Zero);
      if (num == 0 && !hkResult.IsInvalid)
        return new TransactedRegistryKey(hkResult, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree, false, Transaction.Current, transactionHandle)
        {
          keyName = this.keyName + "\\" + name,
          checkMode = permissionCheck
        };
      if (num == 5 || num == 1346)
        throw new SecurityException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Security_RegistryPermission"));
      return (TransactedRegistryKey) null;
    }

    internal TransactedRegistryKey InternalOpenSubKey(
      string name,
      bool writable)
    {
      TransactedRegistryKey.ValidateKeyName(name);
      this.EnsureNotDisposed();
      int registryKeyAccess = TransactedRegistryKey.GetRegistryKeyAccess(writable);
      SafeRegistryHandle hkResult = (SafeRegistryHandle) null;
      SafeTransactionHandle transactionHandle = this.GetTransactionHandle();
      if (this.RegOpenKeyTransactedWrapper(this.hkey, name, 0, registryKeyAccess, out hkResult, transactionHandle, IntPtr.Zero) != 0 || hkResult.IsInvalid)
        return (TransactedRegistryKey) null;
      return new TransactedRegistryKey(hkResult, writable, false, Transaction.Current, transactionHandle)
      {
        keyName = this.keyName + "\\" + name
      };
    }

    public TransactedRegistryKey OpenSubKey(string name) => this.OpenSubKey(name, false);

    public int SubKeyCount
    {
      get
      {
        this.CheckKeyReadPermission();
        return this.InternalSubKeyCount();
      }
    }

    internal int InternalSubKeyCount()
    {
      this.EnsureNotDisposed();
      int lpcSubKeys = 0;
      int lpcValues = 0;
      int errorCode = Win32Native.RegQueryInfoKey(this.hkey, (StringBuilder) null, (int[]) null, Win32Native.NULL, ref lpcSubKeys, (int[]) null, (int[]) null, ref lpcValues, (int[]) null, (int[]) null, (int[]) null, (int[]) null);
      if (errorCode != 0)
        this.Win32Error(errorCode, (string) null);
      return lpcSubKeys;
    }

    public string[] GetSubKeyNames()
    {
      this.CheckKeyReadPermission();
      return this.InternalGetSubKeyNames();
    }

    internal string[] InternalGetSubKeyNames()
    {
      this.EnsureNotDisposed();
      int length = this.InternalSubKeyCount();
      string[] strArray = new string[length];
      if (length > 0)
      {
        StringBuilder lpName = new StringBuilder(256);
        for (int dwIndex = 0; dwIndex < length; ++dwIndex)
        {
          int lpcbName = lpName.Capacity;
          int errorCode = Win32Native.RegEnumKeyEx(this.hkey, dwIndex, lpName, out lpcbName, (int[]) null, (StringBuilder) null, (int[]) null, (long[]) null);
          if (errorCode != 0)
            this.Win32Error(errorCode, (string) null);
          strArray[dwIndex] = lpName.ToString();
        }
      }
      return strArray;
    }

    public int ValueCount
    {
      get
      {
        this.CheckKeyReadPermission();
        return this.InternalValueCount();
      }
    }

    internal int InternalValueCount()
    {
      this.EnsureNotDisposed();
      int lpcValues = 0;
      int lpcSubKeys = 0;
      int errorCode = Win32Native.RegQueryInfoKey(this.hkey, (StringBuilder) null, (int[]) null, Win32Native.NULL, ref lpcSubKeys, (int[]) null, (int[]) null, ref lpcValues, (int[]) null, (int[]) null, (int[]) null, (int[]) null);
      if (errorCode != 0)
        this.Win32Error(errorCode, (string) null);
      return lpcValues;
    }

    public string[] GetValueNames()
    {
      this.CheckKeyReadPermission();
      this.EnsureNotDisposed();
      int length = this.InternalValueCount();
      string[] strArray = new string[length];
      if (length > 0)
      {
        StringBuilder lpValueName = new StringBuilder(256);
        for (int dwIndex = 0; dwIndex < length; ++dwIndex)
        {
          int capacity = lpValueName.Capacity;
          int errorCode = 234;
          while (234 == errorCode)
          {
            int lpcbValueName = capacity;
            errorCode = Win32Native.RegEnumValue(this.hkey, dwIndex, lpValueName, ref lpcbValueName, Win32Native.NULL, (int[]) null, (byte[]) null, (int[]) null);
            switch (errorCode)
            {
              case 0:
                continue;
              case 234:
                if (16383 == capacity)
                  this.Win32Error(errorCode, (string) null);
                capacity *= 2;
                if (16383 < capacity)
                  capacity = 16383;
                lpValueName = new StringBuilder(capacity);
                continue;
              default:
                this.Win32Error(errorCode, (string) null);
                goto case 234;
            }
          }
          strArray[dwIndex] = lpValueName.ToString();
        }
      }
      return strArray;
    }

    public object GetValue(string name)
    {
      this.CheckValueReadPermission(name);
      return this.InternalGetValue(name, (object) null, false, true);
    }

    public object GetValue(string name, object defaultValue)
    {
      this.CheckValueReadPermission(name);
      return this.InternalGetValue(name, defaultValue, false, true);
    }

    [ComVisible(false)]
    public object GetValue(string name, object defaultValue, RegistryValueOptions options)
    {
      if (options < RegistryValueOptions.None || options > RegistryValueOptions.DoNotExpandEnvironmentNames)
        throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_EnumIllegalVal"), (object) options.ToString()));
      bool doNotExpand = options == RegistryValueOptions.DoNotExpandEnvironmentNames;
      this.CheckValueReadPermission(name);
      return this.InternalGetValue(name, defaultValue, doNotExpand, true);
    }

    internal object InternalGetValue(
      string name,
      object defaultValue,
      bool doNotExpand,
      bool checkSecurity)
    {
      if (checkSecurity)
        this.EnsureNotDisposed();
      object obj = defaultValue;
      int lpType = 0;
      int lpcbData = 0;
      switch (Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, (byte[]) null, ref lpcbData))
      {
        case 0:
        case 234:
          int num1;
          switch (lpType)
          {
            case 1:
              StringBuilder lpData1 = new StringBuilder(lpcbData / 2);
              num1 = Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, lpData1, ref lpcbData);
              obj = (object) lpData1.ToString();
              break;
            case 2:
              StringBuilder lpData2 = new StringBuilder(lpcbData / 2);
              num1 = Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, lpData2, ref lpcbData);
              obj = !doNotExpand ? (object) Environment.ExpandEnvironmentVariables(lpData2.ToString()) : (object) lpData2.ToString();
              break;
            case 3:
            case 5:
              byte[] lpData3 = new byte[lpcbData];
              num1 = Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, lpData3, ref lpcbData);
              obj = (object) lpData3;
              break;
            case 4:
              if (lpcbData <= 4)
              {
                int lpData4 = 0;
                num1 = Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, ref lpData4, ref lpcbData);
                obj = (object) lpData4;
                break;
              }
              goto case 11;
            case 7:
              IList<string> stringList = (IList<string>) new List<string>();
              char[] lpData5 = new char[lpcbData / 2];
              int num2 = Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, lpData5, ref lpcbData);
              int startIndex = 0;
              int index;
              for (int length = lpData5.Length; num2 == 0 && startIndex < length; startIndex = index + 1)
              {
                index = startIndex;
                while (index < length && lpData5[index] != char.MinValue)
                  ++index;
                if (index < length)
                {
                  if (index - startIndex > 0)
                    stringList.Add(new string(lpData5, startIndex, index - startIndex));
                  else if (index != length - 1)
                    stringList.Add(string.Empty);
                }
                else
                  stringList.Add(new string(lpData5, startIndex, length - startIndex));
              }
              obj = (object) new string[stringList.Count];
              stringList.CopyTo((string[]) obj, 0);
              break;
            case 11:
              if (lpcbData <= 8)
              {
                long lpData4 = 0;
                num1 = Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, ref lpData4, ref lpcbData);
                obj = (object) lpData4;
                break;
              }
              goto case 3;
          }
          return obj;
        default:
          return obj;
      }
    }

    [ComVisible(false)]
    public RegistryValueKind GetValueKind(string name)
    {
      this.CheckValueReadPermission(name);
      this.EnsureNotDisposed();
      int lpType = 0;
      int lpcbData = 0;
      int errorCode = Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, (byte[]) null, ref lpcbData);
      if (errorCode != 0)
        this.Win32Error(errorCode, (string) null);
      return !Enum.IsDefined(typeof (RegistryValueKind), (object) lpType) ? RegistryValueKind.Unknown : (RegistryValueKind) lpType;
    }

    private bool IsDirty() => (this.state & 1) != 0;

    private bool IsSystemKey() => (this.state & 2) != 0;

    private bool IsWritable() => (this.state & 4) != 0;

    public string Name
    {
      get
      {
        this.EnsureNotDisposed();
        return this.keyName;
      }
    }

    private void SetDirty() => this.state |= 1;

    public void SetValue(string name, object value) => this.SetValue(name, value, RegistryValueKind.Unknown);

    [ComVisible(false)]
    public void SetValue(string name, object value, RegistryValueKind valueKind)
    {
      if (value == null)
        throw new ArgumentNullException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_Value"));
      if (name != null && name.Length > 16383)
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegValueNameStrLenBug"));
      if (!Enum.IsDefined(typeof (RegistryValueKind), (object) valueKind))
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegBadKeyKind"));
      this.EnsureWriteable();
      this.VerifyTransaction();
      if (this.ContainsRegistryValue(name))
        this.CheckValueWritePermission(name);
      else
        this.CheckValueCreatePermission(name);
      if (valueKind == RegistryValueKind.Unknown)
        valueKind = this.CalculateValueKind(value);
      int errorCode = 0;
      try
      {
        switch (valueKind)
        {
          case RegistryValueKind.String:
          case RegistryValueKind.ExpandString:
            string lpData1 = value.ToString();
            if (524288 < lpData1.Length)
              throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_ValueDataLenBug"));
            errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, valueKind, lpData1, lpData1.Length * 2 + 2);
            break;
          case RegistryValueKind.Binary:
            byte[] lpData2 = (byte[]) value;
            if (1048576 < lpData2.Length)
              throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_ValueDataLenBug"));
            errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.Binary, lpData2, lpData2.Length);
            break;
          case RegistryValueKind.DWord:
            int int32 = Convert.ToInt32(value, (IFormatProvider) CultureInfo.InvariantCulture);
            errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.DWord, ref int32, 4);
            break;
          case RegistryValueKind.MultiString:
            string[] strArray = (string[]) ((Array) value).Clone();
            int num = 0;
            for (int index = 0; index < strArray.Length; ++index)
            {
              if (strArray[index] == null)
                throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSetStrArrNull"));
              num += (strArray[index].Length + 1) * 2;
            }
            int cbData = num + 2;
            byte[] numArray1 = 1048576 >= cbData ? new byte[cbData] : throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_ValueDataLenBug"));
            byte[] numArray2;
            if ((numArray2 = numArray1) != null)
            {
              int length = numArray2.Length;
            }
            int byteIndex = 0;
            for (int index1 = 0; index1 < strArray.Length; ++index1)
            {
              int bytes = Encoding.Unicode.GetBytes(strArray[index1], 0, strArray[index1].Length, numArray1, byteIndex);
              int index2 = byteIndex + bytes;
              numArray1[index2] = (byte) 0;
              numArray1[index2 + 1] = (byte) 0;
              byteIndex = index2 + 2;
            }
            errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.MultiString, numArray1, cbData);
            break;
          case RegistryValueKind.QWord:
            long int64 = Convert.ToInt64(value, (IFormatProvider) CultureInfo.InvariantCulture);
            errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.QWord, ref int64, 8);
            break;
        }
      }
      catch (OverflowException ex)
      {
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSetMismatchedKind"));
      }
      catch (InvalidOperationException ex)
      {
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSetMismatchedKind"));
      }
      catch (FormatException ex)
      {
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSetMismatchedKind"));
      }
      catch (InvalidCastException ex)
      {
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSetMismatchedKind"));
      }
      if (errorCode == 0)
        this.SetDirty();
      else
        this.Win32Error(errorCode, (string) null);
    }

    private RegistryValueKind CalculateValueKind(object value)
    {
      switch (value)
      {
        case int _:
          return RegistryValueKind.DWord;
        case Array _:
          switch (value)
          {
            case byte[] _:
              return RegistryValueKind.Binary;
            case string[] _:
              return RegistryValueKind.MultiString;
            default:
              throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegSetBadArrType"), (object) value.GetType().Name));
          }
        default:
          return RegistryValueKind.String;
      }
    }

    public override string ToString()
    {
      this.EnsureNotDisposed();
      return this.keyName;
    }

    public TransactedRegistrySecurity GetAccessControl() => this.GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);

    public TransactedRegistrySecurity GetAccessControl(
      AccessControlSections includeSections)
    {
      this.EnsureNotDisposed();
      return new TransactedRegistrySecurity(this.hkey, this.keyName, includeSections);
    }

    public void SetAccessControl(TransactedRegistrySecurity registrySecurity)
    {
      this.EnsureWriteable();
      if (registrySecurity == null)
        throw new ArgumentNullException(nameof (registrySecurity));
      this.VerifyTransaction();
      registrySecurity.Persist(this.hkey, this.keyName);
    }

    internal void Win32Error(int errorCode, string str)
    {
      switch (errorCode)
      {
        case 2:
          throw new IOException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegKeyNotFound"), (object) errorCode.ToString((IFormatProvider) CultureInfo.InvariantCulture)));
        case 5:
          if (str != null)
            throw new UnauthorizedAccessException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "UnauthorizedAccess_RegistryKeyGeneric_Key"), (object) str));
          throw new UnauthorizedAccessException();
        case 6:
          this.hkey.SetHandleAsInvalid();
          this.hkey = (SafeRegistryHandle) null;
          break;
      }
      throw new IOException(Win32Native.GetMessage(errorCode), errorCode);
    }

    internal static void Win32ErrorStatic(int errorCode, string str)
    {
      if (errorCode != 5)
        throw new IOException(Win32Native.GetMessage(errorCode), errorCode);
      if (str != null)
        throw new UnauthorizedAccessException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "UnauthorizedAccess_RegistryKeyGeneric_Key"), (object) str));
      throw new UnauthorizedAccessException();
    }

    internal static string FixupName(string name)
    {
      if (name.IndexOf('\\') == -1)
        return name;
      StringBuilder path = new StringBuilder(name);
      TransactedRegistryKey.FixupPath(path);
      int index = path.Length - 1;
      if (path[index] == '\\')
        path.Length = index;
      return path.ToString();
    }

    private static void FixupPath(StringBuilder path)
    {
      int length = path.Length;
      bool flag = false;
      char maxValue = char.MaxValue;
      for (int index = 1; index < length - 1; ++index)
      {
        if (path[index] == '\\')
        {
          ++index;
          while (index < length && path[index] == '\\')
          {
            path[index] = maxValue;
            ++index;
            flag = true;
          }
        }
      }
      if (!flag)
        return;
      int index1 = 0;
      int index2 = 0;
      while (index1 < length)
      {
        if ((int) path[index1] == (int) maxValue)
        {
          ++index1;
        }
        else
        {
          path[index2] = path[index1];
          ++index1;
          ++index2;
        }
      }
      path.Length += index2 - index1;
    }

    private void CheckOpenSubKeyPermission(string subkeyName, bool subKeyWritable)
    {
      if (this.checkMode == RegistryKeyPermissionCheck.Default)
        this.CheckSubKeyReadPermission(subkeyName);
      if (!subKeyWritable || this.checkMode != RegistryKeyPermissionCheck.ReadSubTree)
        return;
      this.CheckSubTreeReadWritePermission(subkeyName);
    }

    private void CheckOpenSubKeyPermission(
      string subkeyName,
      RegistryKeyPermissionCheck subKeyCheck)
    {
      if (subKeyCheck == RegistryKeyPermissionCheck.Default && this.checkMode == RegistryKeyPermissionCheck.Default)
        this.CheckSubKeyReadPermission(subkeyName);
      this.CheckSubTreePermission(subkeyName, subKeyCheck);
    }

    private void CheckSubTreePermission(string subkeyName, RegistryKeyPermissionCheck subKeyCheck)
    {
      switch (subKeyCheck)
      {
        case RegistryKeyPermissionCheck.ReadSubTree:
          if (this.checkMode != RegistryKeyPermissionCheck.Default)
            break;
          this.CheckSubTreeReadPermission(subkeyName);
          break;
        case RegistryKeyPermissionCheck.ReadWriteSubTree:
          if (this.checkMode == RegistryKeyPermissionCheck.ReadWriteSubTree)
            break;
          this.CheckSubTreeReadWritePermission(subkeyName);
          break;
      }
    }

    private void CheckSubKeyWritePermission(string subkeyName)
    {
      if (this.checkMode != RegistryKeyPermissionCheck.Default)
        return;
      new RegistryPermission(RegistryPermissionAccess.Write, this.keyName + "\\" + subkeyName + "\\.").Demand();
    }

    private void CheckSubKeyReadPermission(string subkeyName) => new RegistryPermission(RegistryPermissionAccess.Read, this.keyName + "\\" + subkeyName + "\\.").Demand();

    private void CheckSubKeyCreatePermission(string subkeyName)
    {
      if (this.checkMode != RegistryKeyPermissionCheck.Default)
        return;
      new RegistryPermission(RegistryPermissionAccess.Create, this.keyName + "\\" + subkeyName + "\\.").Demand();
    }

    private void CheckSubTreeReadPermission(string subkeyName)
    {
      if (this.checkMode != RegistryKeyPermissionCheck.Default)
        return;
      new RegistryPermission(RegistryPermissionAccess.Read, this.keyName + "\\" + subkeyName + "\\").Demand();
    }

    private void CheckSubTreeWritePermission(string subkeyName)
    {
      if (this.checkMode != RegistryKeyPermissionCheck.Default)
        return;
      new RegistryPermission(RegistryPermissionAccess.Write, this.keyName + "\\" + subkeyName + "\\").Demand();
    }

    private void CheckSubTreeReadWritePermission(string subkeyName) => new RegistryPermission(RegistryPermissionAccess.Read | RegistryPermissionAccess.Write, this.keyName + "\\" + subkeyName).Demand();

    private void CheckValueWritePermission(string valueName)
    {
      if (this.checkMode != RegistryKeyPermissionCheck.Default)
        return;
      new RegistryPermission(RegistryPermissionAccess.Write, this.keyName + "\\" + valueName).Demand();
    }

    private void CheckValueCreatePermission(string valueName)
    {
      if (this.checkMode != RegistryKeyPermissionCheck.Default)
        return;
      new RegistryPermission(RegistryPermissionAccess.Create, this.keyName + "\\" + valueName).Demand();
    }

    private void CheckValueReadPermission(string valueName)
    {
      if (this.checkMode != RegistryKeyPermissionCheck.Default)
        return;
      new RegistryPermission(RegistryPermissionAccess.Read, this.keyName + "\\" + valueName).Demand();
    }

    private void CheckKeyReadPermission()
    {
      if (this.checkMode != RegistryKeyPermissionCheck.Default)
        return;
      new RegistryPermission(RegistryPermissionAccess.Read, this.keyName + "\\.").Demand();
    }

    private bool ContainsRegistryValue(string name)
    {
      int lpType = 0;
      int lpcbData = 0;
      return Win32Native.RegQueryValueEx(this.hkey, name, (int[]) null, ref lpType, (byte[]) null, ref lpcbData) == 0;
    }

    private void EnsureNotDisposed()
    {
      if (this.hkey == null)
        throw new ObjectDisposedException(this.keyName, ResourceManagerCache.GetResourceString("RegistryProviderStrings", "ObjectDisposed_RegKeyClosed"));
    }

    private void EnsureWriteable()
    {
      this.EnsureNotDisposed();
      if (!this.IsWritable())
        throw new UnauthorizedAccessException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "UnauthorizedAccess_RegistryNoWrite"));
    }

    private static int GetRegistryKeyAccess(bool isWritable) => isWritable ? 131103 : 131097;

    private static int GetRegistryKeyAccess(RegistryKeyPermissionCheck mode)
    {
      int num = 0;
      switch (mode)
      {
        case RegistryKeyPermissionCheck.Default:
        case RegistryKeyPermissionCheck.ReadSubTree:
          num = 131097;
          break;
        case RegistryKeyPermissionCheck.ReadWriteSubTree:
          num = 131103;
          break;
      }
      return num;
    }

    private RegistryKeyPermissionCheck GetSubKeyPermissonCheck(
      bool subkeyWritable)
    {
      if (this.checkMode == RegistryKeyPermissionCheck.Default)
        return this.checkMode;
      return subkeyWritable ? RegistryKeyPermissionCheck.ReadWriteSubTree : RegistryKeyPermissionCheck.ReadSubTree;
    }

    private static void ValidateKeyName(string name)
    {
      int num = name != null ? name.IndexOf("\\", StringComparison.OrdinalIgnoreCase) : throw new ArgumentNullException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_Name"));
      int startIndex = 0;
      for (; num != -1; num = name.IndexOf("\\", startIndex, StringComparison.OrdinalIgnoreCase))
      {
        if (num - startIndex > (int) byte.MaxValue)
          throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegKeyStrLenBug"));
        startIndex = num + 1;
      }
      if (name.Length - startIndex > (int) byte.MaxValue)
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Arg_RegKeyStrLenBug"));
    }

    private static void ValidateKeyMode(RegistryKeyPermissionCheck mode)
    {
      if (mode < RegistryKeyPermissionCheck.Default || mode > RegistryKeyPermissionCheck.ReadWriteSubTree)
        throw new ArgumentException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Argument_InvalidRegistryKeyPermissionCheck"));
    }

    private static void ValidateKeyRights(int rights)
    {
      if ((rights & -983104) != 0)
        throw new SecurityException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "Security_RegistryPermission"));
    }

    private void VerifyTransaction()
    {
      if ((Transaction) null == this.myTransaction)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "InvalidOperation_NotAssociatedWithTransaction"));
      if (!this.myTransaction.Equals((object) Transaction.Current))
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "InvalidOperation_MustUseSameTransaction"));
    }
  }
}
