package com.moynapay.merchant

import android.content.Context
import org.json.JSONArray
import org.json.JSONObject

class PaymentEventQueue(context: Context) {
    private val prefs = context.getSharedPreferences("moynapay_payment_events", Context.MODE_PRIVATE)

    fun enqueue(event: RawPaymentEvent) {
        val items = read().toMutableList()
        items.add(event)
        write(items.takeLast(MaxQueuedEvents))
    }

    fun flush(api: MoynaPayApi = ApiProvider.create(), sessions: SessionStore): IngestEventsResult? {
        val deviceId = sessions.deviceId() ?: return null
        val deviceToken = sessions.deviceToken() ?: return null
        val events = read()
        if (events.isEmpty()) return null

        val result = api.uploadEvents(deviceId, deviceToken, events)
        val remaining = events.drop((result.accepted + result.duplicates + result.rejected).coerceAtLeast(0))
        write(remaining)
        return result
    }

    private fun read(): List<RawPaymentEvent> {
        val raw = prefs.getString("events", "[]") ?: "[]"
        val array = runCatching { JSONArray(raw) }.getOrElse { JSONArray() }
        return buildList {
            for (index in 0 until array.length()) {
                val item = array.getJSONObject(index)
                add(
                    RawPaymentEvent(
                        source = item.getString("source"),
                        senderId = item.getString("senderId"),
                        body = item.getString("body"),
                        receivedAt = item.getString("receivedAt")
                    )
                )
            }
        }
    }

    private fun write(events: List<RawPaymentEvent>) {
        val array = JSONArray()
        events.forEach { event ->
            array.put(
                JSONObject()
                    .put("source", event.source)
                    .put("senderId", event.senderId)
                    .put("body", event.body)
                    .put("receivedAt", event.receivedAt)
            )
        }
        prefs.edit().putString("events", array.toString()).apply()
    }

    private companion object {
        const val MaxQueuedEvents = 200
    }
}
