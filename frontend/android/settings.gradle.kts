// D01-L: cấu hình dự án Android của sinh viên (FA 5, FC 10.2).
// Chỉ khai báo kho Google và Maven Central; kho khác chỉ thêm khi task đầu tiên cần đến.
// D02-L: thêm Gradle Plugin Portal vì plugin sinh client (org.openapi.generator) được áp dụng
// bằng khối plugins {} ở app/build.gradle.kts.
pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}

dependencyResolutionManagement {
    // Mọi kho thư viện khai báo tập trung tại đây, không khai báo riêng trong module.
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = "QuanLyMuonTraAndroid"
include(":app")
