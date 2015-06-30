using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IN8
{
    public class Emulator
    {
        //Setup memory and registers

        public byte[] MEM = new byte[0x10000];
        public byte A, B, C, D, E, H, L = 0;
        public ushort IP, SP = 0;
        public byte O = 0;
        public uint CLOCK = 0;
        public byte[] PORT = new byte[0x100];
        public Hardware[] HARDWARE = new Hardware[0x100];
        public uint MEMORY_SIZE = 0x10000;

        private byte N { get { CLOCK++; if (IP < MEMORY_SIZE) return MEM[(short)(IP++)]; else throw new MemoryException(); } }
        private ushort HL { get { return (ushort)(((ushort)H << 8) + (ushort)L); } }
        private ushort DE { get { return (ushort)(((ushort)D << 8) + (ushort)E); } }

        private void CHECK_MEM(int PTR) { if (PTR >= MEMORY_SIZE) throw new MemoryException(); }
        
        public byte STATE_FLAGS = 0;

        public void AttachHardware(Hardware h, params byte[] ports)
        {
            foreach (var port in ports)
                HARDWARE[port] = h;
        }

        public void ALIGN_FAULT()
        {
            STATE_FLAGS |= 0x01;
        }

        public void STOP()
        {
            STATE_FLAGS |= 0x02;
        }

        public void MEMORY_FAULT()
        {
            STATE_FLAGS |= 0x04;
        }

        private class MemoryException : Exception
        {
            public MemoryException() : base("Memory access out of range.") { }
        }

        public void Step()
        {
            if (STATE_FLAGS != 0) return;

            try
            {
                switch (N)
                {
                    case 0x00: /* CAL     */
                        {
                            CLOCK += 4; CHECK_MEM(SP - 1); CHECK_MEM(SP - 2);
                            MEM[(ushort)(--SP)] = (byte)IP; MEM[(ushort)(--SP)] = (byte)(IP >> 8); IP = HL;
                            break;
                        }
                    case 0x01: /* MTA A B */ { CLOCK += 1; A = B; break; }
                    case 0x02: /* MTA A C */ { CLOCK += 1; A = C; break; }
                    case 0x03: /* MTA A D */ { CLOCK += 1; A = D; break; }
                    case 0x04: /* MTA A E */ { CLOCK += 1; A = E; break; }
                    case 0x05: /* MTA A H */ { CLOCK += 1; A = H; break; }
                    case 0x06: /* MTA A L */ { CLOCK += 1; A = L; break; }
                    case 0x07: /* MTA A N */ { CLOCK += 1; var X = N; A = X; break; }
                    case 0x08: /* MTA B A */ { CLOCK += 1; B = A; break; }
                    case 0x09: /* RET     */
                        {
                            CLOCK += 4; CHECK_MEM(SP); CHECK_MEM(SP + 1);
                            H = MEM[(ushort)(SP++)]; L = MEM[(ushort)(SP++)]; IP = HL;
                            break;
                        }
                    case 0x0A: /* MTA B C */ { CLOCK += 1; B = C; break; }
                    case 0x0B: /* MTA B D */ { CLOCK += 1; B = D; break; }
                    case 0x0C: /* MTA B E */ { CLOCK += 1; B = E; break; }
                    case 0x0D: /* MTA B H */ { CLOCK += 1; B = H; break; }
                    case 0x0E: /* MTA B L */ { CLOCK += 1; B = L; break; }
                    case 0x0F: /* MTA B N */ { CLOCK += 1; var X = N; B = X; break; }
                    case 0x10: /* SSL     */ { CLOCK += 1; A <<= B; break; }
                    case 0x11: /* NPB     */ { CLOCK += 1; break; }
                    case 0x12: /* MFA A C */ { CLOCK += 1; C = A; break; }
                    case 0x13: /* MFA A D */ { CLOCK += 1; D = A; break; }
                    case 0x14: /* MFA A E */ { CLOCK += 1; E = A; break; }
                    case 0x15: /* MFA A H */ { CLOCK += 1; H = A; break; }
                    case 0x16: /* MFA A L */ { CLOCK += 1; L = A; break; }
                    case 0x17: /* NOT A   */ { CLOCK += 2; A = (byte)~A; break; }
                    case 0x18: /* NPC     */ { CLOCK += 1; break; }
                    case 0x19: /* SSR     */ { CLOCK += 1; A >>= B; break; }
                    case 0x1A: /* MFA B C */ { CLOCK += 1; C = B; break; }
                    case 0x1B: /* MFA B D */ { CLOCK += 1; D = B; break; }
                    case 0x1C: /* MFA B E */ { CLOCK += 1; E = B; break; }
                    case 0x1D: /* MFA B H */ { CLOCK += 1; H = B; break; }
                    case 0x1E: /* MFA B L */ { CLOCK += 1; L = B; break; }
                    case 0x1F: /* NOT B   */ { CLOCK += 2; B = (byte)~B; break; }
                    case 0x20: /* ADD A A */ { CLOCK += 2; var Y = A; A += A; O = (byte)((Y + A) >> 8); break; }
                    case 0x21: /* ADD A B */ { CLOCK += 2; var Y = A; A += B; O = (byte)((Y + B) >> 8); break; }
                    case 0x22: /* ADD A C */ { CLOCK += 2; var Y = A; A += C; O = (byte)((Y + C) >> 8); break; }
                    case 0x23: /* ADD A D */ { CLOCK += 2; var Y = A; A += D; O = (byte)((Y + D) >> 8); break; }
                    case 0x24: /* ADD A E */ { CLOCK += 2; var Y = A; A += E; O = (byte)((Y + E) >> 8); break; }
                    case 0x25: /* ADD A H */ { CLOCK += 2; var Y = A; A += H; O = (byte)((Y + H) >> 8); break; }
                    case 0x26: /* ADD A L */ { CLOCK += 2; var Y = A; A += L; O = (byte)((Y + L) >> 8); break; }
                    case 0x27: /* ADD A N */ { CLOCK += 2; var X = N; var Y = A; A += X; O = (byte)((Y + X) >> 8); break; }
                    case 0x28: /* ADD B A */ { CLOCK += 2; var Y = B; B += A; O = (byte)((Y + A) >> 8); break; }
                    case 0x29: /* ADD B B */ { CLOCK += 2; var Y = B; B += B; O = (byte)((Y + B) >> 8); break; }
                    case 0x2A: /* ADD B C */ { CLOCK += 2; var Y = B; B += C; O = (byte)((Y + C) >> 8); break; }
                    case 0x2B: /* ADD B D */ { CLOCK += 2; var Y = B; B += D; O = (byte)((Y + D) >> 8); break; }
                    case 0x2C: /* ADD B E */ { CLOCK += 2; var Y = B; B += E; O = (byte)((Y + E) >> 8); break; }
                    case 0x2D: /* ADD B H */ { CLOCK += 2; var Y = B; B += H; O = (byte)((Y + H) >> 8); break; }
                    case 0x2E: /* ADD B L */ { CLOCK += 2; var Y = B; B += L; O = (byte)((Y + L) >> 8); break; }
                    case 0x2F: /* ADD B N */ { CLOCK += 2; var X = N; var Y = B; B += X; O = (byte)((Y + X) >> 8); break; }
                    case 0x30: /* SUB A A */ { CLOCK += 2; var Y = A; A -= A; O = (byte)((Y << 8) - (A << 8)); break; }
                    case 0x31: /* SUB A B */ { CLOCK += 2; var Y = A; A -= B; O = (byte)((Y << 8) - (B << 8)); break; }
                    case 0x32: /* SUB A C */ { CLOCK += 2; var Y = A; A -= C; O = (byte)((Y << 8) - (C << 8)); break; }
                    case 0x33: /* SUB A D */ { CLOCK += 2; var Y = A; A -= D; O = (byte)((Y << 8) - (D << 8)); break; }
                    case 0x34: /* SUB A E */ { CLOCK += 2; var Y = A; A -= E; O = (byte)((Y << 8) - (E << 8)); break; }
                    case 0x35: /* SUB A H */ { CLOCK += 2; var Y = A; A -= H; O = (byte)((Y << 8) - (H << 8)); break; }
                    case 0x36: /* SUB A L */ { CLOCK += 2; var Y = A; A -= L; O = (byte)((Y << 8) - (L << 8)); break; }
                    case 0x37: /* SUB A N */ { CLOCK += 2; var X = N; var Y = A; A -= X; O = (byte)((Y << 8) - (X << 8)); break; }
                    case 0x38: /* SUB B A */ { CLOCK += 2; var Y = B; B -= A; O = (byte)((Y << 8) - (A << 8)); break; }
                    case 0x39: /* SUB B B */ { CLOCK += 2; var Y = B; B -= B; O = (byte)((Y << 8) - (B << 8)); break; }
                    case 0x3A: /* SUB B C */ { CLOCK += 2; var Y = B; B -= C; O = (byte)((Y << 8) - (C << 8)); break; }
                    case 0x3B: /* SUB B D */ { CLOCK += 2; var Y = B; B -= D; O = (byte)((Y << 8) - (D << 8)); break; }
                    case 0x3C: /* SUB B E */ { CLOCK += 2; var Y = B; B -= E; O = (byte)((Y << 8) - (E << 8)); break; }
                    case 0x3D: /* SUB B H */ { CLOCK += 2; var Y = B; B -= H; O = (byte)((Y << 8) - (H << 8)); break; }
                    case 0x3E: /* SUB B L */ { CLOCK += 2; var Y = B; B -= L; O = (byte)((Y << 8) - (L << 8)); break; }
                    case 0x3F: /* SUB B N */ { CLOCK += 2; var X = N; var Y = B; B -= X; O = (byte)((Y << 8) - (X << 8)); break; }
                    case 0x40: /* MUL A A */ { CLOCK += 8; var Y = A; A *= A; O = (byte)((Y * A) >> 8); break; }
                    case 0x41: /* MUL A B */ { CLOCK += 8; var Y = A; A *= B; O = (byte)((Y * B) >> 8); break; }
                    case 0x42: /* MUL A C */ { CLOCK += 8; var Y = A; A *= C; O = (byte)((Y * C) >> 8); break; }
                    case 0x43: /* MUL A D */ { CLOCK += 8; var Y = A; A *= D; O = (byte)((Y * D) >> 8); break; }
                    case 0x44: /* MUL A E */ { CLOCK += 8; var Y = A; A *= E; O = (byte)((Y * E) >> 8); break; }
                    case 0x45: /* MUL A H */ { CLOCK += 8; var Y = A; A *= H; O = (byte)((Y * H) >> 8); break; }
                    case 0x46: /* MUL A L */ { CLOCK += 8; var Y = A; A *= L; O = (byte)((Y * L) >> 8); break; }
                    case 0x47: /* MUL A N */ { CLOCK += 8; var X = N; var Y = A; A *= X; O = (byte)((Y * X) >> 8); break; }
                    case 0x48: /* MUL B A */ { CLOCK += 8; var Y = B; B *= A; O = (byte)((Y * A) >> 8); break; }
                    case 0x49: /* MUL B B */ { CLOCK += 8; var Y = B; B *= B; O = (byte)((Y * B) >> 8); break; }
                    case 0x4A: /* MUL B C */ { CLOCK += 8; var Y = B; B *= C; O = (byte)((Y * C) >> 8); break; }
                    case 0x4B: /* MUL B D */ { CLOCK += 8; var Y = B; B *= D; O = (byte)((Y * D) >> 8); break; }
                    case 0x4C: /* MUL B E */ { CLOCK += 8; var Y = B; B *= E; O = (byte)((Y * E) >> 8); break; }
                    case 0x4D: /* MUL B H */ { CLOCK += 8; var Y = B; B *= H; O = (byte)((Y * H) >> 8); break; }
                    case 0x4E: /* MUL B L */ { CLOCK += 8; var Y = B; B *= L; O = (byte)((Y * L) >> 8); break; }
                    case 0x4F: /* MUL B N */ { CLOCK += 8; var X = N; var Y = B; B *= X; O = (byte)((Y * X) >> 8); break; }
                    case 0x50: /* DIV A A */ { CLOCK += 32; A /= A; O = 0; break; }
                    case 0x51: /* DIV A B */ { CLOCK += 32; A /= B; O = 0; break; }
                    case 0x52: /* DIV A C */ { CLOCK += 32; A /= C; O = 0; break; }
                    case 0x53: /* DIV A D */ { CLOCK += 32; A /= D; O = 0; break; }
                    case 0x54: /* DIV A E */ { CLOCK += 32; A /= E; O = 0; break; }
                    case 0x55: /* DIV A H */ { CLOCK += 32; A /= H; O = 0; break; }
                    case 0x56: /* DIV A L */ { CLOCK += 32; A /= L; O = 0; break; }
                    case 0x57: /* DIV A N */ { CLOCK += 32; var X = N; A /= X; O = 0; break; }
                    case 0x58: /* DIV B A */ { CLOCK += 32; B /= A; O = 0; break; }
                    case 0x59: /* DIV B B */ { CLOCK += 32; B /= B; O = 0; break; }
                    case 0x5A: /* DIV B C */ { CLOCK += 32; B /= C; O = 0; break; }
                    case 0x5B: /* DIV B D */ { CLOCK += 32; B /= D; O = 0; break; }
                    case 0x5C: /* DIV B E */ { CLOCK += 32; B /= E; O = 0; break; }
                    case 0x5D: /* DIV B H */ { CLOCK += 32; B /= H; O = 0; break; }
                    case 0x5E: /* DIV B L */ { CLOCK += 32; B /= L; O = 0; break; }
                    case 0x5F: /* DIV B N */ { CLOCK += 32; var X = N; B /= X; O = 0; break; }
                    case 0x60: /* MOD A A */ { CLOCK += 32; A %= A; O = 0; break; }
                    case 0x61: /* MOD A B */ { CLOCK += 32; A %= B; O = 0; break; }
                    case 0x62: /* MOD A C */ { CLOCK += 32; A %= C; O = 0; break; }
                    case 0x63: /* MOD A D */ { CLOCK += 32; A %= D; O = 0; break; }
                    case 0x64: /* MOD A E */ { CLOCK += 32; A %= E; O = 0; break; }
                    case 0x65: /* MOD A H */ { CLOCK += 32; A %= H; O = 0; break; }
                    case 0x66: /* MOD A L */ { CLOCK += 32; A %= L; O = 0; break; }
                    case 0x67: /* MOD A N */ { CLOCK += 32; var X = N; A %= X; O = 0; break; }
                    case 0x68: /* MOD B A */ { CLOCK += 32; B %= A; O = 0; break; }
                    case 0x69: /* MOD B B */ { CLOCK += 32; B %= B; O = 0; break; }
                    case 0x6A: /* MOD B C */ { CLOCK += 32; B %= C; O = 0; break; }
                    case 0x6B: /* MOD B D */ { CLOCK += 32; B %= D; O = 0; break; }
                    case 0x6C: /* MOD B E */ { CLOCK += 32; B %= E; O = 0; break; }
                    case 0x6D: /* MOD B H */ { CLOCK += 32; B %= H; O = 0; break; }
                    case 0x6E: /* MOD B L */ { CLOCK += 32; B %= L; O = 0; break; }
                    case 0x6F: /* MOD B N */ { CLOCK += 32; var X = N; B %= X; O = 0; break; }
                    case 0x70: /* OVA     */ { CLOCK += 1; A = O; break; }
                    case 0x71: /* AND A B */ { CLOCK += 2; A &= B; O = 0; break; }
                    case 0x72: /* AND A C */ { CLOCK += 2; A &= C; O = 0; break; }
                    case 0x73: /* AND A D */ { CLOCK += 2; A &= D; O = 0; break; }
                    case 0x74: /* AND A E */ { CLOCK += 2; A &= E; O = 0; break; }
                    case 0x75: /* AND A H */ { CLOCK += 2; A &= H; O = 0; break; }
                    case 0x76: /* AND A L */ { CLOCK += 2; A &= L; O = 0; break; }
                    case 0x77: /* AND A N */ { CLOCK += 2; var X = N; A &= X; O = 0; break; }
                    case 0x78: /* AND B A */ { CLOCK += 2; B &= A; O = 0; break; }
                    case 0x79: /* OVB     */ { CLOCK += 1; B = O; break; }
                    case 0x7A: /* AND B C */ { CLOCK += 2; B &= C; O = 0; break; }
                    case 0x7B: /* AND B D */ { CLOCK += 2; B &= D; O = 0; break; }
                    case 0x7C: /* AND B E */ { CLOCK += 2; B &= E; O = 0; break; }
                    case 0x7D: /* AND B H */ { CLOCK += 2; B &= H; O = 0; break; }
                    case 0x7E: /* AND B L */ { CLOCK += 2; B &= L; O = 0; break; }
                    case 0x7F: /* AND B N */ { CLOCK += 2; var X = N; B &= X; O = 0; break; }
                    case 0x80: /* MUS     */ { CLOCK += 8; var Y = B; B = (byte)((sbyte)A * (sbyte)Y); O = (byte)(((sbyte)A * (sbyte)Y) >> 8); break; }
                    case 0x81: /* BOR A B */ { CLOCK += 2; A |= B; O = 0; break; }
                    case 0x82: /* BOR A C */ { CLOCK += 2; A |= C; O = 0; break; }
                    case 0x83: /* BOR A D */ { CLOCK += 2; A |= D; O = 0; break; }
                    case 0x84: /* BOR A E */ { CLOCK += 2; A |= E; O = 0; break; }
                    case 0x85: /* BOR A H */ { CLOCK += 2; A |= H; O = 0; break; }
                    case 0x86: /* BOR A L */ { CLOCK += 2; A |= L; O = 0; break; }
                    case 0x87: /* BOR A N */ { CLOCK += 2; var X = N; A |= X; O = 0; break; }
                    case 0x88: /* BOR B A */ { CLOCK += 2; B |= A; O = 0; break; }
                    case 0x89: /* DIS     */ { CLOCK += 32; B = (byte)((sbyte)A / (sbyte)B); O = 0; break; }
                    case 0x8A: /* BOR B C */ { CLOCK += 2; B |= C; O = 0; break; }
                    case 0x8B: /* BOR B D */ { CLOCK += 2; B |= D; O = 0; break; }
                    case 0x8C: /* BOR B E */ { CLOCK += 2; B |= E; O = 0; break; }
                    case 0x8D: /* BOR B H */ { CLOCK += 2; B |= H; O = 0; break; }
                    case 0x8E: /* BOR B L */ { CLOCK += 2; B |= L; O = 0; break; }
                    case 0x8F: /* BOR B N */ { CLOCK += 2; var X = N; B |= X; O = 0; break; }
                    case 0x90: /* XOR A A */ { CLOCK += 2; A ^= A; O = 0; break; }
                    case 0x91: /* XOR A B */ { CLOCK += 2; A ^= B; O = 0; break; }
                    case 0x92: /* XOR A C */ { CLOCK += 2; A ^= C; O = 0; break; }
                    case 0x93: /* XOR A D */ { CLOCK += 2; A ^= D; O = 0; break; }
                    case 0x94: /* XOR A E */ { CLOCK += 2; A ^= E; O = 0; break; }
                    case 0x95: /* XOR A H */ { CLOCK += 2; A ^= H; O = 0; break; }
                    case 0x96: /* XOR A L */ { CLOCK += 2; A ^= L; O = 0; break; }
                    case 0x97: /* XOR A N */ { CLOCK += 2; var X = N; A ^= X; O = 0; break; }
                    case 0x98: /* XOR B A */ { CLOCK += 2; B ^= A; O = 0; break; }
                    case 0x99: /* XOR B B */ { CLOCK += 2; B ^= B; O = 0; break; }
                    case 0x9A: /* XOR B C */ { CLOCK += 2; B ^= C; O = 0; break; }
                    case 0x9B: /* XOR B D */ { CLOCK += 2; B ^= D; O = 0; break; }
                    case 0x9C: /* XOR B E */ { CLOCK += 2; B ^= E; O = 0; break; }
                    case 0x9D: /* XOR B H */ { CLOCK += 2; B ^= H; O = 0; break; }
                    case 0x9E: /* XOR B L */ { CLOCK += 2; B ^= L; O = 0; break; }
                    case 0x9F: /* XOR B N */ { CLOCK += 2; var X = N; B ^= X; O = 0; break; }
                    case 0xA0: /* PSH A   */ { CLOCK += 4; CHECK_MEM(SP - 1); MEM[(ushort)(--SP)] = A; break; }
                    case 0xA1: /* PSH B   */ { CLOCK += 4; CHECK_MEM(SP - 1); MEM[(ushort)(--SP)] = B; break; }
                    case 0xA2: /* PSH C   */ { CLOCK += 4; CHECK_MEM(SP - 1); MEM[(ushort)(--SP)] = C; break; }
                    case 0xA3: /* PSH D   */ { CLOCK += 4; CHECK_MEM(SP - 1); MEM[(ushort)(--SP)] = D; break; }
                    case 0xA4: /* PSH E   */ { CLOCK += 4; CHECK_MEM(SP - 1); MEM[(ushort)(--SP)] = E; break; }
                    case 0xA5: /* PSH H   */ { CLOCK += 4; CHECK_MEM(SP - 1); MEM[(ushort)(--SP)] = H; break; }
                    case 0xA6: /* PSH L   */ { CLOCK += 4; CHECK_MEM(SP - 1); MEM[(ushort)(--SP)] = L; break; }
                    case 0xA7: /* PSH N   */ { CLOCK += 4; CHECK_MEM(SP - 1); MEM[(ushort)(--SP)] = N; break; }
                    case 0xA8: /* POP A   */ { CLOCK += 4; CHECK_MEM(SP); A = MEM[(ushort)(SP++)]; break; }
                    case 0xA9: /* POP B   */ { CLOCK += 4; CHECK_MEM(SP); B = MEM[(ushort)(SP++)]; break; }
                    case 0xAA: /* POP C   */ { CLOCK += 4; CHECK_MEM(SP); C = MEM[(ushort)(SP++)]; break; }
                    case 0xAB: /* POP D   */ { CLOCK += 4; CHECK_MEM(SP); D = MEM[(ushort)(SP++)]; break; }
                    case 0xAC: /* POP E   */ { CLOCK += 4; CHECK_MEM(SP); E = MEM[(ushort)(SP++)]; break; }
                    case 0xAD: /* POP H   */ { CLOCK += 4; CHECK_MEM(SP); H = MEM[(ushort)(SP++)]; break; }
                    case 0xAE: /* POP L   */ { CLOCK += 4; CHECK_MEM(SP); L = MEM[(ushort)(SP++)]; break; }
                    case 0xAF: /* RSP     */ { CLOCK += 2; H = (byte)(SP >> 8); L = (byte)SP; break; }
                    case 0xB0: /* PEK A   */ { CLOCK += 2; CHECK_MEM(SP); A = MEM[SP]; break; }
                    case 0xB1: /* PEK B   */ { CLOCK += 2; CHECK_MEM(SP); B = MEM[SP]; break; }
                    case 0xB2: /* PEK C   */ { CLOCK += 2; CHECK_MEM(SP); C = MEM[SP]; break; }
                    case 0xB3: /* PEK D   */ { CLOCK += 2; CHECK_MEM(SP); D = MEM[SP]; break; }
                    case 0xB4: /* PEK E   */ { CLOCK += 2; CHECK_MEM(SP); E = MEM[SP]; break; }
                    case 0xB5: /* PEK H   */ { CLOCK += 2; CHECK_MEM(SP); H = MEM[SP]; break; }
                    case 0xB6: /* PEK L   */ { CLOCK += 2; CHECK_MEM(SP); L = MEM[SP]; break; }
                    case 0xB7: /* SSP     */ { CLOCK += 2; SP = HL; break; }
                    case 0xB8: /* LOD A   */ { CLOCK += 8; CHECK_MEM(HL); A = MEM[HL]; break; }
                    case 0xB9: /* LOD B   */ { CLOCK += 8; CHECK_MEM(HL); B = MEM[HL]; break; }
                    case 0xBA: /* LOD C   */ { CLOCK += 8; CHECK_MEM(HL); C = MEM[HL]; break; }
                    case 0xBB: /* LOD D   */ { CLOCK += 8; CHECK_MEM(HL); D = MEM[HL]; break; }
                    case 0xBC: /* LOD E   */ { CLOCK += 8; CHECK_MEM(HL); E = MEM[HL]; break; }
                    case 0xBD: /* LOD H   */ { CLOCK += 8; CHECK_MEM(HL); H = MEM[HL]; break; }
                    case 0xBE: /* LOD L   */ { CLOCK += 8; CHECK_MEM(HL); L = MEM[HL]; break; }
                    case 0xBF: /* LLT     */ { CLOCK += 2; H = N; L = N; break; }
                    case 0xC0: /* STR A   */ { CLOCK += 8; CHECK_MEM(HL); MEM[HL] = A; break; }
                    case 0xC1: /* STR B   */ { CLOCK += 8; CHECK_MEM(HL); MEM[HL] = B; break; }
                    case 0xC2: /* STR C   */ { CLOCK += 8; CHECK_MEM(HL); MEM[HL] = C; break; }
                    case 0xC3: /* STR D   */ { CLOCK += 8; CHECK_MEM(HL); MEM[HL] = D; break; }
                    case 0xC4: /* STR E   */ { CLOCK += 8; CHECK_MEM(HL); MEM[HL] = E; break; }
                    case 0xC5: /* STR H   */ { CLOCK += 8; CHECK_MEM(HL); MEM[HL] = H; break; }
                    case 0xC6: /* STR L   */ { CLOCK += 8; CHECK_MEM(HL); MEM[HL] = L; break; }
                    case 0xC7: /* STR N   */ { CLOCK += 8; CHECK_MEM(HL); MEM[HL] = N; break; }
                    case 0xC8: /* LDW     */
                        {
                            CLOCK += 12; CHECK_MEM(HL); CHECK_MEM(HL + 1);
                            if (HL % 2 == 1) ALIGN_FAULT(); A = MEM[HL]; B = MEM[HL + 1]; break;
                        }
                    case 0xC9: /* SDW     */
                        {
                            CLOCK += 12; CHECK_MEM(HL); CHECK_MEM(HL + 1);
                            if (HL % 2 == 1) ALIGN_FAULT(); MEM[HL] = A; MEM[HL + 1] = B; break;
                        }
                    case 0xCA: /* STP     */ { CLOCK += 1; STOP(); break; }
                    case 0xCB: /* CFP     */ { CLOCK += 2; H = D; L = E; break; }
                    case 0xCC: /* SWP     */ { CLOCK += 2; var T = D; D = H; H = T; T = E; E = L; L = T; break; }
                    case 0xCD: /* RIP     */ { CLOCK += 2; H = (byte)(IP >> 8); L = (byte)IP; break; }
                    case 0xCE: /* JMP     */ { CLOCK += 2; IP = HL; break; }
                    case 0xCF: /* JPL     */ { CLOCK += 2; H = N; L = N; IP = HL; break; }
                    case 0xD0: /* BIE     */ { CLOCK += 8; if (A == B) IP = HL; break; }
                    case 0xD1: /* BNE     */ { CLOCK += 8; if (A != B) IP = HL; break; }
                    case 0xD2: /* BGT     */ { CLOCK += 8; if (A > B) IP = HL; break; }
                    case 0xD3: /* BLT     */ { CLOCK += 8; if (A < B) IP = HL; break; }
                    case 0xD4: /* BEG     */ { CLOCK += 8; if (A >= B) IP = HL; break; }
                    case 0xD5: /* BEL     */ { CLOCK += 8; if (A <= B) IP = HL; break; }
                    case 0xD6: /* BSL     */ { CLOCK += 8; if ((sbyte)A < (sbyte)B) IP = HL; break; }
                    case 0xD7: /* BSG     */ { CLOCK += 8; if ((sbyte)A > (sbyte)B) IP = HL; break; }
                    case 0xD8: /* CAD A   */ { CLOCK += 4; var T = HL; T += A; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xD9: /* CAD B   */ { CLOCK += 4; var T = HL; T += B; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xDA: /* CAD C   */ { CLOCK += 4; var T = HL; T += C; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xDB: /* CAD D   */ { CLOCK += 4; var T = HL; T += D; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xDC: /* CAD E   */ { CLOCK += 4; var T = HL; T += E; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xDD: /* CAD H   */ { CLOCK += 4; var T = HL; T += H; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xDE: /* CAD L   */ { CLOCK += 4; var T = HL; T += L; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xDF: /* CAD N   */ { CLOCK += 4; var T = HL; T += N; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE0: /* CSB A   */ { CLOCK += 4; var T = HL; T -= A; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE1: /* CSB B   */ { CLOCK += 4; var T = HL; T -= B; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE2: /* CSB C   */ { CLOCK += 4; var T = HL; T -= C; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE3: /* CSB D   */ { CLOCK += 4; var T = HL; T -= D; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE4: /* CSB E   */ { CLOCK += 4; var T = HL; T -= E; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE5: /* CSB H   */ { CLOCK += 4; var T = HL; T -= H; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE6: /* CSB L   */ { CLOCK += 4; var T = HL; T -= L; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE7: /* CSB N   */ { CLOCK += 4; var T = HL; T -= N; L = (byte)T; H = (byte)(T >> 8); break; }
                    case 0xE8: /* ADS A   */ { CLOCK += 4; SP += A; break; }
                    case 0xE9: /* ADS B   */ { CLOCK += 4; SP += B; break; }
                    case 0xEA: /* ADS C   */ { CLOCK += 4; SP += C; break; }
                    case 0xEB: /* ADS D   */ { CLOCK += 4; SP += D; break; }
                    case 0xEC: /* ADS E   */ { CLOCK += 4; SP += E; break; }
                    case 0xED: /* ADS H   */ { CLOCK += 4; SP += H; break; }
                    case 0xEE: /* ADS L   */ { CLOCK += 4; SP += L; break; }
                    case 0xEF: /* ADS N   */ { CLOCK += 4; SP += N; break; }
                    case 0xF0: /* SBS A   */ { CLOCK += 4; SP -= A; break; }
                    case 0xF1: /* SBS B   */ { CLOCK += 4; SP -= B; break; }
                    case 0xF2: /* SBS C   */ { CLOCK += 4; SP -= C; break; }
                    case 0xF3: /* SBS D   */ { CLOCK += 4; SP -= D; break; }
                    case 0xF4: /* SBS E   */ { CLOCK += 4; SP -= E; break; }
                    case 0xF5: /* SBS H   */ { CLOCK += 4; SP -= H; break; }
                    case 0XF6: /* SBS L   */ { CLOCK += 4; SP -= L; break; }
                    case 0xF7: /* SBS N   */ { CLOCK += 4; SP -= N; break; }
                    case 0xF8: /* OUT     */ { CLOCK += 2; PORT[A] = B; if (HARDWARE[A] != null) HARDWARE[A].PortWritten(A, B); break; }
                    case 0xF9: /* IIN     */ { CLOCK += 2; B = PORT[A]; break; }
                    case 0xFA: /* NPE     */ { CLOCK += 1; break; }
                    case 0xFB: /* JIG     */ { CLOCK += 16; if (HL >= DE) { H = N; L = N; IP = HL; } else { IP += 2; } break; }
                    case 0xFC: /* NPG     */ { CLOCK += 1; MEMORY_FAULT(); break; }
                    case 0xFD: /* CLK     */ { CLOCK += 4; D = (byte)(CLOCK >> 24); E = (byte)(CLOCK >> 16); H = (byte)(CLOCK >> 8); L = (byte)CLOCK; break; }
                    case 0xFE: /* SFJ     */ { CLOCK += 2; IP += N; break; }
                    case 0xFF: /* SBJ     */ { CLOCK += 2; IP -= N; break; }

                }
            }
            catch (MemoryException)
            {
                MEMORY_FAULT();
                return;
            }
        }
    }
}
