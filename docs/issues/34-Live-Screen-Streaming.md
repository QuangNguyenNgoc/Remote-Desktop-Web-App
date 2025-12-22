# Issue: Live Screen Streaming & Recording

## 🎯 Mục tiêu

Triển khai tính năng xem màn hình Agent **real-time** (live streaming) và khả năng **ghi lại thành video**.

---

## 📋 Phạm vi

### Phase 1: Live Streaming (MVP)
- [ ] Stream màn hình Agent real-time (5-15 FPS)
- [ ] UI controls: Start/Stop stream
- [ ] Hiển thị FPS và latency indicator
- [ ] Auto-reconnect khi mất kết nối

### Phase 2: Recording (Optional)
- [ ] Ghi stream thành video
- [ ] Download video file

---

## 🏗️ Kiến trúc đề xuất

### Agent Side

```
ScreenStreamService
├── StartStreaming(fps, quality)
├── StopStreaming()
└── CaptureLoop() → SignalR.SendFrame(base64)
```

**Options:**
- **Simple**: Loop screenshot + JPEG compress + SignalR send
- **Advanced**: FFmpeg pipe để encode H.264 stream

### Web Side

```
RemoteScreenTab.razor
├── Start Live / Stop Live buttons
├── <canvas> hoặc <img> để render frames
├── FPS counter
└── Recording controls (Phase 2)
```

---

## 🖼️ Recording Options (Mở rộng)

### Option A: Web-side Recording (MediaRecorder API)

```
Stream Frames → Canvas → MediaRecorder → WebM file
```

**Pros:**
- Không cần thêm dependencies ở Agent
- Browser xử lý encoding
- Download trực tiếp từ browser

**Cons:**
- Giới hạn format (WebM)
- Không có audio

### Option B: Agent-side Recording (FFmpeg)

```
ScreenCapture → FFmpeg pipe → MP4 file → Transfer to Web
```

**Pros:**
- Chất lượng cao, nhiều format
- Có thể thêm **audio** (system audio hoặc mic)
- Không phụ thuộc browser

**Cons:**
- Cần bundle FFmpeg (~80MB) hoặc yêu cầu user cài
- File transfer sau khi record xong

### Option C: Hybrid (Khuyến nghị cho tương lai)

1. **Live Stream** - Real-time viewing (Phase 1)
2. **Quick Record** - Web-side MediaRecorder (không audio)
3. **Full Record** - Agent-side FFmpeg (có audio) - Advanced feature

---

## 🔊 Audio Recording (Tương lai)

Nếu muốn record **có âm thanh**:

| Audio Source | Approach | Complexity |
|--------------|----------|------------|
| System Audio | NAudio/WASAPI loopback | ⭐⭐⭐ |
| Microphone | NAudio/DirectSound | ⭐⭐ |
| Both | Mix streams | ⭐⭐⭐⭐ |

**Yêu cầu:**
- NAudio library (C#)
- FFmpeg để mux video + audio
- Hoặc ScreenRecorderLib (all-in-one)

---

## 📊 Bandwidth & Performance

| Quality | Resolution | FPS | Est. Bandwidth |
|---------|------------|-----|----------------|
| Low | 720p | 5 | ~200 KB/s |
| Medium | 1080p | 10 | ~800 KB/s |
| High | 1080p | 15 | ~1.5 MB/s |

**Optimizations:**
- JPEG quality slider (50-90%)
- Resolution scaling
- Delta compression (chỉ gửi vùng thay đổi)
- WebP format (tốt hơn JPEG)

---

## 🔗 Dependencies

### Current (Reuse)
- `ScreenshotService` - Screenshot capture
- SignalR Hub - Transport

### New (Phase 1)
- Không cần thêm gì

### New (Phase 2 - Recording với audio)
- **NAudio** - Audio capture
- **FFmpeg** - Encoding (optional, bundle hoặc user install)
- Hoặc **ScreenRecorderLib** - All-in-one recording library

---

## ✅ Acceptance Criteria

### Phase 1
- [ ] Nhấn "Start Live" → xem màn hình Agent real-time
- [ ] FPS hiển thị chính xác
- [ ] Nhấn "Stop Live" → dừng stream
- [ ] Không crash khi Agent disconnect

### Phase 2
- [ ] Nhấn "Record" → bắt đầu ghi
- [ ] Nhấn "Stop Record" → download WebM file
- [ ] (Optional) Audio recording với FFmpeg

---

## 📝 Notes

- Phase 1 có thể làm nhanh (~1-2 ngày) vì reuse screenshot logic
- Recording trên Web (MediaRecorder) đơn giản, không cần Agent thay đổi
- Recording với audio cần Agent thay đổi và thêm dependencies
- Cân nhắc: Quality vs Bandwidth tradeoff
