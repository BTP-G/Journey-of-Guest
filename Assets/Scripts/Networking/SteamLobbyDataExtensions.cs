using Steamworks;
using System.Globalization;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Networking.P2P {
    /// <summary>
    /// Steam SLobby Data 只能存字符串。本扩展按 InvariantCulture 编解码标量，用法对齐 PlayerPrefs。
    /// 缺键或空串视为不存在；解析失败时 Get 返回 default、TryGet 返回 false。Bool 固定写入 "1"/"0"。
    /// 成员写入只作用于本端（Steam SetLobbyMemberData）。
    /// </summary>
    public static class SteamLobbyDataExtensions {
        private const string TrueValue = "1";
        private const string FalseValue = "0";

        public static bool HasKey(this SLobby lobby, string key) {
            return HasValue(lobby.GetData(key));
        }

        public static string GetString(this SLobby lobby, string key, string defaultValue = "") {
            var value = lobby.GetData(key);
            return HasValue(value) ? value : defaultValue;
        }

        public static bool SetString(this SLobby lobby, string key, string value) {
            return lobby.SetData(key, value);
        }

        public static int GetInt(this SLobby lobby, string key, int defaultValue = 0) {
            return lobby.TryGetInt(key, out var value) ? value : defaultValue;
        }

        public static bool SetInt(this SLobby lobby, string key, int value) {
            return lobby.SetData(key, Format(value));
        }

        public static bool TryGetInt(this SLobby lobby, string key, out int value) {
            return TryParseInt(lobby.GetData(key), out value);
        }

        public static float GetFloat(this SLobby lobby, string key, float defaultValue = 0f) {
            return lobby.TryGetFloat(key, out var value) ? value : defaultValue;
        }

        public static bool SetFloat(this SLobby lobby, string key, float value) {
            return lobby.SetData(key, Format(value));
        }

        public static bool TryGetFloat(this SLobby lobby, string key, out float value) {
            return TryParseFloat(lobby.GetData(key), out value);
        }

        public static bool GetBool(this SLobby lobby, string key, bool defaultValue = false) {
            return lobby.TryGetBool(key, out var value) ? value : defaultValue;
        }

        public static bool SetBool(this SLobby lobby, string key, bool value) {
            return lobby.SetData(key, Format(value));
        }

        public static bool TryGetBool(this SLobby lobby, string key, out bool value) {
            return TryParseBool(lobby.GetData(key), out value);
        }

        public static byte GetByte(this SLobby lobby, string key, byte defaultValue = 0) {
            return lobby.TryGetByte(key, out var value) ? value : defaultValue;
        }

        public static bool SetByte(this SLobby lobby, string key, byte value) {
            return lobby.SetData(key, Format(value));
        }

        public static bool TryGetByte(this SLobby lobby, string key, out byte value) {
            return TryParseByte(lobby.GetData(key), out value);
        }

        public static uint GetUInt(this SLobby lobby, string key, uint defaultValue = 0) {
            return lobby.TryGetUInt(key, out var value) ? value : defaultValue;
        }

        public static bool SetUInt(this SLobby lobby, string key, uint value) {
            return lobby.SetData(key, Format(value));
        }

        public static bool TryGetUInt(this SLobby lobby, string key, out uint value) {
            return TryParseUInt(lobby.GetData(key), out value);
        }

        public static ulong GetULong(this SLobby lobby, string key, ulong defaultValue = 0) {
            return lobby.TryGetULong(key, out var value) ? value : defaultValue;
        }

        public static bool SetULong(this SLobby lobby, string key, ulong value) {
            return lobby.SetData(key, Format(value));
        }

        public static bool TryGetULong(this SLobby lobby, string key, out ulong value) {
            return TryParseULong(lobby.GetData(key), out value);
        }

        public static bool HasMemberKey(this SLobby lobby, Friend member, string key) {
            return HasValue(lobby.GetMemberData(member, key));
        }

        public static string GetMemberString(this SLobby lobby, Friend member, string key, string defaultValue = "") {
            var value = lobby.GetMemberData(member, key);
            return HasValue(value) ? value : defaultValue;
        }

        public static void SetMemberString(this SLobby lobby, string key, string value) {
            lobby.SetMemberData(key, value);
        }

        public static int GetMemberInt(this SLobby lobby, Friend member, string key, int defaultValue = 0) {
            return lobby.TryGetMemberInt(member, key, out var value) ? value : defaultValue;
        }

        public static void SetMemberInt(this SLobby lobby, string key, int value) {
            lobby.SetMemberData(key, Format(value));
        }

        public static bool TryGetMemberInt(this SLobby lobby, Friend member, string key, out int value) {
            return TryParseInt(lobby.GetMemberData(member, key), out value);
        }

        public static float GetMemberFloat(this SLobby lobby, Friend member, string key, float defaultValue = 0f) {
            return lobby.TryGetMemberFloat(member, key, out var value) ? value : defaultValue;
        }

        public static void SetMemberFloat(this SLobby lobby, string key, float value) {
            lobby.SetMemberData(key, Format(value));
        }

        public static bool TryGetMemberFloat(this SLobby lobby, Friend member, string key, out float value) {
            return TryParseFloat(lobby.GetMemberData(member, key), out value);
        }

        public static bool GetMemberBool(this SLobby lobby, Friend member, string key, bool defaultValue = false) {
            return lobby.TryGetMemberBool(member, key, out var value) ? value : defaultValue;
        }

        public static void SetMemberBool(this SLobby lobby, string key, bool value) {
            lobby.SetMemberData(key, Format(value));
        }

        public static bool TryGetMemberBool(this SLobby lobby, Friend member, string key, out bool value) {
            return TryParseBool(lobby.GetMemberData(member, key), out value);
        }

        public static byte GetMemberByte(this SLobby lobby, Friend member, string key, byte defaultValue = 0) {
            return lobby.TryGetMemberByte(member, key, out var value) ? value : defaultValue;
        }

        public static void SetMemberByte(this SLobby lobby, string key, byte value) {
            lobby.SetMemberData(key, Format(value));
        }

        public static bool TryGetMemberByte(this SLobby lobby, Friend member, string key, out byte value) {
            return TryParseByte(lobby.GetMemberData(member, key), out value);
        }

        public static uint GetMemberUInt(this SLobby lobby, Friend member, string key, uint defaultValue = 0) {
            return lobby.TryGetMemberUInt(member, key, out var value) ? value : defaultValue;
        }

        public static void SetMemberUInt(this SLobby lobby, string key, uint value) {
            lobby.SetMemberData(key, Format(value));
        }

        public static bool TryGetMemberUInt(this SLobby lobby, Friend member, string key, out uint value) {
            return TryParseUInt(lobby.GetMemberData(member, key), out value);
        }

        public static ulong GetMemberULong(this SLobby lobby, Friend member, string key, ulong defaultValue = 0) {
            return lobby.TryGetMemberULong(member, key, out var value) ? value : defaultValue;
        }

        public static void SetMemberULong(this SLobby lobby, string key, ulong value) {
            lobby.SetMemberData(key, Format(value));
        }

        public static bool TryGetMemberULong(this SLobby lobby, Friend member, string key, out ulong value) {
            return TryParseULong(lobby.GetMemberData(member, key), out value);
        }

        private static bool HasValue(string value) {
            return value != null && value.Length != 0;
        }

        private static string Format(bool value) {
            return value ? TrueValue : FalseValue;
        }

        private static string Format(byte value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Format(int value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Format(uint value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Format(ulong value) {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Format(float value) {
            return value.ToString("G9", CultureInfo.InvariantCulture);
        }

        private static bool TryParseInt(string raw, out int value) {
            if (!HasValue(raw)) {
                value = 0;
                return false;
            }

            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseUInt(string raw, out uint value) {
            if (!HasValue(raw)) {
                value = 0;
                return false;
            }

            return uint.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseULong(string raw, out ulong value) {
            if (!HasValue(raw)) {
                value = 0;
                return false;
            }

            return ulong.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseByte(string raw, out byte value) {
            if (!HasValue(raw)) {
                value = 0;
                return false;
            }

            return byte.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseFloat(string raw, out float value) {
            if (!HasValue(raw)) {
                value = 0f;
                return false;
            }

            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseBool(string raw, out bool value) {
            if (!HasValue(raw)) {
                value = false;
                return false;
            }

            if (raw == TrueValue) {
                value = true;
                return true;
            }

            if (raw == FalseValue) {
                value = false;
                return true;
            }

            return bool.TryParse(raw, out value);
        }
    }
}
