using System;
using System.Collections.Generic;
using UnityEngine;

namespace jp.go.aist3ddbclient
{
    /// <summary>
    /// インターフェース：ScrollViewのModel
    /// </summary>
    public interface IOptimizedScrollViewModel<T>
    {
        void AddItem(T item);
        bool RemoveItem(T item);
        void ClearItems();
        T GetItem(int index);
        int GetItemCount();
        List<T> GetItems();
        List<T> Search(Predicate<T> predicate);
    }

    /// <summary>
    /// ScrollViewのModelクラス
    /// </summary>
    public class OptimizedScrollViewModel<T> : IOptimizedScrollViewModel<T>
    {
        private List<T> items = new List<T>();

        // デバッグログの有効/無効を設定するフラグ
        private static bool enableDebugLogs = false;
        private static string categly = "OptimizedScrollViewModel";

        // 静的コンストラクタを追加
        static OptimizedScrollViewModel()
        {
            // デバッグログの設定
            DebugUtility.SetDebugLogEnabled(categly, enableDebugLogs);
        }

        public void AddItem(T item)
        {
            if (item == null)
            {
                Debug.LogError("モデルにnullアイテムを追加できません。");
                return;
            }
            items.Add(item);
            DebugUtility.Log(categly, $"アイテムが追加されました。総アイテム数: {items.Count}");
        }

        public bool RemoveItem(T item)
        {
            if (item == null)
            {
                Debug.LogError("モデルからnullアイテムを削除できません。");
                return false;
            }
            bool removed = items.Remove(item);
            if (removed)
                DebugUtility.Log(categly, $"アイテムが削除されました。総アイテム数: {items.Count}");
            else
                Debug.LogWarning("存在しないアイテムの削除を試みました。");
            return removed;
        }

        public void ClearItems()
        {
            items.Clear();
            DebugUtility.Log(categly, "モデル内の全アイテムがクリアされました。");
        }

        public T GetItem(int index)
        {
            if (index < 0 || index >= items.Count)
            {
                Debug.LogError($"GetItem: インデックス {index} は範囲外です。");
                return default(T);
            }
            return items[index];
        }

        public int GetItemCount()
        {
            return items.Count;
        }

        public List<T> GetItems()
        {
            return new List<T>(items);
        }

        public List<T> Search(Predicate<T> predicate)
        {
            if (predicate == null)
            {
                Debug.LogError("検索条件がnullです。");
                return new List<T>();
            }
            return items.FindAll(predicate);
        }
    }
}

