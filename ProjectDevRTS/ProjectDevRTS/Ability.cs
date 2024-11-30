using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ProjectDevRTS
{
    public class Ability
    {
        bool enabled;
        float cooldown;
        int resourceCost;
        int energyCost;

        int usersTeam;
        float offset;
        string name;

        public Ability(int a, int cost, string type,float size)
        {
            usersTeam = a;
            resourceCost = cost;
            name = type;

            offset = size + 0.1f;//So you don't create it on top of the activating entity
        }

        public void useAbility(Game1 myGame, Vector3 position) //name is unit type
        {
            myGame.allEntities.Add(new Entity(myGame, name,new Vector3(position.X+ offset, position.Y, position.Z+ offset), usersTeam));
            myGame.Components.Add(myGame.allEntities[myGame.allEntities.Count-1]);
        }

        public int getResourceCost()
        {
            return resourceCost;
        }
    }



}
