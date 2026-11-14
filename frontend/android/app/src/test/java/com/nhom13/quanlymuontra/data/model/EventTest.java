package com.nhom13.quanlymuontra.data.model;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;

import org.junit.Test;

public class EventTest {

    @Test
    public void FC63_GetContentIfNotHandled_LanDauTraNoiDungLanSauTraNull() {
        Event<String> suKien = new Event<>("dieu huong");

        assertEquals("dieu huong", suKien.getContentIfNotHandled());
        assertNull(suKien.getContentIfNotHandled());
        assertNull(suKien.getContentIfNotHandled());
    }
}
