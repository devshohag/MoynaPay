package com.moynapay.merchant

import android.app.Activity
import android.os.Bundle
import android.view.View
import android.widget.EditText
import android.widget.Toast

/** Inputs can be edited locally for layout review; save never sends or stores them. */
class CallSettingsActivity : Activity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.white)
        window.navigationBarColor = getColor(R.color.surface)
        window.decorView.systemUiVisibility = View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR
        setContentView(R.layout.activity_call_settings)

        findViewById<View>(R.id.settingsBack).setOnClickListener { finish() }
        findViewById<View>(R.id.saveCallSettings).setOnClickListener { previewOnly() }
        findViewById<View>(R.id.hoursControl).setOnClickListener { previewOnly() }
        findViewById<View>(R.id.retryControl).setOnClickListener { previewOnly() }

        val script = findViewById<EditText>(R.id.callScript)
        mapOf(
            R.id.tokenShop to "{দোকান}",
            R.id.tokenName to "{নাম}",
            R.id.tokenAmount to "{টাকা}",
            R.id.tokenProduct to "{পণ্য}"
        ).forEach { (id, token) ->
            findViewById<View>(id).setOnClickListener {
                val cursor = script.selectionStart.coerceAtLeast(0)
                script.text.insert(cursor, token)
            }
        }
    }

    private fun previewOnly() {
        Toast.makeText(this, R.string.settings_preview_only, Toast.LENGTH_SHORT).show()
    }
}
