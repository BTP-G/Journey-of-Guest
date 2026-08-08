using Cysharp.Text;
using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class Utf16ValueStringBuilderExtensions {

        // ====== Color (Color32) ======
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendColored(ref this Utf16ValueStringBuilder sb, ReadOnlySpan<char> text, ReadOnlySpan<char> color) {
            sb.Append("<color=");
            sb.Append(color);
            sb.Append('>');
            sb.Append(text);
            sb.Append("</color>");
        }

        // ====== Size ======
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendSize(ref this Utf16ValueStringBuilder sb, ReadOnlySpan<char> text, int size) {
            sb.Append("<size=");
            sb.Append(size);
            sb.Append('>');
            sb.Append(text);
            sb.Append("</size>");
        }

        // ====== Bold ======
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendBold(ref this Utf16ValueStringBuilder sb, ReadOnlySpan<char> text) {
            sb.Append("<b>");
            sb.Append(text);
            sb.Append("</b>");
        }

        // ====== Italic ======
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendItalic(ref this Utf16ValueStringBuilder sb, ReadOnlySpan<char> text) {
            sb.Append("<i>");
            sb.Append(text);
            sb.Append("</i>");
        }

        // ====== Underline ======
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendUnderline(ref this Utf16ValueStringBuilder sb, ReadOnlySpan<char> text) {
            sb.Append("<u>");
            sb.Append(text);
            sb.Append("</u>");
        }

        // ====== Strikethrough ======
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendStrikethrough(ref this Utf16ValueStringBuilder sb, ReadOnlySpan<char> text) {
            sb.Append("<s>");
            sb.Append(text);
            sb.Append("</s>");
        }
    }
}