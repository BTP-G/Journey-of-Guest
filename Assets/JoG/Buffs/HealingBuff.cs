using JoG.Attributes;
using JoG.BuffSystem;
using System;
using Unity.Netcode;
using UnityEngine;
using YooAsset;

namespace JoG.Buffs {

    [Serializable]
    public sealed class HealingBuff : TickableBuff<HealingBuff> {
        public NetworkObjectReference healer;
        public uint healAmountPerTick = 1;
        public ushort healCount;

        [FromAsset("HealingSprite")]
        private static AssetHandle iconSprite;

        public uint TotalHealAmount => healAmountPerTick * healCount;
        public override EBuffType Type => EBuffType.Healing;

        public override Sprite IconSprite => iconSprite.GetAssetObject<Sprite>();

        public override string Name => nameof(HealingBuff);

        public override string Description => $"每隔0.5秒恢复{healAmountPerTick}HP。";

        public override float TickInterval => 0.5f;

        public override ushort Count => healCount;

        protected override void OnSerialize(FastBufferWriter writer) {
            BytePacker.WriteValueBitPacked(writer, healCount);
            BytePacker.WriteValueBitPacked(writer, healAmountPerTick);
            writer.WriteNetworkSerializable(healer);
        }

        protected override void OnDeserialize(FastBufferReader reader) {
            ByteUnpacker.ReadValueBitPacked(reader, out healCount);
            ByteUnpacker.ReadValueBitPacked(reader, out healAmountPerTick);
            reader.ReadNetworkSerializable(out healer);
        }

        protected override void MergeWith(HealingBuff buff) {
            var totalHealAmountPerTick = healAmountPerTick + buff.healAmountPerTick;
            healCount = (ushort)((TotalHealAmount + buff.TotalHealAmount) / totalHealAmountPerTick);
            healAmountPerTick = totalHealAmountPerTick;
        }

        protected override void OnTick() {
            if (healCount is 0) {
                RemoveSelfOnLocal();
                return;
            }
            if (Owner.IsOwner) {
                Owner.Handle(new HealingMessage {
                    value = healAmountPerTick,
                    cofficient = 1,
                    healer = healer,
                });
            }
            --healCount;
        }
    }
}