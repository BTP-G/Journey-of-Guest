using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace Xoderony.EditorBenchmarks {

    public static class EqualityCallBenchmark {

        // 可调整次数。Editor 下建议不要太大以免卡死。
        private const int Iterations = 10_000_000;

        private const int Warmups = 3;

        [MenuItem("Tools/Benchmark/Run Equality Call Benchmark")]
        public static void RunBenchmark() {
            UnityEngine.Debug.Log("Equality Call Benchmark start...");

            var rand = new System.Random(123);
            var a = new int[Iterations];
            var b = new int[Iterations];
            for (var i = 0; i < Iterations; i++) {
                a[i] = rand.Next();
                b[i] = (i & 1) == 0 ? a[i] : rand.Next(); // half equal
            }

            // Warm-up JIT
            for (var w = 0; w < Warmups; w++) {
                RunDirect(a, b);
                RunDefaultComparer(a, b);
                RunInterfaceComparer(a, b);
                RunDelegate(a, b);
                RunDelegate2(a, b);
                RunDelegate3(a, b);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            System.Threading.Thread.Sleep(50);

            var sw = Stopwatch.StartNew();
            var res0 = RunDirect(a, b);
            sw.Stop();
            UnityEngine.Debug.Log($"Direct (x==y) : {sw.ElapsedMilliseconds} ms, matches={res0}");

            GC.Collect(); GC.WaitForPendingFinalizers(); System.Threading.Thread.Sleep(50);

            sw.Restart();
            var res1 = RunDefaultComparer(a, b);
            sw.Stop();
            UnityEngine.Debug.Log($"EqualityComparer<T>.Default : {sw.ElapsedMilliseconds} ms, matches={res1}");

            GC.Collect(); GC.WaitForPendingFinalizers(); System.Threading.Thread.Sleep(50);

            sw.Restart();
            var res2 = RunInterfaceComparer(a, b);
            sw.Stop();
            UnityEngine.Debug.Log($"IEqualityComparer<T> (interface) : {sw.ElapsedMilliseconds} ms, matches={res2}");

            GC.Collect(); GC.WaitForPendingFinalizers(); System.Threading.Thread.Sleep(50);

            sw.Restart();
            var res3 = RunDelegate(a, b);
            sw.Stop();
            UnityEngine.Debug.Log($"Delegate.Invoke : {sw.ElapsedMilliseconds} ms, matches={res3}");
            sw.Restart();
            var res4 = RunDelegate2(a, b);
            sw.Stop();
            UnityEngine.Debug.Log($"Delegate2.Invoke : {sw.ElapsedMilliseconds} ms, matches={res4}");
            sw.Restart();
            var res5 = RunDelegate3(a, b);
            sw.Stop();
            UnityEngine.Debug.Log($"Delegate3.Invoke : {sw.ElapsedMilliseconds} ms, matches={res5}");

            UnityEngine.Debug.Log("Equality Call Benchmark finished.");
        }

        // 直接内联比较
        private static int RunDirect(int[] a, int[] b) {
            var cnt = 0;
            for (var i = 0; i < a.Length; i++) {
                if (a[i] == b[i]) {
                    cnt++;
                }
            }
            return cnt;
        }

        // EqualityComparer<T>.Default 调用（可能被 JIT intrinsic 优化）
        private static int RunDefaultComparer(int[] a, int[] b) {
            var cnt = 0;
            var eq = EqualityComparer<int>.Default;
            for (var i = 0; i < a.Length; i++) {
                if (eq.Equals(a[i], b[i])) {
                    cnt++;
                }
            }
            return cnt;
        }

        // 通过接口字段调用（接收自定义 comparer 的情形）
        private static int RunInterfaceComparer(int[] a, int[] b) {
            var cnt = 0;
            IEqualityComparer<int> ieq = EqualityComparer<int>.Default;
            for (var i = 0; i < a.Length; i++) {
                if (ieq.Equals(a[i], b[i])) {
                    cnt++;
                }
            }
            return cnt;
        }

        // 委托调用（示例为静态委托以避免闭包分配）
        private static int RunDelegate(int[] a, int[] b) {
            var cnt = 0;
            Func<int, int, bool> cmp = (x, y) => x == y;
            for (var i = 0; i < a.Length; i++) {
                if (cmp(a[i], b[i])) {
                    cnt++;
                }
            }
            return cnt;
        }

        // 委托调用2（示例为静态委托以避免闭包分配）
        private static int RunDelegate2(int[] a, int[] b) {
            var cnt = 0;
            Func<int, int, bool> cmp = EqualityComparer<int>.Default.Equals;
            for (var i = 0; i < a.Length; i++) {
                if (cmp(a[i], b[i])) {
                    cnt++;
                }
            }
            return cnt;
        }

        // 委托调用3（示例为静态委托以避免闭包分配）
        private static int RunDelegate3(int[] a, int[] b) {
            var cnt = 0;
            IEqualityComparer<int> ieq = EqualityComparer<int>.Default;
            Func<int, int, bool> cmp = ieq.Equals;
            for (var i = 0; i < a.Length; i++) {
                if (cmp(a[i], b[i])) {
                    cnt++;
                }
            }
            return cnt;
        }
    }
}