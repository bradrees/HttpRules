using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RulesConsoleApp
{
    class Program
    {
        private static HttpRulesCore.RuleEngine engine;

        static void Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

                engine = new HttpRulesCore.RuleEngine();
                engine.Start(@"L:\Dev\HttpRules\HttpRules\RulesWPF\Rules\Rules.xml");

                Console.ReadKey();
            }
            finally
            {
                engine.Shutdown();
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
            engine.Shutdown();
        }
    }
}
