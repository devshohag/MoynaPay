package com.moynapay.merchant

data class OtpRequestResult(
    val sent: Boolean,
    val devOtp: String?,
    val message: String?
)

data class LoginSession(
    val merchantId: String,
    val merchantName: String,
    val accessToken: String,
    val refreshToken: String
)

data class PairingTokenResult(
    val token: String,
    val expiresAt: String
)

data class PairedDeviceSession(
    val deviceId: String,
    val deviceToken: String,
    val fingerprint: String
)

data class BootstrapSummary(
    val merchantName: String,
    val callsEnabled: Boolean,
    val paymentsEnabled: Boolean,
    val courierEnabled: Boolean,
    val rawJson: String
)

data class HomeNumbers(
    val rawJson: String
)

data class OrderSummaryDto(
    val reference: String,
    val status: String,
    val customerName: String,
    val amount: String
)

data class OrdersPage(
    val items: List<OrderSummaryDto>,
    val nextBefore: String?,
    val rawJson: String
)

data class DeviceHeartbeatResult(
    val deviceId: String,
    val permissionState: String?,
    val batteryPercent: Int?,
    val networkType: String?,
    val rawJson: String
)

data class RawPaymentEvent(
    val source: String,
    val senderId: String,
    val body: String,
    val receivedAt: String
)

data class IngestEventsResult(
    val accepted: Int,
    val duplicates: Int,
    val rejected: Int,
    val rawJson: String
)
