using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;

namespace Astral.Projector
{
   class Countdown : INotifyPropertyChanged
   {
      private int _count = 10;
      private int _current = 10;
      private DispatcherTimer _timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

      public Countdown()
      {
         _timer.Tick += timer_tick;
      }

      public int CurrentCount
      {
         get { return _current; }
         set
         {
            _current = value;
            RaisePropertyChanged("CurrentCount");
         }
      }

      public int Count
      {
         get { return _count; }
         set
         {
            _count = value;
            RaisePropertyChanged("Count");
         }
      }

      public void Start()
      {
         CurrentCount = Count;
         _timer.Start();
      }

      public event Action CountdownCompleted = () => {};

      void timer_tick(object sender, EventArgs e)
      {
         CurrentCount--;
         if (CurrentCount <= 0)
         {
            _timer.Stop();
            CountdownCompleted();
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      private void RaisePropertyChanged(string property)
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
         }
      }
   }
}
