using Microsoft.Practices.Unity;
using SourceControlSync.DataAWS;
using SourceControlSync.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceControlSync.WebApi.Factories
{
    public class UnityRepositoryFactory : IRepositoryFactory
    {
        private readonly IUnityContainer _unityContainer;

        public UnityRepositoryFactory(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public ISourceRepository CreateSourceRepository(string connectionString)
        {
            var downloadRequest = _unityContainer.Resolve<IDownloadRequest>(
                new ParameterOverride("connectionString", connectionString));
            return _unityContainer.Resolve<ISourceRepository>(new ParameterOverride("downloadRequest", downloadRequest));
        }

        public IDestinationRepository CreateDestinationRepository(string connectionString)
        {
            var deleteItemCommand = _unityContainer.Resolve<IItemCommand>("deleteItemCommand",
                new ParameterOverride("connectionString", connectionString));
            var uploadItemCommand = _unityContainer.Resolve<IItemCommand>("uploadItemCommand",
                new ParameterOverride("connectionString", connectionString));
            var nullItemCommand = _unityContainer.Resolve<IItemCommand>("nullItemCommand");
            return _unityContainer.Resolve<IDestinationRepository>(
                new ParameterOverride("deleteCommand", deleteItemCommand),
                new ParameterOverride("uploadCommand", uploadItemCommand),
                new ParameterOverride("nullCommand", nullItemCommand)
                );
        }
    }
}