// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemotingDecoder
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace System.Management.Automation
{
  internal static class RemotingDecoder
  {
    [TraceSource("RemotingDecoder", "RemotingDecoder")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RemotingDecoder), nameof (RemotingDecoder));

    private static T ConvertPropertyValueTo<T>(string propertyName, object propertyValue)
    {
      using (RemotingDecoder._trace.TraceMethod())
      {
        if (propertyName == null)
          throw RemotingDecoder._trace.NewArgumentNullException(nameof (propertyName));
        if (typeof (T).IsEnum)
        {
          if (propertyValue is string)
          {
            try
            {
              return (T) Enum.Parse(typeof (T), (string) propertyValue, true);
            }
            catch (ArgumentException ex)
            {
              throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastPropertyToExpectedType, new object[3]
              {
                (object) propertyName,
                (object) typeof (T).FullName,
                (object) propertyValue.GetType().FullName
              });
            }
          }
          else
          {
            try
            {
              Type underlyingType = Enum.GetUnderlyingType(typeof (T));
              return (T) LanguagePrimitives.ConvertTo(propertyValue, underlyingType, (IFormatProvider) CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException ex)
            {
              throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastPropertyToExpectedType, new object[3]
              {
                (object) propertyName,
                (object) typeof (T).FullName,
                (object) propertyValue.GetType().FullName
              });
            }
          }
        }
        else
        {
          if (typeof (T).Equals(typeof (PSObject)))
            return propertyValue == null ? default (T) : (T) PSObject.AsPSObject(propertyValue);
          switch (propertyValue)
          {
            case null:
              if (!typeof (T).IsValueType)
                return default (T);
              if (typeof (T).IsGenericType && typeof (T).GetGenericTypeDefinition().Equals(typeof (Nullable<>)))
                return default (T);
              throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastPropertyToExpectedType, new object[3]
              {
                (object) propertyName,
                (object) typeof (T).FullName,
                propertyValue != null ? (object) propertyValue.GetType().FullName : (object) "null"
              });
            case T obj:
              return obj;
            case PSObject _:
              PSObject psObject = (PSObject) propertyValue;
              return RemotingDecoder.ConvertPropertyValueTo<T>(propertyName, psObject.BaseObject);
            case Hashtable _:
              if (typeof (T).Equals(typeof (PSPrimitiveDictionary)))
              {
                try
                {
                  return (T) new PSPrimitiveDictionary((Hashtable) propertyValue);
                }
                catch (ArgumentException ex)
                {
                  throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastPropertyToExpectedType, new object[3]
                  {
                    (object) propertyName,
                    (object) typeof (T).FullName,
                    propertyValue != null ? (object) propertyValue.GetType().FullName : (object) "null"
                  });
                }
              }
              else
                break;
          }
          throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastPropertyToExpectedType, new object[3]
          {
            (object) propertyName,
            (object) typeof (T).FullName,
            (object) propertyValue.GetType().FullName
          });
        }
      }
    }

    private static PSPropertyInfo GetProperty(PSObject psObject, string propertyName)
    {
      using (RemotingDecoder._trace.TraceMethod())
      {
        if (psObject == null)
          throw RemotingDecoder._trace.NewArgumentNullException(nameof (psObject));
        if (propertyName == null)
          throw RemotingDecoder._trace.NewArgumentNullException(nameof (propertyName));
        return psObject.Properties[propertyName] ?? throw new PSRemotingDataStructureException(PSRemotingErrorId.MissingProperty, new object[1]
        {
          (object) propertyName
        });
      }
    }

    internal static T GetPropertyValue<T>(PSObject psObject, string propertyName)
    {
      using (RemotingDecoder._trace.TraceMethod())
      {
        if (psObject == null)
          throw RemotingDecoder._trace.NewArgumentNullException(nameof (psObject));
        if (propertyName == null)
          throw RemotingDecoder._trace.NewArgumentNullException(nameof (propertyName));
        object propertyValue = RemotingDecoder.GetProperty(psObject, propertyName).Value;
        return RemotingDecoder.ConvertPropertyValueTo<T>(propertyName, propertyValue);
      }
    }

    internal static IEnumerable<T> EnumerateListProperty<T>(
      PSObject psObject,
      string propertyName)
    {
      using (RemotingDecoder._trace.TraceMethod())
      {
        if (psObject == null)
          throw RemotingDecoder._trace.NewArgumentNullException(nameof (psObject));
        IEnumerable e = propertyName != null ? RemotingDecoder.GetPropertyValue<IEnumerable>(psObject, propertyName) : throw RemotingDecoder._trace.NewArgumentNullException(nameof (propertyName));
        if (e != null)
        {
          foreach (object propertyValue in e)
            yield return RemotingDecoder.ConvertPropertyValueTo<T>(propertyName, propertyValue);
        }
      }
    }

    internal static IEnumerable<KeyValuePair<KeyType, ValueType>> EnumerateHashtableProperty<KeyType, ValueType>(
      PSObject psObject,
      string propertyName)
    {
      using (RemotingDecoder._trace.TraceMethod())
      {
        if (psObject == null)
          throw RemotingDecoder._trace.NewArgumentNullException(nameof (psObject));
        Hashtable h = propertyName != null ? RemotingDecoder.GetPropertyValue<Hashtable>(psObject, propertyName) : throw RemotingDecoder._trace.NewArgumentNullException(nameof (propertyName));
        if (h != null)
        {
          foreach (DictionaryEntry dictionaryEntry in h)
          {
            KeyType key = RemotingDecoder.ConvertPropertyValueTo<KeyType>(propertyName, dictionaryEntry.Key);
            ValueType value = RemotingDecoder.ConvertPropertyValueTo<ValueType>(propertyName, dictionaryEntry.Value);
            yield return new KeyValuePair<KeyType, ValueType>(key, value);
          }
        }
      }
    }

    internal static RunspacePoolStateInfo GetRunspacePoolStateInfo(
      PSObject dataAsPSObject)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return dataAsPSObject != null ? new RunspacePoolStateInfo(RemotingDecoder.GetPropertyValue<RunspacePoolState>(dataAsPSObject, "RunspaceState"), RemotingDecoder.GetExceptionFromStateInfoObject(dataAsPSObject)) : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));
    }

    internal static PSPrimitiveDictionary GetApplicationPrivateData(
      PSObject dataAsPSObject)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return dataAsPSObject != null ? RemotingDecoder.GetPropertyValue<PSPrimitiveDictionary>(dataAsPSObject, "ApplicationPrivateData") : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));
    }

    internal static string GetPublicKey(PSObject dataAsPSObject) => dataAsPSObject != null ? RemotingDecoder.GetPropertyValue<string>(dataAsPSObject, "PublicKey") : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));

    internal static string GetEncryptedSessionKey(PSObject dataAsPSObject) => dataAsPSObject != null ? RemotingDecoder.GetPropertyValue<string>(dataAsPSObject, "EncryptedSessionKey") : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));

    internal static PSEventArgs GetPSEventArgs(PSObject dataAsPSObject)
    {
      using (RemotingDecoder._trace.TraceMethod())
      {
        int eventIdentifier = dataAsPSObject != null ? RemotingDecoder.GetPropertyValue<int>(dataAsPSObject, "PSEventArgs.EventIdentifier") : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));
        string propertyValue1 = RemotingDecoder.GetPropertyValue<string>(dataAsPSObject, "PSEventArgs.SourceIdentifier");
        object propertyValue2 = RemotingDecoder.GetPropertyValue<object>(dataAsPSObject, "PSEventArgs.Sender");
        object propertyValue3 = RemotingDecoder.GetPropertyValue<object>(dataAsPSObject, "PSEventArgs.MessageData");
        string propertyValue4 = RemotingDecoder.GetPropertyValue<string>(dataAsPSObject, "PSEventArgs.ComputerName");
        Guid propertyValue5 = RemotingDecoder.GetPropertyValue<Guid>(dataAsPSObject, "PSEventArgs.RunspaceId");
        ArrayList arrayList = new ArrayList();
        foreach (object obj in RemotingDecoder.EnumerateListProperty<object>(dataAsPSObject, "PSEventArgs.SourceArgs"))
          arrayList.Add(obj);
        return new PSEventArgs(propertyValue4, propertyValue5, eventIdentifier, propertyValue1, propertyValue2, arrayList.ToArray(), propertyValue3 == null ? (PSObject) null : PSObject.AsPSObject(propertyValue3))
        {
          TimeGenerated = RemotingDecoder.GetPropertyValue<DateTime>(dataAsPSObject, "PSEventArgs.TimeGenerated")
        };
      }
    }

    internal static int GetMinRunspaces(PSObject dataAsPSObject)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return dataAsPSObject != null ? RemotingDecoder.GetPropertyValue<int>(dataAsPSObject, "MinRunspaces") : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));
    }

    internal static int GetMaxRunspaces(PSObject dataAsPSObject)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return dataAsPSObject != null ? RemotingDecoder.GetPropertyValue<int>(dataAsPSObject, "MaxRunspaces") : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));
    }

    internal static PSPrimitiveDictionary GetApplicationArguments(
      PSObject dataAsPSObject)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return dataAsPSObject != null ? RemotingDecoder.GetPropertyValue<PSPrimitiveDictionary>(dataAsPSObject, "ApplicationArguments") : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));
    }

    internal static PSThreadOptions GetThreadOptions(PSObject dataAsPSObject)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return dataAsPSObject != null ? RemotingDecoder.GetPropertyValue<PSThreadOptions>(dataAsPSObject, "PSThreadOptions") : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));
    }

    internal static HostInfo GetHostInfo(PSObject dataAsPSObject)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return dataAsPSObject != null ? RemoteHostEncoder.DecodeObject((object) RemotingDecoder.GetPropertyValue<PSObject>(dataAsPSObject, "HostInfo"), typeof (HostInfo)) as HostInfo : throw RemotingDecoder._trace.NewArgumentNullException(nameof (dataAsPSObject));
    }

    private static Exception GetExceptionFromStateInfoObject(PSObject stateInfo)
    {
      using (RemotingDecoder._trace.TraceMethod())
      {
        PSPropertyInfo property = stateInfo.Properties["ExceptionAsErrorRecord"];
        return property != null && property.Value != null ? RemotingDecoder.GetExceptionFromSerializedErrorRecord(property.Value) : (Exception) null;
      }
    }

    internal static Exception GetExceptionFromSerializedErrorRecord(
      object serializedErrorRecord)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return (ErrorRecord.FromPSObjectForRemoting(PSObject.AsPSObject(serializedErrorRecord)) ?? throw new PSRemotingDataStructureException(PSRemotingErrorId.DecodingErrorForErrorRecord, new object[0])).Exception;
    }

    internal static object GetPowerShellOutput(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return data;
    }

    internal static PSInvocationStateInfo GetPowerShellStateInfo(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return data is PSObject psObject ? new PSInvocationStateInfo(RemotingDecoder.GetPropertyValue<PSInvocationState>(psObject, "PipelineState"), RemotingDecoder.GetExceptionFromStateInfoObject(psObject)) : throw new PSRemotingDataStructureException(PSRemotingErrorId.DecodingErrorForPowerShellStateInfo, new object[0]);
    }

    internal static ErrorRecord GetPowerShellError(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return data != null ? ErrorRecord.FromPSObjectForRemoting(data as PSObject) : throw RemotingDecoder._trace.NewArgumentNullException(nameof (data));
    }

    internal static WarningRecord GetPowerShellWarning(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return data != null ? new WarningRecord((PSObject) data) : throw RemotingDecoder._trace.NewArgumentNullException(nameof (data));
    }

    internal static VerboseRecord GetPowerShellVerbose(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return data != null ? new VerboseRecord((PSObject) data) : throw RemotingDecoder._trace.NewArgumentNullException(nameof (data));
    }

    internal static DebugRecord GetPowerShellDebug(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return data != null ? new DebugRecord((PSObject) data) : throw RemotingDecoder._trace.NewArgumentNullException(nameof (data));
    }

    internal static ProgressRecord GetPowerShellProgress(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return ProgressRecord.FromPSObjectForRemoting(PSObject.AsPSObject(data) ?? throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastRemotingDataToPSObject, new object[1]
        {
          (object) data.GetType().FullName
        }));
    }

    internal static PowerShell GetPowerShell(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return PowerShell.FromPSObjectForRemoting(RemotingDecoder.GetPropertyValue<PSObject>(PSObject.AsPSObject(data) ?? throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastRemotingDataToPSObject, new object[1]
        {
          (object) data.GetType().FullName
        }), "PowerShell"));
    }

    internal static PowerShell GetCommandDiscoveryPipeline(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
      {
        PSObject psObject = PSObject.AsPSObject(data);
        CommandTypes commandTypes = psObject != null ? RemotingDecoder.GetPropertyValue<CommandTypes>(psObject, "CommandType") : throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastRemotingDataToPSObject, new object[1]
        {
          (object) data.GetType().FullName
        });
        string[] strArray1;
        if (RemotingDecoder.GetPropertyValue<PSObject>(psObject, "Name") != null)
          strArray1 = new List<string>(RemotingDecoder.EnumerateListProperty<string>(psObject, "Name")).ToArray();
        else
          strArray1 = new string[1]{ "*" };
        string[] strArray2;
        if (RemotingDecoder.GetPropertyValue<PSObject>(psObject, "Namespace") != null)
          strArray2 = new List<string>(RemotingDecoder.EnumerateListProperty<string>(psObject, "Namespace")).ToArray();
        else
          strArray2 = new string[1]{ "" };
        object[] objArray = RemotingDecoder.GetPropertyValue<PSObject>(psObject, "ArgumentList") == null ? (object[]) null : new List<object>(RemotingDecoder.EnumerateListProperty<object>(psObject, "ArgumentList")).ToArray();
        PowerShell powerShell = PowerShell.Create();
        powerShell.AddCommand("Get-Command");
        powerShell.AddParameter("Name", (object) strArray1);
        powerShell.AddParameter("CommandType", (object) commandTypes);
        powerShell.AddParameter("Module", (object) strArray2);
        powerShell.AddParameter("ArgumentList", (object) objArray);
        return powerShell;
      }
    }

    internal static bool GetNoInput(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return RemotingDecoder.GetPropertyValue<bool>(PSObject.AsPSObject(data) ?? throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastRemotingDataToPSObject, new object[1]
        {
          (object) data.GetType().FullName
        }), "NoInput");
    }

    internal static bool GetAddToHistory(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return RemotingDecoder.GetPropertyValue<bool>(PSObject.AsPSObject(data) ?? throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastRemotingDataToPSObject, new object[1]
        {
          (object) data.GetType().FullName
        }), "AddToHistory");
    }

    internal static ApartmentState GetApartmentState(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return RemotingDecoder.GetPropertyValue<ApartmentState>(PSObject.AsPSObject(data), "ApartmentState");
    }

    internal static RemoteStreamOptions GetRemoteStreamOptions(object data)
    {
      using (RemotingDecoder._trace.TraceMethod())
        return RemotingDecoder.GetPropertyValue<RemoteStreamOptions>(PSObject.AsPSObject(data), "RemoteStreamOptions");
    }

    internal static RemoteSessionCapability GetSessionCapability(object data)
    {
      RemoteSessionCapability sessionCapability = data is PSObject psObject ? new RemoteSessionCapability(RemotingDestination.InvalidDestination, RemotingDecoder.GetPropertyValue<Version>(psObject, "protocolversion"), RemotingDecoder.GetPropertyValue<Version>(psObject, "PSVersion"), RemotingDecoder.GetPropertyValue<Version>(psObject, "SerializationVersion")) : throw new PSRemotingDataStructureException(PSRemotingErrorId.CantCastRemotingDataToPSObject, new object[1]
      {
        (object) data.GetType().FullName
      });
      if (psObject.Properties["TimeZone"] != null)
      {
        byte[] propertyValue = RemotingDecoder.GetPropertyValue<byte[]>(psObject, "TimeZone");
        sessionCapability.TimeZone = RemoteSessionCapability.ConvertFromByteToTimeZone(propertyValue);
      }
      return sessionCapability;
    }
  }
}
