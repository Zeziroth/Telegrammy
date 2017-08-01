using System;
using System.Collections.Generic;
using System.Threading;

namespace MainWindow
{
    public class CommandController
    {
        private Dictionary<List<string>, Dictionary<string, Action>> commands = null;

        public CommandController(ref Dictionary<List<string>, Dictionary<string, Action>> _commands)
        {
            commands = _commands;
        }

        private bool ValidCommand(string cmd)
        {
            string term = cmd.ToLower();
            foreach (List<string> commandTree in commands.Keys)
            {
                if (commandTree.Contains(term))
                {
                    return true;
                }
            }
            return false;
        }
        internal void HandleCommand(string cmd)
        {
            string term = cmd.ToLower();
            if (ValidCommand(term))
            {
                List<string> commandTree = GetCommandEntry(term);
                foreach (Action action in commands[commandTree].Values)
                {
                    Thread t = new Thread(() => { action(); });
                    t.Start();
                    return;
                }
            }
        }
        private List<string> GetCommandEntry(string cmd)
        {
            foreach (List<string> command in commands.Keys)
            {
                if (command.Contains(cmd.ToLower()))
                {
                    return command;
                }
            }
            return null;
        }
    }
}
