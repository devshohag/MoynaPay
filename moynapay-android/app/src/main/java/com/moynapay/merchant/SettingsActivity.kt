package com.moynapay.merchant

import android.app.Activity
import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.CompoundButton
import android.widget.Toast

/** All values are mock data. In particular, no usable API secret exists in the app. */
class SettingsActivity : Activity() {
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

        // The displayed URL and masked key are examples; copying them would suggest usable credentials.
        findViewById<View>(R.id.apiAddress).setOnClickListener { previewOnly() }
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
    }

    private fun showPreview(title: String, message: String) {
        AlertDialog.Builder(this).setTitle(title).setMessage(message)
            .setPositiveButton(R.string.close, null).show()
    }

    private fun previewOnly() {
        Toast.makeText(this, R.string.action_preview_only, Toast.LENGTH_SHORT).show()
    }
}
