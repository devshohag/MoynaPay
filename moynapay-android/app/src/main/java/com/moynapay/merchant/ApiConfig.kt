package com.moynapay.merchant

import android.content.Context

enum class ApiMode {
    Fake,
    Real
}

data class ApiConfig(
    val mode: ApiMode,
    val baseUrl: String
) {
    companion object {
        val current = ApiConfig(
            mode = ApiMode.Fake,
            baseUrl = "http://10.0.2.2:5000"
        )
    }
}

class ApiConfigStore(context: Context) {
    private val prefs = context.getSharedPreferences("moynapay_api_config", Context.MODE_PRIVATE)

    fun load(): ApiConfig {
        val mode = runCatching {
            ApiMode.valueOf(prefs.getString("mode", ApiConfig.current.mode.name) ?: ApiConfig.current.mode.name)
        }.getOrDefault(ApiConfig.current.mode)
        val baseUrl = prefs.getString("baseUrl", ApiConfig.current.baseUrl)
            ?.trim()
            ?.ifBlank { ApiConfig.current.baseUrl }
            ?: ApiConfig.current.baseUrl

        return ApiConfig(mode = mode, baseUrl = baseUrl)
    }

    fun save(config: ApiConfig) {
        prefs.edit()
            .putString("mode", config.mode.name)
            .putString("baseUrl", config.baseUrl.trim().trimEnd('/'))
            .apply()
    }

    fun reset() {
        prefs.edit().clear().apply()
    }
}
