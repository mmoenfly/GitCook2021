// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.SerializedDataStream
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation.Internal;
using System.Threading;

namespace System.Management.Automation.Remoting
{
  internal class SerializedDataStream : Stream, IDisposable
  {
    [TraceSource("SerializedDataStream", "SerializedDataStream")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (SerializedDataStream), nameof (SerializedDataStream));
    private static long _objectIdSequenceNumber = 0;
    private bool isEntered;
    private FragmentedRemoteObject currentFragment;
    private long fragmentId;
    private int fragmentSize;
    private object syncObject;
    private bool isDisposed;
    private bool notifyOnWriteFragmentImmediately;
    private Queue<MemoryStream> queuedStreams;
    private MemoryStream writeStream;
    private MemoryStream readStream;
    private int writeOffset;
    private int readOffSet;
    private long length;
    private SerializedDataStream.OnDataAvailableCallback onDataAvailableCallback;
    private bool disposed;

    internal SerializedDataStream(int fragmentSize)
    {
      SerializedDataStream._trace.WriteLine("Creating SerializedDataStream with fragmentsize : {0}", (object) fragmentSize);
      this.syncObject = new object();
      this.currentFragment = new FragmentedRemoteObject();
      this.queuedStreams = new Queue<MemoryStream>();
      this.fragmentSize = fragmentSize;
    }

    internal SerializedDataStream(
      int fragmentSize,
      SerializedDataStream.OnDataAvailableCallback callbackToNotify)
      : this(fragmentSize)
    {
      if (callbackToNotify == null)
        return;
      this.notifyOnWriteFragmentImmediately = true;
      this.onDataAvailableCallback = callbackToNotify;
    }

    internal void Enter()
    {
      this.isEntered = true;
      this.fragmentId = 0L;
      this.currentFragment.ObjectId = SerializedDataStream.GetObjectId();
      this.currentFragment.FragmentId = this.fragmentId;
      this.currentFragment.IsStartFragment = true;
      this.currentFragment.BlobLength = 0;
      this.currentFragment.Blob = new byte[this.fragmentSize];
    }

    internal void Exit()
    {
      this.isEntered = false;
      if (this.currentFragment.BlobLength <= 0)
        return;
      this.currentFragment.IsEndFragment = true;
      this.WriteCurrentFragmentAndReset();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      int sourceIndex = offset;
      int num1 = count;
      while (num1 > 0)
      {
        int num2 = this.fragmentSize - 21 - this.currentFragment.BlobLength;
        if (num2 > 0)
        {
          int length = num1 > num2 ? num2 : num1;
          num1 -= length;
          Array.Copy((Array) buffer, sourceIndex, (Array) this.currentFragment.Blob, this.currentFragment.BlobLength, length);
          this.currentFragment.BlobLength += length;
          sourceIndex += length;
          if (num1 > 0)
            this.WriteCurrentFragmentAndReset();
        }
        else
          this.WriteCurrentFragmentAndReset();
      }
    }

    public override void WriteByte(byte value) => this.Write(new byte[1]
    {
      value
    }, 0, 1);

    internal byte[] ReadOrRegisterCallback(
      SerializedDataStream.OnDataAvailableCallback callback)
    {
      lock (this.syncObject)
      {
        if (this.length <= 0L)
        {
          this.onDataAvailableCallback = callback;
          return (byte[]) null;
        }
        int count = this.length > (long) this.fragmentSize ? this.fragmentSize : (int) this.length;
        byte[] buffer = new byte[count];
        this.Read(buffer, 0, count);
        return buffer;
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int offset1 = offset;
      int num1 = 0;
      Collection<MemoryStream> collection = new Collection<MemoryStream>();
      MemoryStream memoryStream1 = (MemoryStream) null;
      lock (this.syncObject)
      {
        if (this.isDisposed)
          return 0;
        while (num1 < count)
        {
          if (this.readStream == null)
          {
            if (this.queuedStreams.Count > 0)
            {
              this.readStream = this.queuedStreams.Dequeue();
              if (!this.readStream.CanRead || memoryStream1 == this.readStream)
              {
                this.readStream = (MemoryStream) null;
                continue;
              }
            }
            else
              this.readStream = this.writeStream;
            this.readOffSet = 0;
          }
          this.readStream.Position = (long) this.readOffSet;
          int num2 = this.readStream.Read(buffer, offset1, count - num1);
          SerializedDataStream._trace.WriteLine("Read {0} data from readstream: {1}", (object) num2, (object) this.readStream.GetHashCode());
          num1 += num2;
          offset1 += num2;
          this.readOffSet += num2;
          this.length -= (long) num2;
          if (this.readStream.Capacity == this.readOffSet && this.readStream != this.writeStream)
          {
            SerializedDataStream._trace.WriteLine("Adding readstream {0} to dispose collection.", (object) this.readStream.GetHashCode());
            collection.Add(this.readStream);
            memoryStream1 = this.readStream;
            this.readStream = (MemoryStream) null;
          }
        }
      }
      foreach (MemoryStream memoryStream2 in collection)
      {
        SerializedDataStream._trace.WriteLine("Disposing stream: {0}", (object) memoryStream2.GetHashCode());
        memoryStream2.Dispose();
      }
      return num1;
    }

    private void WriteCurrentFragmentAndReset()
    {
      using (IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Transport))
      {
        etwTracer.AnalyticChannel.WriteVerbose(PSEventId.SentRemotingFragment, PSOpcode.Send, PSTask.None, (object) this.currentFragment.ObjectId, (object) this.currentFragment.FragmentId, (object) (this.currentFragment.IsStartFragment ? 1 : 0), (object) (this.currentFragment.IsEndFragment ? 1 : 0), (object) (uint) this.currentFragment.BlobLength, (object) new PSETWBinaryBlob(this.currentFragment.Blob, 0, this.currentFragment.BlobLength));
        byte[] bytes = this.currentFragment.GetBytes();
        int length = bytes.Length;
        int offset = 0;
        if (!this.notifyOnWriteFragmentImmediately)
        {
          lock (this.syncObject)
          {
            if (this.isDisposed)
              return;
            if (this.writeStream == null)
            {
              this.writeStream = new MemoryStream(this.fragmentSize);
              SerializedDataStream._trace.WriteLine("Created write stream: {0}", (object) this.writeStream.GetHashCode());
              this.writeOffset = 0;
            }
            while (length > 0)
            {
              int num = this.writeStream.Capacity - this.writeOffset;
              if (num == 0)
              {
                this.EnqueueWriteStream();
                num = this.writeStream.Capacity - this.writeOffset;
              }
              int count = length > num ? num : length;
              length -= count;
              this.writeStream.Position = (long) this.writeOffset;
              this.writeStream.Write(bytes, offset, count);
              offset += count;
              this.writeOffset += count;
              this.length += (long) count;
            }
          }
        }
        if (this.onDataAvailableCallback != null)
          this.onDataAvailableCallback(bytes, this.currentFragment.IsEndFragment);
        this.currentFragment.FragmentId = ++this.fragmentId;
        this.currentFragment.IsStartFragment = false;
        this.currentFragment.IsEndFragment = false;
        this.currentFragment.BlobLength = 0;
        this.currentFragment.Blob = new byte[this.fragmentSize];
      }
    }

    private void EnqueueWriteStream()
    {
      SerializedDataStream._trace.WriteLine("Queuing write stream: {0} Length: {1} Capacity: {2}", (object) this.writeStream.GetHashCode(), (object) this.writeStream.Length, (object) this.writeStream.Capacity);
      this.queuedStreams.Enqueue(this.writeStream);
      this.writeStream = new MemoryStream(this.fragmentSize);
      this.writeOffset = 0;
      SerializedDataStream._trace.WriteLine("Created write stream: {0}", (object) this.writeStream.GetHashCode());
    }

    private static long GetObjectId() => Interlocked.Increment(ref SerializedDataStream._objectIdSequenceNumber);

    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      lock (this.syncObject)
      {
        foreach (MemoryStream queuedStream in this.queuedStreams)
        {
          if (queuedStream.CanRead)
            queuedStream.Dispose();
        }
        if (this.readStream != null && this.readStream.CanRead)
          this.readStream.Dispose();
        if (this.writeStream != null && this.writeStream.CanRead)
          this.writeStream.Dispose();
        this.isDisposed = true;
      }
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => this.length;

    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public new void Dispose()
    {
      if (!this.disposed)
      {
        GC.SuppressFinalize((object) this);
        this.disposed = true;
      }
      base.Dispose();
    }

    internal delegate void OnDataAvailableCallback(byte[] data, bool isEndFragment);
  }
}
