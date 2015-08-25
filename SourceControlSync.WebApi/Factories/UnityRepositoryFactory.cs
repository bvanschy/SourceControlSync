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
        private readonly IUnityContainer unityContainer;

        public UnityRepositoryFactory(IUnityContainer unityContainer)
        {
            this.unityContainer = unityContainer;
        }

        public ISourceRepository CreateSourceRepository(string connectionString)
        {
            var downloadRequest = this.unityContainer.Resolve<IDownloadRequest>(
                new ParameterOverride("connectionString", connectionString));
            return this.unityContainer.Resolve<ISourceRepository>(new ParameterOverride("downloadRequest", downloadRequest));
        }

        public IDestinationRepository CreateDestinationRepository(string connectionString)
        {
            var deleteItemCommand = this.unityContainer.Resolve<IItemCommand>("deleteItemCommand",
                new ParameterOverride("connectionString", connectionString));
            var uploadItemCommand = this.unityContainer.Resolve<IItemCommand>("uploadItemCommand",
                new ParameterOverride("connectionString", connectionString));
            var nullItemCommand = this.unityContainer.Resolve<IItemCommand>("nullItemCommand");
            return this.unityContainer.Resolve<IDestinationRepository>(
                new ParameterOverride("deleteCommand", deleteItemCommand),
                new ParameterOverride("uploadCommand", uploadItemCommand),
                new ParameterOverride("nullCommand", nullItemCommand)
                );
        }
    }
}