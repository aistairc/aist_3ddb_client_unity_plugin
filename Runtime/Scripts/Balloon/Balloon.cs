using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace jp.go.aist3ddbclient 
{
    public class Balloon : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _balloonTitle;
        [SerializeField]
        private TextMeshProUGUI _balloonLocation;
        [SerializeField]
        private TextMeshProUGUI _balloonAuthor;
        [SerializeField]
        private TextMeshProUGUI _balloonCreationDate;
        [SerializeField]
        private TextMeshProUGUI _balloonLicense;
        [SerializeField]
        private TextMeshProUGUI _balloonDescription;

        public void SetData(Feature feature)
        {
            _balloonTitle.text = feature.properties.title;
            _balloonLocation.text = $"場所: {feature.properties.location}";
            _balloonAuthor.text = $"作成者: {feature.properties.author}"; 
            _balloonCreationDate.text = $"作成日: {feature.properties.creation_date}";
            _balloonLicense.text = $"ライセンス: {feature.properties.license}";
            _balloonDescription.text = feature.properties.description;
        }
    }
}