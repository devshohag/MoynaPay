package com.moynapay.merchant

import android.app.Activity
import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.CompoundButton
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.Switch
import android.widget.TextView
import android.widget.Toast

class SettingsActivity : Activity() {
    private val configStore: ApiConfigStore by lazy { ApiConfigStore(this) }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.white)
        window.navigationBarColor = getColor(R.color.white)
        window.decorView.systemUiVisibility = View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR
        setContentView(R.layout.activity_settings)

        findViewById<View>(R.id.navHome).setOnClickListener {
            startActivity(Intent(this, HomeActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP))
            finish()
        }
        findViewById<View>(R.id.navOrders).setOnClickListener {
            startActivity(Intent(this, OrdersActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP))
            finish()
        }
        findViewById<View>(R.id.navCalls).setOnClickListener {
            startActivity(Intent(this, HumanQueueActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP))
            finish()
        }
        findViewById<View>(R.id.navPayments).setOnClickListener {
            startActivity(Intent(this, PaymentsActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP))
            finish()
        }

        findViewById<View>(R.id.apiAddress).setOnClickListener { showApiConfigDialog() }
        findViewById<View>(R.id.apiKey).setOnClickListener { previewOnly() }
        findViewById<View>(R.id.callServiceRow).setOnClickListener {
            startActivity(Intent(this, CallSettingsActivity::class.java))
        }
        findViewById<View>(R.id.paymentServiceRow).setOnClickListener { previewOnly() }
        findViewById<View>(R.id.courierServiceRow).setOnClickListener { previewOnly() }
        for (id in listOf(R.id.callServiceSwitch, R.id.paymentServiceSwitch, R.id.courierServiceSwitch)) {
            findViewById<CompoundButton>(id).setOnClickListener { button ->
                val toggle = button as CompoundButton
                toggle.isChecked = !toggle.isChecked
                previewOnly()
            }
        }
        findViewById<View>(R.id.webhookRow).setOnClickListener {
            showPreview(
                getString(R.string.webhook_title),
                "ডেমো webhook: https://shop.example.com/moynapay\n\nএটি সংরক্ষিত ঠিকানা নয়। API যুক্ত হলে এখানে HTTPS ঠিকানা ও test result দেখা যাবে।"
            )
        }
        findViewById<View>(R.id.profileRow).setOnClickListener {
            showPreview(
                getString(R.string.shop_profile),
                "রাফি ফ্যাশন\nযোগাযোগ: ০১৭১১ ২২৩৩৪৪\n\nনমুনা দোকানের তথ্য; এখানে কোনো পরিবর্তন সংরক্ষিত হয় না।"
            )
        }
        findViewById<View>(R.id.logoutRow).setOnClickListener { previewOnly() }

        renderApiConfig()
    }

    private fun showPreview(title: String, message: String) {
        AlertDialog.Builder(this).setTitle(title).setMessage(message)
            .setPositiveButton(R.string.close, null).show()
    }

    private fun previewOnly() {
        Toast.makeText(this, R.string.action_preview_only, Toast.LENGTH_SHORT).show()
    }

    private fun renderApiConfig() {
        val config = configStore.load()
        findViewById<TextView>(R.id.apiModeValue).text =
            if (config.mode == ApiMode.Real) "Real API" else "Fake API"
        findViewById<TextView>(R.id.apiAddressValue).text = config.baseUrl
    }

    private fun showApiConfigDialog() {
        val current = configStore.load()
        val form = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(48, 20, 48, 0)
        }
        val modeSwitch = Switch(this).apply {
            text = "Use real API"
            isChecked = current.mode == ApiMode.Real
        }
        val baseUrlInput = EditText(this).apply {
            hint = "https://your-tunnel.example.com"
            setSingleLine(true)
            setText(current.baseUrl)
        }

        form.addView(modeSwitch)
        form.addView(baseUrlInput)

        AlertDialog.Builder(this)
            .setTitle("API connection")
            .setView(form)
            .setPositiveButton("Save") { _, _ ->
                val mode = if (modeSwitch.isChecked) ApiMode.Real else ApiMode.Fake
                val baseUrl = baseUrlInput.text.toString().trim()
                    .ifBlank { ApiConfig.current.baseUrl }
                    .trimEnd('/')
                configStore.save(ApiConfig(mode = mode, baseUrl = baseUrl))
                SessionStore(this).clear()
                renderApiConfig()
                Toast.makeText(this, "API config saved. Please login again.", Toast.LENGTH_LONG).show()
            }
            .setNeutralButton("Reset") { _, _ ->
                configStore.reset()
                SessionStore(this).clear()
                renderApiConfig()
                Toast.makeText(this, "API config reset.", Toast.LENGTH_SHORT).show()
            }
            .setNegativeButton(R.string.close, null)
            .show()
    }
}
