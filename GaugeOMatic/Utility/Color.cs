using FFXIVClientStructs.FFXIV.Client.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

// ReSharper disable UnusedMember.Global

namespace GaugeOMatic.Utility;

public static class Color
{
    public interface IColor
    {
        public Vector4 AsVec4();
        public Vector3 AsVec3();
        public IColor FromVec4(Vector4 v);
        public IColor FromVec3(Vector3 v);
    }

    /// <summary>this is just a reimplemenetation of <see cref ="ByteColor"/> but look, i gave it all these operators</summary>
    public struct ColorRGB : IColor
    {
        public byte R = 255;
        public byte G = 255;
        public byte B = 255;
        public byte A = 255;

        public ColorRGB(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public ColorRGB(byte all)
        {
            R = all;
            G = all;
            B = all;
            A = 255;
        }

        private static byte FromFloat(float f) => (byte)(f * 255f);
        public static implicit operator ColorRGB(Vector3 v) => new(FromFloat(v.X), FromFloat(v.Y), FromFloat(v.Z));
        public static implicit operator ColorRGB(Vector4 v) => new(FromFloat(v.X), FromFloat(v.Y), FromFloat(v.Z), FromFloat(v.W));
        public static implicit operator ColorRGB(ByteColor b) => new(b.R, b.G, b.B, b.A);
        public static implicit operator ColorRGB(uint u) => new((byte)(u >> 24), (byte)(u >> 16), (byte)(u >> 8), (byte)u);

        public static implicit operator ColorRGB(string s) => Convert.ToUInt32(s, 16);
        public static implicit operator ColorRGB(byte s) => new(s);

        private static float ToFloat(byte b) => b / 255f;
        public static implicit operator Vector3(ColorRGB c) => new(ToFloat(c.R), ToFloat(c.G), ToFloat(c.B));
        public static implicit operator Vector4(ColorRGB c) => new(c, c.A / 255f);
        public static implicit operator ByteColor(ColorRGB c) => new() { R = c.R, G = c.G, B = c.B, A = c.A };
        public static implicit operator uint(ColorRGB c) => (uint)((c.R << 24) + (c.G << 16) + (c.B << 8) + c.A);
        public static implicit operator string(ColorRGB c) => ((uint)c).ToString("x");

        public readonly uint ToRGBAu => (uint)((R << 24) + (G << 16) + (B << 8) + A);
        public static ColorRGB FromAGBR(uint u) => new((byte)u, (byte)(u >> 16), (byte)(u >> 8), (byte)(u >> 24));

        public readonly uint ToABGR => (uint)((A << 24) + (B << 16) + (G << 8) + R);

        public readonly Vector4 AsVec4() => this;
        public readonly Vector3 AsVec3() => this;
        public readonly IColor FromVec4(Vector4 v) => (ColorRGB)v;
        public readonly IColor FromVec3(Vector3 v) => (ColorRGB)v;
    }

    /// <summary>
    /// kinda like <see cref ="ColorRGB"/> but for making Add values color picker friendly.<br/>
    /// also still throws in the alpha because sometimes i want that alongside the Adds
    /// </summary>
    public struct AddRGB : IColor, IEquatable<AddRGB>
    {
        public short R = 0;
        public short G = 0;
        public short B = 0;
        public byte A = 255;

        public AddRGB(short r, short g, short b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public AddRGB(short all)
        {
            R = all;
            G = all;
            B = all;
            A = 255;
        }

        private static short FromFloat(float f) => (short)((f - 0.5f) * 510);
        public static implicit operator AddRGB(Vector3 v) => new(FromFloat(v.X), FromFloat(v.Y), FromFloat(v.Z));

        public static implicit operator AddRGB(Vector4 v) => new(FromFloat(v.X), FromFloat(v.Y), FromFloat(v.Z), (byte)(v.W * 255f));

        public static implicit operator AddRGB(string s) => (Vector4)(ColorRGB)s;
        public static implicit operator AddRGB(short s) => new(s);

        private static float ToFloat(short s) => (float)((s / 510f) + 0.5);
        public static implicit operator Vector3(AddRGB a) => new(ToFloat(a.R), ToFloat(a.G), ToFloat(a.B));
        public static implicit operator Vector4(AddRGB a) => new(a, a.A / 255f);
        public static implicit operator string(AddRGB a) => (ColorRGB)(Vector4)a;

        public static AddRGB operator +(AddRGB a, AddRGB b) => new((short)(a.R + b.R), (short)(a.G + b.G), (short)(a.B + b.B), (byte)Math.Clamp(a.A + b.A, 0, 255));
        public static AddRGB operator -(AddRGB a, AddRGB b) => new((short)(a.R - b.R), (short)(a.G - b.G), (short)(a.B - b.B), (byte)Math.Clamp(a.A - b.A, 0, 255));
        public static AddRGB operator *(AddRGB a, float n) => new((short)(a.R * n), (short)(a.G * n), (short)(a.B * n));
        public static AddRGB operator *(float n, AddRGB a) => a * n;
        public static AddRGB operator /(AddRGB a, float n) => a * (1f / n);

        public readonly Vector4 AsVec4() => this;
        public readonly Vector3 AsVec3() => this;
        public readonly IColor FromVec4(Vector4 v) => (AddRGB)v;
        public readonly IColor FromVec3(Vector3 v) => (AddRGB)v;

        public readonly AddRGB Transform(int min, int max)
        {
            var oldMax = Math.Max(Math.Max(R, G), B);
            var oldMin = Math.Min(Math.Min(R, G), B);

            if (oldMax == oldMin) return new((short)((min + max) / 2));

            var r = (short)(((R - oldMin) / (oldMax - oldMin) * (max - min)) + min);
            var g = (short)(((G - oldMin) / (oldMax - oldMin) * (max - min)) + min);
            var b = (short)(((B - oldMin) / (oldMax - oldMin) * (max - min)) + min);

            return new(r, g, b);
        }

        public readonly uint ToBitField() =>
            (uint)(Math.Clamp(R, -255L, 255L) + 255) +
            ((uint)(Math.Clamp(G, -255L, 255L) + 255u) << 10) +
            ((uint)(Math.Clamp(B, -255L, 255L) + 255u) << 22);

        public readonly bool Equals(AddRGB other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override readonly bool Equals(object? obj) => obj is AddRGB other && Equals(other);
        public override readonly int GetHashCode() => HashCode.Combine(R, G, B, A);
        public static bool operator ==(AddRGB left, AddRGB right) => left.Equals(right);
        public static bool operator !=(AddRGB left, AddRGB right) => !left.Equals(right);
    }

    public struct ColorSet
    {
        public ColorRGB Base = new(255, 255, 255);
        public AddRGB Add = new(0, 0, 0);
        public ColorRGB Multiply = new(100, 100, 100);

        public ColorSet()
        {
            Base = new(255, 255, 255);
            Add = new(0, 0, 0);
            Multiply = new(100, 100, 100);
        }

        public ColorSet(ColorRGB @base, AddRGB add, ColorRGB multiply) : this()
        {
            Base = @base;
            Add = add;
            Multiply = multiply;
        }
    }

    // ReSharper disable once UnusedType.Global
    public struct Gradient
    {
        public SortedList<uint, ColorRGB> Points;

        public Gradient()
        {
            Points = new()
            {
                {0,new(0,0,0)},
                {100,new(255,255,255)}
            };
        }

        public Gradient(params KeyValuePair<uint, ColorRGB>[] points)
        {
            Points = new();
            foreach (var p in points) Points.Add(p.Key, p.Value);
        }

        public readonly ColorRGB ColorAt(float f)
        {
            var f100 = f * 100;
            if (Points.Count < 1) return new();
            if (Points.Count < 2) return Points[0];
            if (f100 < Points.Keys.Min()) return Points[Points.Keys.Min()];
            if (f100 > Points.Keys.Max()) return Points[Points.Keys.Max()];

            var after = Points.Last(p => p.Key < f100);
            var before = Points.First(p => p.Key > f100);

            return Vector4.Lerp(before.Value, after.Value, (f100 - before.Key) / (after.Key - before.Key));
        }
    }
}
