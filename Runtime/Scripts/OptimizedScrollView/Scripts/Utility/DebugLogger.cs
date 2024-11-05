using System.Collections.Generic;
using UnityEngine;

namespace jp.go.aist3ddbclient
{
    public static class DebugUtility
    {
        // カテゴリごとのデバッグログの有効/無効を管理
        private static Dictionary<string, bool> debugFlags = new Dictionary<string, bool>();

        // デバッグログを出力するメソッド
        public static void Log(string category, string message)
        {
            if (IsDebugLogEnabled(category))
            {
                Debug.Log($"[{category}] {message}");
            }
        }

        // デバッグログの有効/無効を設定するメソッド
        public static void SetDebugLogEnabled(string category, bool enabled)
        {
            if (debugFlags.ContainsKey(category))
            {
                debugFlags[category] = enabled;
            }
            else
            {
                debugFlags.Add(category, enabled);
            }
        }

        // デバッグログが有効かどうかを確認するメソッド
        private static bool IsDebugLogEnabled(string category)
        {
            if (debugFlags.ContainsKey(category))
            {
                return debugFlags[category];
            }
            return false;
        }
    }
}