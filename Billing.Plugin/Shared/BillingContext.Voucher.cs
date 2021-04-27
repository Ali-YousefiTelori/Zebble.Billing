﻿namespace Zebble.Billing
{
    using Olive;
    using System;
    using System.Threading.Tasks;
    using Zebble;

    partial class BillingContext
    {
        public async Task<VoucherApplyStatus?> ApplyVoucher(string code)
        {
            if (code.IsEmpty()) throw new ArgumentNullException(nameof(code));

            if (await UIContext.IsOffline()) throw new Exception("Network connection is not available.");

            if (User == null) throw new Exception("User is not available.");

            var url = new Uri(Options.BaseUri, Options.VoucherApplyPath).ToString();
            var @params = new { User.Ticket, User.UserId, Code = code };

            var result = await BaseApi.Post<ApplyVoucherResult>(url, @params, OnError.Ignore, showWaiting: false);

            if (result?.Status == VoucherApplyStatus.Succeeded) await Refresh();

            return result?.Status;
        }
    }
}
