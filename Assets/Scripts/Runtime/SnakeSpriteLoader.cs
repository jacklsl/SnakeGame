using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SnakeSpriteLoader
{
    // 精灵缓存，避免重复加载
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    static SnakeSpriteLoader()
    {
        SceneManager.sceneUnloaded += _ => spriteCache.Clear();
    }

    /// <summary>
    /// 加载精灵 - 编辑器下使用 AssetDatabase，构建版本使用 Resources.Load
    /// 构建版本会从 Assets/Resources/ 下按原资源路径加载
    /// </summary>
    /// <param name="assetPath">精灵资源路径（编辑器路径如 Assets/snakesprites/png/xxx.png）</param>
    /// <param name="subSpriteName">子精灵名称（用于 Multiple 模式的精灵图）</param>
    public static Sprite LoadSprite(string assetPath, string subSpriteName = null)
    {
        // 生成缓存键
        string cacheKey = string.IsNullOrEmpty(subSpriteName) ? assetPath : $"{assetPath}[{subSpriteName}]";

        // 检查缓存
        if (spriteCache.TryGetValue(cacheKey, out Sprite cachedSprite))
            return cachedSprite;

        Sprite loadedSprite = null;

#if UNITY_EDITOR
        loadedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (loadedSprite == null || !string.IsNullOrEmpty(subSpriteName))
        {
            loadedSprite = PickSprite(AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath), subSpriteName);
        }
#else
        // 构建版本：从 Resources 文件夹加载
        // 将 Assets/snakesprites/png/xxx.png 转换为 Resources 路径
        // 假设精灵已复制到 Assets/Resources/ 下对应路径
        string resourcesPath = GetResourcesPath(assetPath);
        loadedSprite = Resources.Load<Sprite>(resourcesPath);

        if (loadedSprite == null || !string.IsNullOrEmpty(subSpriteName))
        {
            loadedSprite = PickSprite(Resources.LoadAll<Sprite>(resourcesPath), subSpriteName);
        }

        if (loadedSprite == null)
        {
            Debug.LogWarning($"Resources 加载失败: {resourcesPath}，尝试文件加载: {assetPath}");
            loadedSprite = LoadSpriteFromFile(assetPath);
        }
#endif

        if (loadedSprite != null)
        {
            // 加入缓存
            spriteCache[cacheKey] = loadedSprite;
        }
        else
        {
            Debug.LogError($"无法加载精灵: {assetPath}");
        }

        return loadedSprite;
    }

    private static Sprite PickSprite(Object[] assets, string subSpriteName)
    {
        if (assets == null || assets.Length == 0)
            return null;

        Sprite firstSprite = null;
        foreach (Object asset in assets)
        {
            Sprite sprite = asset as Sprite;
            if (sprite == null)
                continue;

            if (firstSprite == null)
                firstSprite = sprite;

            if (!string.IsNullOrEmpty(subSpriteName) && sprite.name == subSpriteName)
                return sprite;
        }

        return string.IsNullOrEmpty(subSpriteName) ? firstSprite : null;
    }

    /// <summary>
    /// 从文件直接加载精灵（构建版本备用方案）
    /// </summary>
    private static Sprite LoadSpriteFromFile(string assetPath)
    {
        if (!File.Exists(assetPath))
            return null;

        byte[] fileData = File.ReadAllBytes(assetPath);
        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(fileData))
            return null;

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// 将 Assets 路径转换为 Resources 加载路径
    /// </summary>
    private static string GetResourcesPath(string assetPath)
    {
        // 处理路径分隔符
        string normalizedPath = assetPath.Replace('\\', '/');

        // 查找 Resources 文件夹后的路径
        int resourcesIndex = normalizedPath.IndexOf("/Resources/", System.StringComparison.OrdinalIgnoreCase);
        if (resourcesIndex >= 0)
        {
            // 提取 Resources/ 之后的部分，去掉文件扩展名
            string relativePath = normalizedPath.Substring(resourcesIndex + "/Resources/".Length);
            return Path.ChangeExtension(relativePath, null);
        }

        // 如果没有 Resources 文件夹，尝试从 Assets/ 后提取路径
        int assetsIndex = normalizedPath.IndexOf("Assets/", System.StringComparison.OrdinalIgnoreCase);
        if (assetsIndex >= 0)
        {
            string relativePath = normalizedPath.Substring(assetsIndex + "Assets/".Length);
            return Path.ChangeExtension(relativePath, null);
        }

        // 直接使用文件名（不含扩展名）
        return Path.GetFileNameWithoutExtension(assetPath);
    }

    /// <summary>
    /// 创建纯色精灵
    /// </summary>
    public static Sprite CreateSolidSprite(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    /// <summary>
    /// 清除精灵缓存（场景切换时调用）
    /// </summary>
    public static void ClearCache()
    {
        spriteCache.Clear();
    }
}
