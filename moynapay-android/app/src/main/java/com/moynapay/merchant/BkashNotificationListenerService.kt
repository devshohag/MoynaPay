package com.moynapay.merchant

import android.service.notification.NotificationListenerService
import android.service.notification.StatusBarNotification

class BkashNotificationListenerService : NotificationListenerService() {
    override fun onNotificationPosted(sbn: StatusBarNotification) {
        val extras = sbn.notification.extras
        val title = extras.getCharSequence("android.title")?.toString()
        val text = extras.getCharSequence("android.text")?.toString()
        val bigText = extras.getCharSequence("android.bigText")?.toString()
        val body = listOfNotNull(text, bigText)
            .distinct()
            .joinToString("\n")
            .ifBlank { null }

        val event = BkashEventExtractor.fromNotification(
            title = title ?: sbn.packageName,
            text = body,
            receivedAtMillis = sbn.postTime
        ) ?: return

        PaymentListenerUploader.enqueueAndFlush(this, event)
    }
}
