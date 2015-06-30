using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace IN8
{
    public class HardwareAttribute : Attribute
    {
        public string ID;
        public HardwareAttribute(String ID) { this.ID = ID; }
    }

    public interface Hardware
    {
        void InitializeGraphics(GraphicsDevice Device, ContentManager Content);
        void Connect(Emulator CPU, params byte[] ports);
        void PortWritten(byte port, byte value);
        void Draw();
        Point PreferredSize { get; }
        String DebugString { get; }
    }
}
