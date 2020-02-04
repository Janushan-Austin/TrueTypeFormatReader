using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueTypeFormatReader
{
	public class BinaryReader
	{
		Byte[] Data;
		uint Pos;
		public BinaryReader(byte[] buffer)
		{
			Data = buffer;
			Pos = 0;
		}

		public uint Seek(uint pos)
		{
			uint OldPos = Pos;
			Pos = pos;
			return OldPos;
		}

		public uint Tell()
		{
			return Pos;
		}

		public byte getUint8()
		{
			byte result = Data[Pos];
			Pos++;
			return result;

		}

		public ushort getUint16()
		{
			ushort result = 0;
			result = (ushort)(getUint8() << 8);
			result |= getUint8();
			return result;
		}

		public uint getUint32()
		{
			uint result = 0;
			result = (uint)(getUint8() << 24);
			result |= (uint)(getUint8() << 16);
			result |= (uint)(getUint8() << 8);
			result |= (uint)(getUint8());
			return result;
		}

		public short getInt16()
		{
			short result = (short)getUint16();
			return result;
		}

		public int getInt32()
		{
			int result = getUint8() << 24;
			result |= getUint8() << 16;
			result |= getUint8() << 8;
			result |= getUint8();
			return result;
		}

		public ushort getFword()
		{
			return getUint16();
		}

		public decimal get2Dot14()
		{
			return getInt16() / (decimal)(1 << 14);
		}

		public decimal getFixed()
		{
			return getUint32() / (decimal)(1 << 16);
		}

		public string getString(uint length)
		{
			string result = "";
			for (uint i = 0; i < length; i++)
			{
				result += (char)getUint8();
			}

			return result;
		}
		public string getString(int length)
		{
			if (length > 0)
			{
				return getString((uint)length);
			}
			else
			{
				return "";
			}
		}

		public Int64 getDate()
		{
			Int64 fileTime = getUint32() * 0x100000000 + getUint32();
			Int64 utcTime = fileTime + -2082844800; // -2082844800 is UTC January 1 1904 (start time of utc)
			return utcTime;
		}
	}
}
