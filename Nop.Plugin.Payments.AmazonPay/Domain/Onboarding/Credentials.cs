using Newtonsoft.Json;

namespace Nop.Plugin.Payments.AmazonPay.Domain.Onboarding;

/// <summary>
/// Represents account credentials details
/// </summary>
public class Credentials
{
    #region Properties

    /// <summary>
    /// Gets or sets the merchant id used in API requests and widget render requests
    /// </summary>
    [JsonProperty(PropertyName = "merchantId")]
    public string MerchantId { get; set; }

    /// <summary>
    /// Gets or sets the store identifier
    /// </summary>
    [JsonProperty(PropertyName = "storeId")]
    public string StoreId { get; set; }

    /// <summary>
    /// Gets or sets the public key identifier used to sign API requests
    /// </summary>
    [JsonProperty(PropertyName = "publicKeyId")]
    public string PublicKeyId { get; set; }

    #endregion
}