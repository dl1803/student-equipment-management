import org.gradle.api.DefaultTask
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction

plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.openapi.generator)
}

// D02-L (ĐX-FC-21, FA 3.2, FC 6.4): tạo bản hợp đồng dành riêng cho Android trong thư mục build
// từ app/openapi/openapi.yaml; tệp nguồn không bị sửa. Chỉ có hai thay đổi:
//   1. Bỏ khóa `enum` trong components.schemas để mọi mã trạng thái, loại, kết quả, vai trò và
//      maLoi được sinh thành String. Enum do bộ sinh tạo ném lỗi (hoặc trả null) khi gặp mã chưa
//      biết nên làm mất mã gốc, trong khi giao diện phải hiển thị nguyên mã.
//   2. Đổi tên hai tag có dấu thành XacThuc và SinhVien để lớp API sinh ra có tên ASCII
//      (XacThucApi, SinhVienApi).
abstract class TaoHopDongAndroid : DefaultTask() {

    @get:InputFile
    abstract val nguon: RegularFileProperty

    @get:OutputFile
    abstract val dich: RegularFileProperty

    @TaskAction
    fun tao() {
        val dongNguon = nguon.get().asFile.readLines(Charsets.UTF_8)
        val doiTenTag = mapOf(
            "- X\u00E1c th\u1EF1c" to "- XacThuc",
            "- Sinh vi\u00EAn" to "- SinhVien",
            "- name: X\u00E1c th\u1EF1c" to "- name: XacThuc",
            "- name: Sinh vi\u00EAn" to "- name: SinhVien",
        )
        val mauEnum = Regex("^(\\s*)enum:\\s*$")
        val dongMoi = ArrayList<String>(dongNguon.size)
        var trongSchemas = false
        var soKhoiEnumDaBo = 0
        var soDongTagDaDoi = 0
        var i = 0
        while (i < dongNguon.size) {
            var dong = dongNguon[i]
            if (dong == "  schemas:") {
                trongSchemas = true
            }
            if (trongSchemas) {
                val khop = mauEnum.matchEntire(dong)
                if (khop != null) {
                    // Bỏ dòng `enum:` và mọi dòng `- giá trị` cùng mức thụt lề ngay sau nó.
                    val thutLe = khop.groupValues[1]
                    i++
                    while (i < dongNguon.size && dongNguon[i].startsWith("$thutLe- ")) {
                        i++
                    }
                    soKhoiEnumDaBo++
                    continue
                }
            }
            val noiDung = dong.trim()
            val tenMoi = doiTenTag[noiDung]
            if (tenMoi != null) {
                dong = dong.replace(noiDung, tenMoi)
                soDongTagDaDoi++
            }
            dongMoi.add(dong)
            i++
        }
        check(soKhoiEnumDaBo > 0) { "Không tìm thấy khối enum nào trong components.schemas" }
        check(soDongTagDaDoi > 0) { "Không tìm thấy dòng tag Xác thực hoặc Sinh viên nào" }
        val tepDich = dich.get().asFile
        tepDich.parentFile.mkdirs()
        tepDich.writeText(dongMoi.joinToString("\n") + "\n", Charsets.UTF_8)
        logger.lifecycle(
            "D02-L: bỏ $soKhoiEnumDaBo khối enum, đổi $soDongTagDaDoi dòng tag, " +
                "ghi ${dongMoi.size} dòng vào ${tepDich.name}"
        )
    }
}

val hopDongNguon = layout.projectDirectory.file("openapi/openapi.yaml")
val hopDongAndroid = layout.buildDirectory.file("openapi/openapi-android.yaml")
val thuMucSinh = layout.buildDirectory.dir("generated/openapi")

val taoHopDongAndroid = tasks.register<TaoHopDongAndroid>("taoHopDongAndroid") {
    nguon.set(hopDongNguon)
    dich.set(hopDongAndroid)
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

    sourceSets {
        getByName("main") {
            // D02-L: mã client sinh nằm trong build/, không commit (FA 5.8).
            java.srcDir(layout.buildDirectory.dir("generated/openapi/src/main/java"))
        }
    }
}

// D02-L (FA 3.2, FC 6.4): Java + Retrofit2 + Gson, chỉ sinh các thao tác thuộc tag Xác thực
// (5) và Sinh viên (16) của bản hợp đồng Android.
openApiGenerate {
    generatorName.set("java")
    library.set("retrofit2")
    inputSpec.set(hopDongAndroid.get().asFile.path)
    outputDir.set(thuMucSinh.get().asFile.path)
    invokerPackage.set("com.nhom13.quanlymuontra.generated")
    apiPackage.set("com.nhom13.quanlymuontra.generated.api")
    modelPackage.set("com.nhom13.quanlymuontra.generated.model")
    cleanupOutput.set(true)
    generateApiTests.set(false)
    generateModelTests.set(false)
    generateApiDocumentation.set(false)
    generateModelDocumentation.set(false)
    openapiNormalizer.set(mapOf("FILTER" to "tag:XacThuc|SinhVien"))
    // Ba schema có discriminator thuộc nhóm Quầy và Quản trị, ngoài phạm vi Android (ĐX-FA-16).
    schemaMappings.set(
        mapOf(
            "KiemTraNguonRequest" to "java.lang.Object",
            "LapPhieuMuonRequest" to "java.lang.Object",
            "BaoCaoJson" to "java.lang.Object",
        )
    )
    configOptions.set(
        mapOf(
            "serializationLibrary" to "gson",
            "dateLibrary" to "java8",
            "openApiNullable" to "false",
            "hideGenerationTimestamp" to "true",
        )
    )
    // Sinh toàn bộ API và model, chỉ lấy một tệp hỗ trợ là JSON.java (bộ chuyển đổi Gson của client).
    globalProperties.set(
        mapOf(
            "apis" to "",
            "models" to "",
            "supportingFiles" to "JSON.java,CollectionFormats.java,StringUtil.java",
        )
    )
}

tasks.named("openApiGenerate") {
    dependsOn(taoHopDongAndroid)
}

// Mã sinh phải có trước mọi bước biên dịch.
tasks.configureEach {
    if (name == "preBuild") {
        dependsOn(tasks.named("openApiGenerate"))
    }
}

dependencies {
    implementation(libs.retrofit)
    implementation(libs.okhttp)
    implementation(libs.gson)
    implementation(libs.gson.fire)
    implementation(libs.androidx.lifecycle.viewmodel)
    implementation(libs.androidx.annotation)
    implementation(libs.jakarta.annotation)
    compileOnly(libs.findbugs.jsr305)

    testImplementation(libs.junit)
}
