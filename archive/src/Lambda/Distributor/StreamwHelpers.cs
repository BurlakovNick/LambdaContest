using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Distributor
{
	public static class StreamwHelpers
	{
	    public static readonly Encoding UTF8WithoutPreamble = new UTF8Encoding(false);
        private static readonly byte[] newLineBytes = Encoding.ASCII.GetBytes(Environment.NewLine);

		public static void WriteBytes(this Stream stream, byte[] bytes)
		{
			if (bytes != null && bytes.Length != 0)
				stream.Write(bytes, 0, bytes.Length);
		}

		public static void WriteASCIILine(this Stream stream, string line)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(line + "\r\n");
			stream.Write(bytes, 0, bytes.Length);
		}

		public static void WriteASCIIString(this Stream stream, string line)
		{
			WriteString(stream, line, Encoding.ASCII);
		}

		public static void WriteUTF8String(this Stream stream, string line)
		{
			WriteString(stream, line, UTF8WithoutPreamble);
		}

		public static void WriteUTF8Line(this Stream stream, string line)
		{
			stream.WriteUTF8String(line + "\r\n");
		}

		public static void WriteLine(this Stream stream, string line, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(line + "\r\n");
			stream.Write(bytes, 0, bytes.Length);
		}

		public static void WriteString(this Stream stream, string line, Encoding encoding)
		{
			if (line == null)
				return;
			var bytes = encoding.GetBytes(line);
			stream.Write(bytes, 0, bytes.Length);
		}

		public static void WriteEndOfLine(this Stream stream)
		{
			WriteBytes(stream, newLineBytes);
		}

		public static byte[] ReadLine(this Stream stream)
		{
			var memoryStream = new MemoryStream();
			int second = stream.ReadByte();

			if (second == -1)
				return null;

			do
			{
				int first = second;
				second = stream.ReadByte();
				if (second == 10)
				{
					if (first != 13) memoryStream.WriteByte((byte)first);
					break;
				}
				memoryStream.WriteByte((byte)first);
			} while (second >= 0);

			return memoryStream.ToArray();
		}

		public static byte[] ReadLineWithEndOfLine(this Stream stream)
		{
			var memoryStream = new MemoryStream();
			int second = stream.ReadByte();

			if (second == -1)
				return null;

			memoryStream.WriteByte((byte)second);
			do
			{
				int first = second;
				second = stream.ReadByte();
				memoryStream.WriteByte((byte)second);
				if (first == 13 && second == 10)
					break;
			} while (second >= 0);

			return memoryStream.ToArray();
		}

		public static Stream LoadInMemory(this Stream stream)
		{
			return new MemoryStream(stream.ReadToEnd());
		}

		public static byte[] ReadToEnd(this Stream stream)
		{
			const int bufferSize = 1024;
			var buffer = new byte[bufferSize];
			var result = new MemoryStream();
			int size;
			do
			{
				size = stream.Read(buffer, 0, bufferSize);
				result.Write(buffer, 0, size);
			} while (size > 0);
			return result.ToArray();
		}

		public static string ReadASCIILineString(this Stream stream)
		{
			return ReadLineString(stream, Encoding.ASCII);
		}

		public static string ReadLineString(this Stream stream, Encoding encoding)
		{
			byte[] line = ReadLine(stream);
			return line == null ? null : encoding.GetString(line);
		}

		public static string ReadString(this Stream stream, Encoding encoding)
		{
			return encoding.GetString(stream.ReadToEnd());
		}

		public static string ReadASCIIString(this Stream stream)
		{
			return stream.ReadString(Encoding.ASCII);
		}

		public static string ReadUTF8String(this Stream stream)
		{
			return stream.ReadString(UTF8WithoutPreamble);
		}

		public static void AppendGuidDelimiter(this Stream stream, Guid guid)
		{
			WriteBytes(stream, Encoding.ASCII.GetBytes(Environment.NewLine + guid + Environment.NewLine));
		}

		public static bool EndsWith(byte[] buffer, byte[] substring)
		{
			int length = substring.Length;
			int lengthDiff = buffer.Length - length;
			if (lengthDiff < 0)
				return false;
			for (int i = length - 1; i >= 0; --i)
				if (buffer[lengthDiff + i] != substring[i])
					return false;
			return true;
		}

		public static bool ByteArraysAreEqual(byte[] left, byte[] right)
		{
			int length = left.Length;
			if (length != right.Length)
				return false;
			for (int i = 0; i < length; ++i)
				if (left[i] != right[i]) return false;
			return true;
		}

		public static void WriteTo(this Stream stream, XDocument document, Encoding encoding)
		{
			var writer = new XmlTextWriter(stream, encoding);
			document.Save(writer);
			writer.Flush();
		}

		public static byte[] ReadBytes(this Stream stream, int size)
		{
			var headerBuffer = new byte[size];
			stream.Read(headerBuffer, 0, size);
			return headerBuffer;
		}

		public static T ReadXml<T>(this Stream stream)
		{
			return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
		}

		public static MemoryStream RunOnStream(Action<Stream> action)
		{
			var stream = new MemoryStream();
			action(stream);
			stream.Position = 0;
			return stream;
		}
	}
}