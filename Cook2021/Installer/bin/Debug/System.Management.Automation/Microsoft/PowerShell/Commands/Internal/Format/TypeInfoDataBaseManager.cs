// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.TypeInfoDataBaseManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class TypeInfoDataBaseManager
  {
    [TraceSource("TypeInfoDataBaseManager", "TypeInfoDataBaseManager")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (TypeInfoDataBaseManager), nameof (TypeInfoDataBaseManager));
    private TypeInfoDataBase dataBase;
    internal object databaseLock = new object();
    internal object updateDatabaseLock = new object();
    private bool isShared;
    private List<string> formatFileList;

    internal TypeInfoDataBase Database => this.dataBase;

    internal TypeInfoDataBaseManager()
    {
      this.isShared = false;
      this.formatFileList = new List<string>();
    }

    internal TypeInfoDataBaseManager(
      IEnumerable<string> formatFiles,
      bool isShared,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      this.formatFileList = new List<string>();
      Collection<PSSnapInTypeAndFormatErrors> files = new Collection<PSSnapInTypeAndFormatErrors>();
      Collection<string> loadErrors = new Collection<string>();
      foreach (string formatFile in formatFiles)
      {
        if (string.IsNullOrEmpty(formatFile) || !Path.IsPathRooted(formatFile))
          throw TypeInfoDataBaseManager.tracer.NewArgumentException(nameof (formatFiles), "FormatAndOut.XmlLoading", "FormatFileNotRooted", (object) formatFile);
        files.Add(new PSSnapInTypeAndFormatErrors(string.Empty, formatFile)
        {
          Errors = loadErrors
        });
        this.formatFileList.Add(formatFile);
      }
      MshExpressionFactory expressionFactory = new MshExpressionFactory(new CreateScriptBlockFromString(this.CreateScriptBlock));
      List<XmlLoaderLoggerEntry> logEntries = (List<XmlLoaderLoggerEntry>) null;
      this.LoadFromFile(files, expressionFactory, true, authorizationManager, host, out logEntries);
      this.isShared = isShared;
      if (loadErrors.Count > 0)
        throw new FormatTableLoadException(loadErrors);
    }

    internal TypeInfoDataBase GetTypeInfoDataBase() => this.dataBase;

    private ScriptBlock CreateScriptBlock(string scriptText) => ScriptBlock.Create(scriptText);

    internal void Add(string formatFile, bool shouldPrepend)
    {
      if (string.IsNullOrEmpty(formatFile) || !Path.IsPathRooted(formatFile))
        throw TypeInfoDataBaseManager.tracer.NewArgumentException(nameof (formatFile), "FormatAndOut.XmlLoading", "FormatFileNotRooted", (object) formatFile);
      lock (this.formatFileList)
      {
        if (shouldPrepend)
          this.formatFileList.Insert(0, formatFile);
        else
          this.formatFileList.Add(formatFile);
      }
    }

    internal void Remove(string formatFile)
    {
      lock (this.formatFileList)
        this.formatFileList.Remove(formatFile);
    }

    internal void Update(AuthorizationManager authorizationManager, PSHost host)
    {
      if (this.isShared)
        throw TypeInfoDataBaseManager.tracer.NewInvalidOperationException("FormatAndOut.XmlLoading", "SharedFormattableCannotBeUpdated");
      Collection<PSSnapInTypeAndFormatErrors> mshsnapins = new Collection<PSSnapInTypeAndFormatErrors>();
      lock (this.formatFileList)
      {
        foreach (string formatFile in this.formatFileList)
        {
          PSSnapInTypeAndFormatErrors typeAndFormatErrors = new PSSnapInTypeAndFormatErrors(string.Empty, formatFile);
          mshsnapins.Add(typeAndFormatErrors);
        }
      }
      this.UpdateDataBase(mshsnapins, authorizationManager, host);
    }

    internal void UpdateDataBase(
      Collection<PSSnapInTypeAndFormatErrors> mshsnapins,
      AuthorizationManager authorizationManager,
      PSHost host)
    {
      if (this.isShared)
        throw TypeInfoDataBaseManager.tracer.NewInvalidOperationException("FormatAndOut.XmlLoading", "SharedFormattableCannotBeUpdated");
      MshExpressionFactory expressionFactory = new MshExpressionFactory(new CreateScriptBlockFromString(this.CreateScriptBlock));
      List<XmlLoaderLoggerEntry> logEntries = (List<XmlLoaderLoggerEntry>) null;
      this.LoadFromFile(mshsnapins, expressionFactory, false, authorizationManager, host, out logEntries);
    }

    internal bool LoadFromFile(
      Collection<PSSnapInTypeAndFormatErrors> files,
      MshExpressionFactory expressionFactory,
      bool acceptLoadingErrors,
      AuthorizationManager authorizationManager,
      PSHost host,
      out List<XmlLoaderLoggerEntry> logEntries)
    {
      if (this.isShared)
        throw TypeInfoDataBaseManager.tracer.NewInvalidOperationException("FormatAndOut.XmlLoading", "SharedFormattableCannotBeUpdated");
      bool success;
      try
      {
        TypeInfoDataBase typeInfoDataBase = (TypeInfoDataBase) null;
        lock (this.updateDatabaseLock)
          typeInfoDataBase = TypeInfoDataBaseManager.LoadFromFileHelper(files, expressionFactory, authorizationManager, host, out logEntries, out success);
        lock (this.databaseLock)
        {
          if (!acceptLoadingErrors)
          {
            if (!success)
              goto label_15;
          }
          this.dataBase = typeInfoDataBase;
        }
      }
      finally
      {
        lock (this.databaseLock)
        {
          if (this.dataBase == null)
          {
            TypeInfoDataBase db = new TypeInfoDataBase();
            TypeInfoDataBaseManager.AddPreLoadInstrinsics(db);
            TypeInfoDataBaseManager.AddPostLoadInstrinsics(db);
            this.dataBase = db;
          }
        }
      }
label_15:
      return success;
    }

    private static TypeInfoDataBase LoadFromFileHelper(
      Collection<PSSnapInTypeAndFormatErrors> files,
      MshExpressionFactory expressionFactory,
      AuthorizationManager authorizationManager,
      PSHost host,
      out List<XmlLoaderLoggerEntry> logEntries,
      out bool success)
    {
      success = true;
      logEntries = new List<XmlLoaderLoggerEntry>();
      List<XmlFileLoadInfo> xmlFileLoadInfoList = new List<XmlFileLoadInfo>();
      foreach (PSSnapInTypeAndFormatErrors file in files)
        xmlFileLoadInfoList.Add(new XmlFileLoadInfo(Path.GetPathRoot(file.FullPath), file.FullPath, file.Errors, file.PSSnapinName));
      TypeInfoDataBase db = new TypeInfoDataBase();
      TypeInfoDataBaseManager.AddPreLoadInstrinsics(db);
      foreach (XmlFileLoadInfo info in xmlFileLoadInfoList)
      {
        using (TypeInfoDataBaseLoader infoDataBaseLoader = new TypeInfoDataBaseLoader())
        {
          if (!infoDataBaseLoader.LoadXmlFile(info, db, expressionFactory, authorizationManager, host))
            success = false;
          foreach (XmlLoaderLoggerEntry logEntry in infoDataBaseLoader.LogEntries)
          {
            if (logEntry.entryType == XmlLoaderLoggerEntry.EntryType.Error)
            {
              string str = XmlLoadingResourceManager.FormatString("MshSnapinQualifiedError", (object) info.psSnapinName, (object) logEntry.message);
              info.errors.Add(str);
            }
          }
          logEntries.AddRange((IEnumerable<XmlLoaderLoggerEntry>) infoDataBaseLoader.LogEntries);
        }
      }
      TypeInfoDataBaseManager.AddPostLoadInstrinsics(db);
      return db;
    }

    private static void AddPreLoadInstrinsics(TypeInfoDataBase db)
    {
      using (TypeInfoDataBaseManager.tracer.TraceMethod((object) db))
        ;
    }

    private static void AddPostLoadInstrinsics(TypeInfoDataBase db)
    {
      FormatShapeSelectionOnType shapeSelectionOnType = new FormatShapeSelectionOnType();
      shapeSelectionOnType.appliesTo = new AppliesTo();
      shapeSelectionOnType.appliesTo.AddAppliesToType("Microsoft.PowerShell.Commands.FormatDataLoadingInfo");
      shapeSelectionOnType.formatShape = FormatShape.List;
      db.defaultSettingsSection.shapeSelectionDirectives.formatShapeSelectionOnTypeList.Add(shapeSelectionOnType);
    }
  }
}
