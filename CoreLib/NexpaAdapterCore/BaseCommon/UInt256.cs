using NexpaAdapterStandardLib.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NexpaAdapterStandardLib
{
    /// <summary>
    /// 32바이트 little-endian 바이트 배열로 표현되는 256비트 unsigned int.
    /// 구성 : ulong(64) + ulong(64) + ulong(64) + ulong(64) = UInt256(256)
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public class UInt256 : IComparable<UInt256>, IEquatable<UInt256>, ISerializable
    {
        public const int Length = 32;
        public static readonly UInt256 Zero = new UInt256();

        [FieldOffset(0)] private ulong value1;
        [FieldOffset(8)] private ulong value2;
        [FieldOffset(16)] private ulong value3;
        [FieldOffset(24)] private ulong value4;

        public int Size => Length;

        public UInt256()
        {
        }

        public unsafe UInt256(ReadOnlySpan<byte> value)
        {
            fixed (ulong* p = &value1)
            {
                Span<byte> dst = new Span<byte>(p, Length);
                value[..Length].CopyTo(dst);
            }
        }

        /// <summary>
        /// other 보다 큰 경우 1을 반환, 작으면 -1, 같으면 0
        /// 예 : 01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4 라 가정했을때, 
        /// this.CompareTo(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) returns 1
        /// </summary>
        public int CompareTo(UInt256 other)
        {
            int result = value4.CompareTo(other.value4);
            if (result != 0) return result;
            result = value3.CompareTo(other.value3);
            if (result != 0) return result;
            result = value2.CompareTo(other.value2);
            if (result != 0) return result;
            return value1.CompareTo(other.value1);
        }

        public void Deserialize(BinaryReader reader)
        {
            value1 = reader.ReadUInt64();
            value2 = reader.ReadUInt64();
            value3 = reader.ReadUInt64();
            value4 = reader.ReadUInt64();
        }

        /// <summary>
        /// obj가 같으면 true를 반환하고 그렇지 않으면 false를 반환
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            return Equals(obj as UInt256);
        }

        /// <summary>
        /// obj가 같으면 true를 반환하고 그렇지 않으면 false를 반환
        /// </summary>
        public bool Equals(UInt256 other)
        {
            if (other is null) return false;
            return value1 == other.value1
                && value2 == other.value2
                && value3 == other.value3
                && value4 == other.value4;
        }

        public override int GetHashCode()
        {
            return (int)value1;
        }

        /// <summary>
        /// big-endian 16 진수 문자열을 수신하고 UInt256 little-endian 32 바이트 배열로 저장
        /// 예 : Parse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff01") should create UInt256 01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        /// </summary>
        public static UInt256 Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException();
            if (s.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                s = s.Substring(2);
            if (s.Length != Length * 2)
                throw new FormatException();
            byte[] data = s.HexToBytes();
            Array.Reverse(data);
            return new UInt256(data);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(value1);
            writer.Write(value2);
            writer.Write(value3);
            writer.Write(value4);
        }

        public override string ToString()
        {
            return "0x" + this.ToArray().ToHexString(reverse: true);
        }

        /// <summary>
        /// big-endian 16 진수 문자열을 구문 분석하고 이를 UInt256 little-endian 32 바이트 배열로 저장
        /// 예 : TryParse("0xa400ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff01", result) should create result UInt256 01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4
        /// </summary>
        public static bool TryParse(string s, out UInt256 result)
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
            result = new UInt256(data);
            return true;
        }

        /// <summary>
        /// 왼쪽 UInt256이 오른쪽 UInt256과 같으면 true를 반환
        /// </summary>
        public static bool operator ==(UInt256 left, UInt256 right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// 왼쪽 UIntBase가 오른쪽 UIntBase와 같지 않으면 true를 반환
        /// </summary>
        public static bool operator !=(UInt256 left, UInt256 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 연산자> 왼쪽 UInt256이 오른쪽 UInt256보다 큰 경우 true를 반환
        /// 예 : UInt256(01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) > UInt256(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        /// </summary>
        public static bool operator >(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// 연산자> = 왼쪽 UInt256이 오른쪽 UInt256보다 크거나 같으면 true를 반환
        /// 예 : UInt256(01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) >= UInt256(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) is true
        /// </summary>
        public static bool operator >=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// 연산자 <왼쪽 UInt256이 오른쪽 UInt256보다 작 으면 true를 반환
        /// 예 : UInt256(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) < UInt256(01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        /// </summary>
        public static bool operator <(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// 연산자 <=는 왼쪽 UInt256이 오른쪽 UInt256보다 작거나 같으면 true를 반환
        /// 예 : UInt256(02ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a3) <= UInt256(01ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00ff00a4) is true
        /// </summary>
        public static bool operator <=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) <= 0;
        }
    }
}
