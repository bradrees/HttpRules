// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RulesWPF
{
    #region Using Directives

    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;

    using HttpRulesCore;

    using RulesWPF.Models;

    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants and Fields

        /// <summary>
        /// The engine.
        /// </summary>
        private readonly RuleEngine engine;

        /// <summary>
        /// The rule path.
        /// </summary>
        private readonly string rulePath = @"L:\Dev\HttpRules\HttpRules\RulesWPF\Rules\Rules.xml";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.rulePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"HttpRules\Rules.xml");

            this.engine = new RuleEngine();

            RuleLog.Current.OnRuleLogged += this.EngineRuleMatched;
            this.engine.RulesUpdated += this.EngineRulesUpdated;
            this.engine.ResponseReceived += this.EngineResponseReceived;

            this.SetLogging();

            this.IsVisibleChanged += this.MainWindowIsVisibleChanged;

            this.Loaded += this.MainWindowLoaded;

            this.LogQueue = new LogQueue(Preferences.Current.MaxLogLength);
            this.lvLog.DataContext = this.LogQueue;

            this.ResponseQueue = new ResponseQueue(Preferences.Current.MaxLogLength);
            this.lvTraffic.DataContext = this.ResponseQueue;

            this.RuleCollection = new RuleCollection();
            this.lvRules.DataContext = this.RuleCollection;

            if (Preferences.Current.Enabled)
            {
                this.engine.Start(this.rulePath);
            }

            this.UpdateUIState();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the log queue.
        /// </summary>
        protected LogQueue LogQueue { get; set; }

        /// <summary>
        /// Gets or sets the response queue.
        /// </summary>
        protected ResponseQueue ResponseQueue { get; set; }

        /// <summary>
        /// Gets or sets the rule collection.
        /// </summary>
        protected RuleCollection RuleCollection { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The on closed event.
        /// </summary>
        /// <param name="e">
        /// The event args.
        /// </param>
        protected override void OnClosed(EventArgs e)
        {
            this.engine.Shutdown();

            base.OnClosed(e);
        }

        /// <summary>
        /// The can copy execute handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void CanCopyExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// The cm clear click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void CmClearClick(object sender, RoutedEventArgs e)
        {
            this.LogQueue.Clear();
            this.ResponseQueue.Clear();
        }

        /// <summary>
        /// The edit rules click event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void CmRulesClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.rulePath);
        }

        /// <summary>
        /// The toggle enabled click handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void CmToggleClick(object sender, RoutedEventArgs e)
        {
            this.ToggleEnabled();
        }

        /// <summary>
        /// The toggle logging click event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void CmToggleLoggingClick(object sender, RoutedEventArgs e)
        {
            Preferences.Current.EnableLogging = !Preferences.Current.EnableLogging;
            Preferences.Save();

            this.SetLogging();
        }

        /// <summary>
        /// The copy command handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void CopyCommandHandler(object sender, RoutedEventArgs e)
        {
            if (this.tabControl.SelectedItem == this.tabTraffic)
            {
                Clipboard.SetText(this.ResponseQueue[this.lvTraffic.SelectedIndex].FullUrl);
            }
        }

        /// <summary>
        /// The engine response received event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void EngineResponseReceived(object sender, ResponseSummaryEventArgs e)
        {
            if (Preferences.Current.EnableLogging)
            {
                this.ResponseQueue.Enqueue(new ResponseModel { ResponseCode = e.ResponseCode, FullUrl = e.FullUrl, ResponseCodeText = e.ResponseCodeText });
            }
        }

        /// <summary>
        /// The engine rule matched event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void EngineRuleMatched(object sender, RuleEventArgs e)
        {
            if (Preferences.Current.EnableLogging)
            {
                this.LogQueue.Enqueue(new LogModel { RuleName = e.Rule.Name, Url = e.Path, Message = e.Message });
            }
        }

        /// <summary>
        /// The engine rules updated event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void EngineRulesUpdated(object sender, EventArgs e)
        {
            this.RuleCollection.ClearItems();
            foreach (var rule in this.engine.Rules)
            {
                this.RuleCollection.Add(new RuleModel(rule));
            }
        }

        /// <summary>
        /// The main window is visible changed event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void MainWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Top = AppBarFunctions.GetScreenHeight(this) - this.RestoreBounds.Height;
            this.Left = SystemParameters.PrimaryScreenWidth - this.RestoreBounds.Width;
        }

        /// <summary>
        /// The main window loaded event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;

            // initialize NotifyIcon
            this.tb.TrayLeftMouseUp += this.TbMouseLeftButtonUp;
            this.tb.TrayMouseDoubleClick += this.TbTrayMouseDoubleClick;
        }

        /// <summary>
        /// The rule clicked event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void RuleClick(object sender, RoutedEventArgs e)
        {
            this.tabControl.SelectedItem = this.tabRules;
            this.lvRules.SelectedIndex = this.RuleCollection.Select(r => r.Name).ToList().IndexOf(((LogModel)((Hyperlink)e.Source).DataContext).RuleName);
            this.lvRules.ScrollIntoView(this.lvRules.Items[this.lvRules.SelectedIndex]);
        }

        /// <summary>
        /// Changes the icon depending if the rule engine is enabled.
        /// </summary>
        private void ToggleEnabled()
        {
            Preferences.Current.Enabled = !Preferences.Current.Enabled;
            Preferences.Save();

            if (Preferences.Current.Enabled)
            {
                this.engine.Start(this.rulePath);
            }
            else
            {
                this.engine.Shutdown();
            }

            this.UpdateUIState();
        }

        /// <summary>
        /// Updates the state of the UI.
        /// </summary>
        private void UpdateUIState()
        {
            if (Preferences.Current.Enabled)
            {
                this.cmToggle.Header = "Disable";
                var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/RulesWPF;component/Icons/Earth Security.ico")).Stream;
                this.tb.Icon = new Icon(iconStream);
            }
            else
            {
                this.cmToggle.Header = "Enable";
                var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/RulesWPF;component/Icons/Earth Stop.ico")).Stream;
                this.tb.Icon = new Icon(iconStream);
            }
        }

        /// <summary>
        /// The set logging menu text.
        /// </summary>
        private void SetLogging()
        {
            this.cmToggleLogging.Header = Preferences.Current.EnableLogging ? "Disable Logging" : "Enable Logging";
        }

        /// <summary>
        /// The tb mouse left button up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event args.
        /// </param>
        private void TbMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            switch (this.Visibility)
            {
                case Visibility.Visible:
                    this.Visibility = Visibility.Collapsed;
                    break;
                case Visibility.Collapsed:
                    this.Visibility = Visibility.Visible;
                    this.Activate();
                    break;
            }
        }

        /// <summary>
        /// The tray mouse double click handler, which toggles if the application is running or not.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TbTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.ToggleEnabled();

            if (Preferences.Current.Enabled)
            {
                this.Visibility = Visibility.Visible;
                this.Activate();
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}