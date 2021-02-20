// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.NavigationCmdletProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation.Provider
{
  public abstract class NavigationCmdletProvider : ContainerCmdletProvider
  {
    internal string MakePath(string parent, string child, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(parent, new object[0]))
      {
        this.Context = context;
        return this.MakePath(parent, child);
      }
    }

    internal string GetParentPath(string path, string root, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.GetParentPath(path, root);
      }
    }

    internal string NormalizeRelativePath(
      string path,
      string basePath,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.NormalizeRelativePath(path, basePath);
      }
    }

    internal string GetChildName(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.GetChildName(path);
      }
    }

    internal bool IsItemContainer(string path, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.IsItemContainer(path);
      }
    }

    internal void MoveItem(string path, string destination, CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        this.MoveItem(path, destination);
      }
    }

    internal object MoveItemDynamicParameters(
      string path,
      string destination,
      CmdletProviderContext context)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        this.Context = context;
        return this.MoveItemDynamicParameters(path, destination);
      }
    }

    protected virtual string MakePath(string parent, string child)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (parent == null && child == null)
          throw CmdletProvider.providerBaseTracer.NewArgumentException(nameof (parent));
        string str;
        if (string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
          str = string.Empty;
        else if (string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(child))
          str = child.Replace('/', '\\');
        else if (!string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(child))
        {
          str = !parent.EndsWith('\\'.ToString(), StringComparison.Ordinal) ? parent + (object) '\\' : parent;
        }
        else
        {
          parent = parent.Replace('/', '\\');
          child = child.Replace('/', '\\');
          StringBuilder stringBuilder = new StringBuilder();
          if (parent.EndsWith('\\'.ToString(), StringComparison.Ordinal))
          {
            if (child.StartsWith('\\'.ToString(), StringComparison.Ordinal))
            {
              stringBuilder.Append(parent);
              stringBuilder.Append(child, 1, child.Length - 1);
            }
            else
            {
              stringBuilder.Append(parent);
              stringBuilder.Append(child);
            }
          }
          else if (child.StartsWith('\\'.ToString(), StringComparison.CurrentCulture))
          {
            stringBuilder.Append(parent);
            if (parent.Length == 0)
              stringBuilder.Append(child, 1, child.Length - 1);
            else
              stringBuilder.Append(child);
          }
          else
          {
            stringBuilder.Append(parent);
            if (parent.Length > 0 && child.Length > 0)
              stringBuilder.Append('\\');
            stringBuilder.Append(child);
          }
          str = stringBuilder.ToString();
        }
        CmdletProvider.providerBaseTracer.WriteLine("result={0}", (object) str);
        return str;
      }
    }

    protected virtual string GetParentPath(string path, string root)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        if (string.IsNullOrEmpty(path))
          throw CmdletProvider.providerBaseTracer.NewArgumentException(nameof (path));
        if (root == null && this.PSDriveInfo != (PSDriveInfo) null)
          root = this.PSDriveInfo.Root;
        path = path.Replace('/', '\\');
        path = path.TrimEnd('\\');
        string strB = string.Empty;
        if (root != null)
          strB = root.Replace('/', '\\');
        string str;
        if (string.Compare(path, strB, StringComparison.OrdinalIgnoreCase) == 0)
        {
          str = string.Empty;
        }
        else
        {
          int length = path.LastIndexOf('\\');
          switch (length)
          {
            case -1:
              str = string.Empty;
              goto label_13;
            case 0:
              ++length;
              break;
          }
          str = path.Substring(0, length);
        }
label_13:
        return str;
      }
    }

    protected virtual string NormalizeRelativePath(string path, string basePath)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return this.ContractRelativePath(path, basePath, false, this.Context);
    }

    internal string ContractRelativePath(
      string path,
      string basePath,
      bool allowNonExistingPaths,
      CmdletProviderContext context)
    {
      this.Context = context;
      switch (path)
      {
        case "":
          return string.Empty;
        case null:
          throw CmdletProvider.providerBaseTracer.NewArgumentNullException(nameof (path));
        default:
          if (basePath == null)
            basePath = string.Empty;
          CmdletProvider.providerBaseTracer.WriteLine("basePath = {0}", (object) basePath);
          bool flag = false;
          string str1 = path;
          string str2 = basePath;
          if (!string.Equals(context.ProviderInstance.ProviderInfo.FullName, "ActiveDirectory", StringComparison.OrdinalIgnoreCase))
          {
            str1 = path.Replace('/', '\\');
            str2 = basePath.Replace('/', '\\');
          }
          string str3 = path;
          if (path.EndsWith('\\'.ToString(), StringComparison.OrdinalIgnoreCase))
          {
            path = path.TrimEnd('\\');
            flag = true;
          }
          basePath = basePath.TrimEnd('\\');
          string str4;
          if (string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase) && !str3.EndsWith(string.Concat((object) '\\'), StringComparison.OrdinalIgnoreCase))
            str4 = this.MakePath("..", this.GetChildName(path));
          else if (!str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase) && basePath.Length > 0)
          {
            str4 = string.Empty;
            string commonBase = this.GetCommonBase(str1, str2);
            int count = this.TokenizePathToStack(str2, commonBase).Count;
            if (string.IsNullOrEmpty(commonBase))
              --count;
            for (int index = 0; index < count; ++index)
              str4 = this.MakePath("..", str4);
            if (!string.IsNullOrEmpty(commonBase))
            {
              if (string.Equals(str1, commonBase, StringComparison.OrdinalIgnoreCase) && !str1.EndsWith(string.Concat((object) '\\'), StringComparison.OrdinalIgnoreCase))
              {
                string childName = this.GetChildName(path);
                str4 = this.MakePath(this.MakePath("..", str4), childName);
              }
              else
              {
                foreach (string child in this.TokenizePathToStack(str1, commonBase).ToArray())
                  str4 = this.MakePath(str4, child);
              }
            }
          }
          else
          {
            Stack<string> stack = this.TokenizePathToStack(path, basePath);
            Stack<string> stringStack = new Stack<string>();
            Stack<string> normalizedPathStack;
            try
            {
              normalizedPathStack = NavigationCmdletProvider.NormalizeThePath(stack, path, basePath, allowNonExistingPaths);
            }
            catch (ArgumentException ex)
            {
              this.WriteError(new ErrorRecord((Exception) ex, ex.GetType().FullName, ErrorCategory.InvalidArgument, (object) null));
              str4 = (string) null;
              goto label_27;
            }
            str4 = this.CreateNormalizedRelativePathFromStack(normalizedPathStack);
          }
label_27:
          if (flag)
            str4 += (string) (object) '\\';
          CmdletProvider.providerBaseTracer.WriteLine("result = {0}", (object) str4);
          return str4;
      }
    }

    private string GetCommonBase(string path1, string path2)
    {
      while (!string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase))
      {
        if (path2.Length > path1.Length)
          path2 = this.GetParentPath(path2, (string) null);
        else
          path1 = this.GetParentPath(path1, (string) null);
      }
      return path1;
    }

    protected virtual string GetChildName(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
      {
        path = !string.IsNullOrEmpty(path) ? path.Replace('/', '\\') : throw CmdletProvider.providerBaseTracer.NewArgumentException(nameof (path));
        path = path.TrimEnd('\\');
        int num1 = path.LastIndexOf('\\');
        string str;
        if (num1 == -1)
          str = path;
        else if (this.ItemExists(path, this.Context))
        {
          string parentPath = this.GetParentPath(path, (string) null);
          if (string.IsNullOrEmpty(parentPath))
            str = path;
          else if (parentPath.IndexOf('\\') == parentPath.Length - 1)
          {
            int startIndex = path.IndexOf(parentPath, StringComparison.OrdinalIgnoreCase) + parentPath.Length;
            str = path.Substring(startIndex);
          }
          else
          {
            int num2 = path.IndexOf(parentPath, StringComparison.OrdinalIgnoreCase) + parentPath.Length;
            str = path.Substring(num2 + 1);
          }
        }
        else
          str = path.Substring(num1 + 1);
        CmdletProvider.providerBaseTracer.WriteLine("Result = {0}", (object) str);
        return str;
      }
    }

    protected virtual bool IsItemContainer(string path)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual void MoveItem(string path, string destination)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        throw CmdletProvider.providerBaseTracer.NewNotSupportedException("SessionStateStrings", "CmdletProvider_NotSupported");
    }

    protected virtual object MoveItemDynamicParameters(string path, string destination)
    {
      using (PSTransactionManager.GetEngineProtectionScope())
        return (object) null;
    }

    private Stack<string> TokenizePathToStack(string path, string basePath)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod(path, new object[0]))
      {
        Stack<string> stringStack = new Stack<string>();
        string path1 = path;
        string str = path;
        while (path1.Length > basePath.Length)
        {
          string childName = this.GetChildName(path1);
          if (string.IsNullOrEmpty(childName))
          {
            stringStack.Push(path1);
            break;
          }
          CmdletProvider.providerBaseTracer.WriteLine("tokenizedPathStack.Push({0})", (object) childName);
          stringStack.Push(childName);
          path1 = this.GetParentPath(path1, basePath);
          if (path1.Length < str.Length)
            str = path1;
          else
            break;
        }
        return stringStack;
      }
    }

    private static Stack<string> NormalizeThePath(
      Stack<string> tokenizedPathStack,
      string path,
      string basePath,
      bool allowNonExistingPaths)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod())
      {
        Stack<string> stringStack = new Stack<string>();
        while (tokenizedPathStack.Count > 0)
        {
          string str1 = tokenizedPathStack.Pop();
          CmdletProvider.providerBaseTracer.WriteLine("childName = {0}", (object) str1);
          if (!str1.Equals(".", StringComparison.OrdinalIgnoreCase))
          {
            if (str1.Equals("..", StringComparison.OrdinalIgnoreCase))
            {
              if (stringStack.Count > 0)
              {
                string str2 = stringStack.Pop();
                CmdletProvider.providerBaseTracer.WriteLine("normalizedPathStack.Pop() : {0}", (object) str2);
                continue;
              }
              if (!allowNonExistingPaths)
                throw CmdletProvider.providerBaseTracer.NewArgumentException(nameof (path), "SessionStateStrings", "NormalizeRelativePathOutsideBase", (object) path, (object) basePath);
            }
            CmdletProvider.providerBaseTracer.WriteLine("normalizedPathStack.Push({0})", (object) str1);
            stringStack.Push(str1);
          }
        }
        return stringStack;
      }
    }

    private string CreateNormalizedRelativePathFromStack(Stack<string> normalizedPathStack)
    {
      using (CmdletProvider.providerBaseTracer.TraceMethod())
      {
        string child = string.Empty;
        while (normalizedPathStack.Count > 0)
          child = !string.IsNullOrEmpty(child) ? this.MakePath(normalizedPathStack.Pop(), child) : normalizedPathStack.Pop();
        CmdletProvider.providerBaseTracer.WriteLine("result = {0}", (object) child);
        return child;
      }
    }
  }
}
