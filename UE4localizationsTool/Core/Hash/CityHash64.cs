using System;
using System.Text;
using uint32 = System.UInt32;
using uint64 = System.UInt64;
using uint8 = System.Byte;

namespace UE4localizationsTool.Core.Hash
{

    class CityHash
    {

        public static CityHash Init
        {
            get
            {
                return new CityHash();
            }
        }

        // Some primes between 2^63 and 2^64 for various uses.
        static uint64 k0 = 0xc3a5c85c97cb3127UL;
        static uint64 k1 = 0xb492b66fbe98f273UL;
        static uint64 k2 = 0x9ae16a3b2f90404fUL;

        // Magic numbers for 32-bit hashing.  Copied from Murmur3.
        static uint32 c1 = 0xcc9e2d51;
        static uint32 c2 = 0x1b873593;
        public static bool IsBigEndian { get; set; } = false;
        public class Uint128_64
        {
            public Uint128_64(ulong InLo, ulong InHi)
            {
                lo = InLo;
                hi = InHi;
            }
            public ulong lo;
            public ulong hi;
        };

        // Hash 128 input bits down to 64 bits of output.
        // This is intended to be a reasonably good hash function.
        public ulong CityHash128to64(ref Uint128_64 x)
        {
            // Murmur-inspired hashing.
            const ulong kMul = 0x9ddfea08eb382d69UL;
            ulong a = (x.lo ^ x.hi) * kMul;
            a ^= (a >> 47);
            ulong b = (x.hi ^ a) * kMul;
            b ^= (b >> 47);
            b *= kMul;
            return b;
        }


        unsafe uint64 UNALIGNED_LOAD64(byte* p)
        {

            return *(uint64*)p;
        }

        unsafe uint32 UNALIGNED_LOAD32(byte* p)
        {

            return *(uint32*)p;
        }

        protected uint32 _byteswap_ulong(uint32 value)
        {
            return ((value & 0xFFu) << 24) |
                   ((value & 0xFF00u) << 8) |
                   ((value & 0xFF0000u) >> 8) |
                   ((value >> 24) & 0xFFu);
        }

        protected uint64 _byteswap_uint64(uint64 value)
        {
            return ((value & 0xFFUL) << 56) |
                   ((value & 0xFF00UL) << 40) |
                   ((value & 0xFF0000UL) << 24) |
                   ((value & 0xFF000000UL) << 8) |
                   ((value & 0xFF00000000UL) >> 8) |
                   ((value & 0xFF0000000000UL) >> 24) |
                   ((value & 0xFF000000000000UL) >> 40) |
                   ((value >> 56) & 0xFFUL);
        }

        uint32 bswap_32(uint32 x) => _byteswap_ulong(x);
        uint64 bswap_64(uint64 x) => _byteswap_uint64(x);

        uint32 uint32_in_expected_order(uint32 x) => bswap_32(x);
        uint64 uint64_in_expected_order(uint64 x) => bswap_64(x);



        unsafe uint64 Fetch64(byte* p)
        {
            var val = IsBigEndian ? uint64_in_expected_order(UNALIGNED_LOAD64(p)) : UNALIGNED_LOAD64(p);
            return val;
        }

        unsafe uint32 Fetch32(byte* p)
        {
            var val = IsBigEndian ? uint32_in_expected_order(UNALIGNED_LOAD32(p)) : UNALIGNED_LOAD32(p);
            return val;
        }



        // A 32-bit to 32-bit integer hash copied from Murmur3.
        static uint32 fmix(uint32 h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }

        static uint32 Rotate32(uint32 val, int shift)
        {
            // Avoid shifting by 32: doing so yields an undefined result.
            return shift == 0 ? val : ((val >> shift) | (val << (32 - shift)));
        }


        void SwapValues<T>(ref T a, ref T b)
        {
            T c = a;
            a = b;
            b = c;
        }

        void PERMUTE3<T>(ref T a, ref T b, ref T c)
        {
            do
            {
                SwapValues(ref a, ref b);
                SwapValues(ref a, ref c);
            } while (true);
        }

        uint32 Mur(uint32 a, uint32 h)
        {

            // Helper from Murmur3 for combining two 32-bit values.
            a *= c1;
            a = Rotate32(a, 17);
            a *= c2;
            h ^= a;
            h = Rotate32(h, 19);
            return h * 5 + 0xe6546b64;
        }

        unsafe uint32 Hash32Len13to24(byte* s, uint32 len)
        {
            uint32 a = Fetch32(s - 4 + (len >> 1));
            uint32 b = Fetch32(s + 4);
            uint32 c = Fetch32(s + len - 8);
            uint32 d = Fetch32(s + (len >> 1));
            uint32 e = Fetch32(s);
            uint32 f = Fetch32(s + len - 4);
            uint32 h = len;

            return fmix(Mur(f, Mur(e, Mur(d, Mur(c, Mur(b, Mur(a, h)))))));
        }

        unsafe uint32 Hash32Len0to4(byte* s, uint32 len)
        {

            uint32 b = 0;
            uint32 c = 9;
            byte* bytePtr = s;
            for (uint32 i = 0; i < len; i++)
            {
                byte v = *bytePtr;
                b = b * c1 + v;
                c ^= b;
                bytePtr++;
            }
            return fmix(Mur(b, Mur(len, c)));
        }

        unsafe uint32 Hash32Len5to12(byte* s, uint32 len)
        {
            uint32 a = len, b = len * 5, c = 9, d = b;
            a += Fetch32(s);
            b += Fetch32(s + len - 4);
            c += Fetch32(s + ((len >> 1) & 4));
            return fmix(Mur(c, Mur(b, Mur(a, d))));
        }

        unsafe uint32 CityHash32(byte* s, uint32 len)
        {

            if (len <= 24)
            {
                return len <= 12 ?
                    (len <= 4 ? Hash32Len0to4(s, len) : Hash32Len5to12(s, len)) :
                    Hash32Len13to24(s, len);
            }

            // len > 24
            uint32 h = len, g = c1 * len, f = g;
            uint32 a0 = Rotate32(Fetch32(s + len - 4) * c1, 17) * c2;
            uint32 a1 = Rotate32(Fetch32(s + len - 8) * c1, 17) * c2;
            uint32 a2 = Rotate32(Fetch32(s + len - 16) * c1, 17) * c2;
            uint32 a3 = Rotate32(Fetch32(s + len - 12) * c1, 17) * c2;
            uint32 a4 = Rotate32(Fetch32(s + len - 20) * c1, 17) * c2;
            h ^= a0;
            h = Rotate32(h, 19);
            h = h * 5 + 0xe6546b64;
            h ^= a2;
            h = Rotate32(h, 19);
            h = h * 5 + 0xe6546b64;
            g ^= a1;
            g = Rotate32(g, 19);
            g = g * 5 + 0xe6546b64;
            g ^= a3;
            g = Rotate32(g, 19);
            g = g * 5 + 0xe6546b64;
            f += a4;
            f = Rotate32(f, 19);
            f = f * 5 + 0xe6546b64;
            uint32 iters = (len - 1) / 20;
            do
            {
                uint32 _a0 = Rotate32(Fetch32(s) * c1, 17) * c2;
                uint32 _a1 = Fetch32(s + 4);
                uint32 _a2 = Rotate32(Fetch32(s + 8) * c1, 17) * c2;
                uint32 _a3 = Rotate32(Fetch32(s + 12) * c1, 17) * c2;
                uint32 _a4 = Fetch32(s + 16);
                h ^= _a0;
                h = Rotate32(h, 18);
                h = h * 5 + 0xe6546b64;
                f += _a1;
                f = Rotate32(f, 19);
                f = f * c1;
                g += _a2;
                g = Rotate32(g, 18);
                g = g * 5 + 0xe6546b64;
                h ^= _a3 + _a1;
                h = Rotate32(h, 19);
                h = h * 5 + 0xe6546b64;
                g ^= _a4;
                g = bswap_32(g) * 5;
                h += _a4 * 5;
                h = bswap_32(h);
                f += _a0;
                PERMUTE3(ref f, ref h, ref g);
                s += 20;
            } while (--iters != 0);
            g = Rotate32(g, 11) * c1;
            g = Rotate32(g, 17) * c1;
            f = Rotate32(f, 11) * c1;
            f = Rotate32(f, 17) * c1;
            h = Rotate32(h + g, 19);
            h = h * 5 + 0xe6546b64;
            h = Rotate32(h, 17) * c1;
            h = Rotate32(h + f, 19);
            h = h * 5 + 0xe6546b64;
            h = Rotate32(h, 17) * c1;
            return h;
        }

        // Bitwise right rotate.  Normally this will compile to a single
        // instruction, especially if the shift is a manifest constant.
        static uint64 Rotate(uint64 val, int shift)
        {
            // Avoid shifting by 64: doing so yields an undefined result.
            return shift == 0 ? val : ((val >> shift) | (val << (64 - shift)));
        }

        static uint64 ShiftMix(uint64 val)
        {
            return val ^ (val >> 47);
        }

        uint64 HashLen16(uint64 u, uint64 v)
        {
            var val = new Uint128_64(u, v);
            return CityHash128to64(ref val);
        }

        static uint64 HashLen16(uint64 u, uint64 v, uint64 mul)
        {
            // Murmur-inspired hashing.
            uint64 a = (u ^ v) * mul;
            a ^= (a >> 47);
            uint64 b = (v ^ a) * mul;
            b ^= (b >> 47);
            b *= mul;
            return b;
        }

        unsafe uint64 HashLen0to16(byte* s, uint32 len)
        {


            if (len >= 8)
            {
                uint64 mul = k2 + len * 2;
                uint64 a = Fetch64(s) + k2;
                uint64 b = Fetch64(s + len - 8);
                uint64 c = Rotate(b, 37) * mul + a;
                uint64 d = (Rotate(a, 25) + b) * mul;
                return HashLen16(c, d, mul);
            }
            if (len >= 4)
            {
                uint64 mul = k2 + len * 2;
                uint64 a = Fetch32(s);
                return HashLen16(len + (a << 3), Fetch32(s + len - 4), mul);
            }
            if (len > 0)
            {
                uint8 a = s[0];
                uint8 b = s[len >> 1];
                uint8 c = s[len - 1];
                uint32 y = a + ((uint32)b << 8);
                uint32 z = len + ((uint32)(c) << 2);
                return ShiftMix(y * k2 ^ z * k0) * k2;
            }
            return k2;
        }

        // This probably works well for 16-byte strings as well, but it may be overkill
        // in that case.
        unsafe uint64 HashLen17to32(byte* s, uint32 len)
        {
            uint64 mul = k2 + len * 2;
            uint64 a = Fetch64(s) * k1;
            uint64 b = Fetch64(s + 8);
            uint64 c = Fetch64(s + len - 8) * mul;
            uint64 d = Fetch64(s + len - 16) * k2;
            return HashLen16(Rotate(a + b, 43) + Rotate(c, 30) + d,
                a + Rotate(b + k2, 18) + c, mul);
        }

        // Return a 16-byte hash for 48 bytes.  Quick and dirty.
        // Callers do best to use "random-looking" values for a and b.
        static Uint128_64 WeakHashLen32WithSeeds(
            uint64 w, uint64 x, uint64 y, uint64 z, uint64 a, uint64 b)
        {
            a += w;
            b = Rotate(b + a + z, 21);
            uint64 c = a;
            a += x;
            a += y;
            b += Rotate(a, 44);
            return new Uint128_64((a + z), (b + c));
        }

        // Return a 16-byte hash for s[0] ... s[31], a, and b.  Quick and dirty.
        unsafe Uint128_64 WeakHashLen32WithSeeds(
            byte* s, uint64 a, uint64 b)
        {
            return WeakHashLen32WithSeeds(Fetch64(s),
                Fetch64(s + 8),
                Fetch64(s + 16),
                Fetch64(s + 24),
                a,
                b);
        }

        // Return an 8-byte hash for 33 to 64 bytes.
        unsafe uint64 HashLen33to64(byte* s, uint32 len)
        {


            uint64 mul = k2 + len * 2;
            uint64 a = Fetch64(s) * k2;
            uint64 b = Fetch64(s + 8);
            uint64 c = Fetch64(s + len - 24);
            uint64 d = Fetch64(s + len - 32);
            uint64 e = Fetch64(s + 16) * k2;
            uint64 f = Fetch64(s + 24) * 9;
            uint64 g = Fetch64(s + len - 8);
            uint64 h = Fetch64(s + len - 16) * mul;
            uint64 u = Rotate(a + g, 43) + (Rotate(b, 30) + c) * 9;
            uint64 v = ((a + g) ^ d) + f + 1;
            uint64 w = bswap_64((u + v) * mul) + h;
            uint64 x = Rotate(e + f, 42) + c;
            uint64 y = (bswap_64((v + w) * mul) + g) * mul;
            uint64 z = e + f + c;
            a = bswap_64((x + z) * mul + y) + b;
            b = ShiftMix((z + a) * mul + d + h) * mul;
            return b + x;
        }

        unsafe uint64 CityHash64(byte* s, uint32 len)
        {
            if (len <= 32)
            {
                if (len <= 16)
                {
                    return HashLen0to16(s, len);
                }

                else
                {
                    return HashLen17to32(s, len);
                }
            }

            else if (len <= 64)
            {
                return HashLen33to64(s, len);
            }

            // For strings over 64 bytes we hash the end first, and then as we
            // loop we keep 56 bytes of state: v, w, x, y, and z.
            uint64 x = Fetch64(s + len - 40);
            uint64 y = Fetch64(s + len - 16) + Fetch64(s + len - 56);
            uint64 z = HashLen16(Fetch64(s + len - 48) + len, Fetch64(s + len - 24));
            Uint128_64 v = WeakHashLen32WithSeeds(s + len - 64, len, z);
            Uint128_64 w = WeakHashLen32WithSeeds(s + len - 32, y + k1, x);
            x = x * k1 + Fetch64(s);

            // Decrease len to the nearest multiple of 64, and operate on 64-byte chunks.
            len = (len - 1) & ~(uint32)(63);
            do
            {
                x = Rotate(x + y + v.lo + Fetch64(s + 8), 37) * k1;
                y = Rotate(y + v.hi + Fetch64(s + 48), 42) * k1;
                x ^= w.hi;
                y += v.lo + Fetch64(s + 40);
                z = Rotate(z + w.lo, 33) * k1;
                v = WeakHashLen32WithSeeds(s, v.hi * k1, x + w.lo);
                w = WeakHashLen32WithSeeds(s + 32, z + w.hi, y + Fetch64(s + 16));
                SwapValues(ref z, ref x);
                s += 64;
                len -= 64;
            } while (len != 0);
            return HashLen16(HashLen16(v.lo, w.lo) + ShiftMix(y) * k1 + z,
                HashLen16(v.hi, w.hi) + x);
        }

        public ulong CityHash64(byte[] s)
        {
            unsafe
            {
                fixed (byte* ptr = s)
                {
                    return CityHash64(ptr, (uint)s.Length);
                }
            }
        }

        public ulong CityHash64(string s)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(s);

            unsafe
            {
                fixed (byte* ptr = byteArray)
                {
                    return CityHash64(ptr, (uint)byteArray.Length);
                }
            }
        }

        unsafe uint64 CityHash64WithSeed(byte* s, uint32 len, uint64 seed)
        {


            return CityHash64WithSeeds(s, len, k2, seed);
        }

        unsafe uint64 CityHash64WithSeeds(byte* s, uint32 len, uint64 seed0, uint64 seed1)
        {


            return HashLen16(CityHash64(s, len) - seed0, seed1);
        }

    }
}
