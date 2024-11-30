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
    public class Entity : Microsoft.Xna.Framework.DrawableGameComponent
    {

        Game1 myGame;
        int maxHP;
        int currentHP;
        int moveSpeed;
        float moveSpeedMulti = 0.25f;
        int attackSpeed;
        int attackDamage;
        int attackRange;
        String name;
        Vector3 position;
        float size;
        Entity target;
        Entity subTarget;

        float carriedResource = 0; //Used for builders to carry resource from node to base
        int carryCapacity = 50;

        int searchRange = 50;
        float closestEnemyDistance = 0;

        BoundingBox box;

        int playerNumber; //Positive for team 1, Negative for team 2, 0 for neutral

        //Needs a model
        VertexPositionNormalTexture[] pnverts;
        Vector3[] pts;
        Vector2[] tcs;
        int gDim;
        Vector3 direction;
        Matrix worldTrans = Matrix.Identity;
        Matrix worldRot = Matrix.Identity;
        Matrix scale = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix proj = Matrix.Identity;
        Texture2D texture;
        Effect PointDiffSPecTextureEffect;
        int nlat, nlong;
        float radius;
        VertexPositionColor[] verts;
        float[] lts;
        BasicEffect ceffect;
        
        Vector3 moveLocation;

        Ability[] abilities;

        bool moveSet = false;


        bool selected = false; //Testing flag, used to disable AI while selected
        public Entity(Game game, String type, Vector3 Position,int team) : base(game)
        {
            this.DrawOrder = 1;
            name = type;
            /*
            maxHP = 100;
            currentHP = 100;
            moveSpeed = 1;
            attackSpeed = 2;
            attackDamage = 10;
            size = 1;
            attackRange = 5;
            */

            myGame = (Game1)game;
            int correctType = -1;
            for (int i = 0; i < myGame.entityTypes.Length; i++)
            {
                if(type == myGame.entityTypes[i].getName())
                { correctType = i; }
            }
            if (correctType!=-1)
            {
                maxHP = myGame.entityTypes[correctType].getMaxHP();
                currentHP = maxHP;
                moveSpeed = myGame.entityTypes[correctType].getMoveSpeed();
                attackSpeed = myGame.entityTypes[correctType].getAttackSpeed();
                attackDamage = myGame.entityTypes[correctType].getAttackDamage();
                attackRange = myGame.entityTypes[correctType] .getAttackRange();
                size = myGame.entityTypes[correctType].getSize();
            }

            playerNumber = team;


            position = Position;
            moveLocation = position;


            //BoundingBox 
            Vector3 bbBottom = new Vector3(-size/2, -size / 2, -size / 2);
            Vector3 bbTop = new Vector3(size / 2, size / 2, size / 2);
            box = new BoundingBox(bbBottom, bbTop);
            
            gDim = myGame.getgdim();

            if (name == "Builder")
            {
                abilities = new Ability[1];
                abilities[0] = new Ability(playerNumber, 20,"Ranged",size);
            }
            if(name == "PlayerBase")
            {
                abilities = new Ability[2];
                abilities[0] = new Ability(playerNumber, 20, "Builder",size);
                abilities[1] = new Ability(playerNumber, 20, "Ranged",size);
            }

            //Temp Visuals
            {

                myGame = (Game1)game;
                pnverts = new VertexPositionNormalTexture[36];

                int iv = 0;
                pts = new Vector3[8];
                Vector3[] nms = new Vector3[8];
                tcs = new Vector2[8];
                direction = new Vector3(0, 0, 1);

                gDim = myGame.getgdim();
                scale = Matrix.CreateScale(0.1f);

                pts[0].X = -1.0f;
                pts[0].Y = -1.0f;
                pts[0].Z = -1.0f;
                pts[1].X = -1.0f;
                pts[1].Y = 1.0f;
                pts[1].Z = -1.0f;
                pts[2].X = -1.0f;
                pts[2].Y = -1.0f;
                pts[2].Z = 1.0f;
                pts[3].X = -1.0f;
                pts[3].Y = 1.0f;
                pts[3].Z = 1.0f;
                pts[4].X = 1.0f;
                pts[4].Y = -1.0f;
                pts[4].Z = -1.0f;
                pts[5].X = 1.0f;
                pts[5].Y = 1.0f;
                pts[5].Z = -1.0f;
                pts[6].X = 1.0f;
                pts[6].Y = -1.0f;
                pts[6].Z = 1.0f;
                pts[7].X = 1.0f;
                pts[7].Y = 1.0f;
                pts[7].Z = 1.0f;

                for (int i = 0; i < 8; i++)
                {
                    pts[i].Z *= size/1.75f;
                    pts[i].X *= size / 1.75f;
                }

                nms[0].X = 0.0f;
                nms[0].Y = 1.0f;
                nms[0].Z = 0.0f;
                nms[1].X = 0.0f;
                nms[1].Y = -1.0f;
                nms[1].Z = 0.0f;
                nms[2].X = 1.0f;
                nms[2].Y = 0.0f;
                nms[2].Z = 0.0f;
                nms[3].X = -1.0f;
                nms[3].Y = 0.0f;
                nms[3].Z = 0.0f;
                nms[4].X = 0.0f;
                nms[4].Y = 0.0f;
                nms[4].Z = 1.0f;
                nms[5].X = 0.0f;
                nms[5].Y = 0.0f;
                nms[5].Z = -1.0f;

                tcs[0].X = 0.0f;
                tcs[0].Y = 0.0f;
                tcs[1].X = 0.25f;
                tcs[1].Y = 0.0f;
                tcs[2].X = 0.0f;
                tcs[2].Y = 0.5f;
                tcs[3].X = 0.25f;
                tcs[3].Y = 0.5f;

                pnverts[iv++] = new VertexPositionNormalTexture(pts[0], nms[3], tcs[1]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[1], nms[3], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[2], nms[3], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[2], nms[3], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[1], nms[3], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[3], nms[3], tcs[2]);

                pnverts[iv++] = new VertexPositionNormalTexture(pts[4], nms[1], tcs[1]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[0], nms[1], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[6], nms[1], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[6], nms[1], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[0], nms[1], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[2], nms[1], tcs[2]);

                pnverts[iv++] = new VertexPositionNormalTexture(pts[7], nms[0], tcs[1]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[3], nms[0], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[5], nms[0], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[5], nms[0], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[3], nms[0], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[1], nms[0], tcs[2]);

                pnverts[iv++] = new VertexPositionNormalTexture(pts[4], nms[5], tcs[1]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[0], nms[5], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[5], nms[5], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[5], nms[5], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[0], nms[5], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[1], nms[5], tcs[2]);

                pnverts[iv++] = new VertexPositionNormalTexture(pts[6], nms[4], tcs[1]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[7], nms[4], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[2], nms[4], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[2], nms[4], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[7], nms[4], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[3], nms[4], tcs[2]);

                pnverts[iv++] = new VertexPositionNormalTexture(pts[4], nms[2], tcs[1]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[6], nms[2], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[5], nms[2], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[5], nms[2], tcs[0]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[6], nms[2], tcs[3]);
                pnverts[iv++] = new VertexPositionNormalTexture(pts[7], nms[2], tcs[2]);



            }


            if (playerNumber >= 0) { closestEnemyDistance = 0; }
            else { closestEnemyDistance = 9999; }

        }


        public override void Initialize()
        {
            ceffect = new BasicEffect(GraphicsDevice);
            ceffect.VertexColorEnabled = true;
            ceffect.World = worldRot * worldTrans;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            if(playerNumber < 0) { texture = Game.Content.Load<Texture2D>(@"Wrinkled_PaperRed"); }
            if (playerNumber == 0) { texture = Game.Content.Load<Texture2D>(@"Wrinkled_PaperYellow"); }
            if (playerNumber > 0) { texture = Game.Content.Load<Texture2D>(@"Wrinkled_PaperGreen"); }

            PointDiffSPecTextureEffect = Game.Content.Load<Effect>("DiffuseTexture");
        }

        public override void Update(GameTime gameTime)
        {
            runAI(gameTime); //Contains targetting checks

            position.Y = myGame.getHTSAt((int)position.X+gDim/2, (int)position.Z + gDim / 2)+2;

            //Update bounding box
            Vector3 bbBottom = new Vector3(position.X- size / 2, position.Y -size / 2, position.Z -size / 2);
            Vector3 bbTop = new Vector3(position.X + size / 2, position.Y + size / 2, position.Z + size / 2);
            box = new BoundingBox(bbBottom, bbTop);

            SetTranslation(position);
            base.Update(gameTime);
        }

        public void runAI(GameTime gameTime)
        {
            if (playerNumber >= 0) { closestEnemyDistance = 0; }
            else { closestEnemyDistance = 9999; }
            if (name != "Builder")
            {
                for (int i = 0; i < myGame.allEntities.Count; i++)
                {

                    target = null;
                    if (myGame.allEntities[i].playerNumber * playerNumber < 0)
                    {
                        float distanceCalc = (myGame.allEntities[i].position.X - position.X) * (myGame.allEntities[i].position.X - position.X) + (myGame.allEntities[i].position.Z - position.Z) * (myGame.allEntities[i].position.Z - position.Z);
                        if (distanceCalc < closestEnemyDistance)
                        {
                            closestEnemyDistance = distanceCalc;
                            
                        }
                        if (myGame.allEntities[i].position != position && distanceCalc < searchRange * searchRange)
                        {
                            target = myGame.allEntities[i];
                        }
                    }
                }

                if (target != null)
                {
                    //If within attack range of an enemy then attack, otherwise move towards the target
                    if ((target.position.X - position.X) * (target.position.X - position.X) + (target.position.Z - position.Z) * (target.position.Z - position.Z) < attackRange * attackRange && !(target.playerNumber * playerNumber > 0))
                    {
                        attack(target);
                    }
                    else
                    {
                        Vector3 targetLocation;
                        targetLocation = Vector3.Normalize(target.getPosition() - position);
                        if (targetLocation.X < 99999) //was having a weird NaN issue, likely due to normalize freaking out
                        {
                            move(targetLocation * moveSpeed);
                        }
                    }
                }
                else
                {
                    move(Vector3.Normalize(moveLocation - position));

                }
            }
            if (name == "Builder")
            {
                if (target != null)
                {
                    if (target.getName() == "ResourceNode")
                    {
                        if (carriedResource < carryCapacity)
                        {
                            if ((target.position.X - position.X) * (target.position.X - position.X) + (target.position.Z - position.Z) * (target.position.Z - position.Z) < attackRange * attackRange)
                            {
                                carriedResource += (float)gameTime.ElapsedGameTime.TotalSeconds * 5;
                            }
                            else
                            {
                                moveLocation = Vector3.Normalize(target.getPosition() - position);
                                moveSet = true;
                                if (moveLocation.X < 99999) //was having a weird NaN issue, likely due to normalize freaking out
                                {
                                    move(moveLocation * moveSpeed);
                                }
                            }
                        }
                        else
                        {
                            carriedResource = carryCapacity;
                            for (int i = 0; i < myGame.allEntities.Count; i++)
                            {
                                if (myGame.allEntities[i].name == "PlayerBase")
                                {
                                    subTarget = myGame.allEntities[i];
                                }
                            }
                            moveLocation = Vector3.Normalize(subTarget.getPosition() - position);
                            moveSet = true;
                            if (moveLocation.X < 99999) //was having a weird NaN issue, likely due to normalize freaking out
                            {
                                move(moveLocation * moveSpeed);
                            }
                            if ((subTarget.position.X - position.X) * (subTarget.position.X - position.X) + (subTarget.position.Z - position.Z) * (subTarget.position.Z - position.Z) < attackRange * attackRange)
                            {
                                myGame.modifyResources((int)carriedResource);
                                carriedResource = 0;
                                myGame.playDepositAudio();
                            }
                        }

                    }
                }
                else
                {
                    move(Vector3.Normalize(moveLocation - position));
                }
            }
        //Behavior for the units to spread apart
        /* Currently not working
            if (moveLocation.X - position.X < 0.5f && moveLocation.Z - position.Z < 0.5f && target == null) { moveSet = false; } // If you're close enough to your move location then disable flag
            if (!moveSet) //If not moving then enable spread apart behavior
            {
                for (int i = 0; i < myGame.allEntities.Count; i++)
                {
                    if (myGame.allEntities[i].position != position && (myGame.allEntities[i].position.X - position.X) * (myGame.allEntities[i].position.X - position.X) + (myGame.allEntities[i].position.Z - position.Z) * (myGame.allEntities[i].position.Z - position.Z) < size * size*4)
                    {
                        moveLocation = Vector3.Normalize(myGame.allEntities[i].getPosition() - position );
                        move(Vector3.Normalize(-moveLocation - position));
                    }
                }
            }
        */
        }

        public override void Draw(GameTime gameTime)
        {
            
            if (closestEnemyDistance < 700)
            {
                GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.BlendState = BlendState.Opaque;


                PointDiffSPecTextureEffect.Parameters["World"].SetValue(worldRot * worldTrans);
                PointDiffSPecTextureEffect.Parameters["View"].SetValue(myGame.getCamera().view);
                PointDiffSPecTextureEffect.Parameters["Projection"].SetValue(myGame.getCamera().proj);
                PointDiffSPecTextureEffect.Parameters["ModelTexture"].SetValue(texture);


                foreach (EffectPass pass in PointDiffSPecTextureEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, pnverts, 0, 12);
                }

            }
            base.Draw(gameTime);

        }


        public void SetTranslation(Vector3 trns)
        { 
            position = trns;
            worldTrans = Matrix.CreateTranslation(position);
            ceffect.World = scale * worldRot * worldTrans;
        }

        public void move(Vector3 movement)
        {
            Vector3 temp = position + movement * moveSpeed * moveSpeedMulti;
            if (temp.X > -gDim/2 && temp.X < gDim/2 && temp.Z > -gDim/2 && temp.Z < gDim/2)
            {
                if (myGame.getNormalAt((int)temp.X + gDim / 2, (int)temp.Z + gDim / 2).Y > 0.7)
                    position = temp;
            }
        }

        public void setTargetLocation(Vector3 targetLocation)
        {
            moveLocation = targetLocation;
            moveSet = true;
        }


        public int getMaxHP()
        {
            return maxHP;
        }

        public int getCurrentHP()
        {
            return currentHP;
        }

        public BoundingBox getBox()
        {
            return box;
        }

        public void activateAbility(int a)
        {
            abilities[a].useAbility(myGame, position);
        }

        public Vector3 getPosition()
        {
            return position;
        }

        public void setSelected()
        {
            selected = true;
        }
        public void setSelectedFalse()
        { selected = false; }   

        public void attack(Entity entity)
        {
            entity.currentHP -= attackDamage;
        }

        public int getAbilityCost(int a)
        {
            return abilities[a].getResourceCost();
        }

        public String getName() { return name; }

        public void setTarget(Entity entity)
        {
            target = entity;
        }

        public Entity getTarget()
        {
            return target;
        }

        public float getCarriedResources()
        {
            return carriedResource;
        }

        public Ability[] getAbilities()
        {
            return abilities;
        }

        public int getPlayerNumber()
        {
            return playerNumber;
        }
    }
}
