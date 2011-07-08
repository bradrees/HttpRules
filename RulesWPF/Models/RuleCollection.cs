using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace RulesWPF.Models
{
    public class RuleCollection : ObservableCollection<RuleModel>
{
    // Fields
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

    public new void Add(RuleModel model)
    {
        Action add = () => base.Add(model);
        if (this._dispatcher.CheckAccess())
        {
            add.Invoke();
        }
        else
        {
            this._dispatcher.Invoke(DispatcherPriority.DataBind, add);
        }
    }

    public new void ClearItems()
    {
        Action clear = base.ClearItems;
        if (this._dispatcher.CheckAccess())
        {
            clear.Invoke();
        }
        else
        {
            this._dispatcher.Invoke(DispatcherPriority.DataBind, clear);
        }
    }

    public new void Remove(RuleModel model)
    {
        Action remove = () => base.Remove(model);
        if (this._dispatcher.CheckAccess())
        {
            remove.Invoke();
        }
        else
        {
            this._dispatcher.Invoke(DispatcherPriority.DataBind, remove);
        }
    }
}

 

}
