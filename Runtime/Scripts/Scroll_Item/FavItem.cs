using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CesiumForUnity;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace jp.go.aist3ddbclient
{
	public class FavItem : UIBehaviour 
	{
		private RectTransform _rectTransform;

		private Image _uiBackground;

		[SerializeField]
		private TextMeshProUGUI _uiTitle;

		[SerializeField]
		private Button _deleteButton;

        private Feature _feature;
		private Cesium3DTileset _tileset;
		private OptimizedScrollViewPresenter<Feature> _favPresenter;

		private CesiumFlyToController _flyToController;

		private System.Random _random = new System.Random();

        protected override void Start()
		{
			_rectTransform = GetComponent<RectTransform>();
			_uiBackground = GetComponent<Image>();

			var viewButton = GetComponent<Button>();
			viewButton.onClick.AddListener(OnViewModel);
			_deleteButton.onClick.AddListener(OnClickDelete);
		}

		public void SetDatasource(Feature feature, Cesium3DTileset cesium3DTileset, OptimizedScrollViewPresenter<Feature> favPresenter, CesiumFlyToController flyToController)
		{
			_feature = feature;
			_tileset = cesium3DTileset;
			_favPresenter = favPresenter;
			_flyToController = flyToController;
			
			_uiTitle.text = feature.properties.title;
		}

		public void OnClickDelete()
		{
			_tileset.url = "";
			_favPresenter.RemoveItem(_feature);
		}

        public void OnViewModel()
        {
			_tileset.url = _feature.properties.threeD_tiles_url;

			List<(double X, double Y)> result = new List<(double X, double Y)>();
			foreach(var geometry in _feature.geometries)
			{
				//Debug.Log($"type={geometry.type}");
				if (geometry.type == "Polygon")
				{
					var coordinates = geometry.coordinates;
					CenterOfMassCalculator.ProcessPolygon(result, (JArray)coordinates);
				} else if (geometry.type == "MultiPolygon")
				{
					var coordinates = geometry.coordinates;
					CenterOfMassCalculator.ProcessMultiPolygon(result, (JArray)coordinates);
				}
			}
			(var x, var y) = CenterOfMassCalculator.GetCenterOfMass(result);
			//Debug.Log($"x={x}, y={y}");
			Fly(x, y);
        }

		public void Fly(double targetLongitude, double targetLatitude)
		{
			double targetHeight = 300.0;
            // 目的地の緯度・経度・高度を double3 に格納
            double3 destination = new double3(targetLongitude-0.00305, targetLatitude+-0.003, targetHeight);

            // カメラを指定した緯度・経度・高度に飛行させる
            // 必要なパラメータ：目的地、Yaw、Pitch、移動による中断可否
            _flyToController.FlyToLocationLongitudeLatitudeHeight(
                destination,                          // 緯度・経度・高度の double3
                0, // 目的地での Yaw（度数法）
                45f,                                  // 目的地での Pitch（度数法）
                true                                  // 移動による中断を許可するか
            );
		}
    }
}