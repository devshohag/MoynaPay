package com.moynapay.merchant

import android.content.Context

interface MoynaPayApi {
    fun requestOtp(msisdn: String): OtpRequestResult

    fun verifyOtp(msisdn: String, otp: String): LoginSession

    fun createPairingToken(accessToken: String): PairingTokenResult

    fun pairDevice(
        pairingToken: String,
        fingerprint: String,
        name: String?,
        model: String?,
        appVersion: String?
    ): PairedDeviceSession

    fun bootstrap(accessToken: String): BootstrapSummary

    fun homeNumbers(accessToken: String): HomeNumbers

    fun orders(accessToken: String, status: String? = null, take: Int = 50): OrdersPage

    fun heartbeat(
        deviceId: String,
        deviceToken: String,
        permissionState: String?,
        batteryPercent: Int?,
        networkType: String?,
        appVersion: String?,
        model: String?
    ): DeviceHeartbeatResult

    fun uploadEvents(
        deviceId: String,
        deviceToken: String,
        events: List<RawPaymentEvent>
    ): IngestEventsResult
}

object ApiProvider {
    fun create(config: ApiConfig = ApiConfig.current): MoynaPayApi =
        when (config.mode) {
            ApiMode.Fake -> FakeMoynaPayApi
            ApiMode.Real -> RealMoynaPayApi(config)
        }

    fun create(context: Context): MoynaPayApi =
        create(ApiConfigStore(context.applicationContext).load())
}
