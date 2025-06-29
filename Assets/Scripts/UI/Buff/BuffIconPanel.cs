using EditorAttributes;
using GuestUnion.ObjectPool.Generic;
using JoG.BuffSystem;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace JoG.UI.Buff {

    public class BuffIconPanel : MonoBehaviour {
        private List<BuffIcon> _icons;
        [SerializeField, Required] private BuffIconPool _iconPool;
        [SerializeField, Required] private Transform _iconContent;

        public void UpdateView(ReadOnlySpan<IBuff> source, ushort activeBuffCount) {
            while (_icons.Count < activeBuffCount) {
                _icons.Add(_iconPool.Rent(_iconContent));
            }
            var icons = _icons.AsSpan();
            var index = 0;
            foreach (var buff in source) {
                if (buff is null || buff.IconSprite is null) continue;
                icons[index].UpdateView(source[index]);
                ++index;
            }
            index = _icons.Count;
            while (index > activeBuffCount) {
                --index;
                _iconPool.Return(icons[index]);
                _icons.RemoveAt(index);
            }
        }

        protected void OnEnable() {
            _icons = ListPool<BuffIcon>.shared.Rent();
        }

        protected void OnDisable() {
            foreach (var buffIcon in _icons.AsSpan()) {
                _iconPool.Return(buffIcon);
            }
            ListPool<BuffIcon>.shared.Return(_icons);
        }
    }
}