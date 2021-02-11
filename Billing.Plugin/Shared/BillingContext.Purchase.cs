﻿namespace Zebble.Billing
{
    using System;
    using System.Threading.Tasks;
    using Olive;

    partial class BillingContext<T>
    {
        public async Task<string> PurchaseSubscription(string productId)
        {
            var product = await ProductProvider.GetById(productId);
            return await new PurchaseSubscriptionCommand<T>(product).Execute()
                 ?? "Failed to connect to the store. Are you connected to the network? If so, try 'Pay with Card'.";
        }

        public async Task<bool> RestoreSubscription(bool userRequest = false)
        {
            var errorMessage = "";
            try { await new RestoreSubscriptionCommand<T>().Execute(); }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.For(typeof(BillingContext<T>)).Error(ex);
            }

            var successful = false;
            try
            {
                await Refresh();
                successful = IsSubscribed();
            }
            catch (Exception ex)
            {
                if (errorMessage.IsEmpty()) errorMessage = ex.Message;
                Log.For(typeof(BillingContext<T>)).Error(ex);
            }

            if (!successful && userRequest)
            {
                if (errorMessage.IsEmpty()) errorMessage = "Unable to find an active subscription.";
                await Alert.Show(errorMessage);
            }

            return successful;
        }

        internal async Task PurchaseAttempt(SubscriptionPurchasedEventArgs<T> args)
        {
            if (await UIContext.IsOffline())
            {
                await Alert.Show("Network connection is not available.");
                return;
            }

            try
            {
                var url = new Uri(Options.BaseUri, Options.PurchaseAttemptPath).ToString();
                var @params = new { User.Ticket, User.UserId, ProductId = args.Product.Id, Platform = PaymentAuthority, args.PurchaseToken };
                await BaseApi.Post(url, @params, OnError.Ignore, showWaiting: false);

                await SubscriptionPurchased.Raise(args);
            }
            catch { }
        }
    }
}
