using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class ArrayExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrueForAll<T>(this T[] array, Predicate<T> match) {
            return Array.TrueForAll(array, match);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindIndex<T>(this T[] array, Predicate<T> match) {
            return Array.FindIndex(array, match);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindLastIndex<T>(this T[] array, Predicate<T> match) {
            return Array.FindLastIndex(array, match);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this T[] array, T value) {
            return Array.IndexOf(array, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this T[] array, T value) {
            return Array.LastIndexOf(array, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Find<T>(this T[] array, Predicate<T> match) {
            return Array.Find(array, match);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FindLast<T>(this T[] array, Predicate<T> match) {
            return Array.FindLast(array, match);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] FindAll<T>(this T[] array, Predicate<T> match) {
            return Array.FindAll(array, match);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] array, Converter<TInput, TOutput> converter) {
            return Array.ConvertAll(array, converter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T>(this T[] array, T value) {
            return Array.BinarySearch(array, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T>(this T[] array, T value, IComparer<T> comparer) {
            return Array.BinarySearch(
                array,
                value,
                comparer
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this T[] array) {
            Array.Clear(
                array,
                0,
                array.Length
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(this T[] array, T value) {
            Array.Fill(array, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this T[] array, Action<T> action) {
            Array.ForEach(array, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reverse<T>(this T[] array) {
            Array.Reverse(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this T[] array) {
            Array.Sort(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this T[] array, IComparer<T> comparer) {
            Array.Sort(array, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] array) {
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] array, int start, int length) {
            return new ReadOnlySpan<T>(
                array,
                start,
                length
            );
        }
    }
}
