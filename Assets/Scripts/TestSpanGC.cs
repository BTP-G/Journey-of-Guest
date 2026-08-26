using System;
using Unity.Profiling;
using UnityEngine;

namespace Expriverse {

    public class TestSpanGC : MonoBehaviour {
        // 让测试数据大点，方便看 GC
        private int[] data = new int[1000];
        private Span<int> Span => new Span<int>(data);

        // 高性能 Recorder，只统计 GC
        private ProfilerRecorder gcRecorder;

        private void Start() {
            // 初始化数组，防止编译器优化掉循环体
            for (var i = 0; i < data.Length; i++) {
                data[i] = i;
            }
            // 开启 GC.Alloc 记录器
            gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        }

        private void Update() {
            // 每 2 秒交替测试 foreach / for
            var useForeach = (Time.time % 2f) < 1f;

            var gcBefore = gcRecorder.LastValue;

            if (useForeach) {
                var sum = 0;
                foreach (var x in Span) {
                    sum += x;
                }
            } else {
                var sum = 0;
                for (var i = 0; i < Span.Length; i++) {
                    sum += Span[i];
                }
            }

            var gcAfter = gcRecorder.LastValue;

            // 只在变化时打印，避免刷屏
            if (gcAfter != gcBefore) {
                Debug.Log($"Frame[{Time.frameCount}]  {(useForeach ? "foreach" : "for")}  GC = {gcAfter} Byte");
            }
        }

        private void OnDestroy() {
            gcRecorder.Dispose();
        }
    }
}
