using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Localization
{
    public static class LocalizationTableLoader
    {
        private const int Magic = 0x4941384E;

        public static Dictionary<int, string> LoadFromResources(string languageCode)
        {
            var asset = Resources.Load<TextAsset>($"Localization/{languageCode}");
            if (asset == null)
            {
                return null;
            }

            return Read(asset.bytes);
        }

        public static Dictionary<int, string> Read(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            var magic = reader.ReadInt32();
            if (magic != Magic)
            {
                throw new InvalidDataException("Invalid localization binary magic.");
            }

            var version = reader.ReadInt32();
            if (version != 1)
            {
                throw new InvalidDataException($"Unsupported localization binary version: {version}.");
            }

            var count = reader.ReadInt32();
            var table = new Dictionary<int, string>(count);

            for (int i = 0; i < count; i++)
            {
                var id = reader.ReadInt32();
                var text = reader.ReadString();
                table[id] = text;
            }

            return table;
        }
    }
}
