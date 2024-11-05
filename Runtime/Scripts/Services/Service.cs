namespace jp.go.aist3ddbclient
{
    public class Service
    {
        private ServiceRepository repository = new ServiceRepository();

        public void GetServices(ServiceRepository.OnDataReceived onDataReceived)
        {
            CoroutineRunner.Instance.StartCoroutine(repository.GetServices(onDataReceived));
        }
    }
}