using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;

namespace Astral.Plane.Utility
{
    // Not sure why I didn't think of this before.
    public class ChangeNotificationWrapper<WrapType, IndexType, ReturnType> : IIndexable<IndexType, ReturnType>
    {
        static Func<WrapType, IndexType, ReturnType> _get;
        static Action<WrapType, IndexType, ReturnType> _set;

        static ChangeNotificationWrapper()
        {
            CompileGet();
            CompileSet();
        }

        private static void CompileGet()
        {
            ParameterExpression type = Expression.Parameter(typeof(WrapType));
            ParameterExpression index = Expression.Parameter(typeof(IndexType));

            // Arrays are a special
            if (typeof(Array).IsAssignableFrom(typeof(WrapType)))
            {
                _get = (Func<WrapType, IndexType, ReturnType>)Expression.Lambda(Expression.ArrayIndex(type, index), type, index).Compile();
            }
            else
            {
                MethodInfo method = typeof(WrapType).GetMethod("get_Item");
                Expression callMethod = Expression.Call(type, method, index);
                _get = (Func<WrapType, IndexType, ReturnType>)Expression.Lambda(callMethod, type, index).Compile();
            }
        }

        private static void CompileSet()
        {
            ParameterExpression type = Expression.Parameter(typeof(WrapType));
            ParameterExpression index = Expression.Parameter(typeof(IndexType));
            ParameterExpression value = Expression.Parameter(typeof(ReturnType));

            if (typeof(Array).IsAssignableFrom(typeof(WrapType)))
            {
                Expression arrayAccess = Expression.ArrayAccess(type, index);
                Expression assignment = Expression.Assign(arrayAccess, value);
                var func = (Func<WrapType, IndexType, ReturnType, ReturnType>)Expression.Lambda(assignment, type, index, value).Compile();
                _set = (WrapType, IndexType, ReturnType) => func(WrapType, IndexType, ReturnType);
            }
            else
            {

                MethodInfo method = typeof(WrapType).GetMethod("set_Item");
                Expression callMethod = Expression.Call(type, method, index, value);
                _set = (Action<WrapType, IndexType, ReturnType>)Expression.Lambda(callMethod, type, index, value).Compile();
            }
        }

        private WrapType _outter;
        public ChangeNotificationWrapper(WrapType outter)
        {
            _outter = outter;
        }

        public ReturnType this[IndexType index]
        {
            get
            {
                if (OnGet != null)
                    OnGet(index);
                return _get(_outter, index);
            }
            set
            {
                if (OnSet != null)
                    OnSet(index, value);
                _set(_outter, index, value);
            }
        }

        public event Action<IndexType> OnGet;
        public event Action<IndexType, ReturnType> OnSet;
    }
}
