using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDevRTS
{
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        public Matrix view { get; set; }
        public Matrix proj { get; protected set; }

        // The Camera position
        public Vector3 CameraPosition = Vector3.Zero;
        // a vector that points the way the camera is looking
        public Vector3 CameraDirection = Vector3.Forward;
        public Vector3 CameraLeft = Vector3.Left;
        public Vector3 CameraUp = Vector3.Up;

        public Vector3 getDirection()
        {
            return CameraDirection;
        }


        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game)
        {
            CameraPosition = pos;
            CameraUp = up;
            CameraDirection = target - pos;
            view = Matrix.CreateLookAt(pos, target, up);
            proj = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            (float)Game.Window.ClientBounds.Width /
            (float)Game.Window.ClientBounds.Height,
            1, 1000);
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }
    }
}