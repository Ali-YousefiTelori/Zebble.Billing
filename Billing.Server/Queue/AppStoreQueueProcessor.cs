﻿namespace Zebble.Billing
{
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;

    public class AppStoreQueueProcessor : IQueueProcessor
    {
        readonly AppStoreOptions _options;

        public SubscriptionPlatform Platform => SubscriptionPlatform.AppStore;

        public AppStoreQueueProcessor(IOptions<AppStoreOptions> options)
        {
            _options = options.Value;
        }

        public Task<int> Process()
        {
            return Task.FromResult(0);
        }
    }
}