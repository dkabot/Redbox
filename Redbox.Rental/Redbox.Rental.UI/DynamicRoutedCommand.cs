using System;
using System.Windows.Input;

// Puyodead1 - I aint fixing all this shit, if you want to, go for it

namespace Redbox.Rental.UI
{
    public class DynamicRoutedCommand : ICommand
    {
        private dynamic _canExecute;

        private dynamic _doExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(dynamic parameter)
        {
            var flag = true;
            //if (DynamicRoutedCommand.<>o__6.<>p__1 == null)
            //{
            //	DynamicRoutedCommand.<>o__6.<>p__1 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //}
            //Func<CallSite, object, bool> target = DynamicRoutedCommand.<>o__6.<>p__1.Target;
            //CallSite <>p__ = DynamicRoutedCommand.<>o__6.<>p__1;
            //if (DynamicRoutedCommand.<>o__6.<>p__0 == null)
            //{
            //	DynamicRoutedCommand.<>o__6.<>p__0 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.NotEqual, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //	{
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
            //	}));
            //}
            //if (target(<>p__, DynamicRoutedCommand.<>o__6.<>p__0.Target(DynamicRoutedCommand.<>o__6.<>p__0, this._canExecute, null)))
            //{
            //	if (DynamicRoutedCommand.<>o__6.<>p__7 == null)
            //	{
            //		DynamicRoutedCommand.<>o__6.<>p__7 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //	}
            //	Func<CallSite, object, bool> target2 = DynamicRoutedCommand.<>o__6.<>p__7.Target;
            //	CallSite <>p__2 = DynamicRoutedCommand.<>o__6.<>p__7;
            //	if (DynamicRoutedCommand.<>o__6.<>p__2 == null)
            //	{
            //		DynamicRoutedCommand.<>o__6.<>p__2 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //		{
            //			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
            //		}));
            //	}
            //	object obj = DynamicRoutedCommand.<>o__6.<>p__2.Target(DynamicRoutedCommand.<>o__6.<>p__2, parameter, null);
            //	if (DynamicRoutedCommand.<>o__6.<>p__6 == null)
            //	{
            //		DynamicRoutedCommand.<>o__6.<>p__6 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //	}
            //	object obj3;
            //	if (!DynamicRoutedCommand.<>o__6.<>p__6.Target(DynamicRoutedCommand.<>o__6.<>p__6, obj))
            //	{
            //		if (DynamicRoutedCommand.<>o__6.<>p__5 == null)
            //		{
            //			DynamicRoutedCommand.<>o__6.<>p__5 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //			{
            //				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            //			}));
            //		}
            //		Func<CallSite, object, object, object> target3 = DynamicRoutedCommand.<>o__6.<>p__5.Target;
            //		CallSite <>p__3 = DynamicRoutedCommand.<>o__6.<>p__5;
            //		object obj2 = obj;
            //		if (DynamicRoutedCommand.<>o__6.<>p__4 == null)
            //		{
            //			DynamicRoutedCommand.<>o__6.<>p__4 = CallSite<Func<CallSite, object, Type, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //			{
            //				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
            //			}));
            //		}
            //		Func<CallSite, object, Type, object> target4 = DynamicRoutedCommand.<>o__6.<>p__4.Target;
            //		CallSite <>p__4 = DynamicRoutedCommand.<>o__6.<>p__4;
            //		if (DynamicRoutedCommand.<>o__6.<>p__3 == null)
            //		{
            //			DynamicRoutedCommand.<>o__6.<>p__3 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetType", null, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //		}
            //		obj3 = target3(<>p__3, obj2, target4(<>p__4, DynamicRoutedCommand.<>o__6.<>p__3.Target(DynamicRoutedCommand.<>o__6.<>p__3, this._canExecute), typeof(Func<bool>)));
            //	}
            //	else
            //	{
            //		obj3 = obj;
            //	}
            //	if (target2(<>p__2, obj3))
            //	{
            //		if (DynamicRoutedCommand.<>o__6.<>p__9 == null)
            //		{
            //			DynamicRoutedCommand.<>o__6.<>p__9 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(DynamicRoutedCommand)));
            //		}
            //		Func<CallSite, object, bool> target5 = DynamicRoutedCommand.<>o__6.<>p__9.Target;
            //		CallSite <>p__5 = DynamicRoutedCommand.<>o__6.<>p__9;
            //		if (DynamicRoutedCommand.<>o__6.<>p__8 == null)
            //		{
            //			DynamicRoutedCommand.<>o__6.<>p__8 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Invoke", null, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //		}
            //		flag = target5(<>p__5, DynamicRoutedCommand.<>o__6.<>p__8.Target(DynamicRoutedCommand.<>o__6.<>p__8, this._canExecute));
            //	}
            //	else
            //	{
            //		if (DynamicRoutedCommand.<>o__6.<>p__11 == null)
            //		{
            //			DynamicRoutedCommand.<>o__6.<>p__11 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(DynamicRoutedCommand)));
            //		}
            //		Func<CallSite, object, bool> target6 = DynamicRoutedCommand.<>o__6.<>p__11.Target;
            //		CallSite <>p__6 = DynamicRoutedCommand.<>o__6.<>p__11;
            //		if (DynamicRoutedCommand.<>o__6.<>p__10 == null)
            //		{
            //			DynamicRoutedCommand.<>o__6.<>p__10 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Invoke", null, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //			{
            //				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            //			}));
            //		}
            //		flag = target6(<>p__6, DynamicRoutedCommand.<>o__6.<>p__10.Target(DynamicRoutedCommand.<>o__6.<>p__10, this._canExecute, parameter));
            //	}
            //}
            return flag;
        }

        public void Execute(dynamic parameter)
        {
            //if (DynamicRoutedCommand.<>o__7.<>p__5 == null)
            //{
            //	DynamicRoutedCommand.<>o__7.<>p__5 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //}
            //Func<CallSite, object, bool> target = DynamicRoutedCommand.<>o__7.<>p__5.Target;
            //CallSite <>p__ = DynamicRoutedCommand.<>o__7.<>p__5;
            //if (DynamicRoutedCommand.<>o__7.<>p__0 == null)
            //{
            //	DynamicRoutedCommand.<>o__7.<>p__0 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //	{
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
            //	}));
            //}
            //object obj = DynamicRoutedCommand.<>o__7.<>p__0.Target(DynamicRoutedCommand.<>o__7.<>p__0, parameter, null);
            //if (DynamicRoutedCommand.<>o__7.<>p__4 == null)
            //{
            //	DynamicRoutedCommand.<>o__7.<>p__4 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsFalse, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //}
            //object obj3;
            //if (!DynamicRoutedCommand.<>o__7.<>p__4.Target(DynamicRoutedCommand.<>o__7.<>p__4, obj))
            //{
            //	if (DynamicRoutedCommand.<>o__7.<>p__3 == null)
            //	{
            //		DynamicRoutedCommand.<>o__7.<>p__3 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.BinaryOperationLogical, ExpressionType.And, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //		{
            //			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            //		}));
            //	}
            //	Func<CallSite, object, object, object> target2 = DynamicRoutedCommand.<>o__7.<>p__3.Target;
            //	CallSite <>p__2 = DynamicRoutedCommand.<>o__7.<>p__3;
            //	object obj2 = obj;
            //	if (DynamicRoutedCommand.<>o__7.<>p__2 == null)
            //	{
            //		DynamicRoutedCommand.<>o__7.<>p__2 = CallSite<Func<CallSite, object, Type, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //		{
            //			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
            //		}));
            //	}
            //	Func<CallSite, object, Type, object> target3 = DynamicRoutedCommand.<>o__7.<>p__2.Target;
            //	CallSite <>p__3 = DynamicRoutedCommand.<>o__7.<>p__2;
            //	if (DynamicRoutedCommand.<>o__7.<>p__1 == null)
            //	{
            //		DynamicRoutedCommand.<>o__7.<>p__1 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetType", null, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //	}
            //	obj3 = target2(<>p__2, obj2, target3(<>p__3, DynamicRoutedCommand.<>o__7.<>p__1.Target(DynamicRoutedCommand.<>o__7.<>p__1, this._doExecute), typeof(Action)));
            //}
            //else
            //{
            //	obj3 = obj;
            //}
            //if (target(<>p__, obj3))
            //{
            //	if (DynamicRoutedCommand.<>o__7.<>p__6 == null)
            //	{
            //		DynamicRoutedCommand.<>o__7.<>p__6 = CallSite<Action<CallSite, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "Invoke", null, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //	}
            //	DynamicRoutedCommand.<>o__7.<>p__6.Target(DynamicRoutedCommand.<>o__7.<>p__6, this._doExecute);
            //	return;
            //}
            //if (DynamicRoutedCommand.<>o__7.<>p__7 == null)
            //{
            //	DynamicRoutedCommand.<>o__7.<>p__7 = CallSite<Action<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "Invoke", null, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //	{
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            //	}));
            //}
            //DynamicRoutedCommand.<>o__7.<>p__7.Target(DynamicRoutedCommand.<>o__7.<>p__7, this._doExecute, parameter);
        }

        public void InitDynamicExecutes(dynamic doExecute, dynamic canExecute)
        {
            //if (DynamicRoutedCommand.<>o__2.<>p__1 == null)
            //{
            //	DynamicRoutedCommand.<>o__2.<>p__1 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.IsTrue, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
            //}
            //Func<CallSite, object, bool> target = DynamicRoutedCommand.<>o__2.<>p__1.Target;
            //CallSite <>p__ = DynamicRoutedCommand.<>o__2.<>p__1;
            //if (DynamicRoutedCommand.<>o__2.<>p__0 == null)
            //{
            //	DynamicRoutedCommand.<>o__2.<>p__0 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(DynamicRoutedCommand), new CSharpArgumentInfo[]
            //	{
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
            //		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant, null)
            //	}));
            //}
            //if (target(<>p__, DynamicRoutedCommand.<>o__2.<>p__0.Target(DynamicRoutedCommand.<>o__2.<>p__0, doExecute, null)))
            //{
            //	throw new NullReferenceException("The doExecute cannot be null.");
            //}
            _doExecute = doExecute;
            _canExecute = canExecute;
        }
    }
}