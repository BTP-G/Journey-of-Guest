using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionPerformanceTest : MonoBehaviour {
    private const int Iterations = 1000000;

    private const int TestValue = 123;

    private readonly List<Action<int>> _listActions = new List<Action<int>>();

    public event Action<int> OnEventAction;

    private void Start() {
        // 注册相同的处理方法（确保公平比较）
        for (var i = 0; i < 100; ++i) {
            OnEventAction += Handler1;
            OnEventAction += Handler2;
            OnEventAction += Handler3;

            _listActions.Add(Handler1);
            _listActions.Add(Handler2);
            _listActions.Add(Handler3);
        }

        Debug.Log($"开始性能测试（{Iterations:N0} 次调用）...\n");

        // 测试 event Action
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < Iterations; i++) {
            OnEventAction?.Invoke(TestValue);
        }
        sw.Stop();
        var eventTime = sw.ElapsedMilliseconds;

        // 测试 List<Action>
        sw.Restart();
        for (var i = 0; i < Iterations; i++) {
            foreach (var action in _listActions) {
                action(TestValue); // 直接调用，非 .Invoke()
            }
        }
        sw.Stop();
        var listTime = sw.ElapsedMilliseconds;

        // 输出结果
        Debug.Log($"✅ event Action<int> 耗时: {eventTime} ms");
        Debug.Log($"✅ List<Action<int>> 耗时: {listTime} ms");

        if (eventTime < listTime) {
            Debug.Log("🏆 event Action 更快！");
        } else if (listTime < eventTime) {
            Debug.Log("🏆 List<Action> 更快！");
        } else {
            Debug.Log("⏱ 两者性能相当。");
        }
    }

    // 所有处理器都是无副作用的纯输出（避免影响性能）
    private void Handler1(int value) { /* 实际项目中可处理逻辑 */ }

    private void Handler2(int value) {
        /* 这里留空以专注性能 */
    }

    private void Handler3(int value) {
    }
}
