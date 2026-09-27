package com.moynapay.merchant

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast

/** needs_human is a waiting order state, not a cancellation. */
class HumanQueueActivity : Activity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.white)
        window.navigationBarColor = getColor(R.color.white)
        window.decorView.systemUiVisibility = View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR
        setContentView(R.layout.activity_human_queue)

        findViewById<View>(R.id.queueBack).setOnClickListener { finish() }
        findViewById<View>(R.id.callSettingsButton).setOnClickListener {
            startActivity(Intent(this, CallSettingsActivity::class.java))
        }
        findViewById<View>(R.id.navHome).setOnClickListener {
            startActivity(Intent(this, HomeActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP))
            finish()
        }
        findViewById<View>(R.id.navOrders).setOnClickListener {
            startActivity(Intent(this, OrdersActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP))
            finish()
        }
        findViewById<View>(R.id.navPayments).setOnClickListener {
            startActivity(Intent(this, PaymentsActivity::class.java))
        }
        findViewById<View>(R.id.navSettings).setOnClickListener {
            startActivity(Intent(this, SettingsActivity::class.java))
        }

        val container = findViewById<LinearLayout>(R.id.humanOrders)
        PreviewOrders.items.filter { it.status == PreviewOrders.Status.NEEDS_HUMAN }.forEach { order ->
            val row = LayoutInflater.from(this).inflate(R.layout.item_human_order, container, false)
            row.findViewById<TextView>(R.id.humanName).text = order.customer
            row.findViewById<TextView>(R.id.humanRefAmount).text = "#${order.reference} · ${order.amount}"
            row.findViewById<TextView>(R.id.humanReason).text = order.humanReason
            row.findViewById<TextView>(R.id.humanQuote).apply {
                if (order.lastHeard == null) visibility = View.GONE
                else text = "“${order.lastHeard}”"
            }
            row.findViewById<Button>(R.id.humanCall).setOnClickListener { previewOnly() }
            row.findViewById<Button>(R.id.humanDecision).apply {
                val cancel = order.reference == "ORD-1043"
                text = getString(if (cancel) R.string.cancel_order else R.string.confirm_order)
                setTextColor(getColor(if (cancel) R.color.red else R.color.ink))
                setOnClickListener { previewOnly() }
            }
            row.setOnClickListener { startActivity(OrderDetailActivity.intent(this, order.reference)) }
            container.addView(row)
        }
    }

    private fun previewOnly() {
        Toast.makeText(this, R.string.action_preview_only, Toast.LENGTH_SHORT).show()
    }
}
