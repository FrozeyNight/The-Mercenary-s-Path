using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurnBaseCombatv2
{
    internal class Entity
    {
        public int entityMaxHealth;
        public int entityHealth;
        public int entityDamage;
        public int entitySpeed;
        public string entityName;
        public string identifier;
        public bool encounter = true;

        public Entity(int entityHealth, int entityDamage, int entitySpeed, string entityName, string identifier)
        {
            this.entityHealth = entityHealth;
            this.entityDamage = entityDamage;
            this.entitySpeed = entitySpeed;
            this.entityName = entityName;
            this.identifier = identifier;
            entityMaxHealth = entityHealth;
        }

        public static bool IsNullOrEmpty(Entity entity) 
        {
            if(entity == null || entity.entityHealth <= 0 || entity.encounter == false)
            {
                return true;
            }
            return false;
        }
    }
}
