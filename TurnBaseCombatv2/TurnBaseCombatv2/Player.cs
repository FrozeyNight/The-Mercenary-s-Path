using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TurnBaseCombatv2
{
    internal class Player : Entity
    {
        public string[] inventory = new string[10];
        public bool encounter = true;
        public int XpNeededForLevelUp = 2;
        public int IncrementPerLevelUp = 2;
        public int PlayerLevel = 0;
        public int PlayerXp = 0;
        public string weapon = "Club[+2]";
        public int decisionPath = 0;
        public int bonusHealth = 0;
        public int bonusDamage = 0;
        public int bonusSpeed = 0;
        public Player(int entityHealth, int entityDamage, int entitySpeed, string entityName, string identifier) : base(entityHealth, entityDamage, entitySpeed, entityName, identifier)
        {
            this.identifier = "p";
            entityMaxHealth = entityHealth;
        }

        public Player DoAction(Player player, int howManyEnemies, Enemy[] enemies)
        {
            
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. Attack");
            Console.WriteLine("2. Inventory");
            Console.WriteLine("3. Escape");
            string playerInput = Console.ReadLine();
            bool inputError = true;
            int playerOutput = 0;
            while (inputError) 
            {
                try
                {
                    playerOutput = int.Parse(playerInput);
                    if (playerOutput > 0 && playerOutput < 4)
                        inputError = false;
                    else
                        throw new Exception();
                }
                catch (Exception)
                {
                    Console.WriteLine("Please input one of the options");
                    playerInput = Console.ReadLine();
                }
            }
            switch (playerOutput)
            {
                case 1:
                    int target = 0;
                    if(howManyEnemies != 1)
                    {
                        target = ChooseTarget(player, howManyEnemies, enemies);
                    }
                    else
                    {
                        target = 0;
                    }

                    if(target <= howManyEnemies)
                    {
                        enemies[target].entityHealth -= player.entityDamage;
                        if(enemies[target].entityHealth > 0)
                            Game.WriteWithDifferentColor($"{enemies[target].entityName} now has {enemies[target].entityHealth} health", ConsoleColor.Green);
                        else
                            Game.WriteWithDifferentColor($"{enemies[target].entityName} died!", ConsoleColor.Green);
                    }
                        
                    break;

                case 2:
                    Console.WriteLine("Which item would you like to use?");
                    int counter = 1;
                    foreach (string item in inventory)
                    {
                        if(item != null)
                            Console.WriteLine($"{counter}. {item}");
                        else
                            Console.WriteLine($"{counter}. empty slot");
                        counter++;
                    }
                    string playerInput2 = Console.ReadLine();
                    int playerOutput2 = -1;
                    while (playerOutput2 < 0 || playerOutput2 > 10)
                    {
                        try
                        {
                            playerOutput2 = int.Parse(playerInput2);
                            if (playerOutput2 < 0 || playerOutput2 > 10)
                                throw new Exception();
                            
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Please choose one of the items in your inventory");
                            playerInput2 = Console.ReadLine();
                        }
                        
                    }

                    IdentifyItem(player, player.inventory[playerOutput2 - 1], playerOutput2 - 1);

                    if (player.inventory[playerOutput2 - 1] == null || player.inventory[playerOutput2 - 1].Contains("]"))
                        player.DoAction(player, howManyEnemies, enemies);

                    //player.inventory[playerOutput2 - 1] = null;

                    break;
                case 3:
                    Random random = new Random();
                    //Entity[] fastToSlowEnemies = Enemy.CalculateEnemyInitiative(enemies);
                    int calculatedEscapeOdds = (int)Math.Round((double)/*fastToSlowEnemies*/enemies[0].entitySpeed / player.entitySpeed, MidpointRounding.ToPositiveInfinity);
                    int playerEscape = random.Next(1, calculatedEscapeOdds + 1);
                    if (playerEscape == 1)
                    {
                        Game.WriteWithDifferentColor("The player has escaped the encounter!", ConsoleColor.Green);
                        encounter = false;
                    }
                    else
                        Game.WriteWithDifferentColor("The player tries to escape, but fails", ConsoleColor.DarkRed);
                    break;


            }
            return player;
        }

        public int ChooseTarget(Player player, int howManyEnemies, Enemy[] enemies)
        {
            int target = 0;
            int counter = 1;
            if (howManyEnemies != 1)
            {
                Console.WriteLine("Choose which enemy to attack");
                foreach (Enemy enemy in enemies)
                {
                     Console.WriteLine($"{counter}. {enemy.entityName}");
                    counter++;
                }
                string playerInput1 = Console.ReadLine();
                bool inputError = true;
                int playerOutput1 = 0;
                while (inputError)
                {
                    try
                    {
                        playerOutput1 = int.Parse(playerInput1);
                        if (playerOutput1 > 0 && playerOutput1 < enemies.Length + 1)
                        {
                            target = playerOutput1 - 1;
                            inputError = false;
                        }
                        else
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Please input one of the options");
                        playerInput1 = Console.ReadLine();
                    }
                }
            }
            return target;
        }

        public void IdentifyItem(Player player, string item, int indexOfChosenItem)
        {
             switch(item) 
             {
                    case "Health Potion":
                        if (player.entityMaxHealth >= player.entityHealth + 4)
                            player.entityHealth += 4;
                        else
                            player.entityHealth += player.entityMaxHealth - player.entityHealth;
                            Game.WriteWithDifferentColor($"You drink a {item} and regenarate 4 health, you now have {player.entityHealth} health", ConsoleColor.Green);
                            player.inventory[indexOfChosenItem] = null;
                        break;
                    case "Speed Potion":
                        player.entitySpeed += 2;
                        player.bonusSpeed += 2;
                        Game.WriteWithDifferentColor($"You drink a {item} and gain 2 speed, you now have {player.entitySpeed} speed", ConsoleColor.Green);
                        player.inventory[indexOfChosenItem] = null;
                        break;
                    case "Strength Potion":
                        player.entityDamage += 2;
                        player.bonusDamage += 2;
                        Game.WriteWithDifferentColor($"You drink a {item} and gain 2 damage, you now have {player.entityDamage} damage", ConsoleColor.Green);
                        player.inventory[indexOfChosenItem] = null;
                        break;
                    case null:
                        if(player.weapon == string.Empty)
                        Game.WriteWithDifferentColor("You might have a great imagination, but you wont just manifest an item with sheer will", ConsoleColor.Yellow);
                        else
                        {
                            
                            Console.WriteLine($"Would you like to put your current weapon ({player.weapon}) in this slot? yes/no" );
                            string playerInput = Console.ReadLine();
                            switch (playerInput)
                            {
                                case "yes":
                                    player.inventory[indexOfChosenItem] = player.weapon;
                                    IdentifyOldWeapon(player, player.weapon);
                                    player.weapon = string.Empty;
                                break;
                                case "no":
                                    Console.WriteLine("oh ok");
                                break;
                            }
                        }
                    break;
                
             }
             if (item != null && item.Contains(']'))
             {
                if(player.weapon == string.Empty)
                {
                    Console.WriteLine("Would you like to use this weapon as your current weapon? yes/no");
                    string playerInput = Console.ReadLine();
                    switch (playerInput)
                    {
                        case "yes":
                            player.weapon = player.inventory[indexOfChosenItem];
                            player.inventory[indexOfChosenItem] = null;
                            IdentifyNewWeapon(player, player.weapon);
                            Game.WriteWithDifferentColor($"You are now using {player.weapon} as your current weapon", ConsoleColor.Green);
                            break;
                        case "no":
                            Console.WriteLine("oh ok");
                            break;
                        default:
                            Game.WriteWithDifferentColor($"You choose not to equip this weapon", ConsoleColor.DarkYellow);
                            break;
                    }

                }
                else
                {
                    Console.WriteLine($"Would you like this weapon to replace your current weapon? ({player.weapon}) yes/no");
                    string playerInput = Console.ReadLine();
                    switch (playerInput)
                    {
                        case "yes":
                            string storeWeapon = player.weapon;
                            IdentifyOldWeapon(player, storeWeapon);
                            player.weapon = player.inventory[indexOfChosenItem];
                            IdentifyNewWeapon(player, player.weapon);
                            player.inventory[indexOfChosenItem] = storeWeapon;
                            Game.WriteWithDifferentColor($"You are now using {player.weapon} as your current weapon", ConsoleColor.Green);
                            break;
                        case "no":
                            Console.WriteLine("oh ok");
                            break;
                        default:
                            Game.WriteWithDifferentColor($"You choose not to replace this weapon", ConsoleColor.DarkYellow);
                            break;
                    }
                }
             }
        }

        public void LevelUp(Player player)
        {
            Game.SaveGame(player);
            Game.WriteWithDifferentColor("-------------LEVEL UP--------------", ConsoleColor.Green);
            Console.WriteLine("What would you like to level up?");
            Console.WriteLine("1. Strength");
            Console.WriteLine("2. Health");
            Console.WriteLine("3. Speed");

            string playerInput = Console.ReadLine();
            bool inputError = true;

            while (inputError)
            {
                try
                {
                    switch (playerInput) 
                    {
                        case "1":
                            player.entityDamage += 1;
                            inputError = false;
                            Console.WriteLine("Your damage has increased!");
                            break;
                        case "2":
                            player.entityMaxHealth += 4;
                            player.entityHealth += 4;
                            inputError = false;
                            Console.WriteLine("Your health has increased!");
                            break;
                        case "3":
                            player.entitySpeed += 1;
                            inputError = false;
                            Console.WriteLine("Your speed has increased!");
                            break;
                        default:
                            Console.WriteLine("Please input one of the options!");
                            throw new Exception();
                            break;
                    }
                }
                catch (Exception)
                {
                    playerInput = Console.ReadLine();
                }
            }

            player.PlayerXp -= player.XpNeededForLevelUp;
            if(player.XpNeededForLevelUp != player.XpNeededForLevelUp * (player.PlayerLevel * player.IncrementPerLevelUp))
            player.XpNeededForLevelUp = player.XpNeededForLevelUp * player.IncrementPerLevelUp;
            player.PlayerLevel++;
            Game.WriteWithDifferentColor("-------------LEVEL UP--------------", ConsoleColor.Green);

            Game.SaveGame(player);
        }

        public static void IdentifyNewWeapon(Player player, string weapon)
        {
            player.entityDamage += int.Parse(player.weapon.Substring(player.weapon.IndexOf("+") + 1, player.weapon.IndexOf("]") - player.weapon.IndexOf("+") - 1));
        }
        public void IdentifyOldWeapon(Player player, string weapon)
        {
            player.entityDamage -= int.Parse(player.weapon.Substring(player.weapon.IndexOf("+") + 1, player.weapon.IndexOf("]") - player.weapon.IndexOf("+") - 1));
        }

    }

}
