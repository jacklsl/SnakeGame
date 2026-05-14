using UnityEngine;

public static class SnakeGameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureCamera();

        GameObject root = GameObject.Find("Snake Game Runtime");
        if (root == null)
            root = new GameObject("Snake Game Runtime");

        // 加载配置
        GameConfig config = Resources.Load<GameConfig>("Config/GameConfig");
        if (config == null)
            config = ScriptableObject.CreateInstance<GameConfig>();

        // 按依赖顺序添加组件
        GridManager gridManager = EnsureComponent<GridManager>(root);
        InjectConfig(gridManager, config);

        SnakeController snakeController = EnsureComponent<SnakeController>(root);
        InjectConfig(snakeController, config);

        FoodSpawner foodSpawner = EnsureComponent<FoodSpawner>(root);
        InjectConfig(foodSpawner, config);

        EnsureComponent<ScoreManager>(root);
        EnsureComponent<GameManager>(root);
        EnsureComponent<InputManager>(root);
        EnsureComponent<UIManager>(root);

        // GridManager 渲染子组件
        if (root.GetComponent<GridBackgroundRenderer>() == null)
            root.AddComponent<GridBackgroundRenderer>();
        if (root.GetComponent<GridWallRenderer>() == null)
            root.AddComponent<GridWallRenderer>();
    }

    private static void InjectConfig<T>(T component, GameConfig config) where T : Component
    {
        var field = typeof(T).GetField("config",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        if (field != null && field.FieldType == typeof(GameConfig))
            field.SetValue(component, config);
    }

    private static void EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 12.5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(0.09f, 0.13f, 0.16f);
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private static T EnsureComponent<T>(GameObject root) where T : Component
    {
        T existing = Object.FindAnyObjectByType<T>();
        if (existing != null)
            return existing;
        return root.AddComponent<T>();
    }
}
