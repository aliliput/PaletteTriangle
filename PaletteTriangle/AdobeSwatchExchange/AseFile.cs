using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PaletteTriangle.AdobeSwatchExchange
{
    public class AseFile
    {
        public static AseFile FromStream(Stream stream)
        {
            var result = new AseFile();

            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            if (!buffer.SequenceEqual(new byte[] { 0x41, 0x53, 0x45, 0x46 }))
                throw new InvalidDataException("Adobe Swatch Exchange ファイルではありません。");

            stream.Read(buffer, 0, 4);
            Array.Reverse(buffer);
            result.Version = new Version(
                BitConverter.ToInt16(buffer, 2),
                BitConverter.ToInt16(buffer, 0)
            );

            stream.Read(buffer, 0, 4);
            Array.Reverse(buffer);
            var blockCount = BitConverter.ToInt32(buffer, 0);

            var groups = new List<Group>();
            var colors = new List<ColorEntry>();
            Group group = null;
            for (var _ = 0; _ < blockCount; _++)
            {
                stream.Read(buffer, 0, 2);
                if (buffer[0] == 0xc0 && buffer[1] == 0x01)
                {
                    // Group start
                    group = new Group();

                    stream.Seek(4, SeekOrigin.Current);

                    stream.Read(buffer, 0, 2);
                    Array.Reverse(buffer, 0, 2);
                    var nameLength = BitConverter.ToUInt16(buffer, 0);
                    group.Name = String.Join("",
                        Enumerable.Range(0, nameLength)
                            .Select(i =>
                            {
                                stream.Read(buffer, 0, 2);
                                Array.Reverse(buffer, 0, 2);
                                return BitConverter.ToChar(buffer, 0);
                            })
                    ).TrimEnd('\0');
                }
                else if (buffer[0] == 0x00 && buffer[1] == 0x01)
                {
                    // Color entry
                    var color = new ColorEntry();

                    stream.Read(buffer, 0, 4);
                    Array.Reverse(buffer);
                    var blockLength = BitConverter.ToInt32(buffer, 0);

                    stream.Read(buffer, 0, 2);
                    Array.Reverse(buffer, 0, 2);
                    var nameLength = BitConverter.ToUInt16(buffer, 0);
                    color.Name = String.Join("",
                        Enumerable.Range(0, nameLength)
                            .Select(i =>
                            {
                                stream.Read(buffer, 0, 2);
                                Array.Reverse(buffer, 0, 2);
                                return BitConverter.ToChar(buffer, 0);
                            })
                    ).TrimEnd('\0');

                    stream.Read(buffer, 0, 4);
                    color.Model = buffer.SequenceEqual("CMYK".Select(c => (byte)c))
                        ? ColorModel.CMYK
                        : buffer.SequenceEqual("RGB ".Select(c => (byte)c))
                            ? ColorModel.RGB
                            : buffer.SequenceEqual("LAB ".Select(c => (byte)c))
                                ? ColorModel.LAB
                                : ColorModel.Gray;

                    blockLength -= 2 // name length
                        + nameLength * 2 // name
                        + 4 // model
                        + 2; // type

                    color.Values = Enumerable.Range(0, blockLength / 4)
                        .Select(i =>
                        {
                            stream.Read(buffer, 0, 4);
                            Array.Reverse(buffer);
                            return BitConverter.ToSingle(buffer, 0);
                        })
                        .ToArray();

                    stream.Read(buffer, 0, 2);
                    Array.Reverse(buffer, 0, 2);
                    color.Type = (ColorType)BitConverter.ToInt16(buffer, 0);

                    colors.Add(color);
                }
                else if (buffer[0] == 0xc0 && buffer[1] == 0x02)
                {
                    // Group end
                    stream.Read(buffer, 0, 4);
                    Array.Reverse(buffer);
                    var blockLength = BitConverter.ToInt32(buffer, 0);
                    stream.Seek(blockLength, SeekOrigin.Current);

                    group.Colors = colors.ToArray();
                    groups.Add(group);
                    group = null;
                }
                else
                {
                    throw new InvalidDataException("不明なグループです。場所: " + stream.Position);
                }
            }

            result.Groups = groups.ToArray();
            return result;
        }

        public static AseFile FromFile(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return FromStream(stream);
            }
        }

        public Version Version { get; set; }
        public Group[] Groups { get; set; }
    }
}
