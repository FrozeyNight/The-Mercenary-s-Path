using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TurnBaseCombatv2
{
    internal class Enemy:Entity
    {
        public string[] inventory = new string[3];
        public Enemy(int entityHealth, int entityDamage, int entitySpeed, string entityName, string identifier, string[] inventory) : base(entityHealth, entityDamage, entitySpeed, entityName, identifier)
        {
            this.identifier = "e";
            entityMaxHealth = entityHealth;
            this.inventory = inventory;
        }

        public static int CalculateEnemyHealth(Enemy[] enemies)
        {
            int enemiesHealth = 0;
            foreach (Enemy enemy in enemies)
            {
                if(enemy != null && enemy.entityHealth > 0 && enemy.encounter != false)
                    enemiesHealth += enemy.entityHealth;
            }
            return enemiesHealth;
        }
        public Enemy DoAction(Enemy enemy, Player player)
        {
            if (enemy.entityHealth <= 0)
                return null;

            Random random = new Random();

            int enemyDecision = 2;
            double halfEntityHealth = enemy.entityMaxHealth / 2;
            double quarterEntityHealth = enemy.entityMaxHealth / 4;

            if (enemy.entityHealth <= halfEntityHealth && enemy.entityHealth > quarterEntityHealth)
            {
                enemyDecision = 3;
            }
            else if(enemy.entityHealth <= quarterEntityHealth)
            {
                enemyDecision = 4;
            }
            int enemyOutput = random.Next(1, enemyDecision);

            if (enemy.inventory == null && enemyOutput == 2)
                enemyOutput = 1;

            switch (enemyOutput)
            {
                case 1:
                    player.entityHealth -= enemy.entityDamage;
                    Game.WriteWithDifferentColor($"{enemy.entityName} attacked {player.entityName} who now has {player.entityHealth} health", ConsoleColor.Red);
                    break;

                case 2:
                    int inventoryLength = 0;
                    if(enemy.inventory != null)
                        inventoryLength = enemy.inventory.Length;

                    int randomNumber = random.Next(1, inventoryLength + 1);
                    string chooseItem = randomNumber.ToString();

                    if(enemy.inventory[randomNumber - 1] != null)
                    {
                        chooseItem = ChooseRandomItem(chooseItem);
                        IdentifyItem(enemy, chooseItem);
                        enemy.inventory[randomNumber - 1] = null;
                    }
                    else
                        Game.WriteWithDifferentColor($"In the heat of battle {enemy.entityName} tried to use an item, but couldn't find it!", ConsoleColor.Yellow);

                    break;
                case 3:
                    int calculatedEscapeOdds = (int)Math.Round((double)player.entitySpeed / enemy.entitySpeed, MidpointRounding.ToPositiveInfinity);
                    int enemyEscape = random.Next(1, calculatedEscapeOdds + 1);
                    if (enemyEscape == 1)
                    {
                        Game.WriteWithDifferentColor($"{enemy.entityName} has escaped the encounter!", ConsoleColor.DarkYellow);
                        enemy.encounter = false;
                        return enemy;
                    }
                    else
                    {   
                        Game.WriteWithDifferentColor($"{enemy.entityName} tries to escape, but fails", ConsoleColor.Yellow);
                    }
                    break;


            }
            return enemy;
        }

        public void IdentifyItem(Enemy enemy, string item)
        {
            switch (item)
            {
                case "Health Potion":
                    enemy.entityHealth += 4;
                    Game.WriteWithDifferentColor($"{enemy.entityName} drank a {item} and regenarated 4 health, it now has {enemy.entityHealth} health", ConsoleColor.DarkYellow);
                    break;
                case "Speed Potion":
                    enemy.entitySpeed += 2;
                    Game.WriteWithDifferentColor($"{enemy.entityName} drank a {item} and gained 2 speed, it now has {enemy.entitySpeed} speed", ConsoleColor.DarkYellow);
                    break;
                case "Strength Potion":
                    enemy.entityDamage += 2;
                    Game.WriteWithDifferentColor($"{enemy.entityName} drank a {item} and gained 2 damage, it now deals {enemy.entityDamage} damage", ConsoleColor.DarkYellow);
                    break;
            }
        }

        public string ChooseRandomItem(string item)
        {
            string chosenItem = "";
            switch (item)
            {
                case "1":
                    chosenItem = "Health Potion";
                    break;
                case "2":
                    chosenItem = "Speed Potion";
                    break;
                case "3":
                    chosenItem = "Strength Potion";
                    break;
            }
            return chosenItem;
        }

        public static Enemy[] CalculateEnemyInitiative(Entity[] entities)
        {
            Enemy[] enemies1 = new Enemy[2*entities.Length];

            int position = 0;
            foreach (Entity entity in entities)
            {
                if(entity is Enemy)
                {
                    enemies1[position] = (Enemy)entity;
                    position++;
                }
            }

            enemies1 = enemies1.Where(x => !Entity.IsNullOrEmpty(x)).ToArray();
            return enemies1;

        }

       

    }
}
