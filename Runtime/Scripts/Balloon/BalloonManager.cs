using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace jp.go.aist3ddbclient
{
    public class BalloonManager : MonoBehaviour
    {
        [SerializeField] 
        private GameObject _balloonPrefab;

        private static GameObject _balloonInstance;
        private static BalloonManager _instance;

        /// <summary>
        /// Singleton instance
        /// </summary>

        public static BalloonManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<BalloonManager>();
                }
                return _instance;
            }
        }
    }
}