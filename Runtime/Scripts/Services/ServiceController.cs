using System;
using TMPro;
using UnityEngine;

namespace jp.go.aist3ddbclient
{
    public class ServiceController : MonoBehaviour
    {
        private Service _service = new Service();
        private ServicesListModel _servicesList = null;

        [SerializeField]
        private TMP_Dropdown _serviceDropdown;

        void Start()
        {
            _service.GetServices(ProcessServices);
        }

        private void ProcessServices(ServicesListModel servicesList)
        {
            int allIndex = 0;

            _servicesList = servicesList;
            _serviceDropdown.options.Clear();
            foreach (var service in servicesList.services)
            {
//                Debug.Log($"ID: {service.id}, Name: {service.name}, Type: {service.type}, Description: {service.description}");
                if (service.type == "3D") {
                    var option = new TMP_Dropdown.OptionData[] { new TMP_Dropdown.OptionData(service.name) };
                    _serviceDropdown.options.Add(option[0]);
                    if (String.Compare(service.name, "3D_ALL", true) == 0) {
                        allIndex = _serviceDropdown.options.Count - 1;
                    }
                }
            }
            _serviceDropdown.value = allIndex;
            _serviceDropdown.RefreshShownValue();
        }
    }
}
