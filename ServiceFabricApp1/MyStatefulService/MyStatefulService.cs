using Common;
using Common.Interfaces;
using DataAccess;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace MyStatefulService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MyStatefulService : StatefulService, IMyStatefulService
    {
        StatefulServiceContext context;

        public MyStatefulService(StatefulServiceContext context)
            : base(context)
        {
            this.context = context;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            //return new ServiceReplicaListener[0];
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }

        public async Task AddToQueueAsync(OptOut request)
        {
            var queue = await StateManager.GetOrAddAsync<IReliableQueue<OptOut>>(QueueNames.OptOutQueue);

            using (var tx = StateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, request);

                await tx.CommitAsync();
            }
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<OptOut>>(QueueNames.OptOutQueue);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await queue.TryDequeueAsync(tx);

                    if (result.HasValue)
                    {
                        var request = result.Value;
                        var config = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                        var connString = config.Settings.Sections["DatabaseSettings"].Parameters["TestConnectionString"].Value;

                        IOptOutRepository repo = new OptOutRepository(connString);

                        ServiceEventSource.Current.ServiceMessage(Context, $"Successfully dequeued OptOut for CampaignId: { request.CampaignId } by { request.EmailAddress }");

                        try
                        {
                            await repo.AddOptOutAsync(request);

                            ServiceEventSource.Current.ServiceMessage(Context, $"OptOut for CampaignId: { request.CampaignId } by { request.EmailAddress } recorded successfully.");

                            // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                            // discarded, and nothing is saved to the secondary replicas.
                            await tx.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            ServiceEventSource.Current.Error($"Error encountered saving OptOut: { ex.Message }");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
