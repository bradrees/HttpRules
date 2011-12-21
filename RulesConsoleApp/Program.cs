using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RulesConsoleApp
{
    class Program
    {
        private static HttpRulesCore.RuleEngine _engine;

        static void Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += Console_CancelKeyPress;

                _engine = new HttpRulesCore.RuleEngine();
                _engine.Start(@"L:\Dev\HttpRules\HttpRules\RulesWPF\Rules\Rules.xml");

                Console.ReadKey();
            }
            finally
            {
                _engine.Shutdown();
            }
        }

        /// <summary>
        /// When the user hits CTRL+C, this event fires.  We use this to shut down and unregister our FiddlerCore.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Shutting down...");
            _engine.Shutdown();
        }
    }
}
