PRAGMA foreign_keys = OFF;

-- ==============================
-- 1. 媒体库（可创建多个库：电影、美剧、动漫等）
-- ==============================
DROP TABLE IF EXISTS media_libraries;
CREATE TABLE media_libraries (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name VARCHAR(100) NOT NULL,         -- 库名
    type VARCHAR(20) NOT NULL,          -- movie / tv
    path VARCHAR(500) NOT NULL,         -- 库路径
	status VARCHAR(20) ,
    is_enabled BOOLEAN DEFAULT 1,
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 2. 媒体
-- ==============================
DROP TABLE IF EXISTS media_items;
CREATE TABLE media_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
	library_id INTEGER NOT NULL,
    name VARCHAR(100) NOT NULL,         -- 媒体名
	title VARCHAR(255) NOT NULL,          -- 标题
	original_title VARCHAR(255),          -- 原始标题
    type VARCHAR(20) NOT NULL,          -- movie / tv
	release_date DATE,                    -- 上映日期
	summary   VARCHAR(4096),             -- 简介
	poster_path VARCHAR(255),            -- 海报
    thumb_path VARCHAR(255),             -- 缩略图
    fanart_path VARCHAR(255),            -- 粉丝图
	
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);



-- ==============================
-- 2. 电影  media_item的type=movie，详细信息
-- ==============================
DROP TABLE IF EXISTS movies;
CREATE TABLE movies (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    library_id INTEGER NOT NULL,
	media_item_id INTEGER NOT NULL,
    num VARCHAR(64),                          -- 编号/排序号
    studio VARCHAR(255),                  -- 制片公司/工作室
    marker VARCHAR(100),                 -- 制作公司
    runtime INTEGER,                     -- 时长(分钟)
    description TEXT,                    -- 详细描述
    
    screenshots_path VARCHAR(4096),      -- 截图路径
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 3. 电视剧/剧集
-- ==============================
DROP TABLE IF EXISTS tv_shows;
CREATE TABLE tv_shows (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    library_id INTEGER NOT NULL,
    title VARCHAR(255) NOT NULL,
    original_title VARCHAR(255),
    overview TEXT,
    release_date DATE,
    poster_path VARCHAR(255),
    backdrop_path VARCHAR(255),
    rating DECIMAL(3,1),
    total_seasons INTEGER DEFAULT 0,     -- 总季数
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 4. 季
-- ==============================
DROP TABLE IF EXISTS seasons;
CREATE TABLE seasons (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    tv_show_id INTEGER NOT NULL,         -- 所属剧集
    season_number INTEGER NOT NULL,      -- 季号
    name VARCHAR(255),                  -- 季名称
    overview TEXT,
    poster_path VARCHAR(255),
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 5. 集
-- ==============================
DROP TABLE IF EXISTS episodes;
CREATE TABLE episodes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    tv_show_id INTEGER NOT NULL,
    season_id INTEGER NOT NULL,
    episode_number INTEGER NOT NULL,    -- 集号
    title VARCHAR(255),
    overview TEXT,
    runtime INTEGER,                    -- 时长
    release_date DATE,
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 6. 媒体文件（电影/季/集 都对应一个文件）
-- ==============================
DROP TABLE IF EXISTS media_files;
CREATE TABLE media_files (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    media_type VARCHAR(20) NOT NULL,     -- movie / episode
    media_id INTEGER NOT NULL,           -- 对应 movies 或 episodes 的ID
    path VARCHAR(500) NOT NULL UNIQUE,          -- 文件路径
    file_name VARCHAR(255) NOT NULL,
    extension VARCHAR(10),
    container VARCHAR(20),               -- mkv, mp4...
    video_codec VARCHAR(20),
	runtime     INTEGER,
    width INTEGER,
    height INTEGER,
    audio_codec VARCHAR(20),
    size_bytes BIGINT,
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 图片资源
-- ==============================
DROP TABLE IF EXISTS media_imgs;
CREATE TABLE media_imgs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
	media_type VARCHAR(20) NOT NULL,     -- movie / episode
    media_id INTEGER NOT NULL,           -- 对应 movies 或 episodes 的ID
    url_name VARCHAR(128) NOT NULL,     -- 例如 p300111.jpg
    name VARCHAR(128) NOT NULL,           -- 对应 movies 或 episodes 的ID
    path VARCHAR(500) NOT NULL UNIQUE,          -- 文件路径
    file_name VARCHAR(255) NOT NULL,
    extension VARCHAR(10),           -- mkv, mp4...
    type VARCHAR(20),                -- poster、thumb、fanart  
    width INTEGER,
    height INTEGER,
    size_bytes BIGINT,
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 7. 媒体标签绑定
-- ==============================
DROP TABLE IF EXISTS media_tags;
CREATE TABLE media_tags (
	lib_id INTEGER NOT NULL,
    media_type VARCHAR(20) NOT NULL,
    media_id INTEGER NOT NULL,
    tag_id INTEGER NOT NULL,
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
	PRIMARY KEY('lib_id', 'media_id', 'tag_id')
);

DROP TABLE IF EXISTS tags;
CREATE TABLE tags (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    tag_name VARCHAR(50) NOT NULL,
    create_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 8. 用户表
-- ==============================
DROP TABLE IF EXISTS app_user;
CREATE TABLE app_user (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(100),
    is_active BOOLEAN DEFAULT 1,
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 9. 我的收藏
-- ==============================
DROP TABLE IF EXISTS my_favor;
CREATE TABLE my_favor (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
	lib_id INTEGER NOT NULL,
    media_type VARCHAR(20) NOT NULL,    -- movie / tv
    media_id INTEGER NOT NULL,  -- movies id / tv_shows id
    create_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 10. 播放记录
-- ==============================
DROP TABLE IF EXISTS play_record;
CREATE TABLE play_record (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
	lib_id INTEGER NOT NULL,
    media_type VARCHAR(20) NOT NULL,
    media_id INTEGER NOT NULL,     -- movies id / episode id
    position_ms BIGINT DEFAULT 0,        -- 播放进度（毫秒）
    is_finished BOOLEAN DEFAULT 0,
    last_play_time TIMESTAMP,
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 11. 人员表（导演、演员、编剧）
-- ==============================
DROP TABLE IF EXISTS staff;
CREATE TABLE staff (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name VARCHAR(100) NOT NULL,         -- 人员姓名
    avatar_path VARCHAR(255),            -- 头像路径
    country VARCHAR(50),                 -- 国籍（可选）
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

-- ==============================
-- 12. 媒体 <-> 人员 关联表
-- ==============================
DROP TABLE IF EXISTS media_staff;
CREATE TABLE media_staff (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    media_type VARCHAR(20) NOT NULL,    -- movie / tv_show / season / episode
    media_id INTEGER NOT NULL,          -- 对应媒体ID
    staff_id INTEGER NOT NULL,         -- 关联人员ID
    role_type VARCHAR(20) NOT NULL,     -- director / actor / writer
    role_name VARCHAR(100),             -- 饰演角色名（演员专用）
    sort_order INTEGER DEFAULT 0,        -- 排序（主演靠前）
    create_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    update_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);

DROP TABLE IF EXISTS system_sync_logs;
CREATE TABLE system_sync_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    media_library_id int NOT NULL,    
    sync_type VARCHAR(64) NOT NULL,          
    status VARCHAR(64) NOT NULL,         
    added_count INTEGER, 
	updated_count INTEGER,
    deleted_count INTEGER,
    error_message VARCHAR(1024),	
    start_time TIMESTAMP DEFAULT (datetime('now','localtime')),
    end_time TIMESTAMP DEFAULT (datetime('now','localtime'))
);