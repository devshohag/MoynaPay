package com.moynapay.merchant

import android.content.Context

class SessionStore(context: Context) {
    private val prefs = context.getSharedPreferences("moynapay_session", Context.MODE_PRIVATE)

    fun saveLogin(session: LoginSession) {
        prefs.edit()
            .putString("merchantId", session.merchantId)
            .putString("merchantName", session.merchantName)
            .putString("accessToken", session.accessToken)
            .putString("refreshToken", session.refreshToken)
            .apply()
    }

    fun accessToken(): String? = prefs.getString("accessToken", null)

    fun merchantName(): String? = prefs.getString("merchantName", null)

    fun saveDevice(session: PairedDeviceSession) {
        prefs.edit()
            .putString("deviceId", session.deviceId)
            .putString("deviceToken", session.deviceToken)
            .putString("deviceFingerprint", session.fingerprint)
            .apply()
    }

    fun hasDevice(): Boolean =
        prefs.getString("deviceId", null) != null &&
            prefs.getString("deviceToken", null) != null

    fun deviceId(): String? = prefs.getString("deviceId", null)

    fun deviceToken(): String? = prefs.getString("deviceToken", null)

    fun clear() {
        prefs.edit().clear().apply()
    }
}
