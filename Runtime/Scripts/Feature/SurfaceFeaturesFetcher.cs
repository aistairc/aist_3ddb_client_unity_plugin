using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Mathematics; // Cesium for Unityの名前空間
using CesiumForUnity;

using Newtonsoft.Json;
using TMPro;
using System;

namespace jp.go.aist3ddbclient
{
    public class SurfaceFeaturesFetcher : MonoBehaviour
    {
        private readonly string _apiUrl = "https://gsrt.digiarc.aist.go.jp/3ddb_demo/api/v1/services/";

        private Camera _camera;

        private StringBuilder _poygonBuilder = new StringBuilder();

        [SerializeField]
        private CesiumGeoreference _georeference;

        [SerializeField]
        private CesiumFlyToController  _flyToController ;


        [SerializeField]
        private Cesium3DTileset _tileset;

        [SerializeField]
        private TMP_Dropdown _serviceNameDropdown;

        [SerializeField]
        private TMP_InputField _keywordText;

        [SerializeField]
        private TextMeshProUGUI _CountText;

        [SerializeField]
        private OptimizedScrollViewView _seachScrollView;

        private OptimizedScrollViewModel<Feature> _searchScrollModel;
        private OptimizedScrollViewPresenter<Feature> _searchScrollPresenter;

        [SerializeField]
        private OptimizedScrollViewView _favScrollView;

        private OptimizedScrollViewModel<Feature> _favScrollModel;
        private OptimizedScrollViewPresenter<Feature> _favScrollPresenter;

        void Start()
        {
            _camera = Camera.main; // メインカメラを取得

            initialize_fav();   // Search側にFavへのコールバックを設定するため、こちらを先に初期化する必要がある
            initialize_serach();
        }

        private void initialize_fav()
        {
            // 必要なコンポーネントが設定されているか確認
            if (_favScrollView == null)
            {
                Debug.LogError("_favScrollView is null.");
            }

            // ModelとPresenterの初期化
            _favScrollModel = new OptimizedScrollViewModel<Feature>();

            // ユーザー提供のアイテム更新コールバックを定義
            Action<int, GameObject, Feature> updateItemCallback = (index, item, data) =>
            {
                var favItem = item.GetComponentInChildren<FavItem>();
                if (favItem != null)
                {
                    favItem.SetDatasource(data, _tileset, _favScrollPresenter, _flyToController);
                    favItem.OnViewModel();
                }
                else
                {
                    Debug.LogWarning($"Search: ${item}にFavItemコンポーネントが見つかりません。");
                }
            };

            _favScrollPresenter = new OptimizedScrollViewPresenter<Feature>(_favScrollModel, _favScrollView, updateItemCallback);
        }

        private void initialize_serach()
        {
            // 必要なコンポーネントが設定されているか確認
            if (_seachScrollView == null)
            {
                Debug.LogError("_seachScrollView is null.");
            }

            // ModelとPresenterの初期化
            _searchScrollModel = new OptimizedScrollViewModel<Feature>();

            // ユーザー提供のアイテム更新コールバックを定義
            Action<int, GameObject, Feature> updateItemCallback = (index, item, data) =>
            {
                var SearchItem = item.GetComponentInChildren<SearchItem>();
                if (SearchItem != null)
                {
                    SearchItem.SetDatasource(data, _tileset, _favScrollPresenter, _flyToController);
                }
                else
                {
                    Debug.LogWarning($"Search: ${item}にSearchItemコンポーネントが見つかりません。");
                }
            };

            _searchScrollPresenter = new OptimizedScrollViewPresenter<Feature>(_searchScrollModel, _seachScrollView, updateItemCallback);
        }

        private string MakeAreaString()
        {
            // ビューポートの4隅のスクリーン座標をワールド座標に変換
            // PoLygon用に左上から時計回りに指定し、最後に左上で閉じる
            Vector3[] viewportPoints = new Vector3[]
            {
                new Vector3(0           , Screen.height, _camera.farClipPlane),
                new Vector3(Screen.width, Screen.height, _camera.farClipPlane),
                new Vector3(Screen.width, 0,             _camera.farClipPlane),
                new Vector3(0           , 0,             _camera.farClipPlane),
                new Vector3(0           , Screen.height, _camera.farClipPlane)
            };

            _poygonBuilder.Clear();
            _poygonBuilder.Append("POLYGON%28%28");

            // それぞれのポイントを地球上の緯度経度に変換（仮の関数を使用）
            foreach (var point in viewportPoints)
            {

                var ray = _camera.ScreenPointToRay(point);
                if (Physics.Raycast(ray, out var hit))
                {
                    var objectHit = hit.transform;
                    var hitPition = new double3(objectHit.position);

                    var value = _georeference.TransformUnityPositionToEarthCenteredEarthFixed(hitPition);
                    var xyz = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(value);

                    var latitude = xyz.y;
                    var longitude = xyz.x;
                    // var height = xyz.z;
                    // Debug.Log($"Latitude: {latitude}, Longitude: {longitude}");
                    _poygonBuilder.Append($"{longitude}+{latitude}%2C");
                }
            }
            // 最後の , (%2C) を削除
            _poygonBuilder.Remove(_poygonBuilder.Length - 3, 3);
            _poygonBuilder.Append("%29%29");

            return _poygonBuilder.ToString();
        }

        public void GetSurfaceFeatures()
        {
            StartCoroutine(GetSurfaceFeaturesCoroutine(MakeAreaString(), 0, 1000));
        }

        public void GetSurfaceFeaturesByKeyword()
        {
            string keyword = _keywordText.text;
            StartCoroutine(GetSurfaceFeaturesBykeywordCoroutine(keyword, 0, 1000));
        }

        IEnumerator GetSurfaceFeaturesCoroutine(string area, int minz, int maxz)
        {
            // 選択中のサービスの名前を取得
            var searvive_name = _serviceNameDropdown.options[_serviceNameDropdown.value].text;
            var url = _apiUrl + $"/{searvive_name}/features" + $"?area={area}&minz={minz}&maxz={maxz}";

            yield return GetFeatures(url);
        }

        IEnumerator GetSurfaceFeaturesBykeywordCoroutine(string keyword, int minz, int maxz)
        {
            // 選択中のサービスの名前を取得
            var searvive_name = _serviceNameDropdown.options[_serviceNameDropdown.value].text;

            // URLの組み立て
            var url = new StringBuilder($"{_apiUrl}/{searvive_name}/features?");
            if (!String.IsNullOrEmpty(keyword))
            {
                url.Append($"string={keyword}&");
            }
            url.Append($"minz={minz}&maxz={maxz}");

            yield return GetFeatures(url.ToString());
        }

        IEnumerator GetFeatures(string url)
        {
            //Debug.Log(url);

            using var webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                yield return null;
            }
            else
            {
                UpdateSearchResult(webRequest.downloadHandler.text);
            }
        }

        private void UpdateSearchResult(string text)
        {
            // Debug.Log(text);
            ClearSearchResult();
            ViewSearchResult(text);
        }

        private void ClearSearchResult()
        {
            _searchScrollPresenter.ClearItems();
        }

        private void ViewSearchResult(string text)
        {
            var json = text;

            FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(text);
            _CountText.text = $"{featureCollection.properties["num"]}件/全データー{featureCollection.properties["all"]}件";

            foreach (var feature in featureCollection.features)
            {
                if (feature.properties != null)
                {
                    _searchScrollPresenter.AddItem(feature);
                }
            }
        }
    }
}