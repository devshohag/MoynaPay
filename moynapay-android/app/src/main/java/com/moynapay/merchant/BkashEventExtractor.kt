package com.moynapay.merchant

import java.time.Instant

object BkashEventExtractor {
    fun fromNotification(title: String?, text: String?, receivedAtMillis: Long): RawPaymentEvent? {
        val sender = title?.trim().orEmpty()
        val body = text?.trim().orEmpty()
        if (!looksLikeBkash(sender, body)) return null

        return RawPaymentEvent(
            source = "Notification",
            senderId = sender.ifBlank { "bKash" },
            body = body,
            receivedAt = Instant.ofEpochMilli(receivedAtMillis).toString()
        )
    }

    fun fromSms(sender: String?, body: String?, receivedAtMillis: Long): RawPaymentEvent? {
        val cleanSender = sender?.trim().orEmpty()
        val cleanBody = body?.trim().orEmpty()
        if (!looksLikeBkash(cleanSender, cleanBody)) return null

        return RawPaymentEvent(
            source = "Sms",
            senderId = cleanSender.ifBlank { "bKash" },
            body = cleanBody,
            receivedAt = Instant.ofEpochMilli(receivedAtMillis).toString()
        )
    }

    private fun looksLikeBkash(sender: String, body: String): Boolean {
        if (body.isBlank()) return false

        val senderMatch = sender.equals("bKash", ignoreCase = true) ||
            sender == "16247" ||
            sender.contains("bkash", ignoreCase = true)
        val bodyMatch = body.contains("TrxID", ignoreCase = true) &&
            body.contains("Tk", ignoreCase = true)

        return senderMatch || bodyMatch
    }
}
