using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Threading
{
    public static class DispatcherHelper
    {
        #region Easy Invoke

        public static void Invoke(this Dispatcher d, Action action)
        {
            d.Invoke(action);
        }

        public static void Invoke<T>(this Dispatcher d, Action<T> action, T arg1)
        {
            d.Invoke(action, arg1);
        }

        public static void Invoke<T1, T2>(this Dispatcher d, Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            d.Invoke(action, arg1, arg2);
        }

        public static void Invoke<T1, T2, T3>(this Dispatcher d, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            d.Invoke(action, arg1, arg2, arg3);
        }

        public static void Invoke<T1, T2, T3, T4>(this Dispatcher d, Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            d.Invoke(action, arg1, arg2, arg3, arg4);
        }

        #endregion

        #region In

        //public static void In2(this Dispatcher d, TimeSpan time, Action action)
        //{
        //    IDisposable cleanup = null;
        //    cleanup = Observable.Interval(time).Subscribe((_) =>
        //        {
        //            cleanup.Dispose();
        //            action();
        //        });
        //}

        public static void In(this Dispatcher d, TimeSpan time, Action action)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = time;
            dt.Tick += new EventHandler((sender, args) => 
                {
                    dt.Stop();
                    action();
                });
            dt.Start();
        }

        public static void In<T1>(this Dispatcher d, TimeSpan time, Action<T1> action, T1 arg1)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = time;
            dt.Tick += new EventHandler((sender, args) =>
            {
                dt.Stop();
                action(arg1);
            });
            dt.Start();
        }

        public static void In<T1, T2>(this Dispatcher d, TimeSpan time, Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = time;
            dt.Tick += new EventHandler((sender, args) =>
            {
                dt.Stop();
                action(arg1, arg2);
            });
            dt.Start();
        }

        public static void In<T1, T2, T3>(this Dispatcher d, TimeSpan time, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = time;
            dt.Tick += new EventHandler((sender, args) =>
            {
                dt.Stop();
                action(arg1, arg2, arg3);
            });
            dt.Start();
        }

        public static void In<T1, T2, T3, T4>(this Dispatcher d, TimeSpan time, Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = time;
            dt.Tick += new EventHandler((sender, args) =>
            {
                dt.Stop();
                action(arg1, arg2, arg3, arg4);
            });
            dt.Start();
        }

        #endregion
    }
}
