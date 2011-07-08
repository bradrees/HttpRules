using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;

namespace RulesWPF.Models
{
    public class LogQueue : ObservableCollection<LogModel>
    {
        public int Size {get;set;}

        readonly Dispatcher _dispatcher;

        public LogQueue(int size)
        {
            this.Size = size;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Enqueue(LogModel model)
        {
            Action enqueue = () =>
            {
                this.Insert(0, model);

                if (this.Count > this.Size)
                {
                    this.RemoveAt(this.Count - 1);
                }
            };

            if (_dispatcher.CheckAccess())
            {
                enqueue();
            }
            else
            {
                _dispatcher.Invoke(DispatcherPriority.DataBind, enqueue);
            }

            
        }
    }
}
