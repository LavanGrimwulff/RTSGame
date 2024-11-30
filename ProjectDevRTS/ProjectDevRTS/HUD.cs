using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace ProjectDevRTS
{
    internal class HUD : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 myGame;
        SpriteBatch spriteBatch;

        private Texture2D hudBack;
        private SpriteFont myFont;

        private Vector2 textPosition = new Vector2(50,750);

        List<Entity> selectedEntities = new List<Entity>();

        Rectangle[] abilityRects;

        Texture2D rect;

        public HUD(Game1 aGame) : base(aGame)
        {
            myGame = (Game1)aGame;
        }
        public void LoadContent()
        {
            base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            hudBack = new Texture2D(GraphicsDevice, 1, 1);
            hudBack.SetData(new[] { Color.Gray } );

            rect = new Texture2D(GraphicsDevice, 1, 1);
            rect.SetData(new[] { Color.Black });

            myFont = myGame.Content.Load<SpriteFont>("Font");

        }
        public override void Update(GameTime gameTime)
        {
            abilityRects = null;
            if (selectedEntities.Count > 0)
            {
                if (selectedEntities[0].getAbilities() != null)
                {
                    if (selectedEntities[0].getAbilities().Length > 0)
                    {
                        abilityRects = new Rectangle[selectedEntities[0].getAbilities().Length];
                        for (int i = 0; i < abilityRects.Length; i++)
                        {
                            abilityRects[i] = new Rectangle(500 + i * 50, 775, 50, 50);
                        }
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {

            spriteBatch.Begin();
            spriteBatch.Draw(hudBack, new Rectangle((int)0, (int)700, 1280, 160), Color.White);
            if (selectedEntities.Count != 0)
            {
                spriteBatch.DrawString(myFont, "HP: " + selectedEntities[0].getCurrentHP() + "/" + selectedEntities[0].getMaxHP(), textPosition, Color.White);
                if (selectedEntities[0].getTarget() != null)
                    spriteBatch.DrawString(myFont, "Target: " + selectedEntities[0].getTarget().getName(), new Vector2(50, 775), Color.White);
                spriteBatch.DrawString(myFont, "Resources: " + selectedEntities[0].getCarriedResources(), new Vector2(50, 800), Color.White);
                spriteBatch.DrawString(myFont, "Position: " + selectedEntities[0].getPosition(), new Vector2(50, 825), Color.White);
                if (abilityRects != null)
                {
                    for (int i = 0; i < abilityRects.Length; i++)
                    {
                        spriteBatch.Draw(rect, abilityRects[i], Color.White);
                    }
                }
            }
            spriteBatch.DrawString(myFont, "Resources: " + myGame.getResources(), new Vector2(50, 50), Color.White);
            spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        }




        public void setUnitsSelected(ref List<Entity> entities)
        {
            selectedEntities = entities;
        }

        public int getAbilityAtLocation(Vector2 location)
        {
            int spot = -1;
            if (abilityRects != null)
            {
                for (int i = 0; i < abilityRects.Length; i++)
                {
                    if (abilityRects[i].Contains(location))
                        spot = i;
                }
            }
            return spot;
        }
    }
}
