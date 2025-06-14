# Play scene Main để chơi.

# 1 số package dùng trong project:
- Dotween
- Odin
- Cartoon FX Remaster Free

# Lưu ý: do thời gian có hạn nên không thể tạo 1 màn chơi hoàn chỉnh. Nhưng vẫn có thể bằng cách tạo data scriptableobject DataSong

# Mô Tả Core Game Logic - MagicTile3

## Giới thiệu chung

Dự án này là một game âm nhạc (rhythm game), nơi người chơi tương tác với các "tiles" (ô) xuất hiện trên màn hình theo nhịp điệu của bài hát. Mục tiêu là chạm chính xác vào các tiles để ghi điểm và hoàn thành bài hát.

## Cấu trúc thư mục Scripts (Tổng quan)

Mã nguồn trong `Assets/Scripts` được tổ chức thành các thư mục con chính:

*   **`Manager`**: Chứa các lớp quản lý singleton cho các hệ thống cốt lõi của game (Game, UI, Âm thanh, Điểm, Sự kiện).
*   **`Controller`**: Chứa các lớp điều khiển logic gameplay chính (Sinh tile, Xử lý chạm, Điều khiển VFX, Camera).
*   **`Tiles`**: Chứa logic liên quan đến các loại tile khác nhau, bao gồm cách chúng được sinh ra và cách tương tác.
*   **`UI`**: Chứa các lớp cho giao diện người dùng, bao gồm các màn hình, popup, và các thành phần UI cơ bản.
    *   **`Base`**: Các lớp cơ sở cho UI.
*   **`Data`**: Chứa các ScriptableObject định nghĩa dữ liệu game, ví dụ như dữ liệu bài hát.
*   **`VFX`**: (Ngụ ý từ `VFXController.cs`) Logic điều khiển hiệu ứng hình ảnh.

## Luồng Game Chính (Core Gameplay Loop)

1.  **Khởi Động Game & Giao Diện:**
    *   `GameManager` là điểm khởi đầu, quản lý trạng thái chung của game.
    *   `UIManager` chịu trách nhiệm hiển thị giao diện người dùng, ban đầu là `GameScreen`.
    *   `GameScreen` hiển thị thông tin như điểm số, thanh tiến trình bài hát và nút bắt đầu.

2.  **Tải và Bắt Đầu Bài Hát:**
    *   Khi người chơi nhấn bắt đầu, `GameManager` gọi `LoadGame(DataSong)`.
    *   `DataSong` là một ScriptableObject chứa thông tin bài hát (tên, ca sĩ) và quan trọng nhất là `tilePointDatas` (dữ liệu vị trí và thời gian của các tiles).
    *   `AudioManager` phát nhạc nền (BGM) tương ứng với bài hát.
    *   `SpawnTileController` nhận `DataSong` để chuẩn bị sinh tiles.
    *   `CameraMove` được thiết lập để di chuyển dựa trên tốc độ và thời lượng bài hát.
    *   `ProgressBarController` được thiết lập với thời lượng bài hát và số lượng sao mục tiêu.

3.  **Sinh Tiles:**
    *   `SpawnTileController` chịu trách nhiệm sinh các tiles (`shortTilePrefab`, `longTilePrefab`) dựa trên `tilePointDatas` từ `DataSong`.
    *   Sử dụng các chiến lược sinh tile khác nhau:
        *   `ShortTileSpawner`: Sinh các tile ngắn (một điểm chạm). Các tile ngắn được quản lý qua một object pool để tối ưu hiệu suất.
        *   `LongTileSpawner`: Sinh các tile dài (yêu cầu giữ hoặc kéo theo đường). Tile dài sử dụng `LineRenderer` để vẽ và `PolygonCollider2D` để tương tác.
    *   Tiles được sinh ra ở các vị trí `_xPositions` xác định trước và di chuyển xuống cùng với camera.
    *   `_speedFactor` (tính từ `GameManager.cameraSpeedFactor` và `CONST.CAMERA_SPEED`) quyết định tốc độ di chuyển và vị trí Y của tiles.

4.  **Tương Tác Người Chơi (Chạm):**
    *   `TouchController` phát hiện các sự kiện chạm thô trên màn hình (`OnTouchDown`, `OnTouchUp`).
    *   `HandleTouchController` lắng nghe các sự kiện từ `TouchController`.
        *   Khi chạm xuống, nó chuyển đổi tọa độ màn hình sang tọa độ thế giới.
        *   Sử dụng `Physics2D.OverlapPoint` để kiểm tra xem có `Collider2D` nào của tile tại vị trí chạm không.
        *   Nếu có tile:
            *   Gọi phương thức `OnTouchDown()` trên component `TileInteract` của tile đó (ví dụ: `ShortTileInteract`, `LongTileInteract`).
            *   Xác định độ chính xác của cú chạm (Perfect/Good) dựa trên vị trí Y của điểm chạm trên màn hình.
            *   `ScoreManager` được thông báo để cập nhật điểm và combo.
            *   `VFXController` được gọi để phát hiệu ứng hình ảnh (ví dụ: `tapVfx`, hiệu ứng Perfect/Good, hiệu ứng số combo).
            *   Đối với tile dài (`LongTileInteract`), animation của `LineRenderer` được kích hoạt.
        *   Nếu không có tile (chạm sai):
            *   Phát sự kiện `EventID.TouchWrong`.
            *   `GameManager.EndGame()` được gọi, kết thúc lượt chơi.
            *   Hiệu ứng `errorTile` được hiển thị.

5.  **Xử Lý Tile và Kết Thúc Game:**
    *   `ShortTileInteract`: Khi được chạm, tile sẽ được trả về pool thông qua `SpawnTileController.ReturnShortTileToPool()`.
    *   `LongTileInteract`: Khi chạm xuống, tile bắt đầu animation. Khi chạm nhấc lên hoặc animation hoàn tất, tile có thể bị vô hiệu hóa hoặc xử lý tiếp.
    *   `SpawnTileController` theo dõi `_currentTileIndex` (tile dự kiến tiếp theo). Nếu tile này trôi qua một giới hạn nhất định dưới camera mà không được tương tác, game sẽ kết thúc (`EventID.OnTileToEnd`, `GameManager.EndGame()`).
    *   Khi bài hát kết thúc (camera đạt `maxHeight`), game cũng có thể chuyển sang trạng thái kết thúc.

6.  **Hiển Thị và Cập Nhật:**
    *   `ScoreManager` cập nhật điểm số, `GameScreen` hiển thị điểm này.
    *   `ProgressBarController` cập nhật thanh tiến trình và kích hoạt animation cho các `StarAnimation` khi đạt các mốc thời gian.
    *   `VFXController` hiển thị các hiệu ứng "Perfect", "Good", "x" (combo) và các hiệu ứng số cho combo.

## Các Thành Phần Chính

### Managers

*   **`GameManager.cs`**:
    *   Quản lý trạng thái game (bắt đầu, kết thúc).
    *   Tải dữ liệu bài hát (`DataSong`).
    *   Điều phối `SpawnTileController`, `AudioManager`, `CameraMove`.
    *   Lưu trữ `cameraSpeedFactor`.
    *   Phát sự kiện `EventID.SetupGameScreen`.
*   **`UIManager.cs`**:
    *   Quản lý việc hiển thị và ẩn các UI elements (Screens, Popups, Notifies, Overlaps) thông qua các Subsystem (`ScreenSubsystem`, `PopupSubsystem`, v.v.).
    *   Sử dụng các lớp `BaseUIElement` làm cơ sở.
*   **`ScoreManager.cs`**:
    *   Theo dõi `CurrentScore`, `PerfectCombo`, `CurrentRateText` (perfect/good).
    *   Cung cấp phương thức `RecordHit()` để cập nhật điểm dựa trên độ chính xác.
    *   Phát sự kiện `OnScoreUpdated` để `GameScreen` cập nhật UI.
*   **`AudioManager.cs`**:
    *   Quản lý việc phát nhạc nền (BGM) và hiệu ứng âm thanh (SE).
    *   Tải âm thanh từ thư mục `Resources/Audio`.
    *   Hỗ trợ fade BGM, thay đổi volume, mute/unmute.
*   **`ListenerManager.cs`**:
    *   Hệ thống sự kiện publish-subscribe đơn giản.
    *   Cho phép các thành phần khác nhau giao tiếp mà không cần tham chiếu trực tiếp, thông qua `EventID`.
    *   Cung cấp các phương thức `Register`, `Unregister`, `Broadcast`.

### Controllers

*   **`SpawnTileController.cs`**:
    *   Chịu trách nhiệm chính cho việc sinh các loại tile (`shortTilePrefab`, `longTilePrefab`) từ `DataSong`.
    *   Quản lý pool cho `shortTilePrefab`.
    *   Sử dụng `ITileSpawner` (`ShortTileSpawner`, `LongTileSpawner`) để tạo instance tile.
    *   Theo dõi tile hiện tại và kiểm tra điều kiện thua (bỏ lỡ tile).
    *   Xử lý sự kiện `EventID.TouchWrong` để hiển thị `errorTile`.
*   **`TouchController.cs`**:
    *   Phát hiện input chạm đa điểm từ người chơi.
    *   Phát các sự kiện `OnTouchDown` và `OnTouchUp` với ID của chạm và vị trí.
*   **`HandleTouchController.cs`**:
    *   Lắng nghe sự kiện từ `TouchController`.
    *   Xác định tile nào được chạm bằng `Physics2D.OverlapPoint`.
    *   Gọi `OnTouchDown()` trên `TileInteract` của tile.
    *   Xác định độ chính xác (Perfect/Good).
    *   Thông báo cho `ScoreManager` và `VFXController`.
    *   Xử lý trường hợp chạm sai (miss) và kết thúc game.
    *   Kích hoạt `tapVfx`.
*   **`VFXController.cs`**:
    *   Quản lý và phát các `ParticleSystem` cho hiệu ứng hình ảnh (Good, Perfect, Combo 'x', số hàng chục, số hàng đơn vị).
    *   Phương thức `PlayVFXWithMultiplier` kết hợp hiệu ứng cơ bản và hiệu ứng số dựa trên combo.
*   **`ProgressBarController.cs`**:
    *   Hiển thị tiến trình bài hát bằng `Slider`.
    *   Sinh và quản lý các `StarAnimation` dọc theo thanh tiến trình.
    *   Kích hoạt animation đổi màu của sao (`img.DOColor`) khi đạt các mốc thời gian.

### Tiles

*   **`TileInteract.cs`**:
    *   Lớp trừu tượng cơ sở cho tất cả các tile có thể tương tác.
    *   Định nghĩa các phương thức `OnTouchDown()` và `OnTouchUp()`.
    *   Có cờ `isInteractable`.
*   **`ShortTileInteract.cs`**:
    *   Kế thừa `TileInteract`.
    *   Khi `OnTouchDown()`, phát sự kiện `EventID.OnTileInteract` và trả tile về pool qua `SpawnerController`.
*   **`LongTileInteract.cs`**:
    *   Kế thừa `TileInteract`.
    *   Sử dụng một `LineRenderer` (`animationLineRenderer`) để hiển thị animation khi tile được giữ.
    *   `OnTouchDown()`: Bắt đầu animation, đánh dấu `isInteractable = false`.
    *   `OnTouchUp()`: Dừng animation.
    *   `Update()`: Xử lý logic animation, di chuyển các điểm của `animationLineRenderer` theo `targetPoints` (lấy từ `originalLineRenderer`).
*   **`ITileSpawner.cs`**:
    *   Interface định nghĩa phương thức `Spawn()` cho việc tạo tile.
*   **`ShortTileSpawner.cs`**:
    *   Triển khai `ITileSpawner` để sinh các tile ngắn.
    *   **Lưu ý:** Logic sinh tile ngắn chủ yếu được xử lý trong `SpawnTileController` với cơ chế pooling. Class này có thể ít được sử dụng trực tiếp nếu pooling là cơ chế chính.
*   **`LongTileSpawner.cs`**:
    *   Triển khai `ITileSpawner` để sinh các tile dài.
    *   Cấu hình `LineRenderer` và `PolygonCollider2D` cho tile dài dựa trên `TilePointData`.
    *   `SetupPolygonCollider`: Tạo hình dạng collider phức tạp cho tile dài.

### Data

*   **`DataSong.cs`**:
    *   `ScriptableObject` chứa dữ liệu cho một bài hát.
    *   `songName`, `singerName`.
    *   `tilePointDatas`: Một danh sách các `TilePointData`.
*   **`TilePointData.cs`** (class lồng trong `DataSong.cs` hoặc file riêng):
    *   Chứa danh sách các `Vector2` (`points`). Mỗi `Vector2` có `x` là thời gian (time) và `y` là chỉ số cột (column index).
    *   Một `TilePointData` với 1 điểm là tile ngắn, nhiều hơn 1 điểm là tile dài.

### UI

*   **`GameScreen.cs`**:
    *   Màn hình chơi game chính, kế thừa `BaseScreen`.
    *   Hiển thị điểm (`scoreText`), thanh tiến trình (`progressBarController`).
    *   Tham chiếu đến `VFXController`.
    *   Xử lý sự kiện `OnScoreUpdated` từ `ScoreManager` để cập nhật UI và kích hoạt hiệu ứng decor.
    *   Xử lý sự kiện `EventID.SetupGameScreen` để cấu hình `ProgressBarController`.
*   **`BaseUIElement.cs`**, **`BaseScreen.cs`**, **`BasePopup.cs`**, **`BaseNotify.cs`**, **`BaseOverlap.cs`**:
    *   Các lớp cơ sở cho hệ thống UI, quản lý `CanvasGroup`, trạng thái `Init`, `Show`, `Hide`.
*   **`StarAnimation.cs`**:
    *   Điều khiển animation của một ngôi sao trên thanh tiến trình (thay đổi màu).
*   **`ProgressBarController.cs`**: (Đã mô tả ở phần Controllers)

### Khác

*   **`CameraMove.cs`**:
    *   Điều khiển camera di chuyển lên trên với tốc độ (`speed`) được tính từ `GameManager.Instance.cameraSpeedFactor` và `CONST.CAMERA_SPEED`.
    *   `maxHeight` xác định giới hạn di chuyển của camera, dựa trên thời lượng bài hát.
    *   `canMove` để bật/tắt di chuyển.
*   **`CONST.cs`**:
    *   Lớp static chứa các hằng số của game như `CAMERA_SPEED`, các giá trị mặc định cho âm thanh (`BGM_VOLUME_DEFAULT`, `SE_VOLUME_DEFAULT`), tốc độ fade BGM.
*   **`TestLineRenderer.cs`**:
    *   Một script thử nghiệm để vẽ `LineRenderer` theo một đường dẫn các điểm (`pathPoints`) với tốc độ (`speed`) nhất định. Có vẻ dùng để debug hoặc thử nghiệm logic animation cho tile dài.

## Hệ thống Sự kiện (`ListenerManager` & `EventID`)

Hệ thống sự kiện đóng vai trò quan trọng trong việc giao tiếp giữa các module mà không cần chúng phải biết về nhau một cách trực tiếp.

*   **`EventID.cs`**: Enum định nghĩa các loại sự kiện có thể xảy ra trong game.
    *   `OnTileInteract`: Khi một tile được tương tác thành công.
    *   `OnTileToEnd`: Khi một tile bị bỏ lỡ và trôi qua màn hình (dẫn đến thua).
    *   `SetupGameScreen`: Khi cần thiết lập màn hình game với dữ liệu bài hát (ví dụ: thời lượng).
    *   `TouchWrong`: Khi người chơi chạm vào vị trí không hợp lệ.
*   **`ListenerManager.cs`**:
    *   Các đối tượng đăng ký (`Register`) để lắng nghe một `EventID` cụ thể với một `Action<object>`.
    *   Các đối tượng khác có thể phát (`Broadcast`) một `EventID` với dữ liệu tùy chọn.
    *   Tất cả các listener đã đăng ký cho `EventID` đó sẽ nhận được thông báo và thực thi `Action` của chúng.
    *   Ví dụ: `ShortTileInteract` phát `EventID.OnTileInteract`, `SpawnTileController` lắng nghe sự kiện này để cập nhật `_currentTileIndex`. `HandleTouchController` phát `EventID.TouchWrong`, `SpawnTileController` lắng nghe để hiển thị `errorTile`.

