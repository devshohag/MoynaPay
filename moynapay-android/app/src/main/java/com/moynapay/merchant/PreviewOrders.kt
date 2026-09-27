package com.moynapay.merchant

/** Screen data from the design brief. Nothing here is fetched or persisted. */
internal object PreviewOrders {
    enum class Status(val label: String, val background: Int, val textColor: Int) {
        CONFIRMED("নিশ্চিত", R.drawable.status_confirmed, R.color.green),
        CALLING("কল চলছে", R.drawable.status_calling, R.color.amber),
        SHIPPED("পাঠানো হয়েছে", R.drawable.status_confirmed, R.color.green),
        CANCELLED("বাতিল", R.drawable.status_cancelled, R.color.red),
        NEEDS_HUMAN("মানুষ লাগবে", R.drawable.status_calling, R.color.amber)
    }

    data class Event(val title: String, val detail: String, val recording: Boolean = false)

    data class Order(
        val reference: String,
        val customer: String,
        val phone: String,
        val amount: String,
        val lastEvent: String,
        val status: Status,
        val time: String,
        val summary: String,
        val chargedAmount: String,
        val paymentNote: String,
        val events: List<Event>,
        val humanReason: String? = null,
        val lastHeard: String? = null
    )

    val items = listOf(
        Order(
            "ORD-1041", "সাদিয়া আক্তার", "০১৭১১ ২২৩৩৪৪", "৳ ১,২৫০",
            "১ চাপা হয়েছে · ২ মিনিট আগে", Status.CONFIRMED, "আজ, দুপুর ১:৪০",
            "দুইটি শার্ট · মাঝারি", "৳ ১,২৫১", "টাকা বাকি",
            listOf(
                Event("অর্ডার এসেছে", "WooCommerce থেকে · ১:৪০"),
                Event("কল দেওয়া হয়েছে", "১ম চেষ্টা · ধরেছেন · ১:৪১"),
                Event("১ চেপেছেন — নিশ্চিত", "৪২ সেকেন্ড · ১:৪২", recording = true),
                Event("টাকার অপেক্ষায়", "০১৭১১১১১১১১ নম্বরে ৳ ১,২৫১")
            )
        ),
        Order(
            "ORD-1040", "রফিকুল ইসলাম", "০১৮১২ ৩৩৪৪৫৫", "৳ ২,৯৯০",
            "২য় চেষ্টা · এখনই", Status.CALLING, "আজ, দুপুর ১:৩৮",
            "শার্ট · ২টি", "৳ ২,৯৯১", "কল চলছে",
            listOf(
                Event("অর্ডার এসেছে", "WooCommerce থেকে · ১:৩৮"),
                Event("কল দেওয়া হয়েছে", "২য় চেষ্টা · এখনই"),
                Event("উত্তরের অপেক্ষায়", "সিদ্ধান্ত এখনো আসেনি")
            )
        ),
        Order(
            "ORD-1038", "তানভীর হাসান", "০১৯২১ ১১২২৩৩", "৳ ৫,৯৯০",
            "Steadfast · SF8821904", Status.SHIPPED, "আজ, দুপুর ১২:৩০",
            "পোশাক · ৩টি", "৳ ৫,৯৯২", "টাকা পাওয়া গেছে",
            listOf(
                Event("অর্ডার এসেছে", "WooCommerce থেকে · ১২:১০"),
                Event("১ চেপেছেন — নিশ্চিত", "কল শেষ · ১২:১৬", recording = true),
                Event("টাকা পাওয়া গেছে", "লেনদেন মেলানো হয়েছে · ১২:৩০"),
                Event("কুরিয়ারে দেওয়া হয়েছে", "Steadfast · SF8821904")
            )
        ),
        Order(
            "ORD-1039", "নুসরাত জাহান", "০১৬৩৩ ৭৭৮৮৯৯", "৳ ৮৯০",
            "০ চাপা হয়েছে · ১ ঘণ্টা আগে", Status.CANCELLED, "আজ, দুপুর ১২:৪০",
            "পোশাক · ১টি", "৳ ৮৯০", "গ্রাহক বাতিল করেছেন",
            listOf(
                Event("অর্ডার এসেছে", "WooCommerce থেকে · ১২:৪০"),
                Event("কল দেওয়া হয়েছে", "১ম চেষ্টা · ধরেছেন"),
                Event("০ চেপেছেন — বাতিল", "গ্রাহক বাতিল করেছেন · ১২:৪২", recording = true)
            )
        ),
        Order(
            "ORD-1044", "আরিফ মাহমুদ", "০১৭১২ ৩৩৪৪৫৫", "৳ ৩,৪৫০",
            "ঠিকানা বদলাতে বলেছেন", Status.NEEDS_HUMAN, "আজ, দুপুর ২:০৫",
            "পোশাক · ২টি", "৳ ৩,৪৫১", "মানুষের সিদ্ধান্তের অপেক্ষায়",
            listOf(
                Event("অর্ডার এসেছে", "WooCommerce থেকে · ২:০৫"),
                Event("কথা বলেছেন", "ঠিকানা পরিবর্তনের অনুরোধ · ২:০৬", recording = true),
                Event("মানুষের সিদ্ধান্তের অপেক্ষায়", "অর্ডার বাতিল হয়নি")
            ),
            humanReason = "কথা বলেছেন", lastHeard = "নিব ভাই, কিন্তু ঠিকানাটা একটু বদলাতে হবে"
        ),
        Order(
            "ORD-1043", "শারমিন সুলতানা", "০১৮১৩ ৪৪৫৫৬৬", "৳ ১,৪৯০",
            "৩ বার ধরেননি", Status.NEEDS_HUMAN, "আজ, দুপুর ১:৫৫",
            "পোশাক · ১টি", "৳ ১,৪৯১", "মানুষের সিদ্ধান্তের অপেক্ষায়",
            listOf(
                Event("অর্ডার এসেছে", "WooCommerce থেকে · ১:৫৫"),
                Event("কল দেওয়া হয়েছে", "৩ বার ধরেননি"),
                Event("মানুষের সিদ্ধান্তের অপেক্ষায়", "অর্ডার বাতিল হয়নি")
            ),
            humanReason = "৩ বার ধরেননি"
        ),
        Order(
            "ORD-1042", "জাহিদ হোসেন", "০১৯২২ ৫৫৬৬৭৭", "৳ ৭৫০",
            "৯ চেপেছেন", Status.NEEDS_HUMAN, "আজ, দুপুর ১:৫০",
            "পোশাক · ১টি", "৳ ৭৫১", "মানুষের সিদ্ধান্তের অপেক্ষায়",
            listOf(
                Event("অর্ডার এসেছে", "WooCommerce থেকে · ১:৫০"),
                Event("৯ চেপেছেন", "মানুষের সাথে কথা বলতে চান", recording = true),
                Event("মানুষের সিদ্ধান্তের অপেক্ষায়", "অর্ডার বাতিল হয়নি")
            ),
            humanReason = "৯ চেপেছেন"
        )
    )

    fun find(reference: String?): Order? = items.firstOrNull { it.reference == reference }
}
