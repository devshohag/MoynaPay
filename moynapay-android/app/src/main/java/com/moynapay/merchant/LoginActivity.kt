package com.moynapay.merchant

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.Toast

class LoginActivity : Activity() {
    private val api: MoynaPayApi by lazy { ApiProvider.create(this) }
    private val sessions: SessionStore by lazy { SessionStore(this) }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = getColor(R.color.green)
        window.navigationBarColor = getColor(R.color.surface)
        window.decorView.systemUiVisibility = 0
        setContentView(R.layout.activity_login)

        findViewById<Button>(R.id.previewButton).setOnClickListener {
            fakeLogin()
        }
    }

    private fun fakeLogin() {
        val phone = findViewById<EditText>(R.id.mobileNumber).text.toString()
            .ifBlank { "01711111111" }
        val otp = findViewById<EditText>(R.id.otpCode).text.toString()
            .ifBlank { "1234" }
        val button = findViewById<Button>(R.id.previewButton)

        button.isEnabled = false
        button.text = "Connecting..."

        Thread {
            try {
                api.requestOtp(phone)
                val session = api.verifyOtp(phone, otp)
                sessions.saveLogin(session)

                runOnUiThread {
                    Toast.makeText(this, "Login ready", Toast.LENGTH_SHORT).show()
                    startActivity(Intent(this, HomeActivity::class.java))
                    finish()
                }
            } catch (error: Throwable) {
                runOnUiThread {
                    button.isEnabled = true
                    button.text = getString(R.string.preview_home)
                    Toast.makeText(this, error.message ?: "Login failed", Toast.LENGTH_LONG).show()
                }
            }
        }.start()
    }
}
