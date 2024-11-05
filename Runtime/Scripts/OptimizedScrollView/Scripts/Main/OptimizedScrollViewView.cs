using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace jp.go.aist3ddbclient
{
    public class OptimizedScrollViewView : MonoBehaviour
    {
        [SerializeField] private GameObject itemPrefab; // アイテムのプレハブ
        [SerializeField] private int bufferCount = 3; // バッファ数
        [SerializeField] private bool enableDebugLogs = false; // デバッグログの有効/無効を設定

        private ScrollRect scrollRect;
        private RectTransform content; // ContentのRectTransform
        private int poolSize;
        private List<GameObject> pool = new List<GameObject>();
        private Action<int, GameObject> updateItemCallback;
        private int totalItems = 0;
        private float contentHeight = 0f;
        private int visibleCount = 0;
        private RectTransform viewport;
        private float itemHeight = 0f;

        public event Action OnViewInitialized; // 初期化完了イベント

        private bool isInitialized = false;
        private static string categly = "OptimizedScrollViewView";

        private IEnumerator Start()
        {
            // レイアウト計算が終わるまで待つ
            yield return new WaitForEndOfFrame();

            // デバッグログの設定
            DebugUtility.SetDebugLogEnabled(categly, enableDebugLogs);

            // デバッグログの使用
            DebugUtility.Log(categly, "初期化が開始されました。");


            if (isInitialized)
            {
                DebugUtility.Log(categly, $"{categly}は既に初期化されています。");
                yield break;
            }

            scrollRect = GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                Debug.LogError("ScrollRectコンポーネントがありません。");
                yield break;
            }

            content = scrollRect.content;
            if (content == null)
            {
                Debug.LogError("ScrollRectのContentが設定されていません。");
                yield break;
            }

            viewport = scrollRect.viewport;
            if (viewport == null)
            {
                Debug.LogError("ScrollRectのViewportが設定されていません。");
                yield break;
            }

            // ItemPrefabの高さを取得
            RectTransform itemRect = itemPrefab.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemHeight = itemRect.rect.height;
            }
            else
            {
                Debug.LogError("ItemPrefabにRectTransformがありません。");
                yield break;
            }

            float viewportHeight = viewport.rect.height;
            visibleCount = Mathf.CeilToInt(viewportHeight / itemHeight) + 1; // +1でアイテムの欠けを防止
            poolSize = visibleCount + bufferCount;

            DebugUtility.Log(categly, $"viewportHeight: {viewportHeight}, itemHeight: {itemHeight}, visibleCount: {visibleCount}, bufferCount: {bufferCount}, poolSize: {poolSize}");

            // プールの初期化
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(itemPrefab, content);
                obj.SetActive(false);
                pool.Add(obj);
                // デバッグログ
                DebugUtility.Log(categly, $"プールにアイテムを追加: {i + 1}/{poolSize}");
            }

            isInitialized = true;
            OnViewInitialized?.Invoke();

            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

            // 初期表示を更新
            UpdateView(0f);
        }

        public void SetUpdateItemCallback(Action<int, GameObject> callback)
        {
            if (callback == null)
            {
                Debug.LogError("UpdateItemCallbackがnullです。");
                return;
            }

            updateItemCallback = callback;
        }

        public void SetTotalItems(int count)
        {
            totalItems = count;
            contentHeight = totalItems * itemHeight;

            // Contentの高さを設定
            content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);

            // スクロール位置をトップにリセット
            scrollRect.verticalNormalizedPosition = 1f;

            // ビューを更新
            UpdateView(0f);
        }

        public void UpdateView(float scrollPosition)
        {
            if (!isInitialized)
            {
                DebugUtility.Log(categly, $"{categly}はまだ初期化されていません。");
                return;
            }

            int firstIndex = Mathf.FloorToInt(scrollPosition / itemHeight);
            firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(totalItems - visibleCount, 0));

            int poolItemIndex = 0;

            for (int i = firstIndex; i < firstIndex + visibleCount && i < totalItems; i++)
            {
                int dataIndex = i;
                GameObject obj = pool[poolItemIndex];

                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                }

                // アイテムの位置を設定
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float yPosition = -(dataIndex * itemHeight);
                    rectTransform.anchoredPosition = new Vector2(0, yPosition);
                    // デバッグログ
                    DebugUtility.Log(categly, $"Item {dataIndex} positioned at Y: {yPosition}");
                }
                else
                {
                    Debug.LogWarning("RectTransformがアイテムに見つかりません。");
                }

                // アイテムを更新
                updateItemCallback?.Invoke(dataIndex, obj);

                // デバッグログ
                DebugUtility.Log(categly, $"表示アイテム: dataIndex={dataIndex}, poolItemIndex={poolItemIndex}");

                poolItemIndex++;
            }

            // 使わないアイテムを非アクティブ化
            for (int i = poolItemIndex; i < poolSize; i++)
            {
                if (pool[i].activeSelf)
                {
                    pool[i].SetActive(false);
                    // デバッグログ
                    DebugUtility.Log(categly, $"非表示アイテム: poolItemIndex={i}");
                }
            }
        }

        private void OnScrollValueChanged(Vector2 normalizedPosition)
        {
            if (!isInitialized)
                return;

            float contentHeightExcess = contentHeight - viewport.rect.height;

            if (contentHeightExcess <= 0)
            {
                UpdateView(0f);
                return;
            }

            float scrollPos = (1f - normalizedPosition.y) * contentHeightExcess;
            scrollPos = Mathf.Clamp(scrollPos, 0f, contentHeightExcess);
            UpdateView(scrollPos);
        }
    }
}
