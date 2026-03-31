using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Game.Localization.Editor
{
    internal static class CsvUtility
    {
        public static List<string[]> Read(string path)
        {
            var content = File.ReadAllText(path, Encoding.UTF8);
            var rows = new List<string[]>();
            var row = new List<string>();
            var cell = new StringBuilder(64);
            var inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                var ch = content[i];

                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < content.Length && content[i + 1] == '"')
                        {
                            cell.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        cell.Append(ch);
                    }

                    continue;
                }

                switch (ch)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        row.Add(cell.ToString());
                        cell.Length = 0;
                        break;
                    case '\r':
                        break;
                    case '\n':
                        row.Add(cell.ToString());
                        rows.Add(row.ToArray());
                        row.Clear();
                        cell.Length = 0;
                        break;
                    default:
                        cell.Append(ch);
                        break;
                }
            }

            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row.ToArray());
            }

            return rows;
        }
    }
}
