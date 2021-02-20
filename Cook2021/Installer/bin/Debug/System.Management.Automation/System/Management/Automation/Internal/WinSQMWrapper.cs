// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.WinSQMWrapper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Management.Automation.Internal
{
  internal static class WinSQMWrapper
  {
    private static Guid WINDOWS_SQM_GLOBALSESSION = new Guid("{ 0x95baba28, 0xed26, 0x49c9, { 0xb7, 0x4f, 0x93, 0xb1, 0x70, 0xe1, 0xb8, 0x49 }}");
    private static readonly IntPtr HGLOBALSESSION = IntPtr.Zero;

    public static bool IsWinSqmOptedIn()
    {
      try
      {
        return WinSQMWrapper.WinSqmIsOptedIn();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      return false;
    }

    public static void WinSqmIncrement(uint dataPointID, uint incAmount) => WinSQMWrapper.FireWinSQMEvent(new WinSQMWrapper.EventDescriptor((ushort) 6, (byte) 0, (byte) 0, (byte) 4, (byte) 2, (ushort) 0, 2251799813685248UL), dataPointID, incAmount);

    public static void WinSqmIncrement(Dictionary<uint, uint> dataToWrite) => WinSQMWrapper.FireWinSQMEvent(new WinSQMWrapper.EventDescriptor((ushort) 6, (byte) 0, (byte) 0, (byte) 4, (byte) 2, (ushort) 0, 2251799813685248UL), dataToWrite);

    public static void WinSqmSet(uint dataPointID, uint dataPointValue) => WinSQMWrapper.FireWinSQMEvent(new WinSQMWrapper.EventDescriptor((ushort) 5, (byte) 0, (byte) 0, (byte) 4, (byte) 0, (ushort) 0, 2251799813685248UL), dataPointID, dataPointValue);

    public static void WinSqmAddToStream(uint dataPointID, string stringData)
    {
      WinSQMWrapper.SqmStreamEntry[] streamEntries = new WinSQMWrapper.SqmStreamEntry[1]
      {
        WinSQMWrapper.SqmStreamEntry.CreateStringSqmStreamEntry(stringData)
      };
      WinSQMWrapper.WinSqmAddToStream(WinSQMWrapper.HGLOBALSESSION, dataPointID, streamEntries.Length, streamEntries);
    }

    public static void WinSqmAddToStream(uint dataPointID, string stringData, uint numericalData)
    {
      WinSQMWrapper.SqmStreamEntry[] streamEntries = new WinSQMWrapper.SqmStreamEntry[2]
      {
        WinSQMWrapper.SqmStreamEntry.CreateStringSqmStreamEntry(stringData),
        WinSQMWrapper.SqmStreamEntry.CreateStringSqmStreamEntry(numericalData.ToString((IFormatProvider) CultureInfo.InvariantCulture))
      };
      WinSQMWrapper.WinSqmAddToStream(WinSQMWrapper.HGLOBALSESSION, dataPointID, streamEntries.Length, streamEntries);
    }

    private static void FireWinSQMEvent(
      WinSQMWrapper.EventDescriptor eventDescriptor,
      Dictionary<uint, uint> dataToWrite)
    {
      Guid empty = Guid.Empty;
      if (!WinSQMWrapper.WinSqmEventEnabled(ref eventDescriptor, ref empty))
        return;
      IntPtr num1 = Marshal.AllocHGlobal(Marshal.SizeOf((object) WinSQMWrapper.WINDOWS_SQM_GLOBALSESSION));
      IntPtr num2 = Marshal.AllocHGlobal(4);
      IntPtr num3 = Marshal.AllocHGlobal(4);
      try
      {
        Marshal.StructureToPtr((object) WinSQMWrapper.WINDOWS_SQM_GLOBALSESSION, num1, true);
        foreach (uint key in dataToWrite.Keys)
        {
          uint num4 = dataToWrite[key];
          Marshal.StructureToPtr((object) key, num2, true);
          Marshal.StructureToPtr((object) num4, num3, true);
          WinSQMWrapper.FireWinSQMEvent(eventDescriptor, num1, num2, num3);
        }
      }
      finally
      {
        Marshal.FreeHGlobal(num1);
        Marshal.FreeHGlobal(num2);
        Marshal.FreeHGlobal(num3);
      }
    }

    private static void FireWinSQMEvent(
      WinSQMWrapper.EventDescriptor eventDescriptor,
      uint dataPointID,
      uint dataPointValue)
    {
      Guid empty = Guid.Empty;
      if (!WinSQMWrapper.WinSqmEventEnabled(ref eventDescriptor, ref empty))
        return;
      IntPtr num1 = Marshal.AllocHGlobal(Marshal.SizeOf((object) WinSQMWrapper.WINDOWS_SQM_GLOBALSESSION));
      IntPtr num2 = Marshal.AllocHGlobal(4);
      IntPtr num3 = Marshal.AllocHGlobal(4);
      try
      {
        Marshal.StructureToPtr((object) WinSQMWrapper.WINDOWS_SQM_GLOBALSESSION, num1, true);
        Marshal.StructureToPtr((object) dataPointID, num2, true);
        Marshal.StructureToPtr((object) dataPointValue, num3, true);
        WinSQMWrapper.FireWinSQMEvent(eventDescriptor, num1, num2, num3);
      }
      finally
      {
        Marshal.FreeHGlobal(num1);
        Marshal.FreeHGlobal(num2);
        Marshal.FreeHGlobal(num3);
      }
    }

    private static void FireWinSQMEvent(
      WinSQMWrapper.EventDescriptor eventDescriptor,
      IntPtr sessionHandle,
      IntPtr dataPointIDHandle,
      IntPtr dataValueHandle)
    {
      WinSQMWrapper.EventDataDescriptor[] userData = new WinSQMWrapper.EventDataDescriptor[3]
      {
        new WinSQMWrapper.EventDataDescriptor(sessionHandle, Marshal.SizeOf((object) WinSQMWrapper.WINDOWS_SQM_GLOBALSESSION)),
        new WinSQMWrapper.EventDataDescriptor(dataPointIDHandle, 4),
        new WinSQMWrapper.EventDataDescriptor(dataValueHandle, 4)
      };
      int num = (int) WinSQMWrapper.WinSqmEventWrite(ref eventDescriptor, userData.Length, userData);
    }

    [DllImport("ntdll.dll")]
    private static extern bool WinSqmIsOptedIn();

    [DllImport("ntdll.dll")]
    private static extern bool WinSqmEventEnabled(
      ref WinSQMWrapper.EventDescriptor eventDescriptor,
      ref Guid guid);

    [DllImport("ntdll.dll")]
    private static extern uint WinSqmEventWrite(
      ref WinSQMWrapper.EventDescriptor eventDescriptor,
      int userDataCount,
      WinSQMWrapper.EventDataDescriptor[] userData);

    [DllImport("ntdll.dll")]
    private static extern void WinSqmAddToStream(
      IntPtr sessionGuid,
      uint dataPointID,
      int sqmStreamEntries,
      WinSQMWrapper.SqmStreamEntry[] streamEntries);

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    private struct EventDescriptor
    {
      [FieldOffset(0)]
      private ushort id;
      [FieldOffset(2)]
      private byte version;
      [FieldOffset(3)]
      private byte channel;
      [FieldOffset(4)]
      private byte level;
      [FieldOffset(5)]
      private byte opcode;
      [FieldOffset(6)]
      private ushort task;
      [FieldOffset(8)]
      private ulong keywords;

      internal EventDescriptor(
        ushort eventId,
        byte eventVersion,
        byte eventChannel,
        byte eventLevel,
        byte eventOpcode,
        ushort eventTask,
        ulong eventKeywords)
      {
        this.id = eventId;
        this.version = eventVersion;
        this.channel = eventChannel;
        this.level = eventLevel;
        this.opcode = eventOpcode;
        this.task = eventTask;
        this.keywords = eventKeywords;
      }
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    private struct EventDataDescriptor
    {
      [FieldOffset(0)]
      private IntPtr dataPointer;
      [FieldOffset(8)]
      private int size;
      [FieldOffset(12)]
      private int reserved;

      internal EventDataDescriptor(IntPtr dp, int sz)
      {
        this.reserved = 0;
        this.size = sz;
        this.dataPointer = dp;
      }
    }

    private struct SqmStreamEntry
    {
      private uint type;
      [MarshalAs(UnmanagedType.LPWStr)]
      private string stringValue;

      internal static WinSQMWrapper.SqmStreamEntry CreateStringSqmStreamEntry(
        string value)
      {
        return new WinSQMWrapper.SqmStreamEntry()
        {
          type = 2,
          stringValue = value
        };
      }
    }
  }
}
