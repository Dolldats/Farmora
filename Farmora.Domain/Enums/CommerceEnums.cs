namespace Farmora.Domain.Enums;

public enum OrderStatus
{
    Pending,
    PaymentPending,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded,
    Failed
}

public enum PaymentStatus
{
    Pending,
    Authorized,
    Captured,
    Failed,
    Refunded,
    PartiallyRefunded,
    Cancelled,
    RequiresReview
}

public enum DeliveryStatus
{
    Pending,
    ReadyForPickup,
    InTransit,
    Delivered,
    Failed,
    Cancelled
}

public enum DeliveryMethod
{
    Pickup,
    FarmerDelivery,
    OperatorDelivery
}

public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected
}

public enum ReportStatus
{
    Open,
    InReview,
    Resolved,
    Dismissed
}

public enum NotificationChannel
{
    InApp,
    Email,
    Sms
}

public enum VerificationDocumentType
{
    NationalId,
    CacCertificate,
    FarmOwnership,
    Other
}

public enum PromotionType
{
    Percentage,
    FixedAmount
}

public enum CouponStatus
{
    Active,
    Expired,
    Exhausted,
    Disabled
}
