﻿namespace Zebble.Billing
{
    using System;
    using System.Threading.Tasks;
    using Plugin.InAppBilling;
    using Olive;

    class RestoreSubscriptionCommand<T> : StoreCommandBase<bool> where T : Product
    {
        protected override async Task<bool> DoExecute()
        {
            return await Fetch(ItemType.Subscription) | await Fetch(ItemType.InAppPurchase);
        }

        async Task<bool> Fetch(ItemType type)
        {
            try
            {
                var purchases = await Billing.GetPurchasesAsync(type);
                if (purchases.None()) return false;

                foreach (var purchase in purchases)
                    await BillingContext<T>.Current.PurchaseAttempt(await purchase.ToEventArgs<T>());

                return true;
            }
            catch (Exception ex)
            {
                Log.For(this).Error(ex);
                return false;
            }
        }
    }
}