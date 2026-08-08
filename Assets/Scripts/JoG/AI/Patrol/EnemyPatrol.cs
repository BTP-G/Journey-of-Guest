using JoG.AI;
using JoG.AI.Patrol;
using UnityEngine;
using VContainer;

public class EnemyPatrol : MonoBehaviour {
    [Min(0)] public float stoppingDistance = 0.5f;
    public JumpInputer pathTarget;
    [Header("巡逻配置")]
    [Inject] internal PatrolService patrolService;

    private PatrolRoute _currentRoute;
    private Transform _currentTarget;
    private int _currentPointIndex;

    private void Update() {

    }

    /// <summary>切换到下一个巡逻点（循环）</summary>
    private void NextPoint() {
        _currentTarget = _currentRoute.GetNextPoint(ref _currentPointIndex);
        //if (_currentTarget != null) {
        //    _agent.SetDestination(_currentTarget._position);
        //}
    }

    /// <summary>调试绘制巡逻路线和点</summary>
    private void OnDrawGizmosSelected() {
        if (_currentRoute != null && _currentRoute.points != null) {
            Gizmos.color = Color.cyan;
            for (var i = 0; i < _currentRoute.points.Length; i++) {
                var point = _currentRoute.points[i];
                if (point != null) {
                    Gizmos.DrawWireSphere(point.position, 0.3f);
                    var nextPoint = _currentRoute.points[(i + 1) % _currentRoute.points.Length];
                    if (nextPoint != null) {
                        Gizmos.DrawLine(point.position, nextPoint.position);
                    }
                }
            }

            // 绘制当前目标点
            if (_currentTarget != null) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_currentTarget.position, 0.5f);
            }
        }
    }
}
