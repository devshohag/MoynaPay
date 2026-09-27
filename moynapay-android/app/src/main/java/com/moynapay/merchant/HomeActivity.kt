package com.moynapay.merchant

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.view.View

/** Static preview content; API-backed data and navigation arrive in later phases. */
class HomeActivity : Activity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.green)
        window.navigationBarColor = getColor(R.color.white)
        setContentView(R.layout.activity_home)

        val openOrders = View.OnClickListener {
            startActivity(Intent(this, OrdersActivity::class.java))
        }
        findViewById<View>(R.id.navOrders).setOnClickListener(openOrders)
        findViewById<View>(R.id.ordersSeeAll).setOnClickListener(openOrders)
        val openCalls = View.OnClickListener {
            startActivity(Intent(this, HumanQueueActivity::class.java))
        }
        findViewById<View>(R.id.navCalls).setOnClickListener(openCalls)
        findViewById<View>(R.id.humanQueueCallout).setOnClickListener(openCalls)
        findViewById<View>(R.id.navPayments).setOnClickListener {
            startActivity(Intent(this, PaymentsActivity::class.java))
        }
        findViewById<View>(R.id.navSettings).setOnClickListener {
            startActivity(Intent(this, SettingsActivity::class.java))
        }
        mapOf(
            R.id.homeOrderSadia to "ORD-1041",
            R.id.homeOrderRafiqul to "ORD-1040",
            R.id.homeOrderNusrat to "ORD-1039"
        ).forEach { (viewId, reference) ->
            findViewById<View>(viewId).setOnClickListener {
                startActivity(OrderDetailActivity.intent(this, reference))
            }
        }
    }
}
