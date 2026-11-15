package com.nhom13.quanlymuontra.data.session;

import android.content.SharedPreferences;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

/**
 * {@link SharedPreferences} trong bộ nhớ cho kiểm thử JVM. Đếm số lần {@code commit} và {@code
 * apply} để kiểm tra cách {@link TokenStore} ghi; {@code commitThanhCong} giả lập lỗi ghi đĩa.
 */
final class FakeSharedPreferences implements SharedPreferences {

    private final Map<String, String> duLieu = new HashMap<>();
    boolean commitThanhCong = true;
    int soLanCommit = 0;
    int soLanApply = 0;

    /** Ghi thẳng một giá trị, dùng để dựng dữ liệu hỏng trong kiểm thử. */
    void datTruc(@NonNull String khoa, @Nullable String giaTri) {
        duLieu.put(khoa, giaTri);
    }

    @NonNull
    Map<String, String> saoChepDuLieu() {
        return new HashMap<>(duLieu);
    }

    @Override
    public Map<String, ?> getAll() {
        return new HashMap<>(duLieu);
    }

    @Nullable
    @Override
    public String getString(String key, @Nullable String defValue) {
        return duLieu.containsKey(key) ? duLieu.get(key) : defValue;
    }

    @Nullable
    @Override
    public Set<String> getStringSet(String key, @Nullable Set<String> defValues) {
        return defValues;
    }

    @Override
    public int getInt(String key, int defValue) {
        return defValue;
    }

    @Override
    public long getLong(String key, long defValue) {
        return defValue;
    }

    @Override
    public float getFloat(String key, float defValue) {
        return defValue;
    }

    @Override
    public boolean getBoolean(String key, boolean defValue) {
        return defValue;
    }

    @Override
    public boolean contains(String key) {
        return duLieu.containsKey(key);
    }

    @Override
    public Editor edit() {
        return new FakeEditor();
    }

    @Override
    public void registerOnSharedPreferenceChangeListener(
            OnSharedPreferenceChangeListener listener) {}

    @Override
    public void unregisterOnSharedPreferenceChangeListener(
            OnSharedPreferenceChangeListener listener) {}

    private final class FakeEditor implements Editor {

        private final Map<String, String> choGhi = new HashMap<>();
        private final Set<String> choXoa = new HashSet<>();

        @Override
        public Editor putString(String key, @Nullable String value) {
            choGhi.put(key, value);
            return this;
        }

        @Override
        public Editor putStringSet(String key, @Nullable Set<String> values) {
            return this;
        }

        @Override
        public Editor putInt(String key, int value) {
            return this;
        }

        @Override
        public Editor putLong(String key, long value) {
            return this;
        }

        @Override
        public Editor putFloat(String key, float value) {
            return this;
        }

        @Override
        public Editor putBoolean(String key, boolean value) {
            return this;
        }

        @Override
        public Editor remove(String key) {
            choXoa.add(key);
            return this;
        }

        @Override
        public Editor clear() {
            choXoa.addAll(duLieu.keySet());
            return this;
        }

        @Override
        public boolean commit() {
            soLanCommit++;
            ap();
            return commitThanhCong;
        }

        @Override
        public void apply() {
            soLanApply++;
            ap();
        }

        private void ap() {
            for (String khoa : choXoa) {
                duLieu.remove(khoa);
            }
            duLieu.putAll(choGhi);
        }
    }
}
