using UnityEngine;
using System;
using Unity.Profiling;

namespace JoG {

    public class TestSpanGC : MonoBehaviour {
        // 让测试数据大点，方便看 GC
        private int[] data = new int[1000];
        private Span<int> Span => new Span<int>(data);

        // 高性能 Recorder，只统计 GC
        private ProfilerRecorder gcRecorder;

        void Start() {
            // 初始化数组，防止编译器优化掉循环体
            for (int i = 0; i < data.Length; i++)
                data[i] = i;
            // 开启 GC.Alloc 记录器
            gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        }

        void Update() {
            // 每 2 秒交替测试 foreach / for
            bool useForeach = (Time.time % 2f) < 1f;

            long gcBefore = gcRecorder.LastValue;

            if (useForeach) {
                int sum = 0;
                foreach (var x in Span) sum += x;
            } else {
                int sum = 0;
                for (int i = 0; i < Span.Length; i++) sum += Span[i];
            }

            long gcAfter = gcRecorder.LastValue;

            // 只在变化时打印，避免刷屏
            if (gcAfter != gcBefore)
                Debug.Log($"Frame[{Time.frameCount}]  {(useForeach ? "foreach" : "for")}  GC = {gcAfter} Byte");
        }

        void OnDestroy() {
            gcRecorder.Dispose();
        }
    }
}
