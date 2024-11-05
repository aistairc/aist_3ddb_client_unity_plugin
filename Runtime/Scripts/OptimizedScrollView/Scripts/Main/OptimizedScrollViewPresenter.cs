using System;
using UnityEngine;

namespace jp.go.aist3ddbclient
{
    /// <summary>
    /// ScrollViewのPresenterクラス
    /// </summary>
    /// <typeparam name="T">データ型</typeparam>
    public class OptimizedScrollViewPresenter<T>
    {
        private IOptimizedScrollViewModel<T> model;
        private OptimizedScrollViewView view;
        private Action<int, GameObject, T> updateItemCallback; // ユーザー提供のコールバック

        private bool enableDebugLogs = false; // デバッグログの有効/無効を設定
        private static string categly = "OptimizedScrollViewPresenter";

        /// <summary>
        /// Presenterのコンストラクタ
        /// </summary>
        /// <param name="model">ScrollViewのModel</param>
        /// <param name="view">ScrollViewのView</param>
        /// <param name="updateItemCallback">アイテム更新のコールバック</param>
        public OptimizedScrollViewPresenter(IOptimizedScrollViewModel<T> model, OptimizedScrollViewView view, Action<int, GameObject, T> updateItemCallback)
        {
            this.model = model;
            this.view = view;
            this.updateItemCallback = updateItemCallback;

            // デバッグログの設定
            DebugUtility.SetDebugLogEnabled(categly, enableDebugLogs);

            // デバッグログの使用
            DebugUtility.Log(categly, "初期化が開始されました。");

            if (this.model == null)
            {
                Debug.LogError("Presenterに渡されたModelがnullです。");
                return;
            }

            if (this.view == null)
            {
                Debug.LogError("Presenterに渡されたViewがnullです。");
                return;
            }

            if (this.updateItemCallback == null)
            {
                Debug.LogError("Presenterに渡されたupdateItemCallbackがnullです。");
                return;
            }

            // Viewの初期化完了イベントにサブスクライブ
            this.view.OnViewInitialized += OnViewInitialized;
        }

        /// <summary>
        /// Viewの初期化完了時に呼ばれるメソッド
        /// </summary>
        private void OnViewInitialized()
        {
            // ユーザー提供のアイテム更新コールバックを設定
            view.SetUpdateItemCallback(UserUpdateItemCallback);

            // Modelから総アイテム数を取得してViewに設定
            view.SetTotalItems(model.GetItemCount());
        }

        /// <summary>
        /// ユーザー提供のアイテム更新コールバック
        /// </summary>
        /// <param name="index">アイテムのインデックス</param>
        /// <param name="item">アイテムのGameObject</param>
        private void UserUpdateItemCallback(int index, GameObject item)
        {
            if (index < 0 || index >= model.GetItemCount())
            {
                Debug.LogError($"UserUpdateItemCallback: インデックス {index} は範囲外です。");
                return;
            }

            T data = model.GetItem(index);
            // ユーザー提供のコールバックを使用してアイテムを更新
            updateItemCallback?.Invoke(index, item, data);
        }

        /// <summary>
        /// アイテムを追加
        /// </summary>
        /// <param name="item">追加するアイテム</param>
        public void AddItem(T item)
        {
            model.AddItem(item);
            view.SetTotalItems(model.GetItemCount());
        }

        /// <summary>
        /// アイテムを削除
        /// </summary>
        /// <param name="item">削除するアイテム</param>
        public void RemoveItem(T item)
        {
            if (model.RemoveItem(item))
            {
                view.SetTotalItems(model.GetItemCount());
            }
        }

        /// <summary>
        /// 全アイテムをクリア
        /// </summary>
        public void ClearItems()
        {
            model.ClearItems();
            view.SetTotalItems(model.GetItemCount());
        }
    }
}
