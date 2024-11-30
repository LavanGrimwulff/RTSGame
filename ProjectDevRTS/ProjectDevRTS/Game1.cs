using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Plane = Microsoft.Xna.Framework.Plane;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace ProjectDevRTS
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Camera camera;
        VertexPositionColor[] verts;
        Matrix worldTrans = Matrix.Identity;
        Matrix worldRot = Matrix.Identity;


        VertexPositionColor[] cverts;
        BasicEffect ceffect;
        int gDim = 300;



        //Adding Yaw
        float CamYAngle;
        float CamXAngle;
        MouseState mouseState;
        MouseState prevMouseState;
        

        //Heightmap stuff
        float[,] hts;
        Random rng;

        //Normals/DotProduct
        Vector3[,] normals;
        Vector3 ldir = new Vector3(0.57735f, 0.57735f, 0.57735f);

        //Need an array of planes corresponding to the terrain for the mouse intersection code
        BoundingBox[] planes;


        //texture
        VertexPositionNormalTexture[] pnverts;
        Texture2D groundTexture;
        Effect DiffuseTextureEffect;
        float texScale = 0.1f;

        Vector4 ambientIntensity = new Vector4(0.1f, 0.1f, 0.1f, 1f);

        Ray mouseRay; //Used to find what the mouse is hovering
        HUD hud;

        public List<Entity> allEntities = new List<Entity>(); //Every entity currently in the world
        List<Entity> selectedEntities = new List<Entity>();//When we select entities store them in an array. Will be passed to hud as a reference

//Enemy Variables
        float spawnSpeed = 500; //Initial delay between enemies spawning
        float timer = 10; // Current timer on how long till next enemy spawns

        float eventSpeed = 3000; //Delay between events
        float eventTimer = 3000000; // Current time till next event

        public EntityType[] entityTypes;


        float screenWidth;
        float screenHeight;

        bool angledView = true;
        bool qUp = true;

        int resourceCounter = 100;


        //For FoW
        Texture2D lightMask;
        RenderTarget2D lightsTarget;


        //Sound Effects
        SoundEffect depositAudio;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //graphics.IsFullScreen = true;

            graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 860;   // set this value to the desired height of your window

            screenWidth = graphics.PreferredBackBufferWidth;
            screenHeight = graphics.PreferredBackBufferHeight;

            graphics.ApplyChanges();
            this.Window.Position = new Point(600, 100);  // set this point to the desired upper left corner of your window
            hud = new HUD(this);
            Components.Add(hud);
            hud.LoadContent();
            
            hud.DrawOrder = 20;




        }
//////////////
///Initialize Function 
///////////
        protected override void Initialize()
        {
            base.Initialize();

            camera = new Camera(this, new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            camera.CameraDirection = new Vector3(1,0.5f,1);
            Components.Add(camera); //Make then add camera

            
            this.IsMouseVisible = true; //Makes the mouse Visible


            rng = new Random();


            //Load data for all creature Types
            string line; //To be used later as we read lines
            string[] temp; //Stores the line after its been split

            string path = Path.Combine(Content.RootDirectory, "UnitTypes.txt");
            StreamReader inputreader = new StreamReader(path);

            var lineCount = File.ReadLines(path).Count(); //Count the number of lines in the file
            entityTypes = new EntityType[lineCount - 3]; //Create an array whose size is based on number of lines in the file minus the three lines I use for the template info

            line = inputreader.ReadLine();
            line = inputreader.ReadLine();
            line = inputreader.ReadLine(); //Gets rid of the three format info lines

            for (int i = 0; i < lineCount -3; i++)
            {
                line = inputreader.ReadLine();
                temp = line.Split(','); //Separates out the line into an array based on commas
                //Temporarily store the values from the file
                string type = temp[0];
                int hp = int.Parse(temp[1]);
                int moveSpeed = int.Parse(temp[2]);
                int attackSpeed = int.Parse(temp[3]);
                int attackDamage = int.Parse(temp[4]);
                int attackRange = int.Parse(temp[5]);
                float size = float.Parse(temp[6]);
                //Create a type with the above values, store for later use
                entityTypes[i] = new EntityType(type,hp,moveSpeed, attackSpeed, attackDamage, attackRange,size);
            }


        }
//////////////
///LoadContent Function 
///////////
        protected override void LoadContent()
        {

            //Yaw
            CamYAngle = 45;


            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);



            //Heightmap stuff
            otherCalcHeights();

            //Normals/DotProduct
            normals = new Vector3[gDim + 1, gDim + 1];


            // Initialize vertices


            //texture
            pnverts = new VertexPositionNormalTexture[gDim * gDim * 6];

            planes = new BoundingBox[gDim * gDim];

            //Average Normals
            for (int i = 0; i < gDim; i++)
            {

                for (int j = 0; j < gDim; j++)
                {

                    float x = (float)(i - (gDim / 2));
                    float y = hts[i, j];
                    float z = (float)(j - (gDim / 2));

                    Vector3 one = new Vector3(x, hts[i + 1, j + 1], z);   //  0,0
                    Vector3 two = new Vector3(x - 1, hts[i, j + 1], z);   //  -1,0
                    Vector3 three = new Vector3(x, hts[i + 1, j], z - 1); //  0,-1
                    Vector3 four = new Vector3(x - 1, hts[i, j], z - 1);  //  -1,-1
                    Vector3 five = new Vector3(x - 1, hts[i, j + 1], z);  //  -1,0
                    Vector3 six = new Vector3(x, hts[i + 1, j], z - 1);   //  0,-1


                    Vector3 sideOne = one - two;
                    Vector3 sideTwo = three - one;
                    Vector3 sideThree = four - five;
                    Vector3 sideFour = six - four;


                    Vector3 oneN = Vector3.Cross(sideOne, sideTwo);
                    Vector3 twoN = Vector3.Cross(sideFour, sideThree);


                    Vector3 average = oneN + twoN;
                    average.Normalize();

                    normals[i, j] = average;

                }
            }

            //Assign colors and create positions
            for (int i = 0; i < gDim; i++)
            {

                for (int j = 0; j < gDim; j++)
                {
                    float x = (float)(i - (gDim / 2));
                    float y = hts[i, j];
                    float z = (float)(j - (gDim / 2));


                    planes[i * gDim + j] = new BoundingBox(new Vector3(x, hts[i + 1, j], z - 1), new Vector3(x - 1, hts[i, j + 1], z));

                    //textures
                    pnverts[i * gDim * 6 + j * 6 + 0].TextureCoordinate = new Vector2((x+gDim/2)/gDim, (z + gDim / 2) / gDim);
                    pnverts[i * gDim * 6 + j * 6 + 0].Position = new Vector3(x, hts[i + 1, j + 1], z);
                    pnverts[i * gDim * 6 + j * 6 + 0].Normal = normals[i + 1, j + 1];

                    pnverts[i * gDim * 6 + j * 6 + 1].TextureCoordinate = new Vector2(((x - 1 + gDim / 2)) / gDim, (z + gDim / 2) / gDim);
                    pnverts[i * gDim * 6 + j * 6 + 1].Position = new Vector3(x - 1, hts[i, j + 1], z);
                    pnverts[i * gDim * 6 + j * 6 + 1].Normal = normals[i, j + 1];

                    pnverts[i * gDim * 6 + j * 6 + 2].TextureCoordinate = new Vector2((x + gDim / 2) / gDim, ((z - 1 + gDim / 2)) / gDim); // 
                    pnverts[i * gDim * 6 + j * 6 + 2].Position = new Vector3(x, hts[i + 1, j], z  - 1);
                    pnverts[i * gDim * 6 + j * 6 + 2].Normal = normals[i + 1, j];

                    pnverts[i * gDim * 6 + j * 6 + 3].TextureCoordinate = new Vector2(((x - 1 + gDim / 2)) / gDim, ((z - 1 + gDim / 2)) / gDim);
                    pnverts[i * gDim * 6 + j * 6 + 3].Position = new Vector3(x - 1, hts[i, j], z - 1);
                    pnverts[i * gDim * 6 + j * 6 + 3].Normal = normals[i, j];

                    pnverts[i * gDim * 6 + j * 6 + 4].TextureCoordinate = new Vector2(((x - 1 + gDim / 2)) / gDim, (z  + gDim / 2) / gDim);
                    pnverts[i * gDim * 6 + j * 6 + 4].Position = new Vector3(x - 1, hts[i, j + 1], z);
                    pnverts[i * gDim * 6 + j * 6 + 4].Normal = normals[i, j + 1];

                    pnverts[i * gDim * 6 + j * 6 + 5].TextureCoordinate = new Vector2((x + gDim / 2) / gDim, ((z - 1 + gDim / 2)) / gDim);
                    pnverts[i * gDim * 6 + j * 6 + 5].Position = new Vector3(x, hts[i + 1, j], z - 1);
                    pnverts[i * gDim * 6 + j * 6 + 5].Normal = normals[i + 1, j];

                }
            }


            // Set cullmode to none
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rs;

            //Texture
            groundTexture = Content.Load<Texture2D>(@"grass");
            DiffuseTextureEffect = Content.Load<Effect>("DiffuseTexture");

            lightMask = Content.Load<Texture2D>("lightmask");
            lightsTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth*2, GraphicsDevice.PresentationParameters.BackBufferHeight*2);

            //Audio files
            depositAudio = Content.Load<SoundEffect>("coinSound");
        }
//////////////
///BeginRun Function, runs after Init/Load but before update starts looping 
///////////
        protected override void BeginRun()
        {
            //3 Starter units
            allEntities.Add(new Entity(this, "Builder", new Vector3(0, 0, 0), 1));
            Components.Add(allEntities[0]);

            allEntities.Add(new Entity(this, "Ranged", new Vector3(-10, 0, 5), 1));
            Components.Add(allEntities[1]);

            allEntities.Add(new Entity(this, "Ranged", new Vector3(10, 0, 5), 1));
            Components.Add(allEntities[2]);

            //Base for player
            allEntities.Add(new Entity(this, "PlayerBase", new Vector3(0, 0, -10), 1));
            Components.Add(allEntities[3]);

            //Resource Node
            allEntities.Add(new Entity(this, "ResourceNode", new Vector3(0, 0, -50), 0));
            Components.Add(allEntities[4]);

        }

//////////////
///Update Function 
///////////
        protected override void Update(GameTime gameTime)
        {

            KeyboardState keyboardState = Keyboard.GetState();
            // Allows the game to exit
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();


            //Enemy Spawn
            timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer <= 0)
            {
                timer = spawnSpeed;
                allEntities.Add(new Entity(this, "Melee", new Vector3(0, 0, 50), -1));
                Components.Add(allEntities[allEntities.Count - 1]);
                if (spawnSpeed > 1) { spawnSpeed -= 0.01f; } //Ramps up spawnspeed each time something spawns
            }

            //Events
            eventTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(eventTimer <= 0)
            {
                eventTimer = eventSpeed;
                switch (rng.Next(0, 3))
                {
                    case 0:
                        Debug.WriteLine("Free resources");
                        resourceCounter += 1000;
                        break;
                    case 1:
                        Debug.WriteLine("Spawning swarm");
                        for (int i = 0; i < rng.Next(5, 8); i++)
                        {
                            allEntities.Add(new Entity(this, "Melee", new Vector3(-15 + rng.Next(0,30), 0, 50), -1));
                            Components.Add(allEntities[allEntities.Count - 1]);
                        }
                        break;
                    case 2:
                        Debug.WriteLine("Spawning Allies");
                        for (int i = 0; i < rng.Next(2, 4); i++)
                        {
                            allEntities.Add(new Entity(this, "Ranged", new Vector3(-15 + rng.Next(0, 30), 0, 0), 1));
                            Components.Add(allEntities[allEntities.Count - 1]);
                        }
                        break;
                    default: break;
                }
            }



            //Controls
            {
                //Control camera with arrows
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    if (camera.CameraPosition.X < 145)
                        camera.CameraPosition.X += 1;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    if (camera.CameraPosition.X > -145)
                        camera.CameraPosition.X -= 1;
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    if (camera.CameraPosition.Z < 145)
                        camera.CameraPosition.Z += 1;
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    if (camera.CameraPosition.Z > -145)
                        camera.CameraPosition.Z -= 1;
                }


                if (keyboardState.IsKeyUp(Keys.Q))
                {
                    
                    qUp = true;
                }
                if (keyboardState.IsKeyDown(Keys.Q))
                {
                    if (qUp)
                    {
                        angledView = !angledView;
                    }
                    qUp = false;
                }

     //Select and move unit
                mouseState = Mouse.GetState();


                if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed)
                {
                    if (mouseState.Y < 700 && mouseState.X > 0 && mouseState.X < screenWidth)
                    {
                        UpdateRayCursor();
                        Nullable<float> checkIntersectDistance = null;
                        {
                            bool foundUnit = false;
                            if (allEntities.Count > 0)
                            {
                                for (int i = 0; i < allEntities.Count; i++)
                                {

                                    checkIntersectDistance = mouseRay.Intersects(allEntities[i].getBox());
                                    if (checkIntersectDistance != null)
                                    {
                                        if(!keyboardState.IsKeyDown(Keys.LeftShift))
                                            selectedEntities.Clear();
                                        selectedEntities.Add(allEntities[i]);
                                        allEntities[i].setSelected();
                                        hud.setUnitsSelected(ref selectedEntities);
                                        foundUnit = true;
                                    }
                                }
                            }

                            if (!foundUnit)
                            {
                                for (int i = 0; i < selectedEntities.Count; i++)
                                {
                                    selectedEntities[i].setSelectedFalse();
                                }
                                selectedEntities.Clear();
                            }
                        }
                    }
                    else if(mouseState.Y < screenHeight)
                    {
                        int spot = hud.getAbilityAtLocation(new Vector2(mouseState.X,mouseState.Y));
                        if (spot != -1)
                        {
                            if (resourceCounter >= selectedEntities[0].getAbilityCost(spot))
                            {
                                resourceCounter -= selectedEntities[0].getAbilityCost(spot);
                                selectedEntities[0].activateAbility(spot);
                            }
                        }
                    }
                }
      //RighMouse button
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    if (mouseState.Y < 700)
                    {
                        Vector3 targetLocation = new Vector3(9999, 9999, 9999);
                        float? distanceToGround = null;
                        bool foundUnit = false;
                        Nullable<float> checkIntersectDistance = null;
                        UpdateRayCursor();

                        if (selectedEntities.Count > 0)
                        {
                            for (int i = 0; i < allEntities.Count; i++)
                            {

                                checkIntersectDistance = mouseRay.Intersects(allEntities[i].getBox());
                                if (checkIntersectDistance != null)
                                {
                                    for(int j = 0;j<selectedEntities.Count;j++)
                                    {
                                        if (selectedEntities[j] != allEntities[i])
                                            selectedEntities[j].setTarget(allEntities[i]);
                                    }
                                    foundUnit = true;
                                }
                            }
                        }
                        if (!foundUnit)
                        {
                            for (int i = 0; i < gDim; i++)
                            {
                                for (int j = 0; j < gDim; j++)
                                {
                                    if (mouseRay.Intersects(planes[i * gDim + j]) != null)
                                    {
                                        distanceToGround = mouseRay.Intersects(planes[i * gDim + j]);
                                    }
                                }
                            }

                            if (distanceToGround != null)
                            { 
                                targetLocation = mouseRay.Position + ((float)(distanceToGround) * mouseRay.Direction);
                                if (selectedEntities.Count > 0 && targetLocation.X != 9999)
                                {
                                    targetLocation.Y = 0;
                                    for (int i = 0; i < selectedEntities.Count; i++)
                                    {
                                        selectedEntities[i].setTargetLocation(targetLocation);
                                    }
                                }
                            }
                        }
                    }
                }


        //moves selected unit, temporary for testing

                if (selectedEntities.Count > 0)
                {
                    if (keyboardState.IsKeyDown(Keys.W))
                    {
                        selectedEntities[0].move(new Vector3(0, 0, 1));
                    }
                    if (keyboardState.IsKeyDown(Keys.D))
                    {
                        selectedEntities[0].move(new Vector3(-1, 0, 0));
                    }
                    if (keyboardState.IsKeyDown(Keys.S))
                    {
                        selectedEntities[0].move(new Vector3(0, 0, -1));
                    }
                    if (keyboardState.IsKeyDown(Keys.A))
                    {
                        selectedEntities[0].move(new Vector3(1, 0, 0));
                    }

                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        if (resourceCounter >= selectedEntities[0].getAbilityCost(0))
                        {
                            resourceCounter -= selectedEntities[0].getAbilityCost(0);
                            selectedEntities[0].activateAbility(0);
                        }
                    }
                }

        //Clean up dead units
                for (int i = 0; i < allEntities.Count; i++)
                {
                    if (allEntities[i].getCurrentHP() <= 0)
                    {
                        Components.Remove(allEntities[i]);
                        allEntities.RemoveAt(i);
                    }
                }



        //Maintain Camera height at a fixed distance above ground
                {
                    float a = hts[(int)camera.CameraPosition.X + gDim / 2, (int)camera.CameraPosition.Z + gDim / 2];
                    float b = hts[(int)camera.CameraPosition.X + 1 + gDim / 2, (int)camera.CameraPosition.Z + gDim / 2];
                    float c = hts[(int)camera.CameraPosition.X + 1 + gDim / 2, (int)camera.CameraPosition.Z + 1 + gDim / 2];
                    float d = hts[(int)camera.CameraPosition.X + gDim / 2, (int)camera.CameraPosition.Z + 1 + gDim / 2];

                    float t = camera.CameraPosition.X - (int)camera.CameraPosition.X;
                    float s = camera.CameraPosition.Z - (int)camera.CameraPosition.Z;

                    camera.CameraPosition.Y = (1 - s) * (t * b + (1 - t) * a) + s * (t * d + (1 - t) * c) + 48;
                }

                if(angledView)
                    camera.CameraDirection = new Vector3(0, -1f, 1);
                else
                    camera.CameraDirection = new Vector3(0, -10f, 1);
                camera.view = Matrix.CreateLookAt(camera.CameraPosition, camera.CameraPosition + camera.CameraDirection, camera.CameraUp);


                prevMouseState = mouseState;
                base.Update(gameTime);
            }
        }


//////////////
///Draw Function 
///////////
        protected override void Draw(GameTime gameTime)
        {

 
            //Generate map of FoW
            GraphicsDevice.SetRenderTarget(lightsTarget);
            GraphicsDevice.Clear(Color.DarkGray);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            for (int i = 0; i < allEntities.Count; i++)
                if (allEntities[i].getPlayerNumber() > 0)
                {
                    spriteBatch.Draw(lightMask, new Vector2((allEntities[i].getPosition().X+135)*8.53f, (allEntities[i].getPosition().Z+135)*5.7f), Color.White);
                }
            spriteBatch.End();

            groundTexture = Content.Load<Texture2D>(@"grass");

            DiffuseTextureEffect.Parameters["ModelTexture"].SetValue(groundTexture);
            DiffuseTextureEffect.Parameters["World"].SetValue(Matrix.Identity);
            DiffuseTextureEffect.Parameters["View"].SetValue(camera.view);
            DiffuseTextureEffect.Parameters["Projection"].SetValue(camera.proj);

            DiffuseTextureEffect.Parameters["LightDir"].SetValue(ldir);
            DiffuseTextureEffect.Parameters["ambientIntensity"].SetValue(ambientIntensity);
            DiffuseTextureEffect.Parameters["AmbientColor"].SetValue(new Vector4(1, 1, 1, 1));


            DiffuseTextureEffect.Parameters["lightMask"].SetValue(lightsTarget);//For FoW


        // Draw
            //Next 3 lines are to fix things that are set when you use spritebatch
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            //return to normal operation after setting rendertarget to FoW map
            GraphicsDevice.SetRenderTarget(null);

            foreach (EffectPass pass in DiffuseTextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, pnverts, 0, gDim * gDim * 2);
            }



            GraphicsDevice.SetRenderTarget(null);
            base.Draw(gameTime);

        }

        public void otherCalcHeights()
        {
            Texture2D heightMap = Content.Load<Texture2D>(@"TestHeightmap");
            // can be .bmp, .dds, .dib, .hdr, .jpg, .pfm, .png, .ppm, .tga

            hts = new float[gDim + 1, gDim + 1];


            int terrainWidth = heightMap.Width;
            int terrainHeight = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);

            for (int x = 0; x <= gDim; x++)
                for (int y = 0; y <= gDim; y++)
                {
                    if ((x < terrainWidth) && (y < terrainHeight))
                    {
                        //hts[x, y] = 0.0f;
                        hts[x, y] = heightMapColors[x + y * terrainWidth].R / 5.0f - 25.0f;
                    }
                    else
                    {
                        hts[x, y] = 0.0f;
                    }
                }
        }


        public void setCamera(BasicEffect effect)
        {
            effect.View = camera.view;
            effect.Projection = camera.proj;


        }

        //Below taken from https://gamedev.stackexchange.com/questions/132617/xna-c-picking-model-with-ray-mouse-cursor-select-the-wrong-model
        //and updated to this version of monogame
        public void UpdateRayCursor()
        {
       
            Vector3 nearPoint = new Vector3(mouseState.Position.ToVector2(), 0);
            Vector3 farPoint = new Vector3(mouseState.Position.ToVector2(), 1f);


            nearPoint = customUnproject(nearPoint, Matrix.Identity, camera.proj, camera.view);
            farPoint = customUnproject(farPoint, Matrix.Identity, camera.proj, camera.view);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            
            mouseRay = new Microsoft.Xna.Framework.Ray(nearPoint, direction);
        }


        public float getHTSAt(int a, int b)
        {
            return hts[a, b];
        }

        public Vector3 getNormalAt(int a, int b)
        {
            return normals[a, b];
        }

        public int getgdim()
        { return gDim; }

        public Camera getCamera()
        {
            return camera;
        }

        public void modifyResources(int a)
        {
            resourceCounter += a;
        }

        public int getResources()
        {
            return resourceCounter;
        }

        public Vector3 customUnproject(Vector3 mouse, Matrix world, Matrix projection, Matrix view) 
        //Attempting to recreate Monogame's unproject function based on what little documentation I can see of how they did it.
        //Basically all I know of its function is what goes in and what comes out, hopefully thats enough                             
        {
            Matrix matrix = Matrix.Invert(world * view * projection);

            mouse.X = ((mouse.X - GraphicsDevice.Viewport.X) / ((float)GraphicsDevice.Viewport.Width) * 2f) - 1f;
            mouse.Y = -(((mouse.Y - GraphicsDevice.Viewport.Y) / ((float)GraphicsDevice.Viewport.Height) * 2f) - 1f);
            mouse.Z = (mouse.Z - GraphicsDevice.Viewport.MinDepth) / (GraphicsDevice.Viewport.MaxDepth - GraphicsDevice.Viewport.MinDepth);
            Vector3 result = Vector3.Transform(mouse, matrix);
            
            //This bit I had to find help online, lost the link in a power loss and can't seem to find it again.
            //Don't fully understand but what they seemed to be saying was that when I set mouse.Z above it can get set to extremely small values and mess things up without this
            float smallTest = (((mouse.X * matrix.M14) + (mouse.Y * matrix.M24)) + (mouse.Z * matrix.M34)) + matrix.M44;
            if(!(((smallTest-1f) >= (float.Epsilon * -1)) && ((smallTest - 1f) <= float.Epsilon))) //Epsilon represents the smallest possible value, not sure how we could ever end up under it but it works
            {
                result.X = result.X / smallTest;
                result.Y = result.Y / smallTest; 
                result.Z = result.Z / smallTest;
            }   
            return result;
        }


    //Audio
        public void playDepositAudio()
        {
            depositAudio.Play();
        }
    }
    

}
