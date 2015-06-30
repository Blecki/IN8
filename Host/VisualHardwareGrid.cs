using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace IN8
{
    public class VisualHardwareGrid : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        IN8.Emulator CPU;
        Texture2D CopperStud;
        Texture2D StudBG;
        SpriteBatch BackgroundSpriteBatch;


        private class HardwareDevice
        {
            internal Hardware driver;
            internal byte[] ports;
            internal Rectangle position;
        }

        private List<HardwareDevice> hardware = new List<HardwareDevice>();

        public VisualHardwareGrid(IN8.Emulator CPU)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;

            this.CPU = CPU;
         
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
        }
            
        public void AddHardware(System.Type hostedHardwareType, Point location, params byte[] ports)
        {
            var newHardware = new HardwareDevice();
            
            newHardware.driver = Activator.CreateInstance(hostedHardwareType) as Hardware;
            if (newHardware.driver == null) throw new InvalidProgramException("Cannot host devices that are not visual hardware.");

            var size = newHardware.driver.PreferredSize;

            newHardware.position = new Rectangle(location.X, location.Y, size.X, size.Y);
            newHardware.ports = ports;

            hardware.Add(newHardware);

            newHardware.driver.Connect(CPU, ports);
        }

        protected override void Initialize()
        {
            foreach (var device in hardware)
                device.driver.InitializeGraphics(GraphicsDevice, Content);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            CopperStud = Content.Load<Texture2D>("copper-stud");
            StudBG = Content.Load<Texture2D>("stud-bg");
            BackgroundSpriteBatch = new SpriteBatch(GraphicsDevice);

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var originalViewport = GraphicsDevice.Viewport;

            BackgroundSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap);
            BackgroundSpriteBatch.Draw(StudBG, GraphicsDevice.Viewport.Bounds, GraphicsDevice.Viewport.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            BackgroundSpriteBatch.Draw(CopperStud, GraphicsDevice.Viewport.Bounds, GraphicsDevice.Viewport.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            BackgroundSpriteBatch.End();            
            
            foreach (var device in hardware)
            {
                GraphicsDevice.Viewport = new Viewport(device.position);
                device.driver.Draw();
            }
            GraphicsDevice.Viewport = originalViewport;
            base.Draw(gameTime);
        }
    }
}
