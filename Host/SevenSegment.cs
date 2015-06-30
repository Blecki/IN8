using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace IN8
{
    [Hardware("7SGM")]
    public class SevenSegment : Hardware
    {
        SpriteBatch batch;
        Texture2D texture;
        Texture2D Black;
        byte state = 0;

        public void InitializeGraphics(
            Microsoft.Xna.Framework.Graphics.GraphicsDevice device,
            Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            batch = new SpriteBatch(device);
            texture = Content.Load<Texture2D>("7-segment");
            Black = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Black.SetData(new Color[] { Color.Black });
        }

        public void Draw()
        {
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            batch.Draw(Black, new Rectangle(0, 0, 32, 64), Color.White);
           
            var ls = state;
            var i = 0;
            while (ls != 0)
            {
                if (ls % 2 == 1) batch.Draw(texture, new Rectangle(0, 0, 32, 64),
                    new Rectangle(32 * i, 0, 32, 64), Color.White);
                ls >>= 1;
                i += 1;
            }

            batch.End();
        }

        public Point PreferredSize { get { return new Point(32, 64); } }

        public void Connect(IN8.Emulator CPU, params byte[] ports)
        {
            if (ports.Length != 1) throw new InvalidOperationException("7-segment display takes exactly one port");
            CPU.AttachHardware(this, ports);
        }

        public void PortWritten(byte port, byte value)
        {
            state = value;
        }

        public void Update()
        {
        }

        public String DebugString { get { return String.Format("7SGM {0:X2}", state); } }
    }
}
