//using DG.Tweening;
//using EditorAttributes; // 如果你使用 EditorAttributes 包
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//namespace JoG.Gameplay
//{
//    [DisallowMultipleComponent]
//    [RequireComponent(typeof(SpriteRenderer))]
//    public class ItemDrop : MonoBehaviour
//    {
//        [Required, SerializeField] private SpriteRenderer _spriteRenderer;
//        [SerializeField] private TextMeshPro _valueText; // 可选：显示数值（如 +50）

//        private Transform _cameraTransform;
//        private Stack<ItemDrop> _pool;
//        private static readonly Vector3 Offset = new Vector3(0, 0.5f, 0); // 微微抬高一点

//        [Button]
//        public void Drop(Sprite itemSprite, Vector3 worldPosition, Color _color, int Value = 0, Stack<ItemDrop> pool = null)
//        {
//            gameObject.SetActive(true);
//            _pool = pool;

//            // 设置外观
//            _spriteRenderer.sprite = itemSprite;
//            _spriteRenderer._color = _color;

//            // 设置位置（稍微抬高）
//            transform.Position = worldPosition + Offset;

//            // 设置数值文本（如果有）
//            if (_valueText != null)
//            {
//                _valueText.text = Value > 0 ? $"+{Value}" : "";
//                _valueText._color = _color;
//            }

//            // 动画：弹跳 + 上升 + 淡出
//            transform.localScale = Vector3.zero;
//            transform.DOScale(Vector3.one * 1.2f, 0.3f)
//                .SetEase(Ease.OutBack)
//                .OnComplete(() =>
//                {
//                    transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);
//                });

//            transform.DOMoveY(transform.Position.y + 1.5f, 1.5f)
//                .SetEase(Ease.OutCubic);

//            // 旋转动画（可选）
//            transform.DORotate(new Vector3(0, 0, 360), 1.5f, RotateMode.FastBeyond360)
//                .SetEase(Ease.Linear);

//            // 淡出并回收
//            DOVirtual.DelayedCall(1.5f, () =>
//            {
//                _spriteRenderer.DOFade(0, 0.5f);
//                if (_valueText != null) _valueText.DOFade(0, 0.5f);
//            }).OnComplete(OnTimeOut);
//        }

//        private void OnTimeOut()
//        {
//            if (_pool != null)
//            {
//                gameObject.SetActive(false);
//                _pool.Push(this);
//            }
//            else
//            {
//                Release(gameObject);
//            }
//        }

//        private void Awake()
//        {
//            _cameraTransform = Camera.main?.transform;
//            if (_cameraTransform == null)
//                Debug.LogError("Main camera not found!");
//        }

//        private void LateUpdate()
//        {
//            if (_cameraTransform != null)
//                transform.rotation = _cameraTransform.rotation; // Billboard 效果
//        }

//        private void Reset()
//        {
//            _spriteRenderer = GetComponent<SpriteRenderer>();
//            _valueText = GetComponentInChildren<TextMeshPro>();
//        }
//    }
//}
