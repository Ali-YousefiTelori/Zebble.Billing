﻿namespace Zebble.Billing
{
    using System;
    using System.Threading.Tasks;
    using Zebble;
    using Olive;

    partial class BillingContext
    {
        public static AsyncEvent<VoucherAppliedEventArgs> VoucherApplied = new();

        public static async Task<DateTime?> ValidateVoucher(string code)
        {
            if (await UIContext.IsOffline())
            {
                await Alert.Show("Network connection is not available.");
                return null;
            }

            try
            {
                var url = BaseUrl + "voucher/apply/" + User.UserId + "/" + code;
                var result = await BaseApi.Post<DateTime?>(url, null, OnError.Ignore, showWaiting: false);
                if (result == null) return null;
                else if (result > LocalTime.UtcNow)
                {
                    await VoucherApplied.Raise(new VoucherAppliedEventArgs { VoucherCode = code });
                }

                return result;
            }
            catch { return null; }
        }

        public static async Task<DateTime?> ApplyVoucher(string code)
        {
            if (await ValidateVoucher(code) != null)
                await RestoreSubscriptions(userRequest: true);

            return User?.SubscriptionExpiry;
        }
    }
}