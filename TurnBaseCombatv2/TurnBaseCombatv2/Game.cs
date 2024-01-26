using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TurnBaseCombatv2
{
    internal class Game
    {
        public static int roundCounter = 1;
        public static int encounterCounter = 1;
        public static int saveCounter = 1;
        public static int lineCounter = 0;
        public static Entity[] InitiateCombat(Player player, Enemy[] enemies) 
        {
            
            Entity[] entities = new Entity[2*enemies.Length + 1 + 4]; // the 4 is here so that if every enemy has the same speed and all of them drink the speed potion it won't be outside the bounds of the array
            entities[(int)player.entitySpeed] = player;

            entities = CalculateInintiative2(entities, enemies);
            //i need to nullify the enemy that escapes the encoutner where i do the other were they are killed i think
            entities = entities.Where(x => !Entity.IsNullOrEmpty(x)).ToArray();
            Array.Reverse(entities);

            enemies = Enemy.CalculateEnemyInitiative(entities);

            Console.WriteLine($"--------------Round {roundCounter}--------------");
            roundCounter++;
            entities = StartCombat(entities, player, enemies);

            return entities;

        }

        public static Enemy[] DeleteAllDead(Enemy[] enemies)
        {
            enemies = enemies.Where(x => !Entity.IsNullOrEmpty(x)).ToArray();

            return enemies;
        }

        public static string[] GenerateInventory(string[] items, int numberOfItems)
        {
            string[] genratedItems = new string[numberOfItems];
            Random random = new Random();
            for (int i = 0; i < numberOfItems; i++)
            {
                genratedItems[i] = items[random.Next(0, items.Length)];
            }

            return genratedItems;

        }

        public static int indexOfKilledEnemy(Enemy[] enemies)
        {
            int i = 0;
            foreach (Enemy enemy in enemies)
            {
                if (enemy == null || enemy.entityHealth <= 0)
                    return i;
                i++;
            }
            return -1;
        }

        public static Entity[] CalculateInintiative2(Entity[] entities, Enemy[] enemies)
        {
            //int iteration = 0;
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && entities[(int)enemy.entitySpeed] == null)
                    entities[(int)enemy.entitySpeed] = enemy;
                else if (enemy != null && entities[(int)enemy.entitySpeed] != null)
                {
                    if (entities[(int)enemy.entitySpeed + 1] == null)
                        entities[(int)enemy.entitySpeed + 1] = enemy;
                    else
                    {
                        int highestIndexOfEntity = GetFastestEnemy(entities);// + iteration; // it returned 6 when the highest index is actually 7
                        foreach (Entity entity in entities)
                        {
                            if (highestIndexOfEntity > -1 && highestIndexOfEntity >= enemy.entitySpeed)
                            {
                                entities[highestIndexOfEntity + 1] = entities[highestIndexOfEntity];
                                highestIndexOfEntity--;
                            }

                        }
                        entities[(int)enemy.entitySpeed] = enemy;
                        //iteration++;
                    }
                }

            }

            return entities;
        }

        public static Entity[] StartCombat(Entity[] entities, Player player, Enemy[] enemies)
        {
            int currentEnemy = 0;
            int originalLength = 0;
            int killedEnemyIndex = 0;
            foreach (Entity entity in entities)
            {
                if (!player.encounter)
                    break;
                else if (enemies.Length == 0)
                    break;
                originalLength = enemies.Length;

                if (entity.identifier.Equals("p"))
                {
                    if (player.entityHealth <= 0)
                        break;

                    enemies = Game.DeleteAllDead(enemies);
                    player = player.DoAction(player, enemies.Length, enemies);
                    killedEnemyIndex = Game.indexOfKilledEnemy(enemies);
                    enemies = Game.DeleteAllDead(enemies);
                    if (originalLength != enemies.Length && currentEnemy > 0)
                    {
                        if (enemies.Length == 1 && killedEnemyIndex < currentEnemy)
                        {
                            currentEnemy--;
                        }
                        else
                        {
                            if (killedEnemyIndex != currentEnemy && killedEnemyIndex != -1 && killedEnemyIndex < currentEnemy)
                                currentEnemy--;
                        }
                    }
                }
                else if (entity.identifier.Equals("e"))
                {
                    enemies = Game.DeleteAllDead(enemies);

                    if (currentEnemy < enemies.Length)
                    {
                        enemies[currentEnemy] = enemies[currentEnemy].DoAction(enemies[currentEnemy], player);
                        /*
                        if (!enemies[currentEnemy].encounter) 
                        {

                            enemies[currentEnemy] = null;
                            currentEnemy--;
                        }
                        */
                        killedEnemyIndex = Game.indexOfKilledEnemy(enemies);


                        if (enemies[currentEnemy] == null && currentEnemy > 0 || originalLength != enemies.Length && currentEnemy > 0)
                        {
                            if (enemies.Length == 1)
                            {
                                currentEnemy--;
                            }
                            else
                            {
                                if (killedEnemyIndex != currentEnemy + 1 && killedEnemyIndex != -1 && killedEnemyIndex < currentEnemy)
                                    currentEnemy--;
                            }
                        }
                        /*
                        if (!enemies[currentEnemy].encounter)
                        {
                            enemies[currentEnemy] = null;
                            currentEnemy--;
                        }
                        //if (!enemies[currentEnemy].encounter)
                        //currentEnemy--;
                        */
                    }

                    if (currentEnemy < enemies.Length && enemies[currentEnemy] != null && !enemies[currentEnemy].encounter && currentEnemy != enemies.Length && currentEnemy >= 0)
                    {
                        currentEnemy--;
                        if(currentEnemy == -1)
                            currentEnemy++;
                    }

                    bool enemyEscaped = true;
                    if (currentEnemy < enemies.Length)
                        enemyEscaped = enemies[currentEnemy].encounter;

                    enemies = Game.DeleteAllDead(enemies);

                    if (currentEnemy > -1 && currentEnemy < enemies.Length && enemies[currentEnemy] != null && enemyEscaped != false)
                        currentEnemy++;
                }
            }
            return entities;
        }

        public static void ResultOfCombat(Player player, Enemy[] enemies, int xpGainedFromCombat)
        {
            enemies = Game.DeleteAllDead(enemies);
            if (player.entityHealth <= 0)
            {
                Console.WriteLine("You Died!");
                Console.WriteLine("-----------END OF COMBAT-----------");
                string deathMessagePath = Path.GetFullPath("TextData/DeathMessage.txt");
                string[] lines = File.ReadAllLines(deathMessagePath);
                WriteLineWordWrap(lines[0]);
                Console.WriteLine("1. Go back (load last save file)");
                Console.WriteLine("2. nah bro im out");
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
                        //string textFilePath = "C:\\Users\\FrozeyNight\\Documents\\CodingThings\\TurnBaseCombatv2\\TurnBaseCombatv2\\bin\\Debug\\net6.0\\TurnBaseCombatv2.exe";
                        string textFilePath = Path.GetFullPath("TurnBaseCombatv2.exe");
                        Console.Clear();
                        Process.Start(textFilePath);
                        Environment.Exit(0);
                        break;
                    case 2:
                        Environment.Exit(0);
                        break;
                }
            }
            else if (enemies.Length == 0)
            {
                player.PlayerXp += xpGainedFromCombat;
                Console.WriteLine("You won the battle!");
            }
            else
            {
                player.PlayerXp += xpGainedFromCombat/2;
                Console.WriteLine("You've escaped the battle!");
                player.encounter = true;
            }
            Console.WriteLine("-----------END OF COMBAT-----------");
            roundCounter = 1;
            player.entitySpeed -= player.bonusSpeed;
            player.entityDamage -= player.bonusDamage;

        }

        public static (Player player, int XpNeededForLevelUp) ReadDataFromTextFile(string textFile)
        {

            Player player = new Player(0, 0, 0, "null", "p");
            // gotta get the saveCounter before we call loadgame since it has to use the correct saveCounter
            //saveCounter = int.Parse(lines[14]);

            string[] lines = File.ReadAllLines(textFile);

            //saveCounter = int.Parse(lines[7]);
            GetSaveCounter();
            LoadGame(player);

            int amountOfEnemies = 0;
            foreach (string line in lines)
            {
                if (line.Contains("!"))
                    amountOfEnemies++;
            }


            int amountOfItems = 0;
            foreach (string line in lines)
            {
                if (line.Contains("^"))
                    amountOfItems = line.Count(x => x == ',');
            }

            string[] items = new string[amountOfItems + 1];

            int enemyConuter = 0;
            int itemCounter = 0;
            int XpNeededForLevelUp = 1;
            foreach (string line in lines)
            {
                string line1 = line;
                /*
                if (line.Contains("&"))
                {

                    int end = line1.IndexOf(",") - 1;

                    int health = int.Parse(line1.Substring(1, end));
                    line1 = line1.Substring(end + 2);
                    end = line1.IndexOf(",") - 1;

                    int damage = int.Parse(line1.Substring(1, end));
                    line1 = line1.Substring(end + 2);
                    end = line1.IndexOf(",") - 1;

                    int speed = int.Parse(line1.Substring(1, end));
                    line1 = line1.Substring(end + 2);
                    end = line1.IndexOf("}") - 1;

                    string name = line1.Substring(1, end);

                    Player player1 = new Player(health, damage, speed, name, "p");
                    player = player1;
                }
                */
                if (line.Contains("^"))
                {
                    int end = line1.IndexOf(",") - 1;

                    for (int i = 0; i <= amountOfItems; i++)
                    {
                        string item = line1.Substring(1, end);
                        line1 = line1.Substring(end + 2);
                        end = line1.IndexOf(",") - 1;
                        if(amountOfItems == i + 1)
                            end = line1.IndexOf("}") - 1;

                        items[itemCounter] = item;
                        itemCounter++;
                    }

                }
                /*
                else if (line.Contains("#"))
                {
                    int end = line1.IndexOf(",") - 1;

                    int inventoryIndex = int.Parse(line1.Substring(1, end));
                    line1 = line1.Substring(end + 2);
                    end = line1.IndexOf("}") - 1;

                    string itemName = line1.Substring(1, end);

                    player.inventory[inventoryIndex] = itemName;
                }
                */
                else if (line1.Contains("$") && !line1.Contains("*")) 
                {
                    int end = line1.IndexOf("$");

                    XpNeededForLevelUp = int.Parse(line1.Substring(0, end));
                }
                else if (line1.Contains('*'))
                {
                    int end = line1.IndexOf("*");

                    XpNeededForLevelUp = int.Parse(line1.Substring(0, end));
                    line1 = line1.Substring(end + 1);

                    end = line1.IndexOf("$");
                    player.IncrementPerLevelUp = int.Parse(line1.Substring(0, end));
                }
                
            }
            

            return (player, XpNeededForLevelUp);
        }

        public static void WriteWithDifferentColor(string message, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /*
        public static int GetFastestEnemy(Enemy[] enemies)
        {
            int highestSpeed = 0;
            foreach (Enemy enemy in enemies)
            {
                if(Array.IndexOf(enemies, enemy) > highestSpeed)
                    highestSpeed = Array.IndexOf(enemies, enemy) + 1;
            }
            return highestSpeed;
        }

        */

        public static int GetFastestEnemy(Entity[] entities)
        {
            int highestSpeed = 0;
            foreach (Entity entity in entities)
            {
                if (Array.IndexOf(entities, entity) > highestSpeed)
                    highestSpeed = Array.IndexOf(entities, entity) + 1;
            }
            return highestSpeed;
        }

        public static (Enemy[] enemies, int amountOfExp) GetEncounter(string textFile, Player player)
        {
            string[] lines = File.ReadAllLines(textFile);

            int amountOfEnemies = 0;
            foreach (string line in lines)
            {
                if (line.Contains("!"))
                    amountOfEnemies++;
            }

            Enemy[] enemies = new Enemy[amountOfEnemies];

            int amountOfItems = 0;
            foreach (string line in lines)
            {
                if (line.Contains("^"))
                    amountOfItems = line.Count(x => x == ',');
            }

            string[] items = new string[amountOfItems + 1];


            int enemyConuter = 0;
            int itemCounter = 0;
            int amountOfExp = 0;
            foreach (string line in lines)
            {
                string line1 = line;
                if (line.Contains("!"))
                {
                    int end = line1.IndexOf(",") - 1;

                    int health = int.Parse(line1.Substring(1, end));
                    line1 = line1.Substring(end + 2);
                    end = line1.IndexOf(",") - 1;

                    int damage = int.Parse(line1.Substring(1, end));
                    line1 = line1.Substring(end + 2);
                    end = line1.IndexOf(",") - 1;

                    int speed = int.Parse(line1.Substring(1, end));
                    line1 = line1.Substring(end + 2);
                    end = line1.IndexOf(",") - 1;

                    string name = line1.Substring(1, end);
                    line1 = line1.Substring(end + 2);
                    end = line1.IndexOf("}") - 1;

                    int amountOfItemsToGenerate = int.Parse(line1.Substring(1, end));

                    Enemy enemy = new Enemy(health, damage, speed, name, "e", GenerateInventory(items, amountOfItems));

                    enemies[enemyConuter] = enemy;
                    enemyConuter++;
                }
                else if (line.Contains("^"))
                {
                    int end = line1.IndexOf(",") - 1;

                    for (int i = 0; i <= amountOfItems; i++)
                    {
                        string item = line1.Substring(1, end);
                        line1 = line1.Substring(end + 2);
                        end = line1.IndexOf(",") - 1;
                        if (amountOfItems == i + 1)
                            end = line1.IndexOf("}") - 1;

                        items[itemCounter] = item;
                        itemCounter++;
                    }

                }
                else if(line.Contains("$")) 
                {

                    switch (line) 
                    {
                        case "yes$":
                            amountOfExp = player.XpNeededForLevelUp;
                            break;
                        case "no$":
                            amountOfExp += 0;
                            break;
                    }

                    if (!line.Contains(","))
                    {
                        int end = line1.IndexOf("$");
                        amountOfExp = int.Parse(line1.Substring(0, 1));
                    }
                    /*
                    else if (line.Contains(","))
                    {
                        int end = line1.IndexOf(",");
                        int amountOfExpNeededToLevelUp = int.Parse(line1.Substring(0, end));
                        line1 = line1.Substring(end + 1);
                        player.XpNeededForLevelUp = amountOfExpNeededToLevelUp;

                        end = line1.IndexOf("$");
                        int amountOfExpGivenByThisEncounter = int.Parse(line1.Substring(0, end));
                        amountOfExp = amountOfExpGivenByThisEncounter;
                    }
                    */

                }

            }

            return (enemies, amountOfExp);
        }

        public static void BeginCombat(Player player, Enemy[] enemies, int xpGainedFromCombat)
        {
            Console.WriteLine("----------START OF COMBAT----------");
            while (player.entityHealth > 0 && Enemy.CalculateEnemyHealth(enemies) > 0 && player.encounter)
            {

                Game.InitiateCombat(player, enemies);
                if (!player.encounter || player.entityHealth <= 0)
                    break;

            }

            Game.ResultOfCombat(player, enemies, xpGainedFromCombat);
        }

        public static void startEncounter(Player player) 
        {
            string textFilePath = "";
            string path = "Encounters/Encounter" + encounterCounter + ".txt";
            Console.WriteLine();
            textFilePath = Path.GetFullPath(path);
            if (!File.Exists(textFilePath))
            {
                Console.WriteLine("You have finished the game!");
                Console.ReadKey();
                Environment.Exit(1);
            }

            (Enemy[] enemies, int playerXp) = Game.GetEncounter(textFilePath, player);
            //player.IdentifyNewWeapon(player, player.weapon);
            //encounterCounter++;
            SaveGame(player);
            //encounterCounter++;
            Game.BeginCombat(player, enemies, playerXp);
            encounterCounter++;
            SaveGame(player);

            if (player.XpNeededForLevelUp <= player.PlayerXp)
                player.LevelUp(player);
        }

        public static void startEncounter(Player player, string encounterName)
        {
            string textFilePath = Path.GetFullPath($"Encounters/{encounterName}");
            if (!File.Exists(textFilePath))
            {
                Console.WriteLine("You have finished the game!");
                Console.ReadKey();
                Environment.Exit(1);
            }

            (Enemy[] enemies, int playerXp) = Game.GetEncounter(textFilePath, player);
            //player.IdentifyNewWeapon(player, player.weapon);
            //encounterCounter++;
            SaveGame(player);
            //encounterCounter++;
            Game.BeginCombat(player, enemies, playerXp);
            encounterCounter++;
            lineCounter++;
            SaveGame(player);

            if (player.XpNeededForLevelUp <= player.PlayerXp)
                player.LevelUp(player);
        }

        public static Player getGameData()
        {
            string path = "GameData.txt";
            string textFilePath = Path.GetFullPath(path);
            (Player player, int XpNeededForLevelUp) = Game.ReadDataFromTextFile(textFilePath);

            LoadGame(player);

            if (player.PlayerLevel != 0)
                player.XpNeededForLevelUp = XpNeededForLevelUp * (player.PlayerLevel * player.IncrementPerLevelUp);
            else
                player.XpNeededForLevelUp = XpNeededForLevelUp;

            if (player.XpNeededForLevelUp <= player.PlayerXp)
                player.LevelUp(player);

            return player;
        }

        public static void SaveGame(Player player)
        {
            string textFilePath = "";
            string path = "SaveData";
            textFilePath = Path.GetFullPath(path);

            if (!File.Exists($"{textFilePath}\\Save{saveCounter - 1}.txt"))
                textFilePath = Path.GetFullPath("SaveData/Save.txt");
            else
                textFilePath = $"{textFilePath}\\Save{saveCounter - 1}.txt";

            if (!CheckSaveSimulatiry(player, textFilePath))
            {

                StreamWriter writer = new StreamWriter(Path.Combine(path, "Save" + saveCounter + ".txt"));

                writer.WriteLine("player stats: (maximum health, health, damage, speed, name, playerXP, playerLevel, weapon, encounter, save, lineCounter, decisionPath, position in inventory (star) name of the item (how much damage does it add))");
                writer.WriteLine(player.entityMaxHealth);
                writer.WriteLine(player.entityHealth);
                writer.WriteLine(player.entityDamage);
                writer.WriteLine(player.entitySpeed);
                writer.WriteLine(player.entityName);
                writer.WriteLine(player.PlayerXp);
                writer.WriteLine(player.PlayerLevel);
                writer.WriteLine(player.weapon);
                writer.WriteLine(encounterCounter);

                saveCounter++;
                //WriteToGameData($"{saveCounter}", Path.GetFullPath("GameData.txt"), 7);
                writer.WriteLine(saveCounter);
                writer.WriteLine(lineCounter);
                writer.WriteLine(player.decisionPath);
                int itemPosition = 0;
                foreach (string item in player.inventory)
                {
                    if (item != null)
                        writer.WriteLine(itemPosition + "*" + item);
                    itemPosition++;
                }
                writer.Close();
            }
        }

        public static void LoadGame(Player player)
        {
            string textFile = $"SaveData/Save{saveCounter - 1}.txt";
            textFile = Path.GetFullPath(textFile);

            string[] lines = null;
            if (File.Exists(textFile))
                lines = File.ReadAllLines(textFile);
            else
            {
                textFile = Path.GetFullPath("SaveData/Save.txt");
                lines = File.ReadAllLines(textFile);
            }

            player.entityMaxHealth = Convert.ToInt32(lines[1]);
            player.entityHealth = Convert.ToInt32(lines[2]);
            player.entityDamage = Convert.ToInt32(lines[3]);
            player.entitySpeed = Convert.ToInt32(lines[4]);
            player.entityName = lines[5];
            player.PlayerXp = Convert.ToInt32(lines[6]);
            player.PlayerLevel = Convert.ToInt32(lines[7]);
            player.weapon = lines[8];
            encounterCounter = Convert.ToInt32(lines[9]);
            saveCounter = Convert.ToInt32(lines[10]);
            lineCounter = Convert.ToInt32(lines[11]);
            player.decisionPath = Convert.ToInt32(lines[12]);
            foreach (string line in lines)
            {
                if (line.Contains('*'))
                {
                    object[] itemData = line.Split('*');
                    player.inventory[int.Parse((string)itemData[0])] = (string)itemData[1];
                }
            }

            if (player.PlayerLevel != 0 && player.XpNeededForLevelUp != player.XpNeededForLevelUp * (player.PlayerLevel * player.IncrementPerLevelUp))
                player.XpNeededForLevelUp = player.XpNeededForLevelUp * (player.PlayerLevel * player.IncrementPerLevelUp);
        }

        public static void WriteToGameData(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit] = newText;
            File.WriteAllLines(fileName, arrLine);
        }

        public static bool CheckSaveSimulatiry(Player player,string fileName)
        {
            fileName = Path.GetFullPath(fileName);

            string[] lines = File.ReadAllLines(fileName);

            object[] inventory = new object[10];
            foreach (string line in lines)
            {
                if (line.Contains('*'))
                {
                    object[] itemData = line.Split('*');
                    inventory[int.Parse((string)itemData[0])] = (string)itemData[1];
                }
            }

            if (player.entityMaxHealth != Convert.ToInt32(lines[1]) || player.entityHealth != Convert.ToInt32(lines[2]) || player.entityDamage != Convert.ToInt32(lines[3]) || player.entitySpeed != Convert.ToInt32(lines[4])
                || player.entityName != lines[5] || player.PlayerXp != Convert.ToInt32(lines[6]) || player.PlayerLevel != Convert.ToInt32(lines[7]) || player.weapon != lines[8] || encounterCounter != Convert.ToInt32(lines[9])
                || !Enumerable.SequenceEqual<object>(player.inventory, inventory)
                )
                return false;
            else
                return true;
        }

        public static void GetSaveCounter()
        {
            string saveFilePath = Path.GetFullPath("SaveData");
            int counter = 0;
            for (counter = 1; counter < int.MaxValue; counter++)
            {
                if (!File.Exists($"{saveFilePath}\\Save{counter}.txt"))
                {
                    saveCounter = counter;
                    break;
                }
            }
        }

        public static void TextData(Player player)
        {
            string textFilePath = "";
            //if (File.Exists(Path.GetFullPath($"TextData/Path{player.decisionPath}.txt")))
            //    textFilePath = Path.GetFullPath($"TextData/Path{player.decisionPath}.txt");
            //else
                textFilePath = Path.GetFullPath("TextData/Text2.txt");
            string[] lines = File.ReadAllLines(textFilePath);

            for (int i = lineCounter; i <= lines.Length - 1; i++)
            {
                string line1 = lines[lineCounter]; // i != linecounter if you change the lineCounter during the loop the i won't change with it
                if (lines[lineCounter].Contains('>') && lines[lineCounter].Contains($"Path{player.decisionPath}") || (lines[lineCounter].Contains('>') && !lines[lineCounter].Contains("Path")))
                {
                    if (lines[lineCounter].Contains($"Path{player.decisionPath}"))
                        line1 = lines[lineCounter].Substring(lines[lineCounter].IndexOf($"{player.decisionPath}") + 2);

                    Game.startEncounter(player, line1.Substring(line1.IndexOf('>') + 1, line1.IndexOf('<') - 1) + ".txt");
                    if (lines[lineCounter].Contains($"Path{player.decisionPath}"))
                        WriteLineWordWrap(lines[lineCounter].Substring(lines[lineCounter].IndexOf($"{player.decisionPath}") + 2));
                    else if (!lines[lineCounter].Contains($"Path{player.decisionPath}") && !lines[lineCounter].Contains($"Path"))
                        WriteLineWordWrap(lines[lineCounter]);
                    i++;
                }
                else if (lines[lineCounter].Contains("decision?") && lines[lineCounter].Contains($"Path{player.decisionPath}") || lines[lineCounter].Contains("decision?") && !lines[lineCounter].Contains("Path"))
                {
                    line1 = line1.Substring(line1.IndexOf('?') + 2);
                    string option1Name = line1.Substring(0, line1.IndexOf('('));
                    line1 = line1.Substring(line1.IndexOf(')') - 1);

                    int pathNumber1 = int.Parse(line1.Substring(0, 1));
                    line1 = line1.Substring(line1.IndexOf('/') + 1);

                    string option2Name = line1.Substring(0, line1.IndexOf('['));
                    line1 = line1.Substring(line1.IndexOf(']') - 1, 1);

                    int pathNumber2 = int.Parse(line1.Substring(0, 1));

                    Console.WriteLine($"1. {option1Name}");
                    Console.WriteLine($"2. {option2Name}");

                    string playerInput = Console.ReadLine();
                    bool inputError = true;
                    int playerOutput = 0;
                    while (inputError)
                    {
                        try
                        {
                            playerOutput = int.Parse(playerInput);
                            if (playerOutput > 0 && playerOutput < 3)
                                inputError = false;
                            else
                                throw new Exception();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Please input one of the options!");
                            playerInput = Console.ReadLine();
                        }
                    }
                    if (playerInput.Equals("1"))
                        player.decisionPath = pathNumber1;
                    else if (playerInput.Equals("2"))
                        player.decisionPath = pathNumber2;

                }
                else if (lines[lineCounter].Contains("give%") && lines[lineCounter].Contains($"Path{player.decisionPath}") || lines[lineCounter].Contains("give%") && !lines[lineCounter].Contains("Path"))
                {
                    if (lines[lineCounter].Contains($"Path{player.decisionPath}"))
                        line1 = lines[lineCounter].Substring(lines[lineCounter].IndexOf($"{player.decisionPath}") + 2);

                    line1 = line1.Substring(line1.IndexOf('%') + 2);

                    bool givingSuccessful = false;
                    for (int counter = 0; counter < player.inventory.Length; counter++)
                    {
                        if (player.inventory[counter] == null)
                        {
                            player.inventory[counter] = line1;
                            Console.WriteLine($"{line1} is added to your inventory");
                            givingSuccessful = true;
                            break;
                        }
                    }

                    if (!givingSuccessful) 
                    {
                        Console.WriteLine("You don't have the inventory space to fit a new item. Which item would you like to replace?");
                        int counter = 1;
                        foreach (string item in player.inventory)
                        {
                            if (item != null)
                                Console.WriteLine($"{counter}. {item}");
                            else
                                Console.WriteLine($"{counter}. empty slot");
                            counter++;
                        }
                        Console.WriteLine(player.inventory.Length + 1 + ". I don't want to replace any item");
                        string playerInput = Console.ReadLine();
                        int playerOutput = -1;
                        while (playerOutput < 0 || playerOutput > 11)
                        {
                            try
                            {
                                playerOutput = int.Parse(playerInput);
                                if (playerOutput < 0 || playerOutput > 11)
                                    throw new Exception();

                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Please choose one of the items in your inventory");
                                playerInput = Console.ReadLine();
                            }

                        }
                        if(playerOutput >= 0 && playerOutput < 11)
                        {
                            WriteWithDifferentColor($"You replace {player.inventory[playerOutput - 1]} with {line1}", ConsoleColor.Green);
                            player.inventory[playerOutput - 1] = line1;
                        }
                        else if(playerOutput == 11)
                            WriteWithDifferentColor("You decide not to take the item.", ConsoleColor.DarkYellow);

                    }

                }
                else if (lines[lineCounter].Contains($"Path{player.decisionPath}") && lines[lineCounter] != string.Empty || !lines[lineCounter].Contains("Path") && lines[lineCounter] != string.Empty)
                {
                    if (lines[lineCounter].Contains($"Path{player.decisionPath}"))
                    {
                        
                        line1 = lines[lineCounter].Substring(lines[lineCounter].IndexOf($"{player.decisionPath}") + 2);
                        if (line1.Substring(0, 1).Contains(" "))
                        {
                            line1 = line1.Substring(0);
                            Console.WriteLine();
                            WriteLineWordWrap(line1.Substring(line1.IndexOf($"{player.decisionPath}") + 2));
                        }
                        else
                            WriteLineWordWrap(line1);

                    }
                    else
                        WriteLineWordWrap(lines[lineCounter]);
                }
                else if (lines[lineCounter].Substring(0, 0).Contains(" "))
                    Console.WriteLine();
                lineCounter++;
            }
            Console.WriteLine("You have finished the game!");
            Console.ReadKey();

        }

        public static void WriteLineWordWrap(string paragraph, int tabSize = 8)
        {
            string[] lines = paragraph
                .Replace("\t", new String(' ', tabSize))
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string process = lines[i];
                List<String> wrapped = new List<string>();

                while (process.Length > Console.WindowWidth)
                {
                    int wrapAt = process.LastIndexOf(' ', Math.Min(Console.WindowWidth - 1, process.Length));
                    if (wrapAt <= 0) break;

                    wrapped.Add(process.Substring(0, wrapAt));
                    process = process.Remove(0, wrapAt + 1);
                }

                foreach (string wrap in wrapped)
                {
                    Console.WriteLine(wrap);
                }

                Console.WriteLine(process);
            }
        }

    }
    
}
