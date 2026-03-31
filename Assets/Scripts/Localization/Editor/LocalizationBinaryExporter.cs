using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Game.Localization.Editor
{
    public static class LocalizationBinaryExporter
    {
        private const int Magic = 0x4941384E;
        private const int Version = 1;
        private const string SourceCsvPath = "Assets/Localization/CSV/i18n_text.csv";
        private const string OutputDirectory = "Assets/Resources/Localization";
        private const string GeneratedKeyPath = "Assets/Scripts/Localization/Generated/I18nKey.cs";

        [MenuItem("Tools/Localization/Export Binary Tables")]
        public static void Export()
        {
            if (!File.Exists(SourceCsvPath))
            {
                Debug.LogError($"Localization CSV not found: {SourceCsvPath}");
                return;
            }

            var rows = CsvUtility.Read(SourceCsvPath);
            if (rows.Count < 2)
            {
                Debug.LogError("Localization CSV is empty.");
                return;
            }

            var header = rows[0];
            var idIndex = RequireColumn(header, "id");
            var keyIndex = RequireColumn(header, "key");
            var commentIndex = FindColumn(header, "comment");
            var moduleIndex = FindColumn(header, "module");
            var languageColumns = CollectLanguageColumns(header, idIndex, keyIndex, commentIndex, moduleIndex);

            if (languageColumns.Count == 0)
            {
                Debug.LogError("Localization CSV does not contain any language columns.");
                return;
            }

            var keys = new List<KeyDefinition>(rows.Count - 1);
            var tables = new Dictionary<string, Dictionary<int, string>>(languageColumns.Count, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < languageColumns.Count; i++)
            {
                tables[languageColumns[i].Name] = new Dictionary<int, string>(rows.Count - 1);
            }

            var usedIds = new HashSet<int>();
            var usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                if (IsRowEmpty(row))
                {
                    continue;
                }

                var idText = GetCell(row, idIndex);
                var key = GetCell(row, keyIndex);

                if (!int.TryParse(idText, out var id))
                {
                    throw new InvalidDataException($"Invalid id '{idText}' at row {rowIndex + 1}.");
                }

                if (!usedIds.Add(id))
                {
                    throw new InvalidDataException($"Duplicate id '{id}' at row {rowIndex + 1}.");
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new InvalidDataException($"Empty key at row {rowIndex + 1}.");
                }

                if (!usedKeys.Add(key))
                {
                    throw new InvalidDataException($"Duplicate key '{key}' at row {rowIndex + 1}.");
                }

                keys.Add(new KeyDefinition(id, key));

                for (int i = 0; i < languageColumns.Count; i++)
                {
                    var column = languageColumns[i];
                    tables[column.Name][id] = GetCell(row, column.Index);
                }
            }

            Directory.CreateDirectory(OutputDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(GeneratedKeyPath) ?? "Assets/Scripts");

            foreach (var pair in tables)
            {
                WriteBinary(Path.Combine(OutputDirectory, $"{pair.Key}.bytes"), pair.Value);
            }

            WriteKeyClass(GeneratedKeyPath, keys);
            AssetDatabase.Refresh();
            Debug.Log($"Localization export completed. Languages: {tables.Count}, Keys: {keys.Count}");
        }

        private static int RequireColumn(IReadOnlyList<string> header, string name)
        {
            var index = FindColumn(header, name);
            if (index < 0)
            {
                throw new InvalidDataException($"Required column not found: {name}");
            }

            return index;
        }

        private static int FindColumn(IReadOnlyList<string> header, string name)
        {
            for (int i = 0; i < header.Count; i++)
            {
                if (string.Equals(header[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static List<LanguageColumn> CollectLanguageColumns(
            IReadOnlyList<string> header,
            int idIndex,
            int keyIndex,
            int commentIndex,
            int moduleIndex)
        {
            var result = new List<LanguageColumn>();

            for (int i = 0; i < header.Count; i++)
            {
                if (i == idIndex || i == keyIndex || i == commentIndex || i == moduleIndex)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(header[i]))
                {
                    continue;
                }

                result.Add(new LanguageColumn(header[i], i));
            }

            return result;
        }

        private static bool IsRowEmpty(IReadOnlyList<string> row)
        {
            for (int i = 0; i < row.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(row[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetCell(IReadOnlyList<string> row, int index)
        {
            if (index < 0 || index >= row.Count)
            {
                return string.Empty;
            }

            return row[index]?.Replace("\\n", "\n") ?? string.Empty;
        }

        private static void WriteBinary(string outputPath, IReadOnlyDictionary<int, string> table)
        {
            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write(Magic);
            writer.Write(Version);
            writer.Write(table.Count);

            foreach (var pair in table.OrderBy(item => item.Key))
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value ?? string.Empty);
            }
        }

        private static void WriteKeyClass(string outputPath, IReadOnlyList<KeyDefinition> keys)
        {
            var builder = new StringBuilder(1024);
            builder.AppendLine("namespace Game.Localization");
            builder.AppendLine("{");
            builder.AppendLine("    public static class I18nKey");
            builder.AppendLine("    {");

            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                builder.Append("        public const int ");
                builder.Append(ToConstantName(key.Name));
                builder.Append(" = ");
                builder.Append(key.Id);
                builder.AppendLine(";");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            File.WriteAllText(outputPath, builder.ToString(), new UTF8Encoding(false));
        }

        private static string ToConstantName(string key)
        {
            var builder = new StringBuilder(key.Length);

            for (int i = 0; i < key.Length; i++)
            {
                var ch = key[i];
                if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(char.ToUpperInvariant(ch));
                }
                else
                {
                    builder.Append('_');
                }
            }

            if (builder.Length == 0 || char.IsDigit(builder[0]))
            {
                builder.Insert(0, '_');
            }

            return builder.ToString();
        }

        private readonly struct KeyDefinition
        {
            public KeyDefinition(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; }

            public string Name { get; }
        }

        private readonly struct LanguageColumn
        {
            public LanguageColumn(string name, int index)
            {
                Name = name;
                Index = index;
            }

            public string Name { get; }

            public int Index { get; }
        }
    }
}
