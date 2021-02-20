// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSSQMAPI
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using System.Collections.Generic;
using System.Globalization;

namespace System.Management.Automation.Internal
{
  internal static class PSSQMAPI
  {
    private static readonly long timeValueThreshold;
    private static readonly Dictionary<string, uint> cmdletData;
    private static readonly Dictionary<uint, uint> dataValueCache;
    private static readonly Dictionary<Guid, long> runspaceDurationData;
    private static long startedAtTick;
    private static readonly object syncObject = new object();
    private static bool isWinSQMEnabled;

    static PSSQMAPI()
    {
      if (!WinSQMWrapper.IsWinSqmOptedIn())
        return;
      PSSQMAPI.dataValueCache = new Dictionary<uint, uint>();
      PSSQMAPI.cmdletData = new Dictionary<string, uint>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      PSSQMAPI.runspaceDurationData = new Dictionary<Guid, long>();
      PSSQMAPI.timeValueThreshold = 600000000L;
      AppDomain.CurrentDomain.ProcessExit += new EventHandler(PSSQMAPI.CurrentDomain_ProcessExit);
      PSSQMAPI.startedAtTick = DateTime.Now.Ticks;
      PSSQMAPI.isWinSQMEnabled = true;
    }

    public static void NoteRunspaceStart(Guid rsInstanceId)
    {
      if (!PSSQMAPI.isWinSQMEnabled)
        return;
      lock (PSSQMAPI.syncObject)
        PSSQMAPI.runspaceDurationData[rsInstanceId] = DateTime.Now.Ticks;
    }

    public static void NoteRunspaceEnd(Guid rsInstanceId)
    {
      if (!PSSQMAPI.isWinSQMEnabled)
        return;
      long ticks1 = DateTime.Now.Ticks;
      long num = ticks1;
      lock (PSSQMAPI.syncObject)
      {
        if (!PSSQMAPI.runspaceDurationData.ContainsKey(rsInstanceId))
          return;
        num = PSSQMAPI.runspaceDurationData[rsInstanceId];
        PSSQMAPI.runspaceDurationData.Remove(rsInstanceId);
      }
      try
      {
        long ticks2 = ticks1 - num;
        if (ticks2 < PSSQMAPI.timeValueThreshold)
          return;
        WinSQMWrapper.WinSqmAddToStream(3088U, new TimeSpan(ticks2).TotalMinutes.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }

    public static void UpdateExecutionPolicy(string shellID, ExecutionPolicy executionPolicy)
    {
      if (!PSSQMAPI.isWinSQMEnabled || !shellID.Equals(Utils.DefaultPowerShellShellID, StringComparison.OrdinalIgnoreCase))
        return;
      lock (PSSQMAPI.syncObject)
        PSSQMAPI.dataValueCache[3086U] = (uint) executionPolicy;
    }

    public static void IncrementData(CommandTypes cmdType)
    {
      if (!PSSQMAPI.isWinSQMEnabled)
        return;
      PSSqmDataPoint psSqmDataPoint;
      switch (cmdType)
      {
        case CommandTypes.Alias:
          psSqmDataPoint = PSSqmDataPoint.Alias;
          break;
        case CommandTypes.Function:
          psSqmDataPoint = PSSqmDataPoint.Function;
          break;
        case CommandTypes.Alias | CommandTypes.Function:
          return;
        case CommandTypes.Filter:
          psSqmDataPoint = PSSqmDataPoint.Filter;
          break;
        case CommandTypes.Cmdlet:
          psSqmDataPoint = PSSqmDataPoint.Cmdlet;
          break;
        case CommandTypes.ExternalScript:
          psSqmDataPoint = PSSqmDataPoint.ExternalScript;
          break;
        case CommandTypes.Application:
          psSqmDataPoint = PSSqmDataPoint.Application;
          break;
        case CommandTypes.Script:
          psSqmDataPoint = PSSqmDataPoint.Script;
          break;
        default:
          return;
      }
      PSSQMAPI.IncrementDataPoint((uint) psSqmDataPoint);
    }

    public static void IncrementData(CmdletInfo cmdlet)
    {
      if (!PSSQMAPI.isWinSQMEnabled || cmdlet.PSSnapIn == null || !cmdlet.PSSnapIn.IsDefault)
        return;
      PSSQMAPI.IncrementDataPoint(cmdlet.Name);
    }

    public static void IncrementDataPoint(uint dataPoint)
    {
      if (!PSSQMAPI.isWinSQMEnabled)
        return;
      lock (PSSQMAPI.syncObject)
      {
        uint num;
        PSSQMAPI.dataValueCache.TryGetValue(dataPoint, out num);
        ++num;
        PSSQMAPI.dataValueCache[dataPoint] = num;
      }
    }

    public static void LogAllDataSuppressExceptions()
    {
      if (!PSSQMAPI.isWinSQMEnabled)
        return;
      lock (PSSQMAPI.syncObject)
      {
        long ticks = DateTime.Now.Ticks;
        if (ticks - PSSQMAPI.startedAtTick < PSSQMAPI.timeValueThreshold)
          return;
        PSSQMAPI.FlushDataSuppressExceptions();
        PSSQMAPI.startedAtTick = ticks;
      }
    }

    private static void IncrementDataPoint(string cmdletName)
    {
      lock (PSSQMAPI.syncObject)
      {
        uint num1;
        PSSQMAPI.cmdletData.TryGetValue(cmdletName, out num1);
        uint num2 = num1 + 1U;
        PSSQMAPI.cmdletData[cmdletName] = num2;
      }
    }

    private static void FlushDataSuppressExceptions()
    {
      try
      {
        uint num = 3086;
        if (PSSQMAPI.dataValueCache.ContainsKey(num))
        {
          WinSQMWrapper.WinSqmSet(num, PSSQMAPI.dataValueCache[num]);
          PSSQMAPI.dataValueCache.Remove(num);
        }
        WinSQMWrapper.WinSqmIncrement(PSSQMAPI.dataValueCache);
        PSSQMAPI.dataValueCache.Clear();
        foreach (string key in PSSQMAPI.cmdletData.Keys)
          WinSQMWrapper.WinSqmAddToStream(3079U, key, PSSQMAPI.cmdletData[key]);
        PSSQMAPI.cmdletData.Clear();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }

    private static void CurrentDomain_ProcessExit(object source, EventArgs args) => PSSQMAPI.LogAllDataSuppressExceptions();
  }
}
