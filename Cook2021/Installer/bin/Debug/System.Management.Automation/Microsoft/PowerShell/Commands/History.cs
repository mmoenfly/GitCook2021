// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.History
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
  internal class History
  {
    internal const string HistorySizeVar = "MaximumHistoryCount";
    internal const int DefaultHistorySize = 64;
    private HistoryInfo[] _buffer;
    private int _capacity;
    private int _countEntriesInBuffer;
    private long _countEntriesAdded;
    private object _syncRoot = new object();
    private System.Management.Automation.ExecutionContext _context;
    [TraceSource("History", "History class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (History), "History class");

    internal History(System.Management.Automation.ExecutionContext context)
    {
      using (History._trace.TraceConstructor((object) this))
      {
        this._context = context;
        context.EngineSessionState.SetVariable(new PSVariable("MaximumHistoryCount", (object) 64, ScopedItemOptions.None, new Collection<Attribute>()
        {
          (Attribute) new ValidateRangeAttribute((object) 1, (object) (int) short.MaxValue)
        })
        {
          Description = ResourceManagerCache.GetResourceString("SessionStateStrings", "MaxHistoryCountDescription")
        }, false, CommandOrigin.Internal);
        this._capacity = 64;
        this._buffer = new HistoryInfo[this._capacity];
      }
    }

    internal long AddEntry(
      long pipelineId,
      string cmdline,
      PipelineState status,
      DateTime startTime,
      DateTime endTime,
      bool skipIfLocked)
    {
      using (History._trace.TraceMethod())
      {
        if (!Monitor.TryEnter(this._syncRoot, skipIfLocked ? 0 : -1))
          return -1;
        try
        {
          this.ReallocateBufferIfNeeded();
          HistoryInfo entry = new HistoryInfo(pipelineId, cmdline, status, startTime, endTime);
          long id = this.Add(entry);
          entry.SetId(id);
          return id;
        }
        finally
        {
          Monitor.Exit(this._syncRoot);
        }
      }
    }

    internal void UpdateEntry(long id, PipelineState status, DateTime endTime, bool skipIfLocked)
    {
      using (History._trace.TraceMethod())
      {
        if (!Monitor.TryEnter(this._syncRoot, skipIfLocked ? 0 : -1))
          return;
        try
        {
          HistoryInfo entry = this.CoreGetEntry(id);
          if (entry == null)
            return;
          entry.SetStatus(status);
          entry.SetEndTime(endTime);
        }
        finally
        {
          Monitor.Exit(this._syncRoot);
        }
      }
    }

    internal HistoryInfo GetEntry(long id)
    {
      using (History._trace.TraceMethod())
      {
        lock (this._syncRoot)
        {
          this.ReallocateBufferIfNeeded();
          HistoryInfo entry = this.CoreGetEntry(id);
          return entry != null && !entry.Cleared ? entry.Clone() : (HistoryInfo) null;
        }
      }
    }

    internal HistoryInfo[] GetEntries(long id, long count, SwitchParameter newest)
    {
      using (History._trace.TraceMethod())
      {
        this.ReallocateBufferIfNeeded();
        if (count < -1L)
          throw History._trace.NewArgumentOutOfRangeException(nameof (count), (object) count);
        if (newest.ToString() == null)
          throw History._trace.NewArgumentNullException(nameof (newest));
        if (count == -1L || count > this._countEntriesAdded || count > (long) this._countEntriesInBuffer)
          count = (long) this._countEntriesInBuffer;
        if (count == 0L || this._countEntriesInBuffer == 0)
          return new HistoryInfo[0];
        lock (this._syncRoot)
        {
          ArrayList arrayList = new ArrayList();
          if (id > 0L)
          {
            long num1 = id;
            if (!newest.IsPresent)
            {
              long num2 = num1 - count + 1L;
              if (num2 < 1L)
                num2 = 1L;
              for (long id1 = num1; id1 >= num2 && num2 > 1L; --id1)
              {
                if (this._buffer[this.GetIndexFromId(id1)] != null && this._buffer[this.GetIndexFromId(id1)].Cleared)
                  --num2;
              }
              for (long id1 = num2; id1 <= num1; ++id1)
              {
                if (this._buffer[this.GetIndexFromId(id1)] != null && !this._buffer[this.GetIndexFromId(id1)].Cleared)
                  arrayList.Add((object) this._buffer[this.GetIndexFromId(id1)].Clone());
              }
            }
            else
            {
              long num2 = num1 + count - 1L;
              if (num2 >= this._countEntriesAdded)
                num2 = this._countEntriesAdded;
              for (long id1 = num1; id1 <= num2 && num2 < this._countEntriesAdded; ++id1)
              {
                if (this._buffer[this.GetIndexFromId(id1)] != null && this._buffer[this.GetIndexFromId(id1)].Cleared)
                  ++num2;
              }
              for (long id1 = num2; id1 >= num1; --id1)
              {
                if (this._buffer[this.GetIndexFromId(id1)] != null && !this._buffer[this.GetIndexFromId(id1)].Cleared)
                  arrayList.Add((object) this._buffer[this.GetIndexFromId(id1)].Clone());
              }
            }
          }
          else
          {
            long num1 = 0;
            if (this._capacity != 64)
              num1 = this.SmallestIDinBuffer();
            if (!newest.IsPresent)
            {
              long id1 = 1;
              if (this._capacity != 64 && this._countEntriesAdded > (long) this._capacity)
                id1 = num1;
              long num2 = count - 1L;
              while (num2 >= 0L && id1 <= this._countEntriesAdded)
              {
                if (this._buffer[this.GetIndexFromId(id1)].Cleared)
                {
                  ++id1;
                }
                else
                {
                  arrayList.Add((object) this._buffer[this.GetIndexFromId(id1)].Clone());
                  --num2;
                  ++id1;
                }
              }
            }
            else
            {
              long countEntriesAdded = this._countEntriesAdded;
              long num2 = count - 1L;
              while (num2 >= 0L && (this._capacity == 64 || this._countEntriesAdded <= (long) this._capacity || countEntriesAdded >= num1) && countEntriesAdded >= 1L)
              {
                if (this._buffer[this.GetIndexFromId(countEntriesAdded)].Cleared)
                {
                  --countEntriesAdded;
                }
                else
                {
                  arrayList.Add((object) this._buffer[this.GetIndexFromId(countEntriesAdded)].Clone());
                  --num2;
                  --countEntriesAdded;
                }
              }
            }
          }
          HistoryInfo[] historyInfoArray = new HistoryInfo[arrayList.Count];
          arrayList.CopyTo((Array) historyInfoArray);
          return historyInfoArray;
        }
      }
    }

    internal HistoryInfo[] GetEntries(
      WildcardPattern wildcardpattern,
      long count,
      SwitchParameter newest)
    {
      using (History._trace.TraceMethod())
      {
        lock (this._syncRoot)
        {
          if (count < -1L)
            throw History._trace.NewArgumentOutOfRangeException(nameof (count), (object) count);
          if (newest.ToString() == null)
            throw History._trace.NewArgumentNullException(nameof (newest));
          if (count > this._countEntriesAdded || count == -1L)
            count = (long) this._countEntriesInBuffer;
          ArrayList arrayList = new ArrayList();
          long num = 1;
          if (this._capacity != 64)
            num = this.SmallestIDinBuffer();
          if (count != 0L)
          {
            if (!newest.IsPresent)
            {
              long id = 1;
              if (this._capacity != 64 && this._countEntriesAdded > (long) this._capacity)
                id = num;
              for (long index = 0; index <= count - 1L && id <= this._countEntriesAdded; ++id)
              {
                if (!this._buffer[this.GetIndexFromId(id)].Cleared && wildcardpattern.IsMatch(this._buffer[this.GetIndexFromId(id)].CommandLine.Trim()))
                {
                  arrayList.Add((object) this._buffer[this.GetIndexFromId(id)].Clone());
                  ++index;
                }
              }
            }
            else
            {
              long countEntriesAdded = this._countEntriesAdded;
              for (long index = 0; index <= count - 1L && (this._capacity == 64 || this._countEntriesAdded <= (long) this._capacity || countEntriesAdded >= num) && countEntriesAdded >= 1L; --countEntriesAdded)
              {
                if (!this._buffer[this.GetIndexFromId(countEntriesAdded)].Cleared && wildcardpattern.IsMatch(this._buffer[this.GetIndexFromId(countEntriesAdded)].CommandLine.Trim()))
                {
                  arrayList.Add((object) this._buffer[this.GetIndexFromId(countEntriesAdded)].Clone());
                  ++index;
                }
              }
            }
          }
          else
          {
            for (long id = 1; id <= this._countEntriesAdded; ++id)
            {
              if (!this._buffer[this.GetIndexFromId(id)].Cleared && wildcardpattern.IsMatch(this._buffer[this.GetIndexFromId(id)].CommandLine.Trim()))
                arrayList.Add((object) this._buffer[this.GetIndexFromId(id)].Clone());
            }
          }
          HistoryInfo[] historyInfoArray = new HistoryInfo[arrayList.Count];
          arrayList.CopyTo((Array) historyInfoArray);
          return historyInfoArray;
        }
      }
    }

    internal void ClearEntry(long id)
    {
      using (History._trace.TraceMethod())
      {
        lock (this._syncRoot)
        {
          if (id < 0L)
            throw History._trace.NewArgumentOutOfRangeException(nameof (id), (object) id);
          if (this._countEntriesInBuffer == 0 || id > this._countEntriesAdded)
            return;
          HistoryInfo entry = this.CoreGetEntry(id);
          if (entry == null)
            return;
          entry.Cleared = true;
          --this._countEntriesInBuffer;
        }
      }
    }

    internal int Buffercapacity() => this._capacity;

    private long Add(HistoryInfo entry)
    {
      using (History._trace.TraceMethod())
      {
        this._buffer[this.GetIndexForNewEntry()] = entry != null ? entry : throw History._trace.NewArgumentNullException(nameof (entry));
        ++this._countEntriesAdded;
        entry.SetId(this._countEntriesAdded);
        this.IncrementCountOfEntriesInBuffer();
        return this._countEntriesAdded;
      }
    }

    private HistoryInfo CoreGetEntry(long id)
    {
      using (History._trace.TraceMethod())
      {
        if (id <= 0L)
          throw History._trace.NewArgumentOutOfRangeException(nameof (id), (object) id);
        return this._countEntriesInBuffer == 0 || id > this._countEntriesAdded ? (HistoryInfo) null : this._buffer[this.GetIndexFromId(id)];
      }
    }

    private long SmallestIDinBuffer()
    {
      using (History._trace.TraceMethod())
      {
        long num = 0;
        if (this._buffer == null)
          return num;
        for (int index = 0; index < this._buffer.Length; ++index)
        {
          if (this._buffer[index] != null && !this._buffer[index].Cleared)
          {
            num = this._buffer[index].Id;
            break;
          }
        }
        for (int index = 0; index < this._buffer.Length; ++index)
        {
          if (this._buffer[index] != null && !this._buffer[index].Cleared && num > this._buffer[index].Id)
            num = this._buffer[index].Id;
        }
        return num;
      }
    }

    private void ReallocateBufferIfNeeded()
    {
      using (History._trace.TraceMethod())
      {
        int historySize = this.GetHistorySize();
        if (historySize == this._capacity)
          return;
        HistoryInfo[] historyInfoArray = new HistoryInfo[historySize];
        int num = this._countEntriesInBuffer;
        if ((long) num < this._countEntriesAdded)
          num = (int) this._countEntriesAdded;
        if (this._countEntriesInBuffer > historySize)
          num = historySize;
        for (int index = num; index > 0; --index)
        {
          long id = this._countEntriesAdded - (long) index + 1L;
          historyInfoArray[History.GetIndexFromId(id, historySize)] = this._buffer[this.GetIndexFromId(id)];
        }
        this._countEntriesInBuffer = num;
        this._capacity = historySize;
        this._buffer = historyInfoArray;
      }
    }

    private int GetIndexForNewEntry()
    {
      using (History._trace.TraceMethod())
        return (int) (this._countEntriesAdded % (long) this._capacity);
    }

    private int GetIndexFromId(long id)
    {
      using (History._trace.TraceMethod())
        return (int) ((id - 1L) % (long) this._capacity);
    }

    private static int GetIndexFromId(long id, int capacity)
    {
      using (History._trace.TraceMethod())
        return (int) ((id - 1L) % (long) capacity);
    }

    private void IncrementCountOfEntriesInBuffer()
    {
      using (History._trace.TraceMethod())
      {
        if (this._countEntriesInBuffer >= this._capacity)
          return;
        ++this._countEntriesInBuffer;
      }
    }

    private int GetHistorySize()
    {
      using (History._trace.TraceMethod())
      {
        int num = 0;
        object variable = this._context.GetVariable("MaximumHistoryCount");
        if (variable != null)
        {
          try
          {
            num = (int) LanguagePrimitives.ConvertTo(variable, typeof (int), (IFormatProvider) CultureInfo.InvariantCulture);
          }
          catch (InvalidCastException ex)
          {
          }
        }
        if (num <= 0)
          num = 64;
        return num;
      }
    }
  }
}
