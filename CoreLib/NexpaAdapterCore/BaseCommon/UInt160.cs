using NexpaAdapterStandardLib.IO;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace NpmCommon
{
    /// <summary>
    /// 20 바이트 little-endian 바이트 배열로 표현되는 160 비트 unsigned int
    /// 구성 : ulong(64) + ulong(64) + uint(32) = UInt160(160)
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public class UInt160 : IComparable<UInt160>, IEquatable<UInt160>, ISerializable
    {
        public const int Length = 20;
        public static readonly UInt160 Zero = new UInt160();

        [FieldOffset(0)] private ulong value1;
        [FieldOffset(8)] private ulong value2;
        [FieldOffset(16)] private uint value3;

        public int Size => Length;

        public UInt160()
        {
        }

        public unsafe UInt160(ReadOnlySpan<byte> value)
        {
            fixed (ulong* p = &value1)
            {
                Span<byte> dst = new Span<byte>(p, Length);
                value[..Length].CopyTo(dst);
            }
        }

        /// <summary>
        /// 이 UInt160이 다른 UInt160보다 크면 CompareTo 메서드는 1을 반환합니다. 더 작 으면 -1; 같으면 0
        /// 예 : assume this is 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4, this.CompareTo(02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) returns 1
        /// </summary>
        public int CompareTo(UInt160 other)
        {
            int result = value3.CompareTo(other.value3);
            if (result != 0) return result;
            result = value2.CompareTo(other.value2);
            if (result != 0) return result;
            return value1.CompareTo(other.value1);
        }

        public void Deserialize(BinaryReader reader)
        {
            value1 = reader.ReadUInt64();
            value2 = reader.ReadUInt64();
            value3 = reader.ReadUInt32();
        }

        /// <summary>
        /// 메소드 Equals는 객체가 같으면 true를 반환하고 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            return Equals(obj as UInt160);
        }

        /// <summary>
        /// 메소드 Equals는 객체가 같으면 true를 반환하고 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public bool Equals(UInt160 other)
        {
            if (other is null) return false;
            return value1 == other.value1
                && value2 == other.value2
                && value3 == other.value3;
        }

        public override int GetHashCode()
        {
            return (int)value1;
        }

        /// <summary>
        /// 메소드 Parse는 빅 엔디안 16 진수 문자열을 수신하고 UInt160 리틀 엔디안 20 바이트 배열로 저장합니다.
        /// Example: Parse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01") should create UInt160 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        /// </summary>
        public static UInt160 Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException();
            if (value.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                value = value.Substring(2);
            if (value.Length != Length * 2)
                throw new FormatException();
            byte[] data = value.HexToBytes();
            Array.Reverse(data);
            return new UInt160(data);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(value1);
            writer.Write(value2);
            writer.Write(value3);
        }

        public override string ToString()
        {
            return "0x" + this.ToArray().ToHexString(reverse: true);
        }

        /// <summary>
        /// TryParse 메서드는 big-endian 16 진수 문자열을 구문 분석하고이를 UInt160 little-endian 20 바이트 배열로 저장하려고합니다.
        /// Example: TryParse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff01", result) should create result UInt160 01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        /// </summary>
        public static bool TryParse(string s, out UInt160 result)
        {
            if (s == null)
            {
                result = null;
                return false;
            }
            if (s.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                s = s.Substring(2);
            if (s.Length != Length * 2)
            {
                result = null;
                return false;
            }
            byte[] data = new byte[Length];
            for (int i = 0; i < Length; i++)
                if (!byte.TryParse(s.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier, null, out data[i]))
                {
                    result = null;
                    return false;
                }
            Array.Reverse(data);
            result = new UInt160(data);
            return true;
        }

        /// <summary>
        /// 왼쪽 UInt160이 오른쪽 UInt160과 같으면 true를 반환합니다.
        /// </summary>
        public static bool operator ==(UInt160 left, UInt160 right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// 왼쪽 UIntBase가 오른쪽 UIntBase와 같지 않으면 true를 반환합니다.
        /// </summary>
        public static bool operator !=(UInt160 left, UInt160 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 연산자> 왼쪽 UInt160이 오른쪽 UInt160보다 큰 경우 true를 반환합니다.
        /// Example: UInt160(01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) > UInt160 (02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        /// </summary>
        public static bool operator >(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// 연산자> 왼쪽 UInt160이 오른쪽 UInt160보다 크거나 같으면 true를 반환합니다.
        /// Example: UInt160(01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) >= UInt160 (02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        /// </summary>
        public static bool operator >=(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// 연산자> 왼쪽 UInt160이 오른쪽 UInt160보다 작 으면 true를 반환합니다.
        /// Example: UInt160(02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) < UInt160 (01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        /// </summary>
        public static bool operator <(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// 연산자> 왼쪽 UInt160이 오른쪽 UInt160보다 작거나 같으면 true를 반환합니다.
        /// Example: UInt160(02ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) < UInt160 (01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        /// </summary>
        public static bool operator <=(UInt160 left, UInt160 right)
        {
            return left.CompareTo(right) <= 0;
        }
    }


}
