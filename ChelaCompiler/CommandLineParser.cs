using System;
using System.Collections.Generic;

namespace Chela
{
    public delegate void ArgHandler();
    public delegate void ArgHandler1(string arg1);
    public delegate void ArgHandler2(string arg1, string arg2);

    ///<summary>
    ///A command line parser.
    ///</summary>
    public class CommandLineParser
    {
        private Dictionary<string, object> argumentHandlers;
        private Dictionary<string, ArgHandler1> prefixes;
        public ArgHandler1 Miscellaneous;
        public ArgHandler Help;

        public CommandLineParser ()
        {
            argumentHandlers = new Dictionary<string, object> ();
            prefixes = new Dictionary<string, ArgHandler1> ();
        }

        public void AddArgument(ArgHandler handler, params string[] names)
        {
            foreach(string name in names)
                argumentHandlers.Add(name, handler);
        }

        public void AddArgument(ArgHandler1 handler, params string[] names)
        {
            foreach(string name in names)
                argumentHandlers.Add(name, handler);
        }

        public void AddArgument(ArgHandler2 handler, params string[] names)
        {
            foreach(string name in names)
                argumentHandlers.Add(name, handler);
        }

        public void AddPrefix(ArgHandler1 handler, params string[] prefix)
        {
            foreach(string pre in prefix)
                prefixes.Add(pre, handler);
        }

        public void Parse(string[] args)
        {
            for(int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];

                // Check the dash.
                if(arg[0] != '-')
                {
                    Miscellaneous(arg);
                    continue;
                }

                // Check the colon.
                int index = arg.IndexOf(':');
                if(index >= 0)
                {
                    // Split the argument name and value
                    string argName = arg.Substring(0, index);
                    string argValue = arg.Substring(index + 1);

                    // In windows some path names can fall in this case.
                    if(argumentHandlers.ContainsKey(argName))
                    {
                        // Invoke the handler.
                        ArgHandler1 handler = (ArgHandler1)argumentHandlers[argName];
                        handler(argValue);
                        continue;
                    }
                }

                // Check full name.
                if(argumentHandlers.ContainsKey(arg))
                {
                    // Found a handler.
                    object handler = argumentHandlers[arg];

                    if(handler is ArgHandler)
                    {
                        ArgHandler h = (ArgHandler) handler;
                        h();
                    }
                    else if(handler is ArgHandler1)
                    {
                        ArgHandler1 h = (ArgHandler1) handler;
                        h(args[++i]);
                    }
                    else if(handler is ArgHandler2)
                    {
                        ArgHandler2 h = (ArgHandler2) handler;
                        h(args[++i], args[++i]);
                    }

                    continue;
                }

                // Find a prefix.
                bool found = false;
                foreach(string prefix in prefixes.Keys)
                {
                    if(!arg.StartsWith(prefix))
                        continue;

                    // Found a prefix.
                    arg = arg.Substring(prefix.Length);
                    prefixes[prefix](arg);
                    found = true;
                }

                // Print help
                if(!found)
                {
                    System.Console.WriteLine("Unknown option " + arg);
                    Help();
                    System.Environment.Exit(-1);
                }
            }
        }
    }
}

