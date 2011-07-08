using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace RulesWPF.Models
{
    public class ResponseQueue : ObservableCollection<ResponseModel>
    {
        private readonly Dispatcher _dispatcher;

        public ResponseQueue(int size)
        {
            this.Size = size;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public int Size { get; set; }

        public void Enqueue(ResponseModel model)
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