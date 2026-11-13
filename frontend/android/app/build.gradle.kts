plugins {
    alias(libs.plugins.android.application)
}

android {
    // D01-L: gói gốc cố định từ đây; đổi sau này kéo theo đổi mọi gói con (FA 5.2).
    namespace = "com.nhom13.quanlymuontra"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.nhom13.quanlymuontra"
        // ĐX-FA-13: minSdk 26 để dùng java.time và Android Keystore AES-GCM mà không cần desugaring.
        minSdk = 26
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    buildFeatures {
        // ĐX-FA-12: giao diện XML Views kèm ViewBinding.
        viewBinding = true
    }
}
