package com.moynapay.merchant

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.provider.Telephony

class BkashSmsReceiver : BroadcastReceiver() {
    override fun onReceive(context: Context, intent: Intent) {
        if (intent.action != Telephony.Sms.Intents.SMS_RECEIVED_ACTION) return

        val messages = Telephony.Sms.Intents.getMessagesFromIntent(intent)
        messages.forEach { sms ->
            val event = BkashEventExtractor.fromSms(
                sender = sms.displayOriginatingAddress ?: sms.originatingAddress,
                body = sms.messageBody,
                receivedAtMillis = sms.timestampMillis
            ) ?: return@forEach

            PaymentListenerUploader.enqueueAndFlush(context, event)
        }
    }
}
