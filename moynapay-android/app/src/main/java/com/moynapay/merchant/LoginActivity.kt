package com.moynapay.merchant

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.view.WindowManager
import android.widget.Button

/** Phase 1 is a design preview. No OTP is sent or verified. */
class LoginActivity : Activity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.green)
        window.navigationBarColor = getColor(R.color.surface)
        window.decorView.systemUiVisibility = 0
        setContentView(R.layout.activity_login)

        findViewById<Button>(R.id.previewButton).setOnClickListener {
            startActivity(Intent(this, HomeActivity::class.java))
        }
    }
}
