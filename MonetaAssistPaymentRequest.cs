﻿using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.MonetaAssist
{
    /// <summary>
    /// Helper class for processing payments through
    /// </summary>
    public class MonetaAssistPaymentRequest
    {
        /// <summary>
        /// The store identifier in the MONETA.RU.
        /// </summary>
        public string MntId { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.MonetaAssist.Fields.Amount")]
        public string MntAmount { get; set; }

        /// <summary>
        /// Order GUID
        /// </summary>
        public string MntTransactionId { get; set; }

        /// <summary>
        /// ISO currency code.
        /// 
        /// Possible values: RUB, USD, EUR 
        /// </summary>
        public string MntCurrencyCode { get; set; }

        /// <summary>
        /// Customer id
        /// </summary>
        public int MntSubscriberId { get; set; }

        /// <summary>
        /// Hashcode
        /// </summary>
        public string MntHashcode { get; set; }

        /// <summary>
        /// Indicating that the request is made in test mode
        /// </summary>
        public int MntTestMode { get; set; }

        /// <summary>
        /// Code to identify the sender and check the integrity of files. 
        /// </summary>
        public string MntSignature
        {
            get
            {
                var text =
                    String.Format("{0}{1}{2}{3}{4}{5}{6}", MntId, MntTransactionId, MntAmount, MntCurrencyCode,
                        MntSubscriberId, MntTestMode, MntHashcode);

                return GetMD5(text);
            }
        }

        /// <summary>
        /// Create MD5 hesh sum from string
        /// </summary>
        /// <param name="strToMD5">string to create MD5 sum</param>
        /// <returns>MD5 hesh sum</returns>
        public string GetMD5(string strToMD5)
        {
            var enc = Encoding.Default.GetEncoder();
            var length = strToMD5.Length;
            var data = new byte[length];
            enc.GetBytes(strToMD5.ToCharArray(), 0, length, data, 0, true);
            byte[] result;

            using (var md5 = new MD5CryptoServiceProvider())
            {
                result = md5.ComputeHash(data);
            }

            return BitConverter.ToString(result)
                .Replace("-", string.Empty).ToLower();
        }

        /// <summary>
        /// MONETA.Assistant url
        /// </summary>
        public string MonetaAssistantUrl
        {
            get
            {
#if DEBUG
                return "https://demo.moneta.ru/assistant.htm";
#endif

#if !DEBUG
                return "https://www.payanyway.ru/assistant.htm";
#endif
            }
        }

        /// <summary>
        /// Create PaymentInfoModel by settings
        /// </summary>
        /// <param name="settings">Payment settings</param>
        /// <param name="customerId">Customer id</param>
        /// <param name="orderGuid">Order GUID</param>
        /// <param name="orderTotal">Total sum</param>
        public static MonetaAssistPaymentRequest CreatePaymentInfoModel(MonetaAssistPaymentSettings settings, int customerId, Guid orderGuid, decimal orderTotal, string currencyCode)
        {
            return new MonetaAssistPaymentRequest
            {
                MntId = settings.MntId,
                MntTestMode = settings.MntTestMode ? 1 : 0,
                MntHashcode = settings.Hashcode,
                MntSubscriberId = customerId,
                MntTransactionId = orderGuid.ToString(),
                MntCurrencyCode = currencyCode,
                MntAmount = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", orderTotal)
            };
        }
    }
}