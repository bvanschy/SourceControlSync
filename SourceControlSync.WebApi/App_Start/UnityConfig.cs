using Microsoft.Practices.Unity;
using SourceControlSync.DataAWS;
using SourceControlSync.DataVSO;
using SourceControlSync.Domain;
using SourceControlSync.WebApi.Factories;
using SourceControlSync.WebApi.TraceListeners;
using SourceControlSync.WebApi.Util;
using System;
using System.Diagnostics;

namespace SourceControlSync.WebApi.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below. Make sure to add a Microsoft.Practices.Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            container.RegisterType<IChangesCalculator, ChangesCalculator>();
            container.RegisterType<IRepositoryFactory, UnityRepositoryFactory>();
            container.RegisterType<ISourceRepository, SourceRepository>();
            container.RegisterType<IDownloadRequest, DownloadRequest>();
            container.RegisterType<IDestinationRepository, DestinationRepository>();
            container.RegisterType<IItemCommand, DeleteItemCommand>("deleteItemCommand");
            container.RegisterType<IItemCommand, UploadItemCommand>("uploadItemCommand");
            container.RegisterType<IItemCommand, NullItemCommand>("nullItemCommand");
            container.RegisterType<TraceListener, SmtpTraceListener>();
            container.RegisterType<ILogger, Logger>();
        }
    }
}
