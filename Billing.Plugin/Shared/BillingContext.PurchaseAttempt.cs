﻿namespace Zebble.Billing
{
    using System;
    using System.Threading.Tasks;
    using Zebble;
    using Zebble.Device;

    partial class BillingContext
    {
        public static async Task PurchaseAttempt(SubscriptionPurchasedEventArgs args)
        {
            if (await UIContext.IsOffline())
            {
                await Alert.Show("Network connection is not available.");
                return;
            }

            try
            {
                var url = new Uri(Options.BaseUri, Options.PurchaseAttemptPath).ToString();
                var @params = new { User.Ticket, User.UserId, args.ProductId, Platform = PaymentAuthority, args.PurchaseToken };
                await BaseApi.Post(url, @params, OnError.Ignore, showWaiting: false);

                await SubscriptionPurchased.Raise(args);
            }
            catch { }
        }

        static string PaymentAuthority
        {
            get
            {
#if CAFEBAZAAR
                return "CafeBazaar";
#else
                return OS.Platform switch
                {
                    DevicePlatform.IOS => "AppStore",
                    DevicePlatform.Android => "GooglePlay",
                    _ => "Windows Store",
                };
#endif
            }
        }
    }
}