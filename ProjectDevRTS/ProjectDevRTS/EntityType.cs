using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDevRTS
{
    public class EntityType
    {
        String name;
        int maxHP;
        int moveSpeed;
        int attackSpeed;
        int attackDamage;
        int attackRange;
        float size;

        public EntityType(string name, int maxHP, int moveSpeed, int attackSpeed, int attackDamage, int attackRange, float size)
        {
            this.name = name;
            this.maxHP = maxHP;
            this.moveSpeed = moveSpeed;
            this.attackSpeed = attackSpeed;
            this.attackDamage = attackDamage;
            this.attackRange = attackRange;
            this.size = size;
        }

        public String getName()
        { return name; }
        public int getMaxHP() { return maxHP;}
        public int getMoveSpeed() {  return moveSpeed;}
        public int getAttackSpeed() {  return attackSpeed;}
        public int getAttackDamage() {  return attackDamage;}
        public int getAttackRange() {  return attackRange;}
        public float getSize() { return size;}  
    }
}
