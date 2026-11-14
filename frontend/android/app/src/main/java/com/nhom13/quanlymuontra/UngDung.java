package com.nhom13.quanlymuontra;

import android.app.Application;
import androidx.annotation.NonNull;

/** Lớp {@link Application}: nơi duy nhất tạo {@link AppContainer} (FA 5.1). */
public class UngDung extends Application {

    private AppContainer appContainer;

    @Override
    public void onCreate() {
        super.onCreate();
        appContainer = new AppContainer();
    }

    @NonNull
    public AppContainer getAppContainer() {
        return appContainer;
    }
}
