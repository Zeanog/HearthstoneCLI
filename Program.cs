﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneCLI {
    class Program {
        static void Main(string[] args)
        {
            while(!Game.Instance.IsOver && Game.Instance.ExecuteNextTurn())
            {
            }
        }
    }
}
