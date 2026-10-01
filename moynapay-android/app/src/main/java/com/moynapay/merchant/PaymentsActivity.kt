package com.moynapay.merchant

import android.app.Activity
import android.Manifest
import android.content.ComponentName
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import android.provider.Settings
import android.view.LayoutInflater
import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast

class PaymentsActivity : Activity() {
    private val api: MoynaPayApi by lazy { ApiProvider.create(this) }
    private val sessions: SessionStore by lazy { SessionStore(this) }

    private data class Transaction(
        val amount: String,
        val detail: String,
        val time: String,
        val reference: String,
        val received: Boolean
    )

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.white)
        window.navigationBarColor = getColor(R.color.white)
        window.decorView.systemUiVisibility = View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR
        setContentView(R.layout.activity_payments)

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
        findViewById<View>(R.id.navSettings).setOnClickListener {
            startActivity(Intent(this, SettingsActivity::class.java))
        }
        findViewById<View>(R.id.pairedDevice).setOnClickListener { pairThisPhoneFake() }
        findViewById<Button>(R.id.pairDeviceButton).setOnClickListener { pairThisPhoneFake() }
        findViewById<Button>(R.id.smsPermissionButton).setOnClickListener { requestSmsPermission() }
        findViewById<Button>(R.id.notificationPermissionButton).setOnClickListener {
            startActivity(Intent(Settings.ACTION_NOTIFICATION_LISTENER_SETTINGS))
        }
        findViewById<Button>(R.id.flushPaymentsButton).setOnClickListener { flushQueuedEvents() }

        val transactions = listOf(
            Transaction("৳ ১,২৫১", "#ORD-1041 · CJ8A7K2LM9", "১:৪৫", "ORD-1041", true),
            Transaction("৳ ৫,৯৯২", "#ORD-1038 · DH6EYO3HO1", "১২:৩০", "ORD-1038", true),
            Transaction("৳ ২,৯৯১", "#ORD-1040 · অপেক্ষায়", "১:৩৮", "ORD-1040", false)
        )
        val container = findViewById<LinearLayout>(R.id.transactionList)
        transactions.forEach { transaction ->
            val row = LayoutInflater.from(this).inflate(R.layout.item_transaction, container, false)
            row.findViewById<TextView>(R.id.transactionAmount).text = transaction.amount
            row.findViewById<TextView>(R.id.transactionDetail).text = transaction.detail
            row.findViewById<TextView>(R.id.transactionTime).text = transaction.time
            row.findViewById<TextView>(R.id.transactionIcon).apply {
                text = if (transaction.received) "✓" else "◷"
                setBackgroundResource(
                    if (transaction.received) R.drawable.status_confirmed else R.drawable.status_calling
                )
                setTextColor(getColor(if (transaction.received) R.color.green else R.color.amber))
            }
            row.setOnClickListener {
                startActivity(OrderDetailActivity.intent(this, transaction.reference))
            }
            container.addView(row)
        }

        renderListenerStatus()
    }

    override fun onResume() {
        super.onResume()
        renderListenerStatus()
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == SmsPermissionRequest) {
            renderListenerStatus()
        }
    }

    private fun pairThisPhoneFake() {
        val accessToken = sessions.accessToken()
        if (accessToken.isNullOrBlank()) {
            Toast.makeText(this, "Login first, then pair this phone.", Toast.LENGTH_SHORT).show()
            startActivity(Intent(this, LoginActivity::class.java))
            return
        }

        Thread {
            try {
                val pairing = api.createPairingToken(accessToken)
                val paired = api.pairDevice(
                    pairingToken = pairing.token,
                    fingerprint = "android-${android.os.Build.FINGERPRINT.hashCode()}",
                    name = "This phone",
                    model = android.os.Build.MODEL,
                    appVersion = "0.1.0"
                )
                sessions.saveDevice(paired)

                runOnUiThread {
                    renderListenerStatus()
                    Toast.makeText(this, "Payment listener paired on this phone.", Toast.LENGTH_LONG).show()
                }
            } catch (error: Throwable) {
                runOnUiThread {
                    Toast.makeText(this, error.message ?: "Pairing failed", Toast.LENGTH_LONG).show()
                }
            }
        }.start()
    }

    private fun flushQueuedEvents() {
        Thread {
            PaymentListenerUploader.flush(this)
            runOnUiThread {
                renderListenerStatus()
                Toast.makeText(this, "Payment listener checked for queued events.", Toast.LENGTH_SHORT).show()
            }
        }.start()
    }

    private fun requestSmsPermission() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M &&
            checkSelfPermission(Manifest.permission.RECEIVE_SMS) != PackageManager.PERMISSION_GRANTED
        ) {
            requestPermissions(
                arrayOf(Manifest.permission.RECEIVE_SMS),
                SmsPermissionRequest
            )
            return
        }

        renderListenerStatus()
        Toast.makeText(this, "SMS permission is already enabled.", Toast.LENGTH_SHORT).show()
    }

    private fun renderListenerStatus() {
        findViewById<TextView>(R.id.deviceStatus).text =
            if (sessions.hasDevice()) "Paired on this phone" else "Tap Pair phone before real listener test"
        findViewById<TextView>(R.id.smsStatus).text =
            if (hasSmsPermission()) "SMS access enabled" else "SMS access needed"
        findViewById<TextView>(R.id.notificationStatus).text =
            if (hasNotificationAccess()) "Notification access enabled" else "Notification access needed"
    }

    private fun hasSmsPermission(): Boolean =
        Build.VERSION.SDK_INT < Build.VERSION_CODES.M ||
            checkSelfPermission(Manifest.permission.RECEIVE_SMS) == PackageManager.PERMISSION_GRANTED

    private fun hasNotificationAccess(): Boolean {
        val enabled = Settings.Secure.getString(contentResolver, "enabled_notification_listeners")
            ?: return false
        val component = ComponentName(this, BkashNotificationListenerService::class.java)
        return enabled.split(":").any { ComponentName.unflattenFromString(it) == component }
    }

    private companion object {
        const val SmsPermissionRequest = 7001
    }
}
