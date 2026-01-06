using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace JoG {

    public class TestSpanGC_V2 : MonoBehaviour {
        private int[] data = new int[200];      // 故意小数组，多次复用
        private Span<int> Span => new Span<int>(data);

        private void Update() {
            const int LOOP = 1000;          // 放大 1000 倍
            Profiler.BeginSample("Span_FOREACH");
            for (int i = 0; i < LOOP; i++) {
                int sum = 0;
                foreach (var x in Span) sum += x;
            }
            Profiler.EndSample();

            Profiler.BeginSample("Span_FOR");
            for (int i = 0; i < LOOP; i++) {
                int sum = 0;
                for (int j = 0; j < Span.Length; j++) sum += Span[j];
            }
            Profiler.EndSample();
        }
    }
}