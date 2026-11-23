package com.nhom13.quanlymuontra;

import androidx.annotation.NonNull;
import com.nhom13.quanlymuontra.ui.tracuu.BoHenGio;
import java.util.ArrayList;
import java.util.List;

/** Bộ hẹn giờ giả cho test: ghi lại các việc đã hẹn và chỉ chạy khi test gọi, không dùng đồng hồ thật. */
public final class BoHenGioGia implements BoHenGio {

    private static final class Viec {
        final Runnable ma;
        final long doTreMs;
        boolean daHuy;
        boolean daChay;

        Viec(Runnable ma, long doTreMs) {
            this.ma = ma;
            this.doTreMs = doTreMs;
        }
    }

    private final List<Viec> cacViec = new ArrayList<>();

    @NonNull
    @Override
    public Runnable datLich(@NonNull Runnable viec, long doTreMs) {
        Viec moi = new Viec(viec, doTreMs);
        cacViec.add(moi);
        return () -> moi.daHuy = true;
    }

    /** Số việc đã hẹn, chưa bị hủy và chưa chạy. */
    public int soViecConHieuLuc() {
        int dem = 0;
        for (Viec v : cacViec) {
            if (!v.daHuy && !v.daChay) {
                dem++;
            }
        }
        return dem;
    }

    /** Độ trễ của việc hẹn gần nhất. */
    public long doTreCuoi() {
        return cacViec.get(cacViec.size() - 1).doTreMs;
    }

    /** Chạy mọi việc còn hiệu lực. */
    public void chayViecConHieuLuc() {
        for (Viec v : new ArrayList<>(cacViec)) {
            if (!v.daHuy && !v.daChay) {
                v.daChay = true;
                v.ma.run();
            }
        }
    }
}
