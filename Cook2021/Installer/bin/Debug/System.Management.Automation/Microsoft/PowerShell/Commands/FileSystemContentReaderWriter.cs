// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.FileSystemContentReaderWriter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Security;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
  internal class FileSystemContentReaderWriter : IContentReader, IContentWriter, IDisposable
  {
    [TraceSource("FileSystemContentStream", "The provider content reader and writer for the file system")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("FileSystemContentStream", "The provider content reader and writer for the file system");
    private string path;
    private FileMode mode;
    private FileAccess access;
    private FileShare share;
    private Encoding encoding;
    private CmdletProvider provider;
    private FileStream stream;
    private StreamReader reader;
    private StreamWriter writer;
    private bool usingByteEncoding;
    private string delimiter = "\n";
    private bool usingDelimiter;
    private bool waitForChanges;
    private long fileOffset;
    private FileAttributes oldAttributes;
    private bool haveOldAttributes;

    public FileSystemContentReaderWriter(
      string path,
      FileMode mode,
      FileAccess access,
      FileShare share,
      Encoding encoding,
      bool usingByteEncoding,
      bool waitForChanges,
      CmdletProvider provider)
    {
      using (FileSystemContentReaderWriter.tracer.TraceConstructor((object) this))
      {
        if (string.IsNullOrEmpty(path))
          throw FileSystemContentReaderWriter.tracer.NewArgumentNullException(nameof (path));
        FileSystemContentReaderWriter.tracer.WriteLine("path = {0}", (object) path);
        FileSystemContentReaderWriter.tracer.WriteLine("mode = {0}", (object) mode);
        FileSystemContentReaderWriter.tracer.WriteLine("access = {0}", (object) access);
        this.path = path;
        this.mode = mode;
        this.access = access;
        this.share = share;
        this.encoding = encoding;
        this.usingByteEncoding = usingByteEncoding;
        this.waitForChanges = waitForChanges;
        this.provider = provider;
        this.CreateStreams(path, mode, access, share, encoding);
      }
    }

    public FileSystemContentReaderWriter(
      string path,
      FileMode mode,
      FileAccess access,
      FileShare share,
      string delimiter,
      Encoding encoding,
      bool waitForChanges,
      CmdletProvider provider)
      : this(path, mode, access, share, encoding, false, waitForChanges, provider)
    {
      this.delimiter = delimiter;
      this.usingDelimiter = true;
    }

    public IList Read(long readCount)
    {
      bool waitChanges = this.waitForChanges;
      using (FileSystemContentReaderWriter.tracer.TraceMethod())
      {
        FileSystemContentReaderWriter.tracer.WriteLine("blocks requested = {0}", (object) readCount);
        ArrayList blocks = new ArrayList();
        bool flag = readCount <= 0L;
        try
        {
          for (long index = 0; index < readCount || flag; ++index)
          {
            if (waitChanges && this.provider.Stopping)
              waitChanges = false;
            if (this.usingByteEncoding)
            {
              if (!this.ReadByteEncoded(waitChanges, blocks))
                break;
            }
            else if (this.usingDelimiter)
            {
              if (!this.ReadDelimited(waitChanges, blocks))
                break;
            }
            else if (!this.ReadByLine(waitChanges, blocks))
              break;
          }
          FileSystemContentReaderWriter.tracer.WriteLine("blocks read = {0}", (object) blocks.Count);
        }
        catch (Exception ex)
        {
          switch (ex)
          {
            case IOException _:
            case ArgumentException _:
            case SecurityException _:
            case UnauthorizedAccessException _:
            case ArgumentNullException _:
              FileSystemContentReaderWriter.tracer.TraceException(ex);
              this.provider.WriteError(new ErrorRecord(ex, "GetContentReaderIOError", ErrorCategory.ReadError, (object) this.path));
              return (IList) null;
            default:
              throw;
          }
        }
        return (IList) blocks.ToArray();
      }
    }

    private bool ReadByLine(bool waitChanges, ArrayList blocks)
    {
      string str = this.reader.ReadLine();
      if (str == null && waitChanges)
      {
        do
        {
          this.WaitForChanges(this.path, this.mode, this.access, this.share, this.reader.CurrentEncoding);
          str = this.reader.ReadLine();
        }
        while (str == null && !this.provider.Stopping);
      }
      if (str != null)
        blocks.Add((object) str);
      return this.reader.Peek() != -1;
    }

    private bool ReadDelimited(bool waitChanges, ArrayList blocks)
    {
      char[] chArray = new char[this.delimiter.Length];
      int count = this.delimiter.Length;
      StringBuilder stringBuilder = new StringBuilder();
      Dictionary<char, int> dictionary = new Dictionary<char, int>();
      foreach (char key in this.delimiter)
        dictionary[key] = this.delimiter.Length - this.delimiter.LastIndexOf(key) - 1;
      int charCount;
      do
      {
        char[] buffer = new char[count];
        charCount = this.reader.Read(buffer, 0, count);
        if (charCount == 0 && waitChanges)
        {
          for (; charCount < count && !this.provider.Stopping; charCount += this.reader.Read(buffer, 0, count - charCount))
            this.WaitForChanges(this.path, this.mode, this.access, this.share, this.reader.CurrentEncoding);
        }
        if (charCount > 0)
        {
          stringBuilder.Append(buffer, 0, charCount);
          count = !dictionary.ContainsKey(stringBuilder[stringBuilder.Length - 1]) ? this.delimiter.Length : dictionary[stringBuilder[stringBuilder.Length - 1]];
          if (count == 0)
            count = 1;
        }
      }
      while (stringBuilder.ToString().IndexOf(this.delimiter, StringComparison.Ordinal) < 0 && charCount != 0);
      if (stringBuilder.Length > 0)
        blocks.Add((object) stringBuilder.ToString());
      return this.reader.Peek() != -1;
    }

    private bool ReadByteEncoded(bool waitChanges, ArrayList blocks)
    {
      int num = this.stream.ReadByte();
      if (num == -1 && waitChanges)
      {
        this.WaitForChanges(this.path, this.mode, this.access, this.share, this.reader.CurrentEncoding);
        num = this.stream.ReadByte();
      }
      if (num == -1)
        return false;
      blocks.Add((object) (byte) num);
      return true;
    }

    private void CreateStreams(
      string filePath,
      FileMode fileMode,
      FileAccess fileAccess,
      FileShare fileShare,
      Encoding fileEncoding)
    {
      if (File.Exists(filePath) && (bool) this.provider.Force)
      {
        this.oldAttributes = File.GetAttributes(filePath);
        this.haveOldAttributes = true;
        File.SetAttributes(this.path, File.GetAttributes(filePath) & ~(FileAttributes.ReadOnly | FileAttributes.Hidden));
      }
      this.stream = new FileStream(filePath, fileMode, fileAccess, fileShare);
      if (this.usingByteEncoding)
        return;
      if ((fileAccess & FileAccess.Read) != (FileAccess) 0)
      {
        this.reader = new StreamReader((Stream) this.stream, fileEncoding);
      }
      else
      {
        if ((fileAccess & FileAccess.Write) == (FileAccess) 0)
          return;
        this.writer = new StreamWriter((Stream) this.stream, fileEncoding);
      }
    }

    private void WaitForChanges(
      string filePath,
      FileMode fileMode,
      FileAccess fileAccess,
      FileShare fileShare,
      Encoding fileEncoding)
    {
      if (this.stream != null)
      {
        this.fileOffset = this.stream.Position;
        this.stream.Close();
      }
      FileInfo fileInfo = new FileInfo(filePath);
      using (FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(fileInfo.DirectoryName, fileInfo.Name))
      {
        fileSystemWatcher.EnableRaisingEvents = true;
        while (!this.provider.Stopping)
        {
          if (!fileSystemWatcher.WaitForChanged(WatcherChangeTypes.All, 500).TimedOut)
            break;
        }
      }
      Thread.Sleep(100);
      this.CreateStreams(filePath, fileMode, fileAccess, fileShare, fileEncoding);
      if (this.fileOffset > this.stream.Length)
        this.fileOffset = 0L;
      this.stream.Seek(this.fileOffset, SeekOrigin.Begin);
      if (this.reader == null)
        return;
      this.reader.DiscardBufferedData();
    }

    public void Seek(long offset, SeekOrigin origin)
    {
      using (FileSystemContentReaderWriter.tracer.TraceMethod())
      {
        if (this.writer != null)
          this.writer.Flush();
        this.stream.Seek(offset, origin);
        if (this.writer != null)
          this.writer.Flush();
        if (this.reader == null)
          return;
        this.reader.DiscardBufferedData();
      }
    }

    public void Close()
    {
      using (FileSystemContentReaderWriter.tracer.TraceMethod())
      {
        bool flag = false;
        if (this.writer != null)
        {
          try
          {
            this.writer.Flush();
            this.writer.Close();
            if (this.haveOldAttributes)
            {
              if ((bool) this.provider.Force)
                File.SetAttributes(this.path, this.oldAttributes);
            }
          }
          finally
          {
            flag = true;
          }
        }
        if (this.reader != null)
        {
          this.reader.Close();
          flag = true;
        }
        if (flag)
          return;
        this.stream.Flush();
        this.stream.Close();
      }
    }

    public IList Write(IList content)
    {
      using (FileSystemContentReaderWriter.tracer.TraceMethod())
      {
        foreach (object content1 in (IEnumerable) content)
        {
          if (content1 is object[] objArray)
          {
            for (int index = 0; index < objArray.Length; ++index)
              this.WriteObject(objArray[index]);
          }
          else
            this.WriteObject(content1);
        }
        return content;
      }
    }

    private void WriteObject(object content)
    {
      using (FileSystemContentReaderWriter.tracer.TraceMethod())
      {
        if (content == null)
          return;
        if (this.usingByteEncoding)
        {
          try
          {
            this.stream.WriteByte((byte) content);
          }
          catch (InvalidCastException ex)
          {
            throw FileSystemContentReaderWriter.tracer.NewArgumentException(nameof (content), "FileSystemProviderStrings", "ByteEncodingError");
          }
        }
        else
          this.writer.WriteLine(content.ToString());
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    internal void Dispose(bool isDisposing)
    {
      if (!isDisposing)
        return;
      using (FileSystemContentReaderWriter.tracer.TraceMethod())
      {
        if (this.stream != null)
          this.stream.Close();
        if (this.reader != null)
          this.reader.Close();
        if (this.writer == null)
          return;
        this.writer.Close();
      }
    }
  }
}
