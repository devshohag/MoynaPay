package com.moynapay.merchant

import android.app.Activity
import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast

class OrderDetailActivity : Activity() {
    companion object {
        private const val EXTRA_REFERENCE = "order_reference"

        fun intent(context: Context, reference: String): Intent =
            Intent(context, OrderDetailActivity::class.java).putExtra(EXTRA_REFERENCE, reference)
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.white)
        window.navigationBarColor = getColor(R.color.white)
        window.decorView.systemUiVisibility = View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR
        val order = PreviewOrders.find(intent.getStringExtra(EXTRA_REFERENCE)) ?: run {
            finish()
            return
        }
        setContentView(R.layout.activity_order_detail)

        findViewById<View>(R.id.backButton).setOnClickListener { finish() }
        text(R.id.detailReference, "#${order.reference}")
        text(R.id.detailTime, order.time)
        text(R.id.customerName, order.customer)
        text(R.id.customerInitial, order.customer.take(2))
        text(R.id.customerPhone, order.phone)
        text(R.id.productSummary, order.summary)
        text(R.id.orderAmount, order.amount)
        text(R.id.chargedAmount, order.chargedAmount)
        text(R.id.paymentNote, order.paymentNote)
        findViewById<TextView>(R.id.detailStatus).apply {
            text = order.status.label
            setBackgroundResource(order.status.background)
            setTextColor(getColor(order.status.textColor))
        }

        val timeline = findViewById<LinearLayout>(R.id.timeline)
        order.events.forEach { event ->
            val row = LayoutInflater.from(this).inflate(R.layout.item_timeline, timeline, false)
            row.findViewById<TextView>(R.id.timelineTitle).text = event.title
            row.findViewById<TextView>(R.id.timelineDetail).text = event.detail
            row.findViewById<View>(R.id.timelineDot).setBackgroundResource(
                if (event.title.contains("অপেক্ষায়")) R.drawable.dot_waiting else R.drawable.dot_confirmed
            )
            if (event.recording) {
                row.findViewById<Button>(R.id.recordingButton).apply {
                    visibility = View.VISIBLE
                    setOnClickListener { previewOnly() }
                }
            }
            timeline.addView(row)
        }
        findViewById<View>(R.id.callButton).setOnClickListener { previewOnly() }
        findViewById<View>(R.id.cancelButton).setOnClickListener { previewOnly() }
        findViewById<View>(R.id.shipButton).setOnClickListener { previewOnly() }
        when (order.status) {
            PreviewOrders.Status.CONFIRMED -> Unit
            PreviewOrders.Status.CALLING -> findViewById<View>(R.id.shipButton).visibility = View.GONE
            PreviewOrders.Status.NEEDS_HUMAN ->
                findViewById<Button>(R.id.shipButton).text = getString(R.string.confirm_order)
            PreviewOrders.Status.SHIPPED, PreviewOrders.Status.CANCELLED ->
                findViewById<View>(R.id.actionFooter).visibility = View.GONE
        }
    }

    private fun text(id: Int, value: String) {
        findViewById<TextView>(id).text = value
    }

    private fun previewOnly() {
        Toast.makeText(this, R.string.action_preview_only, Toast.LENGTH_SHORT).show()
    }
}
