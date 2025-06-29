using JoG.Attributes;
using JoG.BuffSystem;
using JoG.Localization;
using Unity.Netcode;
using UnityEngine;
using YooAsset;

namespace JoG.Buffs {

    public sealed class BleedingBuff : TickableBuff<BleedingBuff> {

        [FromConfig("buff.bleeding.tickInterval")]
        public static float tickInterval;

        public NetworkObjectReference attacker;
        public uint damageValuePerTick;
        public ushort damageCount;

        [FromAsset("BleedingSprite")]
        private static AssetHandle iconSprite;

        public uint TotalDamage => damageValuePerTick * damageCount;

        public override Sprite IconSprite => iconSprite.GetAssetObject<Sprite>();

        public override string Name => LocalizationManager.GetString("buff.bleeding.name");

        public override string Description => LocalizationManager.GetString("buff.bleeding.desc");

        public override float TickInterval => tickInterval;

        public override ushort Count => damageCount;

        public override EBuffType Type => EBuffType.Damage;

        protected override void OnSerialize(FastBufferWriter writer) {
            BytePacker.WriteValueBitPacked(writer, damageCount);
            BytePacker.WriteValueBitPacked(writer, damageValuePerTick);
            writer.WriteNetworkSerializable(attacker);
        }

        protected override void OnDeserialize(FastBufferReader reader) {
            ByteUnpacker.ReadValueBitPacked(reader, out damageCount);
            ByteUnpacker.ReadValueBitPacked(reader, out damageValuePerTick);
            reader.ReadNetworkSerializable(out attacker);
        }

        protected override void MergeWith(BleedingBuff buff) {
            damageCount += buff.damageCount;
        }

        protected override void OnTick() {
            if (damageCount is 0) {
                RemoveSelfOnLocal();
                return;
            }
            if (Owner.IsOwner) {
                Owner.Handle(new DamageMessage {
                    value = damageValuePerTick,
                    cofficient = 1,
                    flags = DamgeFlag.fire,
                    attacker = attacker,
                    position = Owner.Center,
                });
            }
            --damageCount;
        }
    }
}