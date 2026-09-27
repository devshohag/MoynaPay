package com.moynapay.merchant

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.widget.LinearLayout
import android.widget.TextView

class OrdersActivity : Activity() {
    private enum class Filter { ALL, WAITING, CONFIRMED, CANCELLED }

    private lateinit var list: LinearLayout
    private var currentFilter = Filter.ALL

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.white)
        window.navigationBarColor = getColor(R.color.white)
        window.decorView.systemUiVisibility = View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR
        setContentView(R.layout.activity_orders)

        list = findViewById(R.id.orderList)
        val chips = mapOf(
            Filter.ALL to R.id.filterAll,
            Filter.WAITING to R.id.filterWaiting,
            Filter.CONFIRMED to R.id.filterConfirmed,
            Filter.CANCELLED to R.id.filterCancelled
        )
        chips.forEach { (filter, id) ->
            findViewById<TextView>(id).setOnClickListener { showFilter(filter, chips) }
        }
        findViewById<View>(R.id.navHome).setOnClickListener {
            startActivity(Intent(this, HomeActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP))
            finish()
        }
        findViewById<View>(R.id.navCalls).setOnClickListener {
            startActivity(Intent(this, HumanQueueActivity::class.java))
        }
        findViewById<View>(R.id.navPayments).setOnClickListener {
            startActivity(Intent(this, PaymentsActivity::class.java))
        }
        findViewById<View>(R.id.navSettings).setOnClickListener {
            startActivity(Intent(this, SettingsActivity::class.java))
        }
        showFilter(currentFilter, chips)
    }

    private fun showFilter(filter: Filter, chips: Map<Filter, Int>) {
        currentFilter = filter
        chips.forEach { (item, id) ->
            findViewById<TextView>(id).apply {
                setBackgroundResource(if (item == filter) R.drawable.chip_selected else R.drawable.chip_outline)
                setTextColor(getColor(if (item == filter) R.color.white else R.color.ink))
                isSelected = item == filter
            }
        }

        list.removeAllViews()
        val visible = PreviewOrders.items.filter { order ->
            when (filter) {
                Filter.ALL -> true
                Filter.WAITING -> order.status == PreviewOrders.Status.CALLING ||
                    order.status == PreviewOrders.Status.NEEDS_HUMAN
                Filter.CONFIRMED -> order.status == PreviewOrders.Status.CONFIRMED ||
                    order.status == PreviewOrders.Status.SHIPPED
                Filter.CANCELLED -> order.status == PreviewOrders.Status.CANCELLED
            }
        }
        visible.forEach { order ->
            val row = LayoutInflater.from(this).inflate(R.layout.item_order, list, false)
            row.findViewById<TextView>(R.id.orderName).text = order.customer
            row.findViewById<TextView>(R.id.orderMeta).text = "#${order.reference} · ${order.phone}"
            row.findViewById<TextView>(R.id.orderAmount).text = order.amount
            row.findViewById<TextView>(R.id.orderLastEvent).text = order.lastEvent
            row.findViewById<TextView>(R.id.orderStatus).apply {
                text = order.status.label
                setBackgroundResource(order.status.background)
                setTextColor(getColor(order.status.textColor))
            }
            row.setOnClickListener { startActivity(OrderDetailActivity.intent(this, order.reference)) }
            list.addView(row)
        }
    }
}
