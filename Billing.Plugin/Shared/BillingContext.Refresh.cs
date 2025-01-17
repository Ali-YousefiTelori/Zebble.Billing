﻿namespace Zebble.Billing
{
    using System;
    using System.Threading.Tasks;
    using Zebble;
    using Olive;

    partial class BillingContext
    {
        /// <summary>
        /// Queries the latest subscription status from the server in background.
        /// </summary>
        /// <remarks>An active internet connection is required.</remarks>
        public async Task BackgroundRefresh()
        {
            while (User == null) await Task.Delay(500);
            if (Subscription is not null)
            {
                if (Subscription.ExpirationDate is null) return;
                if (Subscription.ExpirationDate?.AddDays(-2) > LocalTime.Now) return;
            }

            await UIContext.AwaitConnection();
            try { await DoRefresh(); }
            catch { /*Ignore*/ }
        }

        /// <summary>
        /// Queries the latest subscription status from the server.
        /// </summary>
        public async Task Refresh()
        {
            try { await DoRefresh(); }
            catch (Exception ex) { Log.For<Subscription>().Error(ex); }
        }

        async Task DoRefresh()
        {
            if (User == null) return;

            var url = new Uri(Options.BaseUri, Options.SubscriptionStatusPath).ToString();
            var @params = new { User.Ticket, User.UserId };
            var current = await BaseApi.Post<Subscription>(url, @params, errorAction: OnError.Ignore);

            Subscription = current;
            await SubscriptionFileStore.Save();

            await SubscriptionRestored.Raise(current.ToEventArgs());
        }
    }
}
