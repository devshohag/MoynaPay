package com.moynapay.merchant

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast

/** Static payment and paired-device preview; no capture service is connected here. */
class PaymentsActivity : Activity() {
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
        findViewById<View>(R.id.pairedDevice).setOnClickListener { previewOnly() }

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
    }

    private fun previewOnly() {
        Toast.makeText(this, R.string.action_preview_only, Toast.LENGTH_SHORT).show()
    }
}
