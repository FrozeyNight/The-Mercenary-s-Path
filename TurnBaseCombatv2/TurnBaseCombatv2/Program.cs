using System;
using System.Runtime.InteropServices;
using TurnBaseCombatv2;

namespace TurnBaseCombatv2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "The Mercenary's Path (demo)";

            Player player = Game.getGameData();

            Game.TextData(player);

        }
        
    }
}
