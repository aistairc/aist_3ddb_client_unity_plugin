namespace jp.go.aist3ddbclient
{
    [System.Serializable]
    public class ServiceModel
    {
        public int id;
        public string name;
        public string type;
        public string description;
    }

    [System.Serializable]
    public class ServicesListModel
    {
        public ServiceModel[] services;
    }
}