// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ArrayReferenceNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ArrayReferenceNode : ParseTreeNode, IAssignableParseTreeNode
  {
    private readonly ParseTreeNode _target;
    private readonly ParseTreeNode _index;
    private readonly List<TypeLiteral> _typeConstraint = new List<TypeLiteral>();

    public ArrayReferenceNode(Token operatorToken, ParseTreeNode target, ParseTreeNode index)
    {
      this.NodeToken = operatorToken;
      this._target = target;
      this._index = index;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      object obj = this.GetValue(PSObject.Base(this._target.Execute(context)), PSObject.Base(this._index.Execute(context)), context);
      foreach (TypeLiteral typeLiteral in this._typeConstraint)
        obj = Parser.ConvertTo(obj, typeLiteral.Type, this.NodeToken);
      if (obj == AutomationNull.Value)
        obj = (object) null;
      return obj;
    }

    internal object GetValue(object target, object index, ExecutionContext context)
    {
      if (LanguagePrimitives.IsNull(target))
        throw InterpreterError.NewInterpreterException(index, typeof (RuntimeException), this.NodeToken, "NullArray");
      if (LanguagePrimitives.IsNull(index))
        throw InterpreterError.NewInterpreterException(index, typeof (RuntimeException), this.NodeToken, "NullArrayIndex");
      switch (target)
      {
        case Array array:
          return this.DoGetValue(array, index, context);
        case IDictionary dictionary:
          return this.DoGetValue(dictionary, index, context);
        case string str:
          return this.DoGetValue(str, index, context);
        default:
          return this.GetItemPropertyValue(target, index, context);
      }
    }

    internal void SetValue(object target, object index, object value)
    {
      if (LanguagePrimitives.IsNull(target))
        throw InterpreterError.NewInterpreterException(index, typeof (RuntimeException), this.NodeToken, "NullArray");
      if (LanguagePrimitives.IsNull(index))
        throw InterpreterError.NewInterpreterException(target, typeof (RuntimeException), this.NodeToken, "NullArrayIndex");
      switch (target)
      {
        case Array array:
          this.DoSetValue(array, index, value);
          break;
        case IDictionary dictionary:
          this.DoSetValue(dictionary, index, value);
          break;
        default:
          this.SetItemPropertyValue(target, index, value);
          break;
      }
    }

    private object DoGetValue(Array array, object index, ExecutionContext context)
    {
      if (array.Rank > 1)
        return this.GetMultiArrayValue(array, index, context);
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(index);
      if (enumerator == null)
        return this.GetArrayElement(array, index, out bool _);
      ArrayList arrayList = new ArrayList();
      while (ParserOps.MoveNext(context, this.NodeToken, enumerator))
      {
        bool failed;
        object arrayElement = this.GetArrayElement(array, ParserOps.Current(this.NodeToken, enumerator), out failed);
        if (!failed)
          arrayList.Add(arrayElement);
      }
      return (object) arrayList.ToArray();
    }

    private object GetMultiArrayValue(Array array, object index, ExecutionContext context)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(index);
      if (enumerator == null)
        this.ReportIndexingError(array, index, (Exception) null);
      Exception reason = (Exception) null;
      int[] numArray = (int[]) null;
      try
      {
        numArray = (int[]) LanguagePrimitives.ConvertTo(index, typeof (int[]), (IFormatProvider) NumberFormatInfo.InvariantInfo);
      }
      catch (InvalidCastException ex)
      {
        reason = (Exception) ex;
      }
      if (numArray != null)
      {
        if (numArray.Length != array.Rank)
          this.ReportIndexingError(array, index, (Exception) null);
        return this.GetArrayElement(array, (object) numArray, out bool _);
      }
      ArrayList arrayList = new ArrayList();
      while (ParserOps.MoveNext(context, this.NodeToken, enumerator))
      {
        if (ParserOps.Current(this.NodeToken, enumerator) is IList)
        {
          bool failed;
          object arrayElement = this.GetArrayElement(array, enumerator.Current, out failed);
          if (!failed)
            arrayList.Add(arrayElement);
        }
        else if (arrayList.Count == 0)
          this.ReportIndexingError(array, index, reason);
      }
      return (object) arrayList.ToArray();
    }

    internal object GetArrayElement(Array array, object index, out bool failed)
    {
      failed = false;
      try
      {
        if (array.Rank == 1)
        {
          int index1 = (int) LanguagePrimitives.ConvertTo(index, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo);
          if (index1 < 0)
            index1 += array.Length;
          return array.GetValue(index1);
        }
        int[] numArray = (int[]) LanguagePrimitives.ConvertTo(index, typeof (int[]), (IFormatProvider) NumberFormatInfo.InvariantInfo);
        return array.GetValue(numArray);
      }
      catch (ScriptCallDepthException ex)
      {
        throw;
      }
      catch (FlowControlException ex)
      {
        throw;
      }
      catch (RuntimeException ex)
      {
        throw;
      }
      catch (InvalidCastException ex)
      {
        failed = true;
      }
      catch (ArgumentException ex)
      {
        this.ReportIndexingError(array, index, (Exception) ex);
      }
      catch (IndexOutOfRangeException ex)
      {
        failed = true;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        failed = true;
      }
      return (object) null;
    }

    private void DoSetValue(Array array, object index, object value)
    {
      if (index is IList list)
      {
        if (array.Rank != 1)
        {
          if (list.Count > 1)
          {
            if (!(list[0] is IList))
              goto label_5;
          }
          else
            goto label_5;
        }
        throw InterpreterError.NewInterpreterException(index, typeof (RuntimeException), this.NodeToken, "ArraySliceAssignmentFailed", (object) ArrayReferenceNode.IndexStringMessage(index));
      }
label_5:
      try
      {
        this.SetArrayElement(array, index, value);
      }
      catch (IndexOutOfRangeException ex)
      {
        throw InterpreterError.NewInterpreterExceptionWithInnerException(index, typeof (RuntimeException), this.NodeToken, "IndexOutOfRange", (Exception) ex, (object) ArrayReferenceNode.IndexStringMessage(index));
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterExceptionWithInnerException(value, typeof (RuntimeException), this.NodeToken, "ArrayAssignmentFailed", ex, (object) ArrayReferenceNode.IndexStringMessage(index), (object) ex.Message);
      }
    }

    internal void SetArrayElement(Array array, object index, object value)
    {
      value = LanguagePrimitives.ConvertTo(value, array.GetType().GetElementType(), (IFormatProvider) CultureInfo.CurrentCulture);
      if (array.Rank == 1)
      {
        int index1 = (int) LanguagePrimitives.ConvertTo(index, typeof (int), (IFormatProvider) NumberFormatInfo.InvariantInfo);
        if (index1 < 0)
          index1 += array.Length;
        array.SetValue(value, index1);
      }
      else
      {
        int[] numArray = (int[]) LanguagePrimitives.ConvertTo(index, typeof (int[]), (IFormatProvider) NumberFormatInfo.InvariantInfo);
        array.SetValue(value, numArray);
      }
    }

    internal void ReportIndexingError(Array array, object index, Exception reason)
    {
      string str = ArrayReferenceNode.IndexStringMessage(index);
      if (reason == null)
        throw InterpreterError.NewInterpreterException(index, typeof (RuntimeException), this.NodeToken, "NeedMultidimensionalIndex", (object) array.Rank, (object) str);
      throw InterpreterError.NewInterpreterExceptionWithInnerException(index, typeof (RuntimeException), this.NodeToken, "NeedMultidimensionalIndex", reason, (object) array.Rank, (object) str);
    }

    internal static string IndexStringMessage(object index)
    {
      string str = PSObject.ToString((ExecutionContext) null, index, ",", (string) null, (IFormatProvider) null, true, true);
      if (str.Length > 20)
        str = str.Substring(0, 20) + " ...";
      return str;
    }

    private object DoGetValue(IDictionary dictionary, object index, ExecutionContext context)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(index);
      if (enumerator != null)
      {
        ArrayList arrayList = new ArrayList();
        while (ParserOps.MoveNext(context, (Token) null, enumerator))
        {
          try
          {
            arrayList.Add(dictionary[enumerator.Current]);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
          }
        }
        return (object) arrayList.ToArray();
      }
      try
      {
        return dictionary[index];
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        return (object) null;
      }
    }

    private void DoSetValue(IDictionary dictionary, object index, object value)
    {
      try
      {
        dictionary[index] = value;
      }
      catch (InvalidCastException ex)
      {
        throw InterpreterError.NewInterpreterExceptionWithInnerException(index, typeof (RuntimeException), this.NodeToken, "KeyTypeMismatch", (Exception) ex, (object) dictionary.GetType().Name, (object) index.GetType().Name);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterExceptionWithInnerException(index, typeof (RuntimeException), this.NodeToken, "ArrayAssignmentFailed", ex, index, (object) ex.Message);
      }
    }

    private object DoGetValue(string str, object index, ExecutionContext context)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(index);
      if (enumerator != null)
      {
        ArrayList arrayList = new ArrayList();
        while (ParserOps.MoveNext(context, this.NodeToken, enumerator))
        {
          int index1 = ParserOps.FixNum(ParserOps.Current(this.NodeToken, enumerator), this.NodeToken);
          if (index1 < 0)
            index1 += str.Length;
          try
          {
            arrayList.Add((object) str[index1]);
          }
          catch (IndexOutOfRangeException ex)
          {
          }
        }
        return (object) arrayList.ToArray();
      }
      int index2 = ParserOps.FixNum(index, this.NodeToken);
      if (index2 < 0)
        index2 += str.Length;
      try
      {
        return (object) str[index2];
      }
      catch (IndexOutOfRangeException ex)
      {
        return (object) null;
      }
    }

    private object GetItemPropertyValue(object obj, object index, ExecutionContext context)
    {
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(index);
      if (enumerator != null)
      {
        ArrayList arrayList = new ArrayList();
        while (ParserOps.MoveNext(context, this.NodeToken, enumerator))
        {
          try
          {
            object obj1 = this.GetItem(obj, enumerator.Current);
            arrayList.Add(obj1);
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            if (this.IsMethodNotFoundException(ex))
              throw InterpreterError.NewInterpreterExceptionWithInnerException(index, typeof (RuntimeException), this.NodeToken, "CannotIndex", ex, (object) obj.GetType().FullName);
          }
        }
        return (object) arrayList.ToArray();
      }
      try
      {
        return this.GetItem(obj, index);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (this.IsMethodNotFoundException(ex))
          throw InterpreterError.NewInterpreterExceptionWithInnerException(index, typeof (RuntimeException), this.NodeToken, "CannotIndex", ex, (object) obj.GetType().FullName);
        return (object) null;
      }
    }

    private object GetItem(object target, object index) => ParserOps.CallMethod(this.NodeToken, target, "get_Item", new object[1]
    {
      index
    }, false, (object) AutomationNull.Value);

    private bool IsMethodNotFoundException(Exception e) => e is RuntimeException runtimeException && runtimeException.ErrorRecord.FullyQualifiedErrorId == "MethodNotFound";

    private void SetItemPropertyValue(object obj, object index, object value)
    {
      try
      {
        this.SetItem(obj, index, value);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterExceptionWithInnerException(index, typeof (RuntimeException), this.NodeToken, "CannotIndex", ex, (object) obj.GetType().FullName);
      }
    }

    private object SetItem(object target, object index, object value) => ParserOps.CallMethod(this.NodeToken, target, "set_Item", new object[2]
    {
      index,
      value
    }, false, (object) AutomationNull.Value);

    public List<TypeLiteral> TypeConstraint => this._typeConstraint;

    public IAssignableValue GetAssignableValue(
      Array input,
      ExecutionContext context)
    {
      return (IAssignableValue) new AssignableArrayReference(this, PSObject.Base(this._target.Execute(input, (Pipe) null, context)), PSObject.Base(this._index.Execute(input, (Pipe) null, context)));
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._target.Accept(visitor);
      this._index.Accept(visitor);
    }
  }
}
