using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.WebApi.Factories;
using SourceControlSync.WebApi.App_Start;
using SourceControlSync.Domain;

namespace SourceControlSync.WebApi.Tests
{
    [TestClass]
    public class UnityRepositoryFactoryTests
    {
        [TestMethod]
        public void CreateSourceRepository()
        {
            var unityContainer = UnityConfig.GetConfiguredContainer();
            var factory = new UnityRepositoryFactory(unityContainer);

            var sourceRepository = factory.CreateSourceRepository("");

            Assert.IsNotNull(sourceRepository);
            Assert.IsInstanceOfType(sourceRepository, typeof(ISourceRepository));
        }

        [TestMethod]
        public void CreateDestinationRepository()
        {
            var unityContainer = UnityConfig.GetConfiguredContainer();
            var factory = new UnityRepositoryFactory(unityContainer);

            var destinationRepository = factory.CreateDestinationRepository("");

            Assert.IsNotNull(destinationRepository);
            Assert.IsInstanceOfType(destinationRepository, typeof(IDestinationRepository));
        }
    }
}
