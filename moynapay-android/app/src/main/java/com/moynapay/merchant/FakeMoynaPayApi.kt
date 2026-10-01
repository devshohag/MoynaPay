package com.moynapay.merchant

import java.util.UUID

object FakeMoynaPayApi : MoynaPayApi {
    private const val MerchantId = "01929999-0000-7000-8000-000000000001"
    private const val DemoOtp = "1234"

    override fun requestOtp(msisdn: String): OtpRequestResult =
        OtpRequestResult(
            sent = true,
            devOtp = DemoOtp,
            message = "Fake OTP accepted until the real API is enabled."
        )

    override fun verifyOtp(msisdn: String, otp: String): LoginSession {
        val cleaned = otp.filter(Char::isDigit).ifBlank { DemoOtp }
        require(cleaned == DemoOtp) { "Use fake OTP $DemoOtp for now." }

        return LoginSession(
            merchantId = MerchantId,
            merchantName = "MoynaPay Demo Store",
            accessToken = "fake_access_${UUID.randomUUID()}",
            refreshToken = "fake_refresh_${UUID.randomUUID()}"
        )
    }

    override fun createPairingToken(accessToken: String): PairingTokenResult =
        PairingTokenResult(
            token = "fake_pair_${UUID.randomUUID()}",
            expiresAt = "fake"
        )

    override fun pairDevice(
        pairingToken: String,
        fingerprint: String,
        name: String?,
        model: String?,
        appVersion: String?
    ): PairedDeviceSession =
        PairedDeviceSession(
            deviceId = UUID.randomUUID().toString(),
            deviceToken = "fake_device_${UUID.randomUUID()}",
            fingerprint = fingerprint
        )

    override fun bootstrap(accessToken: String): BootstrapSummary =
        BootstrapSummary(
            merchantName = "MoynaPay Demo Store",
            callsEnabled = true,
            paymentsEnabled = true,
            courierEnabled = false,
            rawJson = "{}"
        )

    override fun homeNumbers(accessToken: String): HomeNumbers =
        HomeNumbers(rawJson = "{}")

    override fun orders(accessToken: String, status: String?, take: Int): OrdersPage =
        OrdersPage(
            items = listOf(
                OrderSummaryDto("ORD-1041", "Confirmed", "Sadia Akter", "1250"),
                OrderSummaryDto("ORD-1040", "NeedsHuman", "Rafiqul Islam", "2990")
            ),
            nextBefore = null,
            rawJson = "{}"
        )

    override fun heartbeat(
        deviceId: String,
        deviceToken: String,
        permissionState: String?,
        batteryPercent: Int?,
        networkType: String?,
        appVersion: String?,
        model: String?
    ): DeviceHeartbeatResult =
        DeviceHeartbeatResult(
            deviceId = deviceId,
            permissionState = permissionState,
            batteryPercent = batteryPercent,
            networkType = networkType,
            rawJson = "{}"
        )

    override fun uploadEvents(
        deviceId: String,
        deviceToken: String,
        events: List<RawPaymentEvent>
    ): IngestEventsResult =
        IngestEventsResult(
            accepted = events.size,
            duplicates = 0,
            rejected = 0,
            rawJson = "{}"
        )
}
