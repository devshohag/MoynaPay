package com.moynapay.merchant

import org.json.JSONObject
import org.json.JSONArray
import java.io.BufferedReader
import java.io.InputStreamReader
import java.io.OutputStreamWriter
import java.net.HttpURLConnection
import java.net.URLEncoder
import java.net.URL

class RealMoynaPayApi(private val config: ApiConfig) : MoynaPayApi {
    override fun requestOtp(msisdn: String): OtpRequestResult {
        val json = post("/app/v1/auth/otp", JSONObject().put("phone", msisdn))
        return OtpRequestResult(
            sent = json.optBoolean("sent"),
            devOtp = json.optStringOrNull("devOtp"),
            message = null
        )
    }

    override fun verifyOtp(msisdn: String, otp: String): LoginSession {
        val json = post(
            "/app/v1/auth/token",
            JSONObject()
                .put("phone", msisdn)
                .put("otp", otp)
        )

        return LoginSession(
            merchantId = "",
            merchantName = "",
            accessToken = json.getString("accessToken"),
            refreshToken = json.getString("refreshToken")
        )
    }

    override fun createPairingToken(accessToken: String): PairingTokenResult {
        val json = post("/app/v1/devices/pairing-token", JSONObject(), bearer = accessToken)
        return PairingTokenResult(
            token = json.getString("pairingToken"),
            expiresAt = json.optString("expiresAt")
        )
    }

    override fun pairDevice(
        pairingToken: String,
        fingerprint: String,
        name: String?,
        model: String?,
        appVersion: String?
    ): PairedDeviceSession {
        val json = post(
            "/app/v1/devices/pair",
            JSONObject()
                .put("pairingToken", pairingToken)
                .put("fingerprint", fingerprint)
                .put("name", name)
                .put("model", model)
                .put("appVersion", appVersion)
                .put("permissionState", "Healthy")
        )
        val device = json.getJSONObject("device")
        return PairedDeviceSession(
            deviceId = device.getString("id"),
            deviceToken = json.getString("deviceToken"),
            fingerprint = device.optString("fingerprint", fingerprint)
        )
    }

    override fun bootstrap(accessToken: String): BootstrapSummary {
        val json = get("/app/v1/bootstrap", bearer = accessToken)
        val merchant = json.optJSONObject("merchant")
        val services = json.optJSONObject("enabledServices")
        return BootstrapSummary(
            merchantName = merchant?.optString("name").orEmpty(),
            callsEnabled = services?.optBoolean("calls") ?: false,
            paymentsEnabled = services?.optBoolean("payments") ?: false,
            courierEnabled = services?.optBoolean("courier") ?: false,
            rawJson = json.toString()
        )
    }

    override fun homeNumbers(accessToken: String): HomeNumbers =
        HomeNumbers(get("/app/v1/home/numbers", bearer = accessToken).toString())

    override fun orders(accessToken: String, status: String?, take: Int): OrdersPage {
        val query = buildList {
            add("take=${take.coerceIn(1, 100)}")
            if (!status.isNullOrBlank()) {
                add("status=${status.urlEncode()}")
            }
        }.joinToString("&")
        val json = get("/app/v1/orders?$query", bearer = accessToken)
        val itemsJson = json.optJSONArray("items")
        val items = buildList {
            if (itemsJson != null) {
                for (index in 0 until itemsJson.length()) {
                    val item = itemsJson.getJSONObject(index)
                    add(
                        OrderSummaryDto(
                            reference = item.optString("reference"),
                            status = item.optString("status"),
                            customerName = item.optString("customerName"),
                            amount = item.opt("amount")?.toString().orEmpty()
                        )
                    )
                }
            }
        }

        return OrdersPage(
            items = items,
            nextBefore = json.optStringOrNull("nextBefore"),
            rawJson = json.toString()
        )
    }

    override fun heartbeat(
        deviceId: String,
        deviceToken: String,
        permissionState: String?,
        batteryPercent: Int?,
        networkType: String?,
        appVersion: String?,
        model: String?
    ): DeviceHeartbeatResult {
        val json = post(
            "/app/v1/devices/$deviceId/heartbeat",
            JSONObject()
                .put("permissionState", permissionState)
                .put("batteryPercent", batteryPercent)
                .put("networkType", networkType)
                .put("appVersion", appVersion)
                .put("model", model),
            deviceToken = deviceToken
        )

        return DeviceHeartbeatResult(
            deviceId = json.optString("id", deviceId),
            permissionState = json.optStringOrNull("permissionState"),
            batteryPercent = if (json.has("batteryPercent") && !json.isNull("batteryPercent")) {
                json.optInt("batteryPercent")
            } else {
                null
            },
            networkType = json.optStringOrNull("networkType"),
            rawJson = json.toString()
        )
    }

    override fun uploadEvents(
        deviceId: String,
        deviceToken: String,
        events: List<RawPaymentEvent>
    ): IngestEventsResult {
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

        val json = post(
            "/app/v1/devices/$deviceId/events",
            JSONObject().put("events", array),
            deviceToken = deviceToken
        )

        return IngestEventsResult(
            accepted = json.optInt("accepted"),
            duplicates = json.optInt("duplicates"),
            rejected = json.optInt("rejected"),
            rawJson = json.toString()
        )
    }

    private fun get(path: String, bearer: String? = null): JSONObject =
        request("GET", path, null, bearer, null)

    private fun post(
        path: String,
        body: JSONObject,
        bearer: String? = null,
        deviceToken: String? = null
    ): JSONObject =
        request("POST", path, body, bearer, deviceToken)

    private fun request(
        method: String,
        path: String,
        body: JSONObject?,
        bearer: String?,
        deviceToken: String?
    ): JSONObject {
        val connection = (URL(config.baseUrl.trimEnd('/') + path).openConnection()
            as HttpURLConnection)
        connection.requestMethod = method
        connection.connectTimeout = 15_000
        connection.readTimeout = 15_000
        connection.setRequestProperty("Accept", "application/json")
        if (bearer != null) {
            connection.setRequestProperty("Authorization", "Bearer $bearer")
        }
        if (deviceToken != null) {
            connection.setRequestProperty("X-MoynaPay-Device-Token", deviceToken)
        }
        if (body != null) {
            connection.doOutput = true
            connection.setRequestProperty("Content-Type", "application/json")
            OutputStreamWriter(connection.outputStream, Charsets.UTF_8).use {
                it.write(body.toString())
            }
        }

        val status = connection.responseCode
        val stream = if (status in 200..299) connection.inputStream else connection.errorStream
        val text = stream?.let {
            BufferedReader(InputStreamReader(it, Charsets.UTF_8)).use { reader ->
                reader.readText()
            }
        }.orEmpty()
        connection.disconnect()

        if (status !in 200..299) {
            throw IllegalStateException("MoynaPay API $status: ${problemTitle(text)}")
        }

        return if (text.isBlank()) JSONObject() else JSONObject(text)
    }

    private fun problemTitle(text: String): String {
        if (text.isBlank()) return "request failed"
        return runCatching { JSONObject(text).optString("title").ifBlank { text } }.getOrDefault(text)
    }

    private fun String.urlEncode(): String =
        URLEncoder.encode(this, "UTF-8")

    private fun JSONObject.optStringOrNull(name: String): String? =
        if (has(name) && !isNull(name)) optString(name) else null
}
