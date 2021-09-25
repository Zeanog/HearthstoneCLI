using System;
using System.Collections.Generic;


namespace HearthstoneCLI {
    class Program {
        protected static Dictionary<string, Action<string, Dictionary<string, string>>> m_CmdLineHandlers = new Dictionary<string, Action<string, Dictionary<string, string>>>();
        protected static Dictionary<string, string> m_CmdLineArgs = new Dictionary<string, string>();

        protected static void ProcessCmdLineArgs(string[] cmdLineArgs)
        {
            string cmd;
            string args;

            m_CmdLineArgs.Clear();

            for (int ix = 0; ix < cmdLineArgs.Length; ++ix)
            {
                try
                {
                    cmd = cmdLineArgs[ix].Replace("\"", "").Replace("'", "");
                    if (!m_CmdLineHandlers.ContainsKey(cmd))
                    {
                        //Log.Instance.WriteLine("Unknown cmd line arg '{0}'", cmd);
                        System.Diagnostics.Debug.Assert(false);
                        continue;
                    }

                    args = cmdLineArgs[++ix].Replace("\"", "").Replace("'", "");
                    m_CmdLineHandlers[cmd].Invoke(args, m_CmdLineArgs);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
        }

        protected static string FindCmdArg(string cmdName, string defaultVal)
        {
            return m_CmdLineArgs.ContainsKey(cmdName) ? m_CmdLineArgs[cmdName] : defaultVal;
        }

        protected static void RegisterCmdLineHandlers()
        {
            Action<string, Dictionary<string, string>> handler = delegate (string arg, Dictionary<string, string> argMap)
            {
                argMap.Add("GameConfig", arg);
            };
            m_CmdLineHandlers.Add("-c", handler);

            handler = delegate (string arg, Dictionary<string, string> argMap)
            {
                argMap.Add("Player1Def", arg);
            };
            m_CmdLineHandlers.Add("-p1", handler);

            handler = delegate (string arg, Dictionary<string, string> argMap)
            {
                argMap.Add("Player2Def", arg);
            };
            m_CmdLineHandlers.Add("-p2", handler);

            handler = delegate (string arg, Dictionary<string, string> argMap)
            {
                argMap.Add("GameDef", arg);
            };
            m_CmdLineHandlers.Add("-g", handler);
        }

        static Emitter m_TestEmitter = new Emitter();

        static void Main(string[] args)
        {
            try
            {
                RegisterCmdLineHandlers();

                ProcessCmdLineArgs(args);

                if(m_CmdLineArgs.ContainsKey("GameDef"))
                {
                    Game.Create(FindCmdArg("GameDef", ""));
                } else
                {
                    Game.Create(FindCmdArg("GameConfig", ""), FindCmdArg("Player1Def", ""), FindCmdArg("Player2Def", ""));
                }

                //Emitter.ProcessEmitters();

                //m_TestEmitter.Origin = new Vector2i() { X = Console.WindowWidth / 2, Y = Console.WindowHeight / 2 };
                //m_TestEmitter.Start();

                while (!Game.Instance.IsOver && Game.Instance.Execute())
                {
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
