using System.Text;

namespace Game.Localization
{
    public static class LocalizationFastFormatter
    {
        [System.ThreadStatic] private static StringBuilder _builder;

        public static string Format(string template, int arg1)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 16);
            AppendFormatted(builder, template, arg1);
            return builder.ToString();
        }

        public static string Format(string template, int arg1, int arg2)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 24);
            AppendFormatted(builder, template, arg1, arg2);
            return builder.ToString();
        }

        public static string Format(string template, int arg1, int arg2, int arg3)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 32);
            AppendFormatted(builder, template, arg1, arg2, arg3);
            return builder.ToString();
        }

        public static string Format(string template, float arg1)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 16);
            AppendFormatted(builder, template, arg1);
            return builder.ToString();
        }

        public static string Format(string template, float arg1, float arg2)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 24);
            AppendFormatted(builder, template, arg1, arg2);
            return builder.ToString();
        }

        public static string Format(string template, string arg1)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 16);
            AppendFormatted(builder, template, arg1);
            return builder.ToString();
        }

        public static string Format(string template, string arg1, string arg2)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 24);
            AppendFormatted(builder, template, arg1, arg2);
            return builder.ToString();
        }

        public static string Format<T1>(string template, T1 arg1)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 16);
            AppendFormatted(builder, template, arg1, default, default, 1);
            return builder.ToString();
        }

        public static string Format<T1, T2>(string template, T1 arg1, T2 arg2)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 24);
            AppendFormatted(builder, template, arg1, arg2, default, 2);
            return builder.ToString();
        }

        public static string Format<T1, T2, T3>(string template, T1 arg1, T2 arg2, T3 arg3)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var builder = AcquireBuilder(template.Length + 32);
            AppendFormatted(builder, template, arg1, arg2, arg3, 3);
            return builder.ToString();
        }

        private static StringBuilder AcquireBuilder(int capacity)
        {
            var builder = _builder;
            if (builder == null)
            {
                builder = new StringBuilder(capacity);
                _builder = builder;
            }
            else
            {
                builder.Clear();
                if (builder.Capacity < capacity)
                {
                    builder.EnsureCapacity(capacity);
                }
            }

            return builder;
        }

        private static void AppendFormatted<T1, T2, T3>(
            StringBuilder builder,
            string template,
            T1 arg1,
            T2 arg2,
            T3 arg3,
            int argCount)
        {
            for (int i = 0; i < template.Length; i++)
            {
                var ch = template[i];
                if (ch != '{')
                {
                    builder.Append(ch);
                    continue;
                }

                if (TryAppendEscapedOpenBrace(builder, template, ref i))
                {
                    continue;
                }

                if (TryAppendArgument(builder, template, ref i, arg1, arg2, arg3, argCount))
                {
                    continue;
                }

                builder.Append(ch);
            }
        }

        private static bool TryAppendEscapedOpenBrace(StringBuilder builder, string template, ref int index)
        {
            if (index + 1 < template.Length && template[index + 1] == '{')
            {
                builder.Append('{');
                index++;
                return true;
            }

            return false;
        }

        private static bool TryAppendArgument<T1, T2, T3>(
            StringBuilder builder,
            string template,
            ref int index,
            T1 arg1,
            T2 arg2,
            T3 arg3,
            int argCount)
        {
            var start = index;
            var next = index + 1;
            if (next >= template.Length)
            {
                return false;
            }

            var placeholder = template[next] - '0';
            if (placeholder < 0 || placeholder > 2)
            {
                return false;
            }

            next++;
            if (next >= template.Length || template[next] != '}')
            {
                index = start;
                return false;
            }

            if (placeholder >= argCount)
            {
                index = next;
                builder.Append('{').Append(placeholder).Append('}');
                return true;
            }

            switch (placeholder)
            {
                case 0:
                    AppendValue(builder, arg1);
                    break;
                case 1:
                    AppendValue(builder, arg2);
                    break;
                case 2:
                    AppendValue(builder, arg3);
                    break;
            }

            index = next;
            return true;
        }

        private static void AppendFormatted(StringBuilder builder, string template, int arg1)
        {
            for (int i = 0; i < template.Length; i++)
            {
                var ch = template[i];
                if (ch != '{')
                {
                    builder.Append(ch);
                    continue;
                }

                if (TryAppendEscapedOpenBrace(builder, template, ref i))
                {
                    continue;
                }

                if (TryAppendIntArgument(builder, template, ref i, arg1))
                {
                    continue;
                }

                builder.Append(ch);
            }
        }

        private static void AppendFormatted(StringBuilder builder, string template, int arg1, int arg2)
        {
            for (int i = 0; i < template.Length; i++)
            {
                var ch = template[i];
                if (ch != '{')
                {
                    builder.Append(ch);
                    continue;
                }

                if (TryAppendEscapedOpenBrace(builder, template, ref i))
                {
                    continue;
                }

                if (TryAppendIntArgument(builder, template, ref i, arg1, arg2))
                {
                    continue;
                }

                builder.Append(ch);
            }
        }

        private static void AppendFormatted(StringBuilder builder, string template, int arg1, int arg2, int arg3)
        {
            for (int i = 0; i < template.Length; i++)
            {
                var ch = template[i];
                if (ch != '{')
                {
                    builder.Append(ch);
                    continue;
                }

                if (TryAppendEscapedOpenBrace(builder, template, ref i))
                {
                    continue;
                }

                if (TryAppendIntArgument(builder, template, ref i, arg1, arg2, arg3))
                {
                    continue;
                }

                builder.Append(ch);
            }
        }

        private static void AppendFormatted(StringBuilder builder, string template, float arg1)
        {
            for (int i = 0; i < template.Length; i++)
            {
                var ch = template[i];
                if (ch != '{')
                {
                    builder.Append(ch);
                    continue;
                }

                if (TryAppendEscapedOpenBrace(builder, template, ref i))
                {
                    continue;
                }

                if (TryAppendFloatArgument(builder, template, ref i, arg1))
                {
                    continue;
                }

                builder.Append(ch);
            }
        }

        private static void AppendFormatted(StringBuilder builder, string template, float arg1, float arg2)
        {
            for (int i = 0; i < template.Length; i++)
            {
                var ch = template[i];
                if (ch != '{')
                {
                    builder.Append(ch);
                    continue;
                }

                if (TryAppendEscapedOpenBrace(builder, template, ref i))
                {
                    continue;
                }

                if (TryAppendFloatArgument(builder, template, ref i, arg1, arg2))
                {
                    continue;
                }

                builder.Append(ch);
            }
        }

        private static void AppendFormatted(StringBuilder builder, string template, string arg1)
        {
            for (int i = 0; i < template.Length; i++)
            {
                var ch = template[i];
                if (ch != '{')
                {
                    builder.Append(ch);
                    continue;
                }

                if (TryAppendEscapedOpenBrace(builder, template, ref i))
                {
                    continue;
                }

                if (TryAppendStringArgument(builder, template, ref i, arg1))
                {
                    continue;
                }

                builder.Append(ch);
            }
        }

        private static void AppendFormatted(StringBuilder builder, string template, string arg1, string arg2)
        {
            for (int i = 0; i < template.Length; i++)
            {
                var ch = template[i];
                if (ch != '{')
                {
                    builder.Append(ch);
                    continue;
                }

                if (TryAppendEscapedOpenBrace(builder, template, ref i))
                {
                    continue;
                }

                if (TryAppendStringArgument(builder, template, ref i, arg1, arg2))
                {
                    continue;
                }

                builder.Append(ch);
            }
        }

        private static bool TryAppendIntArgument(StringBuilder builder, string template, ref int index, int arg1)
        {
            if (!TryReadPlaceholder(template, ref index, out var placeholder))
            {
                return false;
            }

            if (placeholder == 0)
            {
                builder.Append(arg1);
                return true;
            }

            builder.Append('{').Append(placeholder).Append('}');
            return true;
        }

        private static bool TryAppendIntArgument(StringBuilder builder, string template, ref int index, int arg1, int arg2)
        {
            if (!TryReadPlaceholder(template, ref index, out var placeholder))
            {
                return false;
            }

            switch (placeholder)
            {
                case 0:
                    builder.Append(arg1);
                    return true;
                case 1:
                    builder.Append(arg2);
                    return true;
                default:
                    builder.Append('{').Append(placeholder).Append('}');
                    return true;
            }
        }

        private static bool TryAppendIntArgument(StringBuilder builder, string template, ref int index, int arg1, int arg2, int arg3)
        {
            if (!TryReadPlaceholder(template, ref index, out var placeholder))
            {
                return false;
            }

            switch (placeholder)
            {
                case 0:
                    builder.Append(arg1);
                    return true;
                case 1:
                    builder.Append(arg2);
                    return true;
                case 2:
                    builder.Append(arg3);
                    return true;
                default:
                    builder.Append('{').Append(placeholder).Append('}');
                    return true;
            }
        }

        private static bool TryAppendFloatArgument(StringBuilder builder, string template, ref int index, float arg1)
        {
            if (!TryReadPlaceholder(template, ref index, out var placeholder))
            {
                return false;
            }

            if (placeholder == 0)
            {
                builder.Append(arg1);
                return true;
            }

            builder.Append('{').Append(placeholder).Append('}');
            return true;
        }

        private static bool TryAppendFloatArgument(StringBuilder builder, string template, ref int index, float arg1, float arg2)
        {
            if (!TryReadPlaceholder(template, ref index, out var placeholder))
            {
                return false;
            }

            switch (placeholder)
            {
                case 0:
                    builder.Append(arg1);
                    return true;
                case 1:
                    builder.Append(arg2);
                    return true;
                default:
                    builder.Append('{').Append(placeholder).Append('}');
                    return true;
            }
        }

        private static bool TryAppendStringArgument(StringBuilder builder, string template, ref int index, string arg1)
        {
            if (!TryReadPlaceholder(template, ref index, out var placeholder))
            {
                return false;
            }

            if (placeholder == 0)
            {
                builder.Append(arg1);
                return true;
            }

            builder.Append('{').Append(placeholder).Append('}');
            return true;
        }

        private static bool TryAppendStringArgument(StringBuilder builder, string template, ref int index, string arg1, string arg2)
        {
            if (!TryReadPlaceholder(template, ref index, out var placeholder))
            {
                return false;
            }

            switch (placeholder)
            {
                case 0:
                    builder.Append(arg1);
                    return true;
                case 1:
                    builder.Append(arg2);
                    return true;
                default:
                    builder.Append('{').Append(placeholder).Append('}');
                    return true;
            }
        }

        private static bool TryReadPlaceholder(string template, ref int index, out int placeholder)
        {
            var start = index;
            var next = index + 1;
            placeholder = -1;

            if (next >= template.Length)
            {
                return false;
            }

            placeholder = template[next] - '0';
            if (placeholder < 0 || placeholder > 2)
            {
                placeholder = -1;
                return false;
            }

            next++;
            if (next >= template.Length || template[next] != '}')
            {
                index = start;
                placeholder = -1;
                return false;
            }

            index = next;
            return true;
        }

        private static void AppendValue<T>(StringBuilder builder, T value)
        {
            switch (value)
            {
                case null:
                    return;
                case string stringValue:
                    builder.Append(stringValue);
                    return;
                case int intValue:
                    builder.Append(intValue);
                    return;
                case long longValue:
                    builder.Append(longValue);
                    return;
                case float floatValue:
                    builder.Append(floatValue);
                    return;
                case double doubleValue:
                    builder.Append(doubleValue);
                    return;
                case bool boolValue:
                    builder.Append(boolValue);
                    return;
                default:
                    builder.Append(value);
                    return;
            }
        }
    }
}
