package com.moynapay.merchant

import android.content.Context
import android.os.BatteryManager

object PaymentListenerUploader {
    fun enqueueAndFlush(context: Context, event: RawPaymentEvent) {
        val appContext = context.applicationContext
        Thread {
            val queue = PaymentEventQueue(appContext)
            queue.enqueue(event)
            flush(appContext)
        }.start()
    }

    fun flush(context: Context) {
        val appContext = context.applicationContext
        val sessions = SessionStore(appContext)
        val deviceId = sessions.deviceId() ?: return
        val deviceToken = sessions.deviceToken() ?: return

        runCatching {
            val api = ApiProvider.create(appContext)
            val queue = PaymentEventQueue(appContext)
            queue.flush(api, sessions)
            api.heartbeat(
                deviceId = deviceId,
                deviceToken = deviceToken,
                permissionState = "Healthy",
                batteryPercent = batteryPercent(appContext),
                networkType = null,
                appVersion = "0.1.0",
                model = android.os.Build.MODEL
            )
        }
    }

    private fun batteryPercent(context: Context): Int? {
        val manager = context.getSystemService(Context.BATTERY_SERVICE) as? BatteryManager
        val value = manager?.getIntProperty(BatteryManager.BATTERY_PROPERTY_CAPACITY) ?: return null
        return value.takeIf { it in 0..100 }
    }
}
