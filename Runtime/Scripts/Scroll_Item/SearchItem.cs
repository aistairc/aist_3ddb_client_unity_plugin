using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CesiumForUnity;
using System.Collections;
using UnityEngine.Events;
using System;

namespace jp.go.aist3ddbclient
{
	public class SearchItem : UIBehaviour 
	{
		[SerializeField]
		private GameObject _BalloonPrefab;

		[SerializeField]
		private TextMeshProUGUI _uiTitle;

		[SerializeField]
		private TextMeshProUGUI _uiSummry;

		[SerializeField]
		private Button _infomationButton;

		[SerializeField]
		private Button _addFavButton;

        private Feature _feature;
		private Cesium3DTileset _tileset;
		OptimizedScrollViewPresenter<Feature> _favPresenter;
        private CesiumFlyToController  _cesiumFlyToController;


        protected override void Start()
		{
			_infomationButton.onClick.AddListener(OnClickInfomation);
			_addFavButton.onClick.AddListener(OnClickAddToFav);
		}

		public void SetDatasource(Feature feature, Cesium3DTileset cesium3DTileset, OptimizedScrollViewPresenter<Feature> favPresenter, CesiumFlyToController  cesiumFlyToController)
		{
			_feature = feature;
			_tileset = cesium3DTileset;
			_favPresenter = favPresenter;
			_cesiumFlyToController = cesiumFlyToController;

			_uiTitle.text = feature.properties.title;
			_uiSummry.text = $"{feature.properties.creation_date},{feature.properties.location},{feature.properties.author}";
		}

		public void OnClickInfomation()
		{
			// _BalloonPrefabを画面中央に生成し、一定時間が経過したら削除する
			StartCoroutine(DestroyBalloonAfterDelay());
		}

		private IEnumerator DestroyBalloonAfterDelay()
		{
			var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();


			var balloon = Instantiate(_BalloonPrefab);
			var balloonScript = balloon.GetComponent<Balloon>();
			balloonScript.SetData(_feature);
			
			var ballonRectTransform = balloon.GetComponent<RectTransform>();
			var balloonWidth = ballonRectTransform.rect.width;
			var balloonHeight = ballonRectTransform.rect.height;
			balloon.transform.SetParent(canvas.transform, false);
			balloon.transform.localPosition = new Vector2(-balloonWidth / 2, -balloonHeight / 2);

			yield return new WaitForSeconds(3); // Change the delay time as needed

			Destroy(balloon);
		}

		public void OnClickAddToFav()
		{
			_favPresenter.AddItem(_feature);
		}
	}
}